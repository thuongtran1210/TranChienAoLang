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
    public void InitializeBattleUI()
    {
        SpawnSkillButtons();

        // Reset UI Energy về 0 hoặc giá trị khởi điểm
        UpdateEnergyUI(Owner.Player, 0, 100);
    }

    private void SpawnSkillButtons()
    {
        foreach (Transform child in _skillButtonContainer) Destroy(child.gameObject);
        _spawnedButtons.Clear();

        // Lấy danh sách vịt hiện tại của người chơi
        List<DuckDataSO> myFleet = _fleetManager.GetCurrentFleet();

        foreach (var duck in myFleet)
        {
            if (duck.activeSkill != null)
            {
                SkillButtonView btn = Instantiate(_skillButtonPrefab, _skillButtonContainer);

                // Khi click nút -> Gọi hàm OnSkillClicked
                btn.Setup(duck.activeSkill, OnSkillClicked);

                _spawnedButtons.Add(btn);
            }
        }
    }

    private void OnSkillClicked(DuckSkillSO skill)
    {
        Debug.Log($"UI: Requesting skill {skill.skillName}");
        // Gọi xuống GameManager để chuyển lệnh vào BattleState
        _gameManager.TriggerSkillSelection(skill);
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