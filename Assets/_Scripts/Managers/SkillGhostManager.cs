using UnityEngine;

public class SkillGhostManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BattleEventChannelSO _battleEvents;

    [Header("Settings")]
    [SerializeField] private float _gridCellSize = 1f; // Kích thước 1 ô lưới trong Unity World Unit
    [SerializeField] private SpriteRenderer _ghostRenderer; // Kéo SpriteRenderer của Ghost vào đây

    private void Awake()
    {
        if (_ghostRenderer == null)
        {
            // Tự tạo nếu chưa gán
            GameObject go = new GameObject("GhostVisual");
            go.transform.SetParent(transform);
            _ghostRenderer = go.AddComponent<SpriteRenderer>();
        }
        _ghostRenderer.gameObject.SetActive(false);
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

        // 1. Handle Color (Xanh nếu đúng, Đỏ nếu sai)
        Color c = isValid ? Color.white : Color.red;
        c.a = 0.5f; // Độ trong suốt
        _ghostRenderer.color = c;

        // 2. Handle Scaling (Auto-fit Grid)
        // Công thức: Scale = (TargetSizeInWorld) / (OriginalSpriteSize)

        float targetWidth = sizeInCells.x * _gridCellSize;
        float targetHeight = sizeInCells.y * _gridCellSize;

        Vector2 spriteSize = sprite.bounds.size; // Kích thước gốc của Sprite trong World Unit

        if (spriteSize.x > 0 && spriteSize.y > 0)
        {
            float newScaleX = targetWidth / spriteSize.x;
            float newScaleY = targetHeight / spriteSize.y;

            // Áp dụng scale (giữ Z = 1)
            _ghostRenderer.transform.localScale = new Vector3(newScaleX, newScaleY, 1f);
        }
    }

    private void HideGhost()
    {
        _ghostRenderer.gameObject.SetActive(false);
    }
}