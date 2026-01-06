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

    // Implement Interface Event
    public event Action<Vector2Int, ShotResult> OnGridStateChanged;

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


    private bool CheckPlacementLogic(int size, Vector2Int position, bool isHorizontal)
    {
        for (int i = 0; i < size; i++)
        {
            int x = position.x + (isHorizontal ? i : 0);
            int y = position.y + (isHorizontal ? 0 : i);

            if (!IsValidPosition(new Vector2Int(x, y))) return false;
            if (Cells[x, y].IsOccupied) return false;
        }
        return true;
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
        DuckDataSO data = unit.Data; // Giả sử IGridOccupant có access tới Data

        foreach (Vector2Int pos in GetTargetPositions(data, position, isHorizontal))
        {
            if (IsValidPosition(pos))
            {
                Cells[pos.x, pos.y].OccupiedUnit = unit;
                // Lưu ý: _aliveUnitsCount chỉ nên ++ 1 lần cho mỗi Unit, không phải mỗi Cell
            }
        }
        AliveUnitsCount++;
    }

    // --- BATTLE PHASE METHODS ---

    public ShotResult ShootAt(Vector2Int position)
    {
        // 1. Kiểm tra biên
        if (!IsValidPosition(position)) return ShotResult.Invalid;

        GridCell cell = Cells[position.x, position.y];

        // 2. Kiểm tra đã bắn chưa
        if (cell.IsHit) return ShotResult.Invalid; 

        // 3. Đánh dấu bắn
        cell.IsHit = true;
        ShotResult result = ShotResult.Miss;

        // 4. Xử lý trúng/trượt
        if (cell.IsOccupied && cell.OccupiedUnit != null)
        {
            cell.OccupiedUnit.TakeDamage();

            if (cell.OccupiedUnit.IsSunk)
            {
                AliveUnitsCount--;
                return ShotResult.Sunk;
            }
            return ShotResult.Hit;
        }

        // 5. Bắn Event ra ngoài
        OnGridStateChanged?.Invoke(position, result);

        return result;
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
        // Gọi hàm helper chúng ta vừa viết trong SO
        return data.GetOccupiedCells(pivot, isHorizontal);
    }

}