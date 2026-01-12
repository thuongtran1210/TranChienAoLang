using System.Collections.Generic;
using UnityEngine;

// [SOLID] Single Responsibility Principle: Class này chỉ chịu trách nhiệm
// lắng nghe sự kiện (Event Listener) và ra lệnh cho View.
// Nó KHÔNG chịu trách nhiệm về việc vẽ tile hay quản lý danh sách object.
public class GridHighlightManager : MonoBehaviour
{
    [Header("Core Dependencies")]
    [SerializeField] private TilemapGridView _tilemapGridView;

    [Header("Settings")]
    [SerializeField] private Owner _gridOwner;
    [SerializeField] private BattleEventChannelSO _battleEvents;

    // --- 1. ĐĂNG KÝ SỰ KIỆN (OBSERVER PATTERN) ---
    private void OnEnable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnGridHighlightRequested += HandleHighlightRequested;
            _battleEvents.OnGridHighlightClearRequested += HandleClearRequested;
        }
        else
        {
            Debug.LogError($"{name}: BattleEventChannelSO chưa được gán!", this);
        }
    }

    private void OnDisable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnGridHighlightRequested -= HandleHighlightRequested;
            _battleEvents.OnGridHighlightClearRequested -= HandleClearRequested;
        }
    }

    // --- 2. XỬ LÝ LOGIC ---
    public void Initialize(TilemapGridView tilemapView, Owner owner)
    {
        _tilemapGridView = tilemapView;
        _gridOwner = owner;
    }

    private void HandleHighlightRequested(Owner target, List<Vector2Int> positions, Color color)
    {
        // Guard Clause: Nếu không phải lưới của mình thì bỏ qua
        if (target != _gridOwner) return;

        if (_tilemapGridView != null)
        {
            _tilemapGridView.HighlightCells(positions, color);
        }
    }

    private void HandleClearRequested()
    {
        // Chỉ đơn giản là gọi hàm Clear của View
        if (_tilemapGridView != null)
        {
            _tilemapGridView.ClearHighlights();
        }
    }
}