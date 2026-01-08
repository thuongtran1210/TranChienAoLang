using UnityEngine;

public abstract class DuckSkillSO : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public int energyCost;
    public Sprite icon;

    [TextArea] public string description;

    /// <summary>
    /// Hàm thực thi logic của Skill.
    /// </summary>
    /// <param name="targetGrid">Grid của đối thủ (hoặc Grid tác động)</param>
    /// <param name="targetPos">Vị trí click chuột</param>
    /// <param name="eventChannel">Kênh sự kiện để gửi phản hồi (FX, UI)</param>
    /// <returns>True nếu skill dùng thành công</returns>
    public abstract bool Execute(IGridSystem targetGrid, Vector2Int targetPos, BattleEventChannelSO eventChannel);
}