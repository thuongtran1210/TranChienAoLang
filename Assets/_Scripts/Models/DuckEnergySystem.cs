using UnityEngine;

public class DuckEnergySystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BattleEventChannelSO _battleEvents;
    [SerializeField] private GameBalanceConfigSO _balanceConfig; 
    [SerializeField] private Owner _owner;

    // Runtime State - Hide in Inspector
    private int _currentEnergy;
    private int _maxEnergy; // Có thể thay đổi runtime nếu có buff/debuff

    public int CurrentEnergy => _currentEnergy;

    private void Start() // Dùng Start hoặc Awake để init chỉ số
    {
        if (_balanceConfig == null)
        {
            Debug.LogError($"[DuckEnergySystem] Missing Balance Config on {_owner}");
            return;
        }

        _maxEnergy = _balanceConfig.DefaultMaxEnergy;
        // Reset năng lượng khi bắt đầu game
        SetEnergy(_balanceConfig.DefaultStartingEnergy);
    }

    private void OnEnable()
    {
        if (_battleEvents != null)
            _battleEvents.OnShotFired += HandleShotFired;
    }

    private void OnDisable()
    {
        if (_battleEvents != null)
            _battleEvents.OnShotFired -= HandleShotFired;
    }

    private void HandleShotFired(Owner shooter, ShotResult result, Vector2Int pos)
    {
        if (shooter != _owner) return;

       
        int energyGain = 0;
        switch (result)
        {
            case ShotResult.Miss:
                energyGain = _balanceConfig.EnergyGainOnMiss;
                break;
            case ShotResult.Hit:
                energyGain = _balanceConfig.EnergyGainOnHit;
                break;
            case ShotResult.Sunk:
                energyGain = _balanceConfig.EnergyGainOnSunk;
                break;
        }

        if (energyGain > 0) AddEnergy(energyGain);
    }

    public void AddEnergy(int amount)
    {
        SetEnergy(_currentEnergy + amount);
    }

    public bool TryConsumeEnergy(int amount)
    {
        if (_currentEnergy >= amount)
        {
            SetEnergy(_currentEnergy - amount);
            return true;
        }
        return false;
    }

    private void SetEnergy(int value)
    {
        _currentEnergy = Mathf.Clamp(value, 0, _maxEnergy);
        // Bắn event để UI cập nhật
        _battleEvents.RaiseEnergyChanged(_owner, _currentEnergy, _maxEnergy);
    }
}