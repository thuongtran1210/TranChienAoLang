using System;
using UnityEngine;

[Serializable]
public struct PlacementCheckResult
{
    public bool IsValid;
    public PlacementFailReason Reason;
    public Vector2Int FailedCell;
}

