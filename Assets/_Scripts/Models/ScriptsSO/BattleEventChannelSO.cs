using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Duck Battle/Battle Event Channel",order =1)]
public class BattleEventChannelSO : ScriptableObject
{
    // Sự kiện bắn: (Người bắn, Kết quả, Tọa độ)
    public UnityAction<Owner, ShotResult, Vector2Int> OnShotFired;

    // Sự kiện thay đổi Energy: (Chủ sở hữu, Energy hiện tại, Max Energy)
    public UnityAction<Owner, int, int> OnEnergyChanged;
    public UnityAction<string, Vector2Int> OnSkillFeedback;
    public UnityAction<DuckSkillSO> OnSkillRequested;
    public UnityAction<List<Vector2Int>, Color> OnGridHighlightRequested;
    public UnityAction OnGridHighlightCleared;
    public void RaiseShotFired(Owner shooter, ShotResult result, Vector2Int pos)
    {
        OnShotFired?.Invoke(shooter, result, pos);
    }

    public void RaiseEnergyChanged(Owner owner, int currentEnergy, int maxEnergy)
    {
        OnEnergyChanged?.Invoke(owner, currentEnergy, maxEnergy);
    }


    public void RaiseSkillFeedback(string message, Vector2Int position)
    {
        OnSkillFeedback?.Invoke(message, position);
    }


    public void RaiseSkillRequested(DuckSkillSO skill)
    {
        OnSkillRequested?.Invoke(skill);
    }
    public void RaiseGridHighlight(List<Vector2Int> cells, Color color)
    {
        OnGridHighlightRequested?.Invoke(cells, color);
    }

    public void RaiseClearHighlight()
    {
        OnGridHighlightCleared?.Invoke();
    }
}