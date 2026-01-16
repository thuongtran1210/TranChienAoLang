using System.Collections.Generic;
using UnityEngine;

public class SkillInteractionController : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private BattleEventChannelSO _battleEvents;
    [SerializeField] private GridInputChannelSO _gridInputChannel;

    // [Optimization] Cache visual hiện tại để tránh spam event nếu không đổi
    private Vector2Int _lastHoveredPivot = new Vector2Int(-9999, -9999);
    private DuckSkillSO _currentSelectedSkill;

    private void OnEnable()
    {
        if (_gridInputChannel != null)
            _gridInputChannel.OnGridCellHovered += HandleGridHover;

        if (_battleEvents != null)
        {
            _battleEvents.OnSkillSelected += SelectSkill;
            _battleEvents.OnSkillDeselected += DeselectSkill;
        }
    }

    private void OnDisable()
    {
        if (_gridInputChannel != null)
            _gridInputChannel.OnGridCellHovered -= HandleGridHover;

        if (_battleEvents != null)
        {
            _battleEvents.OnSkillSelected -= SelectSkill;
            _battleEvents.OnSkillDeselected -= DeselectSkill;
        }
    }

    // --- CORE LOGIC ---

    private void HandleGridHover(Vector2Int gridPos, IGridLogic gridLogic)
    {
        // 1. [SRP] Controller chỉ điều phối khi có Skill được chọn
        if (_currentSelectedSkill == null) return;

        // 2. [Fail Fast] Nếu chuột ra khỏi grid
        if (gridLogic == null)
        {
            ClearVisuals();
            _lastHoveredPivot = new Vector2Int(-9999, -9999);
            return;
        }

        // 3. [Optimization] Chỉ tính toán lại khi vị trí thay đổi (Micro-optimization)
        // Lưu ý: GridInputController đã lọc sự kiện, nhưng thêm check ở đây để an toàn cho logic Highlighting nặng
        if (gridPos == _lastHoveredPivot) return;
        _lastHoveredPivot = gridPos;

        UpdateVisuals(gridPos, gridLogic);
    }

    private void UpdateVisuals(Vector2Int gridPos, IGridLogic gridLogic)
    {
        // Calculate World Position for Ghost
        Vector3 cellWorldPos = gridLogic.GetWorldPosition(gridPos);

        // Data Retrieval
        Sprite ghostSprite = _currentSelectedSkill.ghostSprite;
        Vector2Int ghostSize = _currentSelectedSkill.areaSize;

        // Validation Logic
        bool isValidTarget = IsValidTargetGrid(gridLogic.GridOwner, _currentSelectedSkill.targetType);

        // Raise Ghost Update
        _battleEvents.RaiseSkillGhostUpdate(ghostSprite, ghostSize, cellWorldPos, isValidTarget);

        // Raise Grid Highlight (Preview Layer)
        IGridSystem gridSystem = gridLogic.GridSystem;
        if (gridSystem != null)
        {
            // Tính toán vùng ảnh hưởng dựa trên Logic của Skill (Strategy Pattern trong SO)
            List<Vector2Int> affectedCells = _currentSelectedSkill.GetAffectedPositions(gridPos, gridSystem);

            Color highlightColor = isValidTarget ? _currentSelectedSkill.validColor : _currentSelectedSkill.invalidColor;

            _battleEvents.RaiseGridHighlight(gridLogic.GridOwner, affectedCells, highlightColor);
        }
    }

    private bool IsValidTargetGrid(Owner gridOwner, SkillTargetType skillTargetType)
    {
        return skillTargetType switch
        {
            SkillTargetType.Self => gridOwner == Owner.Player,
            SkillTargetType.Enemy => gridOwner == Owner.Enemy,
            SkillTargetType.Any => true,
            _ => false
        };
    }

    private void ClearVisuals()
    {
        _battleEvents.RaiseClearHighlight();
        _battleEvents.RaiseSkillGhostClear();
    }

    // --- STATE HANDLERS ---

    private void SelectSkill(DuckSkillSO skill)
    {
        _currentSelectedSkill = skill;
        // Reset last hovered để đảm bảo visual cập nhật ngay lập tức nếu chuột đang đứng yên
        _lastHoveredPivot = new Vector2Int(-9999, -9999);
    }

    private void DeselectSkill()
    {
        _currentSelectedSkill = null;
        ClearVisuals();
    }
}
