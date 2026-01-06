using System;
using UnityEngine;

public interface IGridSystem
{
    // --- Data Access ---
    int Width { get; }
    int Height { get; }
    GridCell[,] Cells { get; }
   

    // --- Events ---
    event Action<Vector2Int, ShotResult> OnGridStateChanged;

    GridCell GetCell(Vector2Int position);
    GridCell GetCell(int x, int y);

    bool CanPlaceUnit(DuckDataSO data, Vector2Int position, bool isHorizontal);

    void PlaceUnit(IGridOccupant unit, Vector2Int position, bool isHorizontal);
    ShotResult ShootAt(Vector2Int position);

    // Helper
    bool IsValidPosition(Vector2Int pos);
}