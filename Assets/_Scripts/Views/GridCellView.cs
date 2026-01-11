using UnityEngine;

public class GridCellView : MonoBehaviour, IGridInteractable
{
    [SerializeField] private SpriteRenderer baseRenderer;
    [SerializeField] private SpriteRenderer fogRenderer;

    [SerializeField] private Sprite defaultSprite; // Hình mặt nước
    [SerializeField] private Sprite hitSprite;  // Hình 'Nổ/Trúng' 
    [SerializeField] private Sprite missSprite; // Hình 'Nước bắn/Trượt' 

    [SerializeField] private Sprite fogSprite;

    private Color _originalColor = Color.white;

    // Properties
    public Owner CellOwner { get; private set; }
    public GridCell _cellLogic { get; private set; }
    public Vector2Int GridPosition => _cellLogic != null ? _cellLogic.GridPosition : Vector2Int.zero;

    public void Setup(GridCell cellLogic, Owner owner)
    {
        _cellLogic = cellLogic;
        CellOwner = owner;

        // Setup lớp nền (Kết quả/Nước)
        if (baseRenderer != null)
        {
            baseRenderer.sprite = defaultSprite;
            _originalColor = baseRenderer.color;
        }
        else
        {
            Debug.LogError($"[GridCellView] Missing Base Renderer on {gameObject.name}", this);
        }

        // Setup lớp Mây (Fog)
        if (fogRenderer != null)
        {
            fogRenderer.sprite = fogSprite;
            // Logic quan trọng:
            // Đối với Enemy: Mặc định là CÓ mây (chưa khám phá).
            // Đối với Player: KHÔNG có mây (để thấy tàu mình sắp xếp).
            bool showFog = (owner == Owner.Enemy);
            fogRenderer.gameObject.SetActive(showFog);
        }

#if UNITY_EDITOR
        gameObject.name = $"{owner}_Cell_{cellLogic.GridPosition.x}_{cellLogic.GridPosition.y}";
#endif
    }

    public void UpdateVisual(ShotResult shotResult)
    {
        // 1. Cập nhật Sprite kết quả ở lớp dưới
        if (baseRenderer != null)
        {
            switch (shotResult)
            {
                case ShotResult.Hit:
                case ShotResult.Sunk:
                    baseRenderer.sprite = hitSprite;
                    break;

                case ShotResult.Miss:
                    baseRenderer.sprite = missSprite;
                    break;
                    // Mặc định vẫn là defaultSprite (nước)
            }
        }

        // 2. Xử lý Mây: Khi đã bắn (có kết quả), ta tắt lớp mây đi để lộ kết quả bên dưới
        if (fogRenderer != null && shotResult != ShotResult.None) // Giả định None là chưa bắn
        {
            // Có thể thêm hiệu ứng Fade out ở đây sau này (DOTween)
            fogRenderer.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Điều chỉnh Scale của view để vừa khít với kích thước ô lưới logic
    /// </summary>
    /// <param name="targetSize">Kích thước mong muốn (cellSize)</param>
    public void SetVisualSize(float targetSize)
    {
        // Scale base renderer
        ScaleRenderer(baseRenderer, targetSize);
        // Scale fog renderer
        ScaleRenderer(fogRenderer, targetSize);
    }
    private void ScaleRenderer(SpriteRenderer renderer, float targetSize)
    {
        if (renderer == null || renderer.sprite == null) return;

        transform.localScale = Vector3.one;
        Vector2 spriteSize = renderer.sprite.bounds.size;

        if (spriteSize.x == 0 || spriteSize.y == 0) return;

        float scaleFactorX = targetSize / spriteSize.x;
        float scaleFactorY = targetSize / spriteSize.y;

        // Lưu ý: Việc set transform.localScale sẽ ảnh hưởng toàn bộ object con.
        // Nếu Fog là con của object này, chỉ cần scale object cha là đủ.
        // Nếu Fog là object riêng biệt không phụ thuộc, cần xử lý riêng.
        // Trong trường hợp này, giả định FogRenderer nằm trên cùng GameObject hoặc là Child,
        // ta chỉ cần scale transform của GameObject cha là đủ.

        transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
    }
    /// <summary>
    /// Hàm xử lý trạng thái Highlight thống nhất.
    /// Đáp ứng yêu cầu API từ GridView.
    /// </summary>
    /// <param name="isActive">Bật hay tắt highlight</param>
    /// <param name="color">Màu highlight (nếu bật)</param>
    public void SetHighlightState(bool isActive, Color color)
    {
        if (isActive)
        {
            SetColor(color);
        }
        else
        {
            ResetColor();
        }
    }
    /// <summary>
    /// Đặt màu trực tiếp cho Cell (Dùng cho Highlight)
    /// </summary>
    /// <param name="color">Màu cần hiển thị</param>
    public void SetColor(Color color)
    {
        if (baseRenderer == null) return;
        baseRenderer.color = color;
    }

    /// <summary>
    /// Đưa màu về trạng thái ban đầu (Tắt Highlight)
    /// </summary>
    public void ResetColor()
    {
        if (baseRenderer == null) return;
        baseRenderer.color = _originalColor;
    }
}