using UnityEngine;

public class SkillGhostManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BattleEventChannelSO _battleEvents;

    [Header("Settings")]
    [SerializeField] private GameBalanceConfigSO _gameBalanceConfig;
    [SerializeField] private SpriteRenderer _ghostRenderer; 

    private void Awake()
    {
        if (_ghostRenderer == null)
        {
            Debug.LogError($"{name}: Missing SpriteRenderer reference! Disabling component.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnSkillGhostUpdate += UpdateGhost;
            _battleEvents.OnSkillGhostClear += HideGhost;
        }
    }

    private void OnDisable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnSkillGhostUpdate -= UpdateGhost;
            _battleEvents.OnSkillGhostClear -= HideGhost;
        }
    }

    // --- MAIN LOGIC ---

    private void UpdateGhost(Sprite sprite, Vector2Int sizeInCells, Vector3 worldPos, bool isValid)
    {
        if (sprite == null) return;

        _ghostRenderer.gameObject.SetActive(true);
        _ghostRenderer.sprite = sprite;
        _ghostRenderer.transform.position = worldPos;

        // 1. Color Feedback
        Color targetColor = isValid ? Color.white : Color.red;
        targetColor.a = 0.5f;
        _ghostRenderer.color = targetColor;

        // 2. Scale Calculation
        UpdateGhostScale(sprite, sizeInCells);
    }
    private void UpdateGhostScale(Sprite sprite, Vector2Int sizeInCells)
    {
        // Lấy GridCellSize từ Config (Single Source of Truth)
        // Giả sử trong GameBalanceConfigSO bạn đã thêm field gridSize
        float cellSize = _gameBalanceConfig != null ? _gameBalanceConfig.GridCellSize : 1f;

        float targetWidth = sizeInCells.x * cellSize;
        float targetHeight = sizeInCells.y * cellSize;

        Vector2 originalSize = sprite.bounds.size;

        if (originalSize.x > 0 && originalSize.y > 0)
        {
            float newScaleX = targetWidth / originalSize.x;
            float newScaleY = targetHeight / originalSize.y;
            _ghostRenderer.transform.localScale = new Vector3(newScaleX, newScaleY, 1f);
        }
    }

    private void HideGhost()
    {
        _ghostRenderer.gameObject.SetActive(false);
    }
}