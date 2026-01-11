using UnityEngine;

public class GridCellView : MonoBehaviour, IGridInteractable
{
    [SerializeField] private SpriteRenderer baseRenderer;
    [SerializeField] private SpriteRenderer fogRenderer;

    [SerializeField] private SpriteRenderer iconRenderer;

    [SerializeField] private Sprite defaultSprite; // Hình mặt nước
    [SerializeField] private Sprite hitSprite;  // Hình 'Nổ/Trúng' 
    [SerializeField] private Sprite missSprite; // Hình 'Nước bắn/Trượt' 
    [SerializeField] private Sprite sunkSprite;
    [SerializeField] private Sprite fogSprite;

    private Color _originalColor = Color.white;
    private bool _isPlayerCell; 

    // Properties
    public Owner CellOwner { get; private set; }
    public GridCell _cellLogic { get; private set; }
    public Vector2Int GridPosition => _cellLogic != null ? _cellLogic.GridPosition : Vector2Int.zero;
    private void Awake()
    {
        gameObject.SetActive(true);
    }
    public void Setup(GridCell cellLogic, bool isPlayerCell)
    {
        _cellLogic = cellLogic;
        _isPlayerCell = isPlayerCell;

        if (fogRenderer != null) fogRenderer.sprite = fogSprite;
        if (baseRenderer != null)
        {
            baseRenderer.sprite = defaultSprite;
            _originalColor = baseRenderer.color;
        }

        // Đồng bộ visual ngay lập tức theo data của Model
        UpdateVisualState();
    }
    public void UpdateVisualState()
    {
        if (_cellLogic == null) return;

        // 1. Xử lý Fog (Thuần túy dựa trên IsRevealed của Model)
        if (fogRenderer != null)
        {
            bool shouldShowFog = !_isPlayerCell && !_cellLogic.IsRevealed;
            fogRenderer.gameObject.SetActive(shouldShowFog);
        }

        // 2. Xử lý Sprite Nền (Hit/Miss/Default)
        if (baseRenderer != null)
        {
            if (_cellLogic.IsHit)
            {
                // Logic hiển thị Hit/Miss dựa trên việc có Unit hay không
                baseRenderer.sprite = _cellLogic.IsOccupied ? hitSprite : missSprite;
            }
            else
            {
                baseRenderer.sprite = defaultSprite;
            }
        }
    }
    public void UpdateVisual(ShotResult shotResult)
    {
        Debug.Log($"[View] Ô {name} nhận lệnh update: {shotResult}");
        // 1. Nếu bắn trúng, có thể cần tắt Fog ngay lập tức (Logic Game)
        // Tùy design game, thường bắn trúng/trượt đều lộ ô đó ra
        if (fogRenderer != null) fogRenderer.gameObject.SetActive(false);

        // 2. Hiển thị Icon trạng thái
        if (iconRenderer == null) return;

        switch (shotResult)
        {
            case ShotResult.Hit:
                iconRenderer.sprite = hitSprite;
                iconRenderer.gameObject.SetActive(true);
                break;

            case ShotResult.Miss:
                iconRenderer.sprite = missSprite;
                iconRenderer.gameObject.SetActive(true);
                break;

            case ShotResult.Sunk:
                // Nếu không có sprite riêng cho Sunk thì dùng Hit
                iconRenderer.sprite = sunkSprite != null ? sunkSprite : hitSprite;
                iconRenderer.gameObject.SetActive(true);
                break;
        }
    }
    /// <summary>
    /// Điều chỉnh Scale của view để vừa khít với kích thước ô lưới logic
    /// </summary>
    /// <param name="targetSize">Kích thước mong muốn (cellSize)</param>
    public void SetVisualSize(float targetSize)
    {
        ApplyScaleToRenderer(baseRenderer, targetSize);
        ApplyScaleToRenderer(fogRenderer, targetSize);
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
    private void ApplyScaleToRenderer(SpriteRenderer renderer, float targetSize)
    {
        if (renderer == null || renderer.sprite == null) return;

        // Reset localScale của RENDERER CON, không phải transform (Parent)
        renderer.transform.localScale = Vector3.one;

        Vector2 spriteSize = renderer.sprite.bounds.size;
        if (spriteSize.x == 0 || spriteSize.y == 0) return;

        // Tính toán scale factor
        float scaleFactorX = targetSize / spriteSize.x;
        float scaleFactorY = targetSize / spriteSize.y;

        // Áp dụng vào chính Renderer đó
        renderer.transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
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