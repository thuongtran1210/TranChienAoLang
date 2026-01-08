using UnityEngine;

public class GhostSegmentView : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer duckRenderer; // Kéo SpriteRenderer của DuckSegment_PF vào đây
    [SerializeField] private SpriteRenderer leafRenderer; // Kéo SpriteRenderer của LotusLeaf_Visual vào đây

    [Header("Sorting Config")]
    // Vịt luôn phải vẽ sau (đè lên) lá
    [SerializeField] private int baseSortingOrder = 10;

    public void Setup(Sprite duckSprite, int orderOffset)
    {
        // 1. Cài đặt hình ảnh cho vịt
        if (duckSprite != null)
        {
            duckRenderer.sprite = duckSprite;
        }

        // 2. Xử lý Sorting Order để không bị lỗi hiển thị
        // Lá nằm dưới
        if (leafRenderer)
            leafRenderer.sortingOrder = baseSortingOrder + orderOffset;

        // Vịt nằm trên lá (cộng thêm 1)
        if (duckRenderer)
            duckRenderer.sortingOrder = baseSortingOrder + orderOffset + 1;
    }

    public void SetTransparency(float alpha, Color validColor, Color invalidColor, bool isValid)
    {
        Color targetColor = isValid ? validColor : invalidColor;
        targetColor.a = alpha;

        if (duckRenderer) duckRenderer.color = targetColor;
        if (leafRenderer) leafRenderer.color = targetColor;
    }
    public void ManualInit(SpriteRenderer duckR, SpriteRenderer leafR)
    {
        duckRenderer = duckR;
        leafRenderer = leafR;
    }
}