using System;
using UnityEngine;

public interface IGridSystem
{
    // --- Data Access ---
    int Width { get; }
    int Height { get; }
    GridCell[,] Cells { get; }


    // "Đã chìm hết chưa?"
    bool IsAllShipsSunk { get; }
    // --- Events ---
    // Event khi một ô thay đổi (bị bắn)
    event Action<Vector2Int, ShotResult> OnGridStateChanged;
    event Action OnGridReset;

    // --- Methods ---
    GridCell GetCell(Vector2Int position);
    GridCell GetCell(int x, int y);
    bool CanPlaceUnit(DuckDataSO data, Vector2Int position, bool isHorizontal);
    void PlaceUnit(IGridOccupant unit, Vector2Int position, bool isHorizontal);
    ShotResult ShootAt(Vector2Int position);
    bool IsValidPosition(Vector2Int pos);
    void Clear();
}