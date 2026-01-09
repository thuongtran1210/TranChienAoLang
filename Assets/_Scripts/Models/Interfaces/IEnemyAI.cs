
using UnityEngine;

public interface IEnemyAI
{
    Vector2Int GetNextTarget(IGridSystem targetGridSystem);
    void NotifyHit(Vector2Int hitPos, IGridSystem grid);
}