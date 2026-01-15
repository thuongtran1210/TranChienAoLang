using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Duck Battle/Battle Event Channel", order = 1)]
public class BattleEventChannelSO : ScriptableObject
{
    [Header("--- DEBUG CONTROL ---")]
    [SerializeField] private bool _enableDebugLogs = false; 
    [SerializeField][TextArea] private string _channelDescription = "Quản lý sự kiện trong Battle Phase";

    [Header("Combat Events")]
    public UnityAction<Owner, Owner, ShotResult, Vector2Int> OnShotFired;
    public UnityAction<Owner, int, int> OnEnergyChanged;

    [Header("Skill Events")]
    public UnityAction<DuckSkillSO> OnSkillRequested; 
    public UnityAction<string, Vector2Int> OnSkillFeedback;
    public UnityAction<DuckSkillSO> OnSkillSelected;
    public UnityAction OnSkillDeselected;

    [Header("Visual Events")]
    public UnityAction<Owner, List<Vector2Int>, Color> OnGridHighlightRequested;
    public UnityAction OnGridHighlightClearRequested;
    public UnityAction<Owner, List<Vector2Int>, TileBase, float> OnTileIndicatorRequested;
    public UnityAction<Sprite, Vector2Int, Vector3, bool> OnSkillGhostUpdate;
    public UnityAction OnSkillGhostClear;

    public UnityAction<Owner, List<Vector2Int>, Color, float> OnSkillImpactVisualRequested;

    // --- RAISERS ---

    // Khi bắn xong một phát súng
    public void RaiseShotFired(Owner shooter, Owner target, ShotResult result, Vector2Int pos)
    {
        LogEvent($"Shot Fired: Shooter={shooter}, Result={result}, Pos={pos}");
        OnShotFired?.Invoke(shooter, target, result, pos);
    }
    public void RaiseEnergyChanged(Owner owner, int current, int max)
    {
        // Thường energy thay đổi liên tục, có thể comment log này nếu quá spam
        // LogEvent($"Energy Changed: {owner} -> {current}/{max}");
        OnEnergyChanged?.Invoke(owner, current, max);
    }
    public void RaiseSkillFeedback(string message, Vector2Int position) => OnSkillFeedback?.Invoke(message, position);
    public void RaiseSkillRequested(DuckSkillSO skill)
    {
        LogEvent($"Skill REQUESTED: {(skill != null ? skill.skillName : "NULL")}");
        OnSkillRequested?.Invoke(skill);
    }

    // --- ADDED RAISERS ---
    public void RaiseSkillSelected(DuckSkillSO skill)
    {
        LogEvent($"Skill SELECTED (Preview): {skill.skillName}");
        OnSkillSelected?.Invoke(skill);
    }
    public void RaiseSkillDeselected()
    {
        LogEvent("Skill DESELECTED");
        OnSkillDeselected?.Invoke();
    }

    public void RaiseGridHighlight(Owner target, List<Vector2Int> cells, Color color)
    {
         LogEvent($"Highlight Requested: {target}, Count={cells.Count}");
        OnGridHighlightRequested?.Invoke(target, cells, color);
    }

    public void RaiseClearHighlight()
    {
        LogEvent("Clear Highlight Requested");
        OnGridHighlightClearRequested?.Invoke();
    }
    public void RaiseSkillGhostUpdate(Sprite sprite, Vector2Int size, Vector3 worldPos, bool isValid)
    {
        OnSkillGhostUpdate?.Invoke(sprite, size, worldPos, isValid);
    }

    public void RaiseSkillGhostClear() => OnSkillGhostClear?.Invoke();

    public void RaiseSkillImpactVisual(Owner target, List<Vector2Int> cells, Color color, float duration = 0.5f)
    {
        OnSkillImpactVisualRequested?.Invoke(target, cells, color, duration);
    }
    public void RaiseTileIndicator(Owner target, List<Vector2Int> cells, TileBase tile, float duration = 1.0f)
    {

        OnTileIndicatorRequested?.Invoke(target, cells, tile, duration);
    }

    // --- HELPER LOGIC ---
    private void LogEvent(string message)
    {
        if (_enableDebugLogs)
        {
            Debug.Log($"<color=cyan>[CHANNEL: {name}]</color> {message}");
        }
    }
}