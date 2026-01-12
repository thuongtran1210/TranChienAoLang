using UnityEngine;
using UnityEngine.U2D; // Bắt buộc để dùng SpriteAtlas
using System.Collections.Generic;

// Tạo menu để tạo Asset này trong Project
[CreateAssetMenu(fileName = "IconResolver", menuName = "GameSystems/IconResolver")]
public class IconResolver : ScriptableObject
{
    [Header("Configuration")]
    [Tooltip("Kéo Sprite Atlas đã tạo vào đây")]
    [SerializeField] private SpriteAtlas _mainAtlas;

    // Cache để tránh việc gọi GetSprite lặp lại gây garbage collection (Optimization)
    private Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

    /// <summary>
    /// Lấy Sprite dựa trên tên (Item ID).
    /// Áp dụng mẫu Singleton (tạm thời) hoặc Dependency Injection để gọi hàm này.
    /// </summary>
    public Sprite GetIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName))
        {
            Debug.LogWarning($"[IconResolver] Yêu cầu icon rỗng!");
            return null;
        }

        // 1. Kiểm tra Cache
        if (_cache.TryGetValue(iconName, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        // 2. Nếu chưa có, lấy từ Atlas
        // Lưu ý: SpriteAtlas trả về bản clone, cần cẩn thận quản lý bộ nhớ nếu gọi quá nhiều
        Sprite sprite = _mainAtlas.GetSprite(iconName);

        if (sprite != null)
        {
            _cache[iconName] = sprite;
            return sprite;
        }

        Debug.LogError($"[IconResolver] Không tìm thấy icon: {iconName} trong Atlas.");
        return null;
    }

    // Clean up khi ScriptableObject bị unload (Memory Management)
    private void OnDisable()
    {
        _cache.Clear();
    }
}