using UnityEngine;
// Runtime representation of any object that can occupy grid cells (e.g., ships, ducks, etc.)
public interface IGridOccupant
{
    // Kích thước của vật thể 
    int Size { get; }
    DuckDataSO Data { get; }

    // Trạng thái đã chìm/chết 
    bool IsSunk { get; }

    // Hàm nhận sát thương
    void TakeDamage();
}