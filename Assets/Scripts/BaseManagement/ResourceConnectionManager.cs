using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cell
{
    public bool isBase = false;
    public bool isDiscovered = false;
    public int pathVal;
    public int x;
    public int y;
    public bool isConnected = false;
    public bool isConnectedDiscover = false;
    public bool isCalculated = false;
}

public class ResourceConnectionManager : MonoBehaviour
{
    private BaseTexManager _baseTexManager;
    private BaseManager _baseManager;
    private int recursions;
    private Queue<Cell> _cellsToAssign = new Queue<Cell>();
    [SerializeField] private bool _logRecursions;
    [SerializeField] private GameObject _connectParticle;
    [SerializeField] private GameObject _vineParticle;
    [SerializeField] private Transform _growingPlantsParent;
    private void Start()
    {
        _baseTexManager = GetComponent<BaseTexManager>();   
        _baseManager = GetComponent<BaseManager>();
    }

    private void OnDisable()
    {
        OnResourceDiscovered = null;
    }

    public Cell[,] BetterDijkstra(Cell[,] grid, Vector2Int src)
    {
        if (_logRecursions) recursions = 0;
        grid[src.x, src.y].isConnected = true;
        grid[src.x, src.y].pathVal = 0;
        AssignNeighbours(grid, GetNeighbours(grid, grid[src.x, src.y]), grid[src.x, src.y]);
        /*for (int j = 0; j < grid.GetLength(1); j++)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                var msg = "[" + i.ToString() + ", " + j.ToString() + "] = " + grid[i, j].pathVal;
                if(grid[i, j].isConnected) Debug.Log(msg);
            }
        }*/

        //Debug.Log("cells added: " + grid.GetLength(0) * grid.GetLength(1));

        while(_cellsToAssign.Count > 0)
        {
            int cellsAmount = _cellsToAssign.Count;
            for(int i = 0; i < cellsAmount; i++)
            {
                Cell cell = _cellsToAssign.Dequeue();
                AssignNeighbours(grid, GetNeighbours(grid, grid[cell.x, cell.y]), grid[cell.x, cell.y]);
            }
        }


        if(_logRecursions) Debug.Log(recursions);
        return InvertYAxis(InvertXAxis(grid));
    }
    
    private List<Cell> GetNeighbours(Cell[,] grid, Cell cell)
    {
        List<Cell> result = new List<Cell>();
        int rowMinimum = cell.x - 1 < 0 ? cell.x : cell.x - 1;
        int rowMaximum = cell.x + 1 > grid.GetLength(0) ? cell.x : cell.x + 1;
        int columnMinimum = cell.y - 1 < 0 ? cell.y : cell.y - 1;
        int columnMaximum = cell.y + 1 > grid.GetLength(1) ? cell.y : cell.y + 1;
        for (int i = rowMinimum; i <= rowMaximum; i++)
            for (int j = columnMinimum; j <= columnMaximum; j++)
                if (i != cell.x || j != cell.y)
                    result.Add(grid[i, j]);

        /*Debug.Log(cell.isBase);
        Debug.Log("src: " + cell.x + " : " + cell.y);
        foreach(Cell c in result)
        {
            Debug.Log("nbr: " + c.x + " : " + c.y);
        }*/
        return result;
    }

    private void AssignNeighbours(Cell[,] grid, List<Cell> neighbours, Cell origin)
    {
        if (_logRecursions) recursions++;
        foreach (Cell cell in neighbours)
        {
            bool willBeAssigned = false;
            if (cell.isBase || cell.isDiscovered)
            {
                if (cell.isBase && cell.isConnectedDiscover && origin.isConnected)
                {
                    cell.isConnectedDiscover = false;
                    willBeAssigned = true;
                }

                if ((cell.isBase && !origin.isConnectedDiscover && origin.isConnected) || cell.isConnected)
                {
                    cell.isConnected = true;
                }
                else
                {
                    cell.isConnectedDiscover = true;
                }

                if (origin.pathVal + 1 < cell.pathVal && (origin.isConnected || origin.isConnectedDiscover))
                {
                    cell.pathVal = origin.pathVal + 1;
                    willBeAssigned = true;
                }
            }
            if (willBeAssigned)
            {
                if (!_cellsToAssign.Contains(cell))
                {
                    _cellsToAssign.Enqueue(cell);
                }
            }
        }
    }

    public void ConnectToNode(Vector3 position, INode node)
    {
        if (node.type == NodeType.resource) OnResourceDiscovered?.Invoke();

        Cell cell = _baseTexManager.GetCellAtPos(position);
        List<Cell> path = GetShortestPathToBase(cell);

        List<Vector2> paintPositions = new List<Vector2>();
        foreach (Cell nextCell in path)
        {
            Vector2 pos = _baseTexManager.GetPosOfCell(nextCell);
            
            paintPositions.Add(pos);
            /*Debug.Log("PathToBase:");
            Debug.Log("[" + nextCell.x + "/" + nextCell.y + "]");
            Debug.Log("paintpos: " + pos);*/
        }
        paintPositions[0] = new Vector2(position.x, position.z); // override the last cell pos with the node pos
        paintPositions.RemoveAt(1);

        paintPositions = ConvertToCurve(paintPositions);

        float height = 50;//paintPositions[paintPositions.Count - 1].y;
        GameObject particle = Instantiate(_connectParticle, new Vector3(0,height + 1,0), Quaternion.identity);
        ConnectParticle connectParticle = particle.GetComponent<ConnectParticle>();
        connectParticle.baseTexManager = _baseTexManager;
        connectParticle.nodeToConnect = node;
        node.paintPositions = new List<Vector2>(paintPositions);
        connectParticle.growingPlantsParent = _growingPlantsParent;
        connectParticle.FollowPath(paintPositions, _baseManager.connectParticleSizeMod);
        
        //Debug.Log("number of bezier points: "+ paintPositions.Count);
        //_baseTexManager.DrawOnPos(paintPositions, BaseTexManager.DrawSize.medium);
    }

    public void CreateVine(INode node)
    {
        GameObject vine = Instantiate(_vineParticle, new Vector3(0, 50, 0), Quaternion.identity);
        VineParticle vineParticle = vine.GetComponent<VineParticle>();
        vineParticle.nodeToConnect = node;
        vineParticle.FollowPath(node.paintPositions);
    }

    private List<Vector2> ConvertToCurve(List<Vector2> path)
    {
        List<Vector2> curve = new List<Vector2>();

        // steps = amount of points on the resulting curve
        for (int s = 0, steps = path.Count; s < steps; s++)
        {
            float t = (float)s / steps;
            List<Vector2> tempPath = path;
            curve.Add(DeCasteljau(t, path));
        }

        return curve;
    }

    public static Vector2 DeCasteljau(float u, List<Vector2> controlPoints)
    {
        int degree = controlPoints.Count - 1; 

        List<Vector2> bezierPoints = new List<Vector2>();
        for (int level = degree; level >= 0; level--)
        {
            if (level == degree)
            {
                for (int i = 0; i <= degree; i++)
                {
                    bezierPoints.Add(controlPoints[i]);
                }
                continue;
            }

            int lastIdx = bezierPoints.Count;
            int levelIdx = level + 2;
            int idx = lastIdx - levelIdx;
            for (int i = 0; i <= level; i++)
            {
                Vector2 pi = (1 - u) * bezierPoints[idx] + u * bezierPoints[idx + 1];
                bezierPoints.Add(pi);
                ++idx;
            }
        }

        int lastElmnt = bezierPoints.Count - 1;
        return bezierPoints[lastElmnt];
    }

    private List<Cell> GetShortestPathToBase(Cell cell)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = cell;
        path.Add(currentCell);
        while (currentCell.pathVal != 0)
        {
            //Debug.Log("[" + currentCell.x + "/" + currentCell.y + "] value: " + currentCell.pathVal);
            currentCell = GetCellClosestToBase(GetNeighbours(InvertXAxis(InvertYAxis(_baseTexManager.ConnectivityGrid)), currentCell), currentCell.pathVal, cell);
            path.Add(currentCell);
        }
        return path;
    }

    private Cell GetCellClosestToBase(List<Cell> possiblePaths, int currentDistance, Cell origin)
    {
        List<Cell> viablePaths = new List<Cell>();
        foreach (Cell cell in possiblePaths)
        {
            //Debug.Log("[" + cell.x + "/" + cell.y + "] value: " + cell.pathVal);
            if (cell.pathVal < currentDistance)
            {
                viablePaths.Add(cell);
            }
        }

        if (viablePaths.Count == 0)
        {
            Debug.LogError("no valid Path found");
            return null;
        }
        else
        {
            return viablePaths[UnityEngine.Random.Range(0, viablePaths.Count)];
        }
    }

    private T[,] InvertYAxis<T>(T[,] matrix)
    {
        T[,] newMatrix = new T[matrix.GetLength(0), matrix.GetLength(1)];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                newMatrix[i, matrix.GetLength(1) - j - 1] = matrix[i, j];
            }
        }

        return newMatrix;
    }

    private T[,] InvertXAxis<T>(T[,] matrix)
    {
        T[,] newMatrix = new T[matrix.GetLength(0), matrix.GetLength(1)];

        for (int i = 0; i < matrix.GetLength(1); i++)
        {
            for (int j = 0; j < matrix.GetLength(0); j++)
            {
                newMatrix[matrix.GetLength(0) - j - 1, i] = matrix[j, i];
            }
        }

        return newMatrix;
    }


    public delegate void OnResourceDiscoveredDelegate();
    public event OnResourceDiscoveredDelegate OnResourceDiscovered;
}
