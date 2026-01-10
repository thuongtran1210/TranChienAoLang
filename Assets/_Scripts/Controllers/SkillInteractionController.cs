using System.Collections.Generic;
using UnityEngine;

public class SkillInteractionController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridInputController _inputController;
    [SerializeField] private BattleEventChannelSO _battleEvents;
    [SerializeField] private DuckDataSO _playerData;
    // State
    private DuckSkillSO _currentSelectedSkill;

    private void OnEnable()
    {
        if (_inputController != null)
        {
            _inputController.OnGridCellHovered += HandleGridHover;
            _inputController.OnGridCellClicked += HandleGridClick;
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
            _inputController.OnGridCellClicked -= HandleGridClick;
        }

        if (_battleEvents != null)
        {
            _battleEvents.OnSkillSelected -= SelectSkill;
            _battleEvents.OnSkillDeselected -= DeselectSkill;
        }
    }

    // --- CORE LOGIC ---

    private void HandleGridHover(Vector2Int gridPos, IGridLogic gridLogic)
    {

        // Fail Fast: Nếu không có skill hoặc hover ra ngoài grid hệ thống -> Xóa highlight
        if (_currentSelectedSkill == null || gridLogic is not IGridSystem gridSystem)
        {
            _battleEvents.RaiseClearHighlight();
            return;
        }

        // Tính toán vùng ảnh hưởng (Strategy Pattern từ SO)
        List<Vector2Int> affectedCells = _currentSelectedSkill.GetAffectedPositions(gridPos, gridSystem);

        // Kiểm tra tính hợp lệ để chọn màu
        bool canCast = IsValidCast(gridPos, gridLogic, gridSystem);

        // Lấy màu từ ScriptableObject Config
        Color previewColor = canCast ? _currentSelectedSkill.validColor : _currentSelectedSkill.invalidColor;

        // Bắn Event để View vẽ màu
        _battleEvents.RaiseGridHighlight(gridLogic.GridOwner, affectedCells, previewColor);
    }
    // --- 2. LOGIC CLICK (Thực hiện chiêu) ---
    private void HandleGridClick(Vector2Int gridPos, IGridLogic gridLogic)
    {
        if (_currentSelectedSkill == null || gridLogic is not IGridSystem gridSystem) return;

        if (!IsValidCast(gridPos, gridLogic, gridSystem))
        {
            // Optional: Báo lỗi âm thanh hoặc rung camera
            Debug.Log("Invalid Target!");
            return;
        }

        // Thực thi Skill

        bool success = _currentSelectedSkill.Execute(gridSystem, gridPos, _battleEvents, gridLogic.GridOwner);

        if (success)
        {
            // Nếu thành công -> Hủy chọn Skill để quay về trạng thái thường
            _battleEvents.RaiseSkillDeselected();
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
    private bool IsValidCast(Vector2Int centerPos, IGridLogic gridLogic, IGridSystem gridSystem)
    {
        // Rule 1: Target Type (Skill bắn địch thì không được bắn vào sân nhà)
        if (!IsValidTargetGrid(gridLogic.GridOwner, _currentSelectedSkill.targetType)) return false;

        // Rule 2: Vị trí tâm có nằm trong Grid không?
        if (!gridSystem.IsValidPosition(centerPos)) return false;

        // Rule 3: Kiểm tra ô đó đã bị bắn chưa (tránh bắn trùng)
        // Lưu ý: Tùy game logic, có game cho phép bắn bồi. Ở đây giả sử không được bắn ô đã lộ.
        var cell = gridSystem.GetCell(centerPos);
        if (cell != null && cell.IsHit) return false;

        return true;
    }

    private void ClearAllHighlights()
    {
    
        _battleEvents.RaiseClearHighlight();
    }

    // --- PUBLIC METHODS (Event Handlers) ---

    private void SelectSkill(DuckSkillSO skill)
    {
        _currentSelectedSkill = skill;
        Debug.Log($"[SkillController] Selected: {skill.skillName}");
    }

    private void DeselectSkill()
    {
        _currentSelectedSkill = null;
        _battleEvents.RaiseClearHighlight();
    }
}