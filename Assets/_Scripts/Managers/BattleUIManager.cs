using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private FleetManager _fleetManager; 
    [SerializeField] private BattleEventChannelSO _battleEvents;

    [Header("UI Elements")]
    [SerializeField] private Transform _skillButtonContainer;
    [SerializeField] private SkillButtonView _skillButtonPrefab;
    [SerializeField] private Slider _playerEnergySlider;
    [SerializeField] private Slider _enemyEnergySlider;
    [SerializeField] private TextMeshProUGUI _playerEnergyText;
    [SerializeField] private TextMeshProUGUI _enemyEnergyText;

    [Header("UI Elements")]
    [SerializeField] private GameObject _contentRoot;

    private List<SkillButtonView> _spawnedButtons = new List<SkillButtonView>();
    private DuckSkillSO _currentSelectedSkill;
    private int _currentPlayerEnergy;
    private readonly Dictionary<DuckSkillSO, int> _cooldowns = new Dictionary<DuckSkillSO, int>();

    private void OnEnable()
    {
        if (_battleEvents != null)
            _battleEvents.OnEnergyChanged += UpdateEnergyUI;
        if (_battleEvents != null)
        {
            _battleEvents.OnSkillSelected += HandleSkillSelected;
            _battleEvents.OnSkillDeselected += HandleSkillDeselected;
            _battleEvents.OnSkillCooldownChanged += HandleSkillCooldownChanged;
        }
    }

    private void OnDisable()
    {
        if (_battleEvents != null)
            _battleEvents.OnEnergyChanged -= UpdateEnergyUI;
        if (_battleEvents != null)
        {
            _battleEvents.OnSkillSelected -= HandleSkillSelected;
            _battleEvents.OnSkillDeselected -= HandleSkillDeselected;
            _battleEvents.OnSkillCooldownChanged -= HandleSkillCooldownChanged;
        }
    }

    // Được gọi khi bắt đầu Battle (Bạn có thể gọi từ GameManager.EndSetupPhase hoặc Start)
    public void InitializeBattleUI(List<DuckSkillSO> playerSkills)
    {
        Show();
        _cooldowns.Clear();
        _currentPlayerEnergy = 0;
        UpdateEnergyUI(Owner.Player, 0, 100);
        _currentSelectedSkill = null;
        SpawnSkillButtons(playerSkills);
    }
    public void Show()
    {
        if (_contentRoot != null)
            _contentRoot.SetActive(true);
        else
            gameObject.SetActive(true); 
    }
    public void Hide()
    {
        if (_contentRoot != null)
            _contentRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private void SpawnSkillButtons(List<DuckSkillSO> playerSkills)
    {
        foreach (var btn in _spawnedButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        _spawnedButtons.Clear();

        if (_skillButtonContainer == null)
        {
            Debug.LogError("BattleUIManager: _skillButtonContainer is NULL! Cannot spawn buttons.");
            return;
        }
        if (_skillButtonPrefab == null)
        {
            Debug.LogError("BattleUIManager: _skillButtonPrefab is NULL! Cannot spawn buttons.");
            return;
        }

        if (playerSkills == null || playerSkills.Count == 0)
        {
            Debug.LogWarning("BattleUIManager: Player has no skills to spawn buttons for.");
            return;
        }

        foreach (var skill in playerSkills)
        {
            if (skill != null)
            {
                CreateButton(skill);
            }
        }

        RefreshSelectionVisuals();
    }
    private void CreateButton(DuckSkillSO skill)
    {
        if (_skillButtonPrefab == null || _skillButtonContainer == null) return;

        SkillButtonView btnView = Instantiate(_skillButtonPrefab, _skillButtonContainer);

        Sprite ownerIcon = GetOwnerIconForSkill(skill);
        btnView.Setup(skill, ownerIcon, OnSkillClicked);

        _spawnedButtons.Add(btnView);
    }

    private void OnSkillClicked(DuckSkillSO skill)
    {

        _battleEvents.RaiseSkillRequested(skill);
    }

    private void UpdateEnergyUI(Owner owner, int current, int max)
    {
        if (owner == Owner.Player)
        {
            _currentPlayerEnergy = current;
            // Update Player UI
            if (_playerEnergySlider != null)
            {
                _playerEnergySlider.maxValue = max;
                _playerEnergySlider.value = current;
            }

            if (_playerEnergyText != null)
                _playerEnergyText.text = $"{current}/{max}";

            // Cập nhật trạng thái nút Skill của Player
            RefreshSkillButtons();
        }
        else if (owner == Owner.Enemy)
        {
            // Update Enemy UI
            if (_enemyEnergySlider != null) 
            {
                _enemyEnergySlider.maxValue = max;
                _enemyEnergySlider.value = current;
            }

            if (_enemyEnergyText != null)
                _enemyEnergyText.text = $"{current}/{max}";
        }
    }
    private void RefreshSkillButtons()
    {
        foreach (var btn in _spawnedButtons)
        {
            if (btn != null)
            {
                int cooldownRemaining = 0;
                DuckSkillSO skill = btn.SkillData;
                if (skill != null && _cooldowns.TryGetValue(skill, out int value))
                    cooldownRemaining = value;

                btn.UpdateState(_currentPlayerEnergy, cooldownRemaining);
            }
        }
    }

    private void HandleSkillCooldownChanged(DuckSkillSO skill, int remainingTurns)
    {
        if (skill == null)
            return;

        _cooldowns[skill] = Mathf.Max(0, remainingTurns);
        RefreshSkillButtons();
    }

    private Sprite GetOwnerIconForSkill(DuckSkillSO skill)
    {
        if (_fleetManager == null || skill == null)
            return null;

        List<DuckDataSO> fleet = _fleetManager.GetCurrentFleet();
        if (fleet == null)
            return null;

        for (int i = 0; i < fleet.Count; i++)
        {
            DuckDataSO duck = fleet[i];
            if (duck != null && duck.activeSkill == skill && duck.icon != null)
            {
                return duck.icon;
            }
        }

        return null;
    }

    private void HandleSkillSelected(DuckSkillSO skill)
    {
        _currentSelectedSkill = skill;
        RefreshSelectionVisuals();
    }

    private void HandleSkillDeselected()
    {
        _currentSelectedSkill = null;
        RefreshSelectionVisuals();
    }

    private void RefreshSelectionVisuals()
    {
        foreach (var btn in _spawnedButtons)
        {
            if (btn == null)
                continue;

            DuckSkillSO data = btn.SkillData;
            bool isSelected = _currentSelectedSkill != null && data == _currentSelectedSkill;
            btn.SetSelected(isSelected);
        }
    }
}
