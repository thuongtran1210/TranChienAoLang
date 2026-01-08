using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int GridPosition { get; private set; }
    public IGridOccupant OccupiedUnit { get; set; }
    public bool IsHit { get; set; }
    public GridCell(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        OccupiedUnit = null; 
        IsHit = false;
    }

    public bool IsOccupied => OccupiedUnit != null;
}
