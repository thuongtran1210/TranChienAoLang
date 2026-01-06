using System;
using UnityEngine;

public class DuckUnit : IGridOccupant
{
    public DuckDataSO Data { get; private set; }
    public bool IsHorizontal { get; private set; }
    public int Size => Data.size;
    public bool IsSunk => _currentHits >= Data.size;

    private int _currentHits;

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnSunk;

    // Constructor Injection
    public DuckUnit(DuckDataSO data, bool isHorizontal)
    {
        Data = data;
        IsHorizontal = isHorizontal;
        _currentHits = 0;
    }
    public void TakeDamage()
    {
        if (IsSunk) return;
        _currentHits++;

        OnHealthChanged?.Invoke(_currentHits, Data.size);

        if (IsSunk)
        {
            OnSunk?.Invoke();
        }
    }
}
