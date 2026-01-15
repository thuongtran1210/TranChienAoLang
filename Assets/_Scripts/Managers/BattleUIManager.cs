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

    private void OnEnable()
    {
        if (_battleEvents != null)
            _battleEvents.OnEnergyChanged += UpdateEnergyUI;
    }

    private void OnDisable()
    {
        if (_battleEvents != null)
            _battleEvents.OnEnergyChanged -= UpdateEnergyUI;
    }

    // Được gọi khi bắt đầu Battle (Bạn có thể gọi từ GameManager.EndSetupPhase hoặc Start)
    public void InitializeBattleUI(List<DuckSkillSO> playerSkills)
    {
        Debug.Log("BattleUIManager: Initializing...");
        Show();
        UpdateEnergyUI(Owner.Player, 0, 100);

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
    }
    private void CreateButton(DuckSkillSO skill)
    {
        if (_skillButtonPrefab == null || _skillButtonContainer == null) return;

        SkillButtonView btnView = Instantiate(_skillButtonPrefab, _skillButtonContainer);

        // Setup Button: Truyền skill và hàm callback khi click
        btnView.Setup(skill, OnSkillClicked);

        _spawnedButtons.Add(btnView);
    }

    private void OnSkillClicked(DuckSkillSO skill)
    {
        Debug.Log($"UI: Requesting skill {skill.skillName}");

        _battleEvents.RaiseSkillRequested(skill);
    }

    private void UpdateEnergyUI(Owner owner, int current, int max)
    {
        if (owner == Owner.Player)
        {
            // Update Player UI
            if (_playerEnergySlider != null)
            {
                _playerEnergySlider.maxValue = max;
                _playerEnergySlider.value = current;
            }

            if (_playerEnergyText != null)
                _playerEnergyText.text = $"{current}/{max}";

            // Cập nhật trạng thái nút Skill của Player
            UpdateSkillButtonsInteractability(current);
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
    private void UpdateSkillButtonsInteractability(int currentEnergy)
    {
        foreach (var btn in _spawnedButtons)
        {
            if (btn != null)
                btn.UpdateInteractable(currentEnergy);
        }
    }
}