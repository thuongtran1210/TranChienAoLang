using System;
using UnityEngine;

[System.Serializable]
public class DuckUnit : IGridOccupant
{
    // --- PROPERTIES (Dữ liệu) ---

    public Vector2Int PivotGridPos { get; private set; }

    public DuckDataSO Data { get; private set; }
    public bool IsHorizontal { get; private set; }

    // Tính toán nhanh
    public int Size => Data != null ? Data.size : 1;
    public bool IsSunk => _currentHits >= Size;

    // --- STATE  ---
    [SerializeField] private int _currentHits;

    // --- EVENTS  ---
    public event Action<int, int> OnHealthChanged; // (currentHits, maxHealth)
    public event Action OnSunk;

    // --- CONSTRUCTOR ---
    public DuckUnit(DuckDataSO data, Vector2Int gridPos, bool isHorizontal)
    {
        if (data == null)
        {
            Debug.LogError("[DuckUnit] Data cannot be null!");
            return;
        }

        Data = data;
        PivotGridPos = gridPos; // <-- Lưu vị trí
        IsHorizontal = isHorizontal;
        _currentHits = 0;
    }

    // --- LOGIC ---
    public void TakeDamage()
    {
        if (IsSunk) return;

        _currentHits++;

        // Bắn sự kiện thay đổi máu để View cập nhật (nếu cần)
        OnHealthChanged?.Invoke(_currentHits, Size);

        // Kiểm tra chết
        if (IsSunk)
        {
            OnSunk?.Invoke();
            Debug.Log($"[DuckUnit] {Data.name} has been sunk at {PivotGridPos}!");
        }
    }


    public bool IsOccupying(Vector2Int gridPoint)
    {
        for (int i = 0; i < Size; i++)
        {
            Vector2Int checkPos;
            if (IsHorizontal)
                checkPos = PivotGridPos + new Vector2Int(i, 0);
            else
               
                checkPos = PivotGridPos + new Vector2Int(0, -i);

            if (checkPos == gridPoint) return true;
        }
        return false;
    }
}