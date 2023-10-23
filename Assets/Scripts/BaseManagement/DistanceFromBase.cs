using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static NPCAIStateManager;

public class DistanceFromBase : MonoBehaviour
{
    private BaseTexManager _baseTexManager;
    [SerializeField] private int _searchRadius;

    [SerializeField] private Vector3 _nearestPoint;

    private void Start()
    {
        _baseTexManager = GlobalGameManager.Instance.baseTexManager;
        _baseTexManager.OnConnectionsCalculated += OnConnectionsCalculated;
    }

    private void OnDisable()
    {
        _baseTexManager.OnConnectionsCalculated -= OnConnectionsCalculated;
    }

    private void OnConnectionsCalculated()
    {
        // feed this value into material as nearest point
        _nearestPoint = GetNearestBasePoint();
    }

    public Vector3 GetNearestBasePoint()
    {
        Cell[,] grid = _baseTexManager.ConnectivityGrid;

        Vector2 percentPos = _baseTexManager.GetPercentPos(new Vector2(transform.position.x, -transform.position.z));

        Vector2Int gridPos = Vector2Int.zero;
        gridPos.x = (int)Mathf.Round(grid.GetLength(0) * percentPos.x);
        gridPos.y = (int)Mathf.Round(grid.GetLength(1) * (1 - percentPos.y));

        Vector2Int foodPos = new Vector2Int(-1, -1);

        for (int i = 0; i < _searchRadius; i++)
        {
            foodPos = CheckAdjacentCellsForFood(grid, gridPos, i);
            if (foodPos.x >= 0) break;
        }

        if (foodPos.x < 0)
        {
            Debug.LogError("No nearest Point found!");
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue); // return max Distance point
        }

        Vector2 worldPos = _baseTexManager.GetPosFromPercent(new Vector2((float)foodPos.x / grid.GetLength(0), (float)foodPos.y / grid.GetLength(1)));

        return new Vector3(worldPos.x, transform.position.y, worldPos.y);
    }

    private Vector2Int CheckAdjacentCellsForFood(Cell[,] grid, Vector2Int startingPoint, int distance)
    {
        List<Vector2Int> foodLocations = new List<Vector2Int>();
        if (distance == 0)
        {
            if (grid[startingPoint.x, startingPoint.y].isConnected) return startingPoint;
        }
        for (int i = startingPoint.x - distance; i < startingPoint.x + distance; i++)
        {
            int j = Mathf.Clamp(i, 0, grid.GetLength(0) - 1);
            if (grid[j, Mathf.Clamp(startingPoint.y + distance, 0, grid.GetLength(1) - 1)].isConnected) foodLocations.Add(new Vector2Int(i, startingPoint.y + distance));
            if (grid[j, Mathf.Clamp(startingPoint.y - distance, 0, grid.GetLength(1) - 1)].isConnected) foodLocations.Add(new Vector2Int(i, startingPoint.y - distance));
        }
        for (int i = startingPoint.y - distance + 1; i < startingPoint.y + distance - 1; i++)
        {
            int j = Mathf.Clamp(i, 0, grid.GetLength(1) - 1);
            if (grid[Mathf.Clamp(startingPoint.x + distance, 0, grid.GetLength(0) - 1), j].isConnected) foodLocations.Add(new Vector2Int(startingPoint.x + distance, i));
            if (grid[Mathf.Clamp(startingPoint.x - distance, 0, grid.GetLength(0) - 1), j].isConnected) foodLocations.Add(new Vector2Int(startingPoint.x - distance, i));
        }

        if (foodLocations.Count > 0)
        {
            return foodLocations[Random.Range(0, foodLocations.Count)];
        }
        else
        {
            return new Vector2Int(-1, -1);
        }
    }
}
