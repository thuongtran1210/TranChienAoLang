using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _btn;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private GameObject _cooldownOverlay;
    [SerializeField] private TextMeshProUGUI _cooldownText;
    [SerializeField] private Image _selectedFrame;
    [SerializeField] private Image _ownerIconImage;
    [SerializeField] private TextMeshProUGUI _typeText;

    private DuckSkillSO _skillData;
    private System.Action<DuckSkillSO> _onClicked;
    private bool _isSelected;

    public DuckSkillSO SkillData => _skillData;

    public void Setup(DuckSkillSO skill, Sprite ownerIcon, System.Action<DuckSkillSO> onClicked)
    {
        _skillData = skill;
        _onClicked = onClicked;

        if (_skillData != null)
        {
            if (_iconImage != null)
                _iconImage.sprite = _skillData.icon;

            if (_costText != null)
                _costText.text = _skillData.energyCost.ToString();

            if (_ownerIconImage != null)
            {
                _ownerIconImage.sprite = ownerIcon;
                _ownerIconImage.enabled = ownerIcon != null;
            }

            if (_typeText != null)
                _typeText.text = GetTargetTypeLabel(_skillData.targetType);

            if (_btn != null)
            {
                _btn.onClick.RemoveAllListeners();
                _btn.onClick.AddListener(OnClickInternal);
            }
        }

        SetSelected(false);
    }

    private void OnClickInternal()
    {
        if (_skillData != null)
            _onClicked?.Invoke(_skillData);
    }

    public void UpdateInteractable(int currentEnergy)
    {
        UpdateState(currentEnergy, 0);
    }

    public void UpdateState(int currentEnergy, int cooldownTurnsRemaining)
    {
        if (_skillData == null)
            return;

        bool hasEnergy = currentEnergy >= _skillData.energyCost;
        bool isOnCooldown = cooldownTurnsRemaining > 0;
        bool canUse = hasEnergy && !isOnCooldown;

        if (_btn != null)
            _btn.interactable = canUse;

        if (_cooldownOverlay != null)
            _cooldownOverlay.SetActive(!canUse);

        if (_cooldownText != null)
            _cooldownText.text = isOnCooldown ? cooldownTurnsRemaining.ToString() : string.Empty;

        if (_costText != null)
            _costText.color = hasEnergy ? Color.white : Color.red;
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        if (_selectedFrame != null)
            _selectedFrame.enabled = selected;
    }

    private string GetTargetTypeLabel(SkillTargetType targetType)
    {
        switch (targetType)
        {
            case SkillTargetType.Self:
                return "Self";
            case SkillTargetType.Enemy:
                return "Enemy";
            case SkillTargetType.Any:
                return "Any";
            default:
                return string.Empty;
        }
    }
}
