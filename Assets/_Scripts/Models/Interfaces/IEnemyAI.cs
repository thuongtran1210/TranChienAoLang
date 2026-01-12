
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAI
{
    void Initialize(int width, int height);
    Vector2Int GetNextTarget(IGridSystem targetGridSystem);
    AIAction GetDecision(IGridSystem playerGrid, DuckEnergySystem myEnergy, List<DuckSkillSO> availableSkills);
    void NotifyHit(Vector2Int hitPos, IGridSystem grid);
}