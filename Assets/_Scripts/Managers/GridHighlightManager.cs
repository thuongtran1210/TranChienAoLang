using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [SOLID] Single Responsibility Principle: Class này chỉ chịu trách nhiệm
// lắng nghe sự kiện (Event Listener) và ra lệnh cho View.
public class GridHighlightManager : MonoBehaviour
{
    [Header("Core Dependencies")]
    [SerializeField] private TilemapGridView _tilemapGridView;

    [Header("Settings")]
    [SerializeField] private Owner _gridOwner;
    [SerializeField] private BattleEventChannelSO _battleEvents;

    private Coroutine _activeImpactCoroutine;

    // --- 1. ĐĂNG KÝ SỰ KIỆN (OBSERVER PATTERN) ---
    private void OnEnable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnGridHighlightRequested += HandleHighlightRequested;
            _battleEvents.OnGridHighlightClearRequested += HandleClearRequested;
            _battleEvents.OnSkillImpactVisualRequested += HandleImpactVisualRequested;
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
            _battleEvents.OnSkillImpactVisualRequested -= HandleImpactVisualRequested;
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
    private void HandleImpactVisualRequested(Owner target, List<Vector2Int> positions, Color color, float duration)
    {
        if (target != _gridOwner) return;

        // Bắt đầu Coroutine hiệu ứng
        StopImpactEffect();
        _activeImpactCoroutine = StartCoroutine(ImpactEffectRoutine(positions, color, duration));
    }

    private IEnumerator ImpactEffectRoutine(List<Vector2Int> positions, Color color, float duration)
    {
        // 1. Hiển thị Highlight với màu của Skill (thường đậm hơn hoặc sáng hơn)
        _tilemapGridView.HighlightCells(positions, color);

        // 2. Chờ thời gian hiệu ứng (Code xịn có thể dùng Tweening ở đây để Fade out)
        yield return new WaitForSeconds(duration);

        // 3. Tự động tắt sau khi xong
        _tilemapGridView.ClearHighlights();
        _activeImpactCoroutine = null;
    }

    private void StopImpactEffect()
    {
        if (_activeImpactCoroutine != null)
        {
            StopCoroutine(_activeImpactCoroutine);
            _activeImpactCoroutine = null;
        }
    }
}