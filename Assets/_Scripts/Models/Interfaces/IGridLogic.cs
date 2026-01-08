
using System;
using UnityEngine;

// 1. Interface chuyên về Logic & Dữ liệu (Dùng cho AI, Controller kiểm tra luật chơi)
public interface IGridLogic
{
    // Data
    IGridSystem GridSystem { get; }
    Owner GridOwner { get; }
    void Initialize(IGridSystem gridSystem, Owner owner);

    // Helper tính toán tọa độ
    Vector2Int GetGridPosition(Vector3 worldPos);
    Vector3 GetWorldPosition(Vector2Int gridPos);
    bool IsWorldPositionInside(Vector3 worldPos, out Vector2Int gridPos);

    // Core Logic
    bool IsPlacementValid(Vector3 worldPos, DuckDataSO data, bool isHorizontal);
    bool TryPlaceShip(Vector3 worldPos, DuckDataSO data, bool isHorizontal);

    // Event (nếu cần cho logic game)
    event Action<IGridLogic, Vector2Int> OnGridClicked;
}