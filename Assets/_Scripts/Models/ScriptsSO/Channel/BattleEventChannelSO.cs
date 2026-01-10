using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Duck Battle/Battle Event Channel", order = 1)]
public class BattleEventChannelSO : ScriptableObject
{
    [Header("Combat Events")]
    public UnityAction<Owner, ShotResult, Vector2Int> OnShotFired;
    public UnityAction<Owner, int, int> OnEnergyChanged;

    [Header("Skill Events")]
    public UnityAction<DuckSkillSO> OnSkillRequested; 
    public UnityAction<string, Vector2Int> OnSkillFeedback;

    // --- ADDED: Sự kiện chọn Skill (để Preview) ---
    public UnityAction<DuckSkillSO> OnSkillSelected;
    public UnityAction OnSkillDeselected;

    [Header("Visual Events")]
    public UnityAction<Owner, List<Vector2Int>, Color> OnGridHighlightRequested;
    public UnityAction OnGridHighlightClearRequested;
    public UnityAction<Sprite, Vector2Int, Vector3, bool> OnSkillGhostUpdate;
    public UnityAction OnSkillGhostClear;

    // --- RAISERS ---

    // Khi bắn xong một phát súng
    public void RaiseShotFired(Owner shooter, ShotResult result, Vector2Int pos) => OnShotFired?.Invoke(shooter, result, pos);
    public void RaiseEnergyChanged(Owner owner, int current, int max) => OnEnergyChanged?.Invoke(owner, current, max);
    public void RaiseSkillFeedback(string message, Vector2Int position) => OnSkillFeedback?.Invoke(message, position);
    public void RaiseSkillRequested(DuckSkillSO skill) => OnSkillRequested?.Invoke(skill);

    // --- ADDED RAISERS ---
    public void RaiseSkillSelected(DuckSkillSO skill) => OnSkillSelected?.Invoke(skill);
    public void RaiseSkillDeselected() => OnSkillDeselected?.Invoke();

    public void RaiseGridHighlight(Owner target, List<Vector2Int> cells, Color color) => OnGridHighlightRequested?.Invoke(target, cells, color);

    public void RaiseClearHighlight() => OnGridHighlightClearRequested?.Invoke();
    public void RaiseSkillGhostUpdate(Sprite sprite, Vector2Int size, Vector3 worldPos, bool isValid)
            => OnSkillGhostUpdate?.Invoke(sprite, size, worldPos, isValid);

    public void RaiseSkillGhostClear()
        => OnSkillGhostClear?.Invoke();
}