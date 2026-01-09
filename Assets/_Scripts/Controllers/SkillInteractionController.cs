using System.Collections.Generic;
using UnityEngine;

public class SkillInteractionController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridInputController _inputController;
    [SerializeField] private BattleEventChannelSO _battleEvents;

    private DuckSkillSO _currentSelectedSkill;

    private void OnEnable()
    {
        if (_inputController != null)
        {
            _inputController.OnGridCellHovered += HandleGridHover;
        }

        if (_battleEvents != null)
        {
            // Đăng ký sự kiện chọn Skill từ UI
            _battleEvents.OnSkillSelected += SelectSkill;
            _battleEvents.OnSkillDeselected += DeselectSkill;
        }
    }

    private void OnDisable()
    {
        if (_inputController != null)
        {
            _inputController.OnGridCellHovered -= HandleGridHover;
        }

        if (_battleEvents != null)
        {
            _battleEvents.OnSkillSelected -= SelectSkill;
            _battleEvents.OnSkillDeselected -= DeselectSkill;
        }
    }

    // --- MAIN LOGIC ---

    private void HandleGridHover(Vector2Int gridPos, IGridLogic gridLogic)
    {
        // 1. Fail Fast
        if (_currentSelectedSkill == null || gridLogic == null)
        {
            ClearAllHighlights();
            return;
        }

        // 2. Validate Target
        if (!IsValidTargetGrid(gridLogic.GridOwner, _currentSelectedSkill.targetType))
        {
            ClearAllHighlights();
            return;
        }

        // 3. Logic & Visual
        if (gridLogic is IGridSystem gridSystem)
        {
            List<Vector2Int> affectedCells = _currentSelectedSkill.GetAffectedPositions(gridPos, gridSystem);

            // Highlight vùng ảnh hưởng
            _battleEvents.RaiseGridHighlight(gridLogic.GridOwner, affectedCells, _currentSelectedSkill.validColor);
        }
    }

    private bool IsValidTargetGrid(Owner gridOwner, SkillTargetType skillTargetType)
    {
        switch (skillTargetType)
        {
            case SkillTargetType.Self: return gridOwner == Owner.Player;
            case SkillTargetType.Enemy: return gridOwner == Owner.Enemy;
            case SkillTargetType.Any: return true;
            default: return false;
        }
    }

    private void ClearAllHighlights()
    {
    
        _battleEvents.RaiseClearHighlight();
    }

    // --- PUBLIC METHODS (Event Handlers) ---

    private void SelectSkill(DuckSkillSO skill)
    {
        _currentSelectedSkill = skill;
    }

    private void DeselectSkill()
    {
        _currentSelectedSkill = null;
        ClearAllHighlights();
    }
}