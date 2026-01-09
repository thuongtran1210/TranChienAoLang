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
    public void InitializeBattleUI(DuckDataSO playerData)
    {
        UpdateEnergyUI(Owner.Player, 0, 100);

        // 2. Spawn Skill Buttons dựa trên Data của Player
        SpawnSkillButtons(playerData);
    }

    private void SpawnSkillButtons(DuckDataSO playerData)
    {
        // Dọn dẹp nút cũ (nếu có) để tránh duplicate khi Restart game
        foreach (var btn in _spawnedButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        _spawnedButtons.Clear();

        // Validate dữ liệu
        if (playerData == null)
        {
            Debug.LogError("BattleUIManager: Player Data is NULL!");
            return;
        }

        // --- SPAWN LOGIC ---
        // Ví dụ: Unit có 1 Active Skill (hoặc bạn có thể dùng List<DuckSkillSO> nếu Unit có nhiều skill)
        if (playerData.activeSkill != null)
        {
            CreateButton(playerData.activeSkill);
        }

        // Nếu sau này bạn có List skills:
        // foreach(var skill in playerData.skills) { CreateButton(skill); }
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

        // CLEAN ARCHITECTURE: Không gọi _gameManager.Trigger...
        // Hãy bắn Event để bất kỳ ai quan tâm (BattleState) tự xử lý
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