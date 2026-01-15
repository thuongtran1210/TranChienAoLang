
using System;
using UnityEngine;

public interface IGridLogic
{
    // Data
    IGridSystem GridSystem { get; }
    Owner GridOwner { get; }
    void InitializeGrid(IGridSystem gridSystem, Owner owner);

    // Helper tính toán tọa độ (Spatial Logic)
    Vector2Int GetGridPosition(Vector3 worldPos);
    Vector3 GetWorldPosition(Vector2Int gridPos);
    bool IsWorldPositionInside(Vector3 worldPos, out Vector2Int gridPos);

    // Core Gameplay Logic
    bool IsPlacementValid(Vector3 worldPos, DuckDataSO data, bool isHorizontal);
    bool TryPlaceDuck(Vector3 worldPos, DuckDataSO data, bool isHorizontal);
    ShotResult ProcessShot(Vector2Int gridPos, Owner shooter);
    // Event
    event Action<IGridLogic, Vector2Int> OnGridClicked;
}