using UnityEngine;

public interface IGridInteractable
{
    Vector2Int GridPosition { get; }
    Owner CellOwner { get; }
}
