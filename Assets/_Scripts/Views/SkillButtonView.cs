using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _btn;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private GameObject _cooldownOverlay; // Optional: Lớp phủ mờ khi không đủ mana

    private DuckSkillSO _skillData;
    private System.Action<DuckSkillSO> _onClicked;

    public void Setup(DuckSkillSO skill, System.Action<DuckSkillSO> onClicked)
    {
        _skillData = skill;
        _onClicked = onClicked;

        if (_skillData != null)
        {
            _iconImage.sprite = _skillData.icon;
            _costText.text = _skillData.energyCost.ToString();

            _btn.onClick.RemoveAllListeners();
            _btn.onClick.AddListener(() => _onClicked?.Invoke(_skillData));
        }
    }

    // Hàm này được gọi mỗi khi Energy thay đổi để check xem có đủ tiền dùng skill không
    public void UpdateInteractable(int currentEnergy)
    {
        if (_skillData == null) return;

        bool canUse = currentEnergy >= _skillData.energyCost;

        // UI Feedback: Disable nút và hiện overlay
        _btn.interactable = canUse;
        if (_cooldownOverlay != null)
            _cooldownOverlay.SetActive(!canUse);

        // Cập nhật text màu đỏ nếu không đủ tiền (Optional - Polishing)
        _costText.color = canUse ? Color.white : Color.red;
    }
}
