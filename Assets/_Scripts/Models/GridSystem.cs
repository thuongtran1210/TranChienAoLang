using System;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : IGridSystem
{
    // Implement Interface Properties
    public int Width { get; private set; }
    public int Height { get; private set; }
    public GridCell[,] Cells { get; private set; }
    public int AliveUnitsCount { get; private set; }

    public event Action OnGridReset;

    // Implement Interface Event
    public event Action<Vector2Int, ShotResult> OnGridStateChanged;
    public bool IsAllShipsSunk => AliveUnitsCount <= 0;

    public GridSystem(int width, int height)
    {
        Width = width;
        Height = height;
        Cells = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cells[x, y] = new GridCell(new Vector2Int(x, y));
            }
        }
    }

    // --- SETUP PHASE METHODS ---


    public bool CanPlaceUnit(DuckDataSO data, Vector2Int position, bool isHorizontal)
    {
        return CheckPlacementLogic(data, position, isHorizontal);
    }

    private bool CheckPlacementLogic(DuckDataSO data, Vector2Int position, bool isHorizontal)
    {
        foreach (Vector2Int pos in GetTargetPositions(data, position, isHorizontal))
        {
            if (!IsValidPosition(pos))
            {
                // Debug.Log($"Placement Failed: Pos {pos} is Out of Bounds");
                return false;
            }

            if (Cells[pos.x, pos.y].IsOccupied)
            {
                // Debug.Log($"Placement Failed: Pos {pos} is Occupied by {Cells[pos.x, pos.y].OccupiedUnit}");
                return false;
            }
        }
        return true;
    }

    public void PlaceUnit(IGridOccupant unit, Vector2Int position, bool isHorizontal)
    {
        DuckDataSO data = unit.Data;

        foreach (Vector2Int pos in GetTargetPositions(data, position, isHorizontal))
        {
            if (IsValidPosition(pos))
            {
                Cells[pos.x, pos.y].OccupiedUnit = unit;
           
            }
        }
        AliveUnitsCount++;
    }
    public void Clear()
    {
        foreach (var cell in Cells)
        {
            cell.OccupiedUnit = null;
            cell.IsHit = false;
        }
        AliveUnitsCount = 0;

       
        OnGridReset?.Invoke();
    }

    // --- BATTLE PHASE METHODS ---

    public ShotResult ShootAt(Vector2Int position)
    {
        // 1. Validation (Fail Fast)
        if (!IsValidPosition(position)) return ShotResult.Invalid;

        GridCell cell = Cells[position.x, position.y];

        // 2. Check State
        if (cell.IsHit) return ShotResult.Invalid;

        // 3. Update State
        cell.IsHit = true;

        // Mặc định là Miss
        ShotResult finalResult = ShotResult.Miss;

        // 4. Calculate Logic
        if (cell.IsOccupied && cell.OccupiedUnit != null)
        {
            cell.OccupiedUnit.TakeDamage();

            if (cell.OccupiedUnit.IsSunk)
            {
                AliveUnitsCount--;
                finalResult = ShotResult.Sunk;
            }
            else
            {
                finalResult = ShotResult.Hit;
            }
        }
        // Return Result
        return finalResult;
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < Width && pos.y < Height;
    }

    public GridCell GetCell(Vector2Int position)
    {
        if (!IsValidPosition(position))
        {
            return null;
        }
        return Cells[position.x, position.y];
    }
    public GridCell GetCell(int x, int y)
    {
        return GetCell(new Vector2Int(x, y));
    }
    // Helper private để lấy các ô sẽ bị chiếm
    private IEnumerable<Vector2Int> GetTargetPositions(DuckDataSO data, Vector2Int pivot, bool isHorizontal)
    {
        
        return data.GetOccupiedCells(pivot, isHorizontal);
    }

}