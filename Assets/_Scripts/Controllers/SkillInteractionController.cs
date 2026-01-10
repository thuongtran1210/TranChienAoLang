using System.Collections.Generic;
using UnityEngine;

public class SkillInteractionController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridInputController _inputController;

    [Header("EVENTS CHANNEL")]
    [SerializeField] private BattleEventChannelSO _battleEvents;
    [SerializeField] private GridInputChannelSO _gridInputChannel;

    private DuckSkillSO _currentSelectedSkill;

    private void OnEnable()
    {
        if (_gridInputChannel != null)
        {
            _gridInputChannel.OnGridCellHovered += HandleGridHover;
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

        if (_battleEvents != null)
        {
            _battleEvents.OnSkillSelected -= SelectSkill;
            _battleEvents.OnSkillDeselected -= DeselectSkill;

        }
        if (_gridInputChannel != null)
        {
            _gridInputChannel.OnGridCellHovered -= HandleGridHover;
        }
    }

    // --- MAIN LOGIC ---

    private void HandleGridHover(Vector2Int gridPos, IGridLogic gridLogic)
    {
        // 1. Fail Fast
        if (_currentSelectedSkill == null)
        {
            ClearAllHighlights();
            return;
        }

        // Handle trường hợp chuột ra khỏi bàn cờ
        if (gridLogic == null)
        {
            ClearAllHighlights();
            return;
        }
        // 1. Tính toán vị trí World
        // Giả sử Grid của bạn có cellSize = 1. Nếu khác, bạn cần lấy từ GridSystem.
        Vector3 cellWorldPos = gridLogic.GetWorldPosition(gridPos);

        // 2. Lấy Data Visual từ Skill
        Sprite ghostSprite = _currentSelectedSkill.ghostSprite;
        Vector2Int ghostSize = _currentSelectedSkill.areaSize;
        bool isValid = IsValidTargetGrid(gridLogic.GridOwner, _currentSelectedSkill.targetType);

        // 3. RAISE EVENT với đầy đủ thông tin
        _battleEvents.RaiseSkillGhostUpdate(ghostSprite, ghostSize, cellWorldPos, isValid);

        // 5. Logic Highlight các ô 
        if (gridLogic is IGridSystem gridSystem)
        {
            List<Vector2Int> affectedCells = _currentSelectedSkill.GetAffectedPositions(gridPos, gridSystem);
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
        _battleEvents.RaiseSkillGhostClear(); 
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