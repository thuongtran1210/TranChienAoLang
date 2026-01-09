using System.Collections.Generic;
using UnityEngine;

public class SkillTargetingSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridInputController _inputController;
    [SerializeField] private BattleEventChannelSO _eventChannel;
    [SerializeField] private GridHighlightManager _highlightManager;

    [Header("Runtime State")]
    private DuckSkillSO _selectedSkill;
    private IGridSystem _hoveredGrid; // Grid hiện tại đang hover

    private void OnEnable()
    {
        _inputController.OnGridCellHovered += HandleGridHover;
        // Giả sử bạn có event chọn skill từ UI
        // _eventChannel.OnSkillSelected += SelectSkill; 
    }

    private void OnDisable()
    {
        _inputController.OnGridCellHovered -= HandleGridHover;
    }

    // Hàm này được gọi từ UI Manager khi bấm nút Skill
    public void SelectSkill(DuckSkillSO skill)
    {
        _selectedSkill = skill;
    }

    public void DeselectSkill()
    {
        _selectedSkill = null;
        _highlightManager.ClearHighlight();
    }

    private void HandleGridHover(Vector2Int gridPos, IGridLogic gridLogic)
    {
        // 1. Nếu không có skill nào đang chọn -> Return
        if (_selectedSkill == null) return;

        // 2. Nếu chuột ra khỏi grid -> Xóa highlight
        if (gridLogic == null || gridLogic is not IGridSystem gridSystem)
        {
            _highlightManager.ClearHighlight();
            return;
        }

        // 3. Lấy vùng ảnh hưởng từ Skill (PREVIEW LOGIC)
        List<Vector2Int> affectedCells = _selectedSkill.GetAffectedPositions(gridPos, gridSystem);

        // 4. Validation & Coloring
        bool isValidPos = gridSystem.IsValidPosition(gridPos);
        Color previewColor = isValidPos ? _selectedSkill.skillColor : _selectedSkill.invalidColor;

        // Nếu muốn Advanced: Highlight màu đỏ cho các ô nằm ngoài phạm vi
        // Ở đây ta highlight toàn bộ vùng trả về
        _highlightManager.ShowHighlight(affectedCells, previewColor);
    }
}