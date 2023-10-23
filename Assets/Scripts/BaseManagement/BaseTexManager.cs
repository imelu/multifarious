using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Zhdk.Gamelab;
using System.Threading;

#if UNITY_EDITOR
using System.IO;
#endif


public struct StampTexData
{
    public Color[] pixels;
    public int width;
    public int height;
    public int minX;
    public int minY;
    public int maxX;
    public int maxY;
}

public class BaseTexManager : MonoBehaviour
{
    [SerializeField] private RenderTexture rt;
    [SerializeField] private Texture2D drawTex;
    [SerializeField] private Color drawCol;

    [SerializeField] private RenderTexture rtGoo;
    [SerializeField] private RenderTexture rtPlayer;

    [SerializeField] private Transform floorPlane;
    Vector2 mapSize;

    private Texture2D readRt;
    private Texture2D readRtGoo;
    private Texture2D connReadRt;

    private Texture2D DSrt;
    private Texture2D DSrtGoo;
    private Texture2D DSrtPlayer;

    private Vector2Int renderTexSize;
    Rect rectReadPicture;

    private float _areaPercent;
    public float areaPercent { get { return _areaPercent; } }

    private int _startBaseSizePixels;

    private ResourceConnectionManager _resourceConnManager;

    private Cell[,] _ConnectivityGrid;
    public Cell[,] ConnectivityGrid { get { return _ConnectivityGrid; } }
    private Cell[,] _calcGrid;

    private bool _connectionsCalcNeeded = true;
    private bool _connectionsCalculating;
    private bool _areaCalcNeeded = true;
    private bool _areaCalculating;

    [SerializeField] private LayerMask _floorMask;

    private List<GameObject> _connParticlesToDestroy = new List<GameObject>();

    private bool _particleCalculated = false;

    /*[SerializeField] private AnimationCurve _drawSizeCurve;
    [SerializeField] private int _drawSizeFactor;*/

    [SerializeField] private bool _startCalcOnStart;

    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    public enum DrawSize
    {
        small = 20,
        medium = 70,
        large = 115
    }

    private void Start()
    {
        mapSize = new Vector2(floorPlane.localScale.x, floorPlane.localScale.z) * 10;
        renderTexSize = new Vector2Int(rt.width, rt.height);
        readRt = new Texture2D(renderTexSize.x, renderTexSize.y);
        readRtGoo = new Texture2D(renderTexSize.x, renderTexSize.y);
        connReadRt = new Texture2D(renderTexSize.x, renderTexSize.y, TextureFormat.ARGB32, false);
        rectReadPicture = new Rect(0, 0, renderTexSize.x, renderTexSize.y);
        ClearRenderTexture(rt);

        // reenable for the start circle
        //RenderColor(rt, new Vector2(renderTexSize.x / 2, renderTexSize.y / 2), renderTexSize.x * 100 / mapSize.x / 3, drawCol);

        _resourceConnManager = GetComponent<ResourceConnectionManager>();
        //CalculateConnections();

        OnMapChange += MapChanged;

        if (_startCalcOnStart) StartCalculations();

        Resources.UnloadUnusedAssets();
    }

    public void StartCalculations() 
    {
        StartConnectionCalc().Forget();
        StartAreaCalc().Forget();
    }

    private void Update()
    {
        //UpdateGooTex();
    }

    private void OnEnable()
    {
        if(_cancellationToken != null)
        {
            _cancellationToken.Dispose();
        }
        _cancellationToken = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        //eventunsub
        //OnMapChange -= MapChanged;
        OnMapChange = null;
        OnConnectionsCalculated = null;
        OnAreaCalculated = null;
        _connectionsCalcNeeded = false;

        _cancellationToken.Cancel();
        _cancellationToken.Dispose();
    }

    // TODO: add a direction vector to define the color
    public void DrawOnBase(Vector3 pos, DrawSize size)
    {
        Vector2 percentPos = GetPercentPos(new Vector2(-pos.x, pos.z));

        if (OnBasePercent(percentPos))
        {
            int xPix = (int)(percentPos.x * renderTexSize.x);
            int yPix = (int)(percentPos.y * renderTexSize.y);
            RenderColor(rt, new Vector2(xPix, yPix), (int)size * 100 / mapSize.x, drawCol);
        }
    }

    public void DrawOnPos(Vector3 pos, DrawSize size)
    {
        Vector2 percentPos = GetPercentPos(new Vector2(-pos.x, pos.z));

        int xPix = (int)(percentPos.x * renderTexSize.x);
        int yPix = (int)(percentPos.y * renderTexSize.y);
        RenderColor(rt, new Vector2(xPix, yPix), (int)size * 100 / mapSize.x, drawCol);
    }

    /*public void DrawOnPos(List<Vector2> positions, DrawSize size)
    {
        List<Vector2> rtPos = new List<Vector2>();
        foreach(Vector2 pos in positions)
        {
            Vector2 percentPos = GetPercentPos(new Vector2(-pos.x, pos.y));
            int xPix = (int)(percentPos.x * renderTexSize.x);
            int yPix = (int)(percentPos.y * renderTexSize.y);

            rtPos.Add(new Vector2(xPix, yPix));
        }

        RenderColor(rt, rtPos, (int)size * 100 / mapSize.x, drawCol);
    }*/

    public void RemoveOnPos(Vector3 pos, DrawSize size)
    {
        Vector2 percentPos = GetPercentPos(new Vector2(-pos.x, pos.z));
        int xPix = (int)(percentPos.x * renderTexSize.x);
        int yPix = (int)(percentPos.y * renderTexSize.y);
        RenderColor(rt, new Vector2(xPix, yPix), (int)size * 100 / mapSize.x, Color.black);
        OnMapChange?.Invoke();
    }

    // TODO: currently only checks pixel deadcenter of the character, maybe check a small circle below it
    public bool OnBase(Vector3 pos)
    {
        return OnBasePercent(GetPercentPos(new Vector2(-pos.x, pos.z)));
    }

    private bool OnBasePercent(Vector2 percentPos)
    {
        int xPix = (int)(percentPos.x * renderTexSize.x);
        int yPix = (int)(-percentPos.y * renderTexSize.y);
        Color value = Color.black;
        if (readRt != null) value = readRt.GetPixel(xPix, yPix);
        return value.g > 0.1f;
    }

    public bool IsDiscovered(Vector3 pos)
    {
        Cell cell = GetCellAtPos(pos);
        if (cell.pathVal == int.MaxValue) return false;
        return cell.isConnectedDiscover;
    }

    public Vector2 GetPercentPos(Vector2 pos)
    {
        pos += mapSize / 2;
        return pos / mapSize;
    }

    public Vector2 GetPosFromPercent(Vector2 percentPos)
    {
        percentPos *= mapSize;
        return percentPos - mapSize / 2;
    }

    public Vector3 GetPosWithHeight(Vector2 pos)
    {
        Vector3 rayCastOrigin = new Vector3(pos.x, 150, pos.y);
        RaycastHit hit;
        if (Physics.Raycast(rayCastOrigin, transform.TransformDirection(Vector3.down), out hit, 300, _floorMask))
        {
            return hit.point;
        }
        else
        {
            Debug.LogError("ray hit nothing");
            return new Vector3(-1, -1, -1);
        }
    }

    private void UpdateTex()
    {
        //RenderTexture.active = rt;
        // Read pixels
        readRt.ReadPixels(rectReadPicture, 0, 0);
        readRt.Apply();
        //RenderTexture.active = null;
        OnMapChange?.Invoke();
    }

    private void RenderColor(RenderTexture crt, Vector2 pos, float size, Color col)
    {
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, renderTexSize.x, renderTexSize.y, 0);
        RenderTexture.active = crt;
        Graphics.DrawTexture(new Rect(pos.x - size / 2, pos.y - size / 2, size, size), drawTex, new Rect(0, 0, 1, 1), 0, 0, 0, 0, col, null);
        UpdateTex();
        RenderTexture.active = null;
        GL.PopMatrix();
    }

    /*private void RenderColor(RenderTexture crt, List<Vector2> positions, float size, Color col)
    {
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, renderTexSize.x, renderTexSize.y, 0);
        RenderTexture.active = crt;
        float drawSize;
        for(int i = 0; i < positions.Count; i++)
        {
            drawSize = _drawSizeFactor * _drawSizeCurve.Evaluate(1 - (float)i / positions.Count);
            Graphics.DrawTexture(new Rect(positions[i].x - drawSize / 2, positions[i].y - drawSize / 2, drawSize, drawSize), drawTex, new Rect(0, 0, 1, 1), 0, 0, 0, 0, col, null);
        }
        //foreach(Vector2 pos in positions)
        //{
        //    Graphics.DrawTexture(new Rect(pos.x - size / 2, pos.y - size / 2, size, size), drawTex, new Rect(0, 0, 1, 1), 0, 0, 0, 0, col, null);
        //}
        UpdateTex();
        RenderTexture.active = null;
        GL.PopMatrix();
    }*/

    private void ClearRenderTexture(RenderTexture renderTexture)
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }

    private Color[] CombineTextures(Texture2D tex1, Texture2D tex2, Texture2D tex3)
    {
        Color[] pixels = tex1.GetPixels();
        Color[] pixels2 = tex2.GetPixels();
        Color[] pixels3 = tex3.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].g > 0.1f)
            {
                pixels[i] = Color.green;
                pixels[i].a = 1;
            }
            else
            {
                if (pixels2[i].r > 0.1f)
                {
                    pixels[i].r = 1;
                    pixels[i].a = 1;
                }
                /*if (pixels2[i].b > 0.1f)
                {
                    pixels[i] = Color.green;
                    pixels[i].a = 1;
                }*/
                if (pixels2[i].g > pixels[i].g)
                {
                    pixels[i] = Color.green;
                    pixels[i].a = 1;
                }
                if (pixels3[i].b > 0.05f)
                {
                    pixels[i] = Color.blue;
                    pixels[i].a = 1;
                }
            }
        }

        //DrawGridAsTexture(pixels);

        return pixels;
    }

    public void CalculateBase(Color[] pixels)
    {
        int baseCnt = 0;
        int emptyCnt = 0;

        foreach (Color col in pixels)
        {
            if (col.g < 0.1f) emptyCnt++;
            else baseCnt++;
        }

        if (_startBaseSizePixels <= 0) _startBaseSizePixels = baseCnt;

        _areaPercent = (float)(baseCnt - _startBaseSizePixels) / (float)_startBaseSizePixels;

        _areaPercent = _areaPercent < 0 ? 0 : _areaPercent;
    }

    public bool IsConnected(Vector3 ResourcePos)
    {
        if (_ConnectivityGrid == null) return false;
        Cell cell = GetCellAtPos(ResourcePos);
        //Debug.Log("Resource Coord: [" + x + "/" + y + "]");

        return cell.isConnected;
    }

    public Cell GetCellAtPos(Vector3 pos)
    {
        if (_ConnectivityGrid == null) return null;
        Vector2 percentPos = GetPercentPos(new Vector2(pos.x, pos.z));

        int x = (int)Mathf.Round(_ConnectivityGrid.GetLength(0) * percentPos.x);
        int y = (int)Mathf.Round(_ConnectivityGrid.GetLength(1) * (percentPos.y));

        return _ConnectivityGrid[x, y];
    }

    public Vector2 GetPosOfCell(Cell cell)
    {
        Vector2 percentPos = new Vector2((float)(_ConnectivityGrid.GetLength(0) - cell.x) / _ConnectivityGrid.GetLength(0), (float)(_ConnectivityGrid.GetLength(1) - cell.y) / _ConnectivityGrid.GetLength(1));

        return GetPosFromPercent(percentPos);
    }

    public void ConnectionCompleted(GameObject ConnectionParticle)
    {
        RenderTexture.active = rtGoo;
        // Read pixels
        connReadRt.ReadPixels(rectReadPicture, 0, 0);
        connReadRt.Apply();
        RenderTexture.active = null;
        //Debug.Log("do the stamp");
        INode node = ConnectionParticle.GetComponent<ConnectParticle>().nodeToConnect;
        if (node.type == NodeType.resource) _resourceConnManager.CreateVine(node);
        StampConnectionToRT(ConnectionParticle).Forget();
    }

    private async UniTaskVoid StampConnectionToRT(GameObject ConnectionParticle)
    {
        Color[] pixels = connReadRt.GetPixels();
        StampTexData data = await UniTask.RunOnThreadPool(() => GetStampTex(pixels));

        //DrawGridAsTexture(data.pixels, data.width, data.height);

        Texture2D stampTex = new Texture2D(data.width, data.height, TextureFormat.ARGB32, false);

        stampTex.SetPixels(data.pixels);
        stampTex.Apply();

        //Vector2 pos = new Vector2(renderTexSize.x / 2, renderTexSize.y / 2);

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, renderTexSize.x, renderTexSize.y, 0);
        RenderTexture.active = rt;
        //Graphics.DrawTexture(new Rect(pos.x - connReadRt.width / 2, pos.y - connReadRt.height / 2, connReadRt.width, connReadRt.height), connReadRt, new Rect(0, 0, 1, 1), 0, 0, 0, 0, drawCol, null);
        //Graphics.DrawTexture(rectReadPicture, connReadRt, new Rect(0, 0, 1, 1), 0, 0, 0, 0, drawCol, null);
        Graphics.DrawTexture(new Rect(data.minX, connReadRt.height - data.maxY, data.width, data.height), stampTex, new Rect(0, 0, 1, 1), 0, 0, 0, 0, drawCol, null);

        UpdateTex();
        RenderTexture.active = null;
        GL.PopMatrix();
        if (ConnectionParticle != null) _connParticlesToDestroy.Add(ConnectionParticle);
        if (ConnectionParticle != null) ConnectionParticle.GetComponent<ParticleSystem>().Pause();
        _particleCalculated = false;
        //Destroy(ConnectionParticle);

        Destroy(stampTex);
    }

    private StampTexData GetStampTex(Color[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].g > 0.1f) pixels[i] = Color.white;
            else pixels[i] = Color.clear;
        }

        int width = 1024;
        int height = 1024;

        Color[,] pixelGrid = new Color[width, height];

        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % width;
            int y = i / width;
            pixelGrid[x, y] = pixels[i];
        }

        int minX = int.MaxValue, maxX = 0, minY = int.MaxValue, maxY = 0;

        for (int x = 0; x < pixelGrid.GetLength(0); x++)
        {
            for (int y = 0; y < pixelGrid.GetLength(1); y++)
            {
                if (pixelGrid[x, y] == Color.white)
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }
        maxX++;
        maxY++;
        Color[] newPixels = new Color[(maxX - minX) * (maxY - minY)];
        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                newPixels[((y - minY) * (maxX - minX)) + (x - minX)] = pixelGrid[x, y];
            }
        }



        StampTexData data = new StampTexData();
        data.pixels = newPixels;
        data.width = (maxX - minX);
        data.height = (maxY - minY);
        data.minX = minX;
        data.minY = minY;
        data.maxX = maxX;
        data.maxY = maxY;

        return data;
        //return pixels;
    }

    private async UniTaskVoid StartConnectionCalc()
    {
        while (_connectionsCalcNeeded)
        {
            //_connectionsCalcNeeded = false;
            //Debug.LogError("calculating " + gameObject.GetInstanceID());
            _connectionsCalculating = true;
            DSrt = await GetDownsampledTexture(rt, 4);
            //rtTex2D.hideFlags = HideFlags.HideAndDontSave;
            DSrtGoo = await GetDownsampledTexture(rtGoo, 4);
            //rtGooTex2D.hideFlags = HideFlags.HideAndDontSave;
            DSrtPlayer = await GetDownsampledTexture(rtPlayer, 5);
            //rtPlayer2D.hideFlags = HideFlags.HideAndDontSave;
            Color[] pixels = CombineTextures(DSrt, DSrtGoo, DSrtPlayer);

            //Destroy(rtTex2D);
            //Destroy(rtGooTex2D);
            //Destroy(rtPlayer2D);

            //DrawGridAsTexture(pixels);
            int width = DSrt.width;
            int height = DSrt.height;
            await UniTask.RunOnThreadPool(() => CalculateConnections(pixels, width, height));
            OnConnectionsCalculated?.Invoke();

            if (_connParticlesToDestroy.Count > 0 && _particleCalculated)
            {
                for (int i = _connParticlesToDestroy.Count - 1; i >= 0; i--)
                {
                    GameObject particle = _connParticlesToDestroy[i];
                    _connParticlesToDestroy.RemoveAt(i);
                    if (particle != null) Destroy(particle);
                }
            }
            //_connectionsCalculating = false;

            _particleCalculated = true;

            await UniTask.Yield();
        }
    }

    private async UniTaskVoid StartAreaCalc()
    {
        while (_areaCalcNeeded)
        {
            _areaCalcNeeded = false;
            _areaCalculating = true;
            Texture2D rtTex2D = await GetDownsampledTexture(rt, 4);
            rtTex2D.hideFlags = HideFlags.HideAndDontSave;
            Color[] pixels = rtTex2D.GetPixels();
            await UniTask.RunOnThreadPool(() => CalculateBase(pixels));
            OnAreaCalculated?.Invoke(_areaPercent); // TODO add calculated number
            _areaCalculating = false;
            Destroy(rtTex2D);
            await UniTask.Yield();
        }
    }

    private async UniTask<Texture2D> GetDownsampledTexture(RenderTexture _rt, int iterations)
    {
        RenderTextureFormat format = _rt.format;
        int height = _rt.height;
        int width = _rt.width;
        RenderTexture currentSource = RenderTexture.GetTemporary(_rt.width, _rt.height, 16);
        Graphics.Blit(_rt, currentSource);

        RenderTexture currentDestination;

        for (int i = 1; i < iterations; i++)
        {
            width /= 2;
            height /= 2;
            currentDestination = RenderTexture.GetTemporary(width, height, 0, format);
            Graphics.Blit(currentSource, currentDestination);
            RenderTexture.ReleaseTemporary(currentSource);
            currentSource = currentDestination;

            await UniTask.Yield();
        }

        RenderTexture.active = currentSource;
        // Read pixels
        Texture2D tempTex = new Texture2D(currentSource.width, currentSource.height);
        tempTex.hideFlags = HideFlags.HideAndDontSave;
        tempTex.ReadPixels(new Rect(0, 0, currentSource.width, currentSource.height), 0, 0);
        tempTex.Apply();
        RenderTexture.active = rt;

        return tempTex;
    }

    public void CalculateConnections(Color[] pixels, int width, int height)
    {
        if (_calcGrid == null)
        {
            _calcGrid = new Cell[width, height];

            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % width;
                int y = i / width;
                _calcGrid[x, y] = new Cell
                {
                    x = x,
                    y = y,
                    pathVal = int.MaxValue,
                    isBase = pixels[i].g > 0.2f && pixels[i].b < 0.2f,
                    isDiscovered = pixels[i].r > 0.5f && pixels[i].b < 0.2f,
                    isConnected = false,
                    isConnectedDiscover = false
                };
            }
        }
        else
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % width;
                int y = i / width;
                if (_calcGrid[x,y] == null)
                {
                    _calcGrid[x, y] = new Cell
                    {
                        x = x,
                        y = y,
                        pathVal = int.MaxValue,
                        isBase = pixels[i].g > 0.2f && pixels[i].b < 0.2f,
                        isDiscovered = pixels[i].r > 0.5f && pixels[i].b < 0.2f,
                        isConnected = false,
                        isConnectedDiscover = false
                    };
                }
                _calcGrid[x, y].pathVal = int.MaxValue;
                _calcGrid[x, y].isBase = pixels[i].g > 0.2f && pixels[i].b < 0.2f;
                _calcGrid[x, y].isDiscovered = pixels[i].r > 0.5f && pixels[i].b < 0.2f;
                _calcGrid[x, y].isConnected = false;
                _calcGrid[x, y].isConnectedDiscover = false;
            }
        }


        _ConnectivityGrid = _resourceConnManager.BetterDijkstra(_calcGrid, new Vector2Int(width / 2, height / 2));
    }

    private void MapChanged()
    {
        _connectionsCalcNeeded = true;
        _areaCalcNeeded = true;
        if (_connectionsCalcNeeded && !_connectionsCalculating)
        {
            //StartConnectionCalc().Forget();
        }
        if (_areaCalcNeeded && !_areaCalculating)
        {
            StartAreaCalc().Forget();
        }
    }

    #region events

    public delegate void OnMapChangeDelegate();
    public event OnMapChangeDelegate OnMapChange;

    public delegate void OnConnectionsCalculatedDelegate();
    public event OnConnectionsCalculatedDelegate OnConnectionsCalculated;

    public delegate void OnAreaCalculatedDelegate(float percentInc);
    public event OnAreaCalculatedDelegate OnAreaCalculated;

    #endregion

    #region Draw to Texture

#if UNITY_EDITOR
    private static string savePath = "/Textures";
    private static string saveName = "PathfindingGrid";
    public void DrawGridAsTexture(Cell[,] grid)
    {
        Texture2D connTex = new Texture2D(grid.GetLength(0), grid.GetLength(1));
        Color[] newPixels = new Color[grid.GetLength(0) * grid.GetLength(1)];
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                newPixels[i * grid.GetLength(1) + j] = grid[i, j].isConnected ? Color.green : Color.black;
            }
        }
        connTex.SetPixels(newPixels);


        int k = 0;
        while (k < 100)
        {
            try
            {
                AssetDatabase.LoadAssetAtPath("Assets" + savePath + saveName + k.ToString(), typeof(Texture2D));
                k++;
            }
            catch
            {
                break;
            }
        }

        byte[] bytes = connTex.EncodeToPNG();
        var dirPath = Application.dataPath + savePath;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes($"{dirPath}/{saveName}.png", bytes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(string.Format($"Texture saved to: <k><color=gray>Assets{savePath}/{saveName + k.ToString()}.png</color></k>"));
        Selection.activeObject = AssetDatabase.LoadAssetAtPath($"Assets{savePath}/{saveName}.png", typeof(Object));
        EditorUtility.FocusProjectWindow();
    }


    public void DrawGridAsTexture(Color[] newPixels)
    {
        Texture2D connTex = new Texture2D((int)Mathf.Sqrt(newPixels.Length), (int)Mathf.Sqrt(newPixels.Length));
        connTex.SetPixels(newPixels);


        int k = 0;
        while (k < 100)
        {
            try
            {
                AssetDatabase.LoadAssetAtPath("Assets" + savePath + saveName + k.ToString(), typeof(Texture2D));
                k++;
            }
            catch
            {
                break;
            }
        }

        byte[] bytes = connTex.EncodeToPNG();
        var dirPath = Application.dataPath + savePath;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes($"{dirPath}/{saveName}.png", bytes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(string.Format($"Texture saved to: <k><color=gray>Assets{savePath}/{saveName + k.ToString()}.png</color></k>"));
        Selection.activeObject = AssetDatabase.LoadAssetAtPath($"Assets{savePath}/{saveName}.png", typeof(Object));
        EditorUtility.FocusProjectWindow();
    }


    public void DrawGridAsTexture(Color[] newPixels, int width, int height)
    {
        Texture2D connTex = new Texture2D(width, height);
        connTex.SetPixels(newPixels);


        int k = 0;
        while (k < 100)
        {
            try
            {
                AssetDatabase.LoadAssetAtPath("Assets" + savePath + saveName + k.ToString(), typeof(Texture2D));
                k++;
            }
            catch
            {
                break;
            }
        }

        byte[] bytes = connTex.EncodeToPNG();
        var dirPath = Application.dataPath + savePath;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes($"{dirPath}/{saveName}.png", bytes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(string.Format($"Texture saved to: <k><color=gray>Assets{savePath}/{saveName + k.ToString()}.png</color></k>"));
        Selection.activeObject = AssetDatabase.LoadAssetAtPath($"Assets{savePath}/{saveName}.png", typeof(Object));
        EditorUtility.FocusProjectWindow();
    }
#endif

    #endregion
}
