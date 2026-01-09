using UnityEngine;

public class DuckEnergySystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BattleEventChannelSO _battleEvents;
    [SerializeField] private Owner _owner;

    [Header("Settings")]
    [SerializeField] private int _maxEnergy = 100;
    [SerializeField] private int _startingEnergy = 0;

    // Runtime State
    private int _currentEnergy;

    public int CurrentEnergy => _currentEnergy;

    private void OnEnable()
    {
        if (_battleEvents != null)
            _battleEvents.OnShotFired += HandleShotFired;

        _currentEnergy = _startingEnergy;
    }

    private void OnDisable()
    {
        if (_battleEvents != null)
            _battleEvents.OnShotFired -= HandleShotFired;
    }

    private void HandleShotFired(Owner shooter, ShotResult result, Vector2Int pos)
    {
        // Chỉ xử lý nếu người bắn là Owner của System này
        if (shooter != _owner) return;

        int energyGain = 0;

        switch (result)
        {
            case ShotResult.Miss:
                energyGain = 10; // GDD: Miss +10
                break;
            case ShotResult.Hit:
                energyGain = 20; // GDD: Hit +20
                break;
            // Giả sử ShotResult có Enum Sunk, nếu không bạn cần check biến bool IsSunk từ Grid
            case ShotResult.Sunk:
                energyGain = 30; // GDD: Sunk +30
                break;
        }

        AddEnergy(energyGain);
    }

    public void AddEnergy(int amount)
    {
        _currentEnergy = Mathf.Clamp(_currentEnergy + amount, 0, _maxEnergy);

        Debug.Log($"[{_owner}] Energy: {_currentEnergy}/{_maxEnergy} (+{amount})");

        // Bắn sự kiện để UI cập nhật
        _battleEvents.RaiseEnergyChanged(_owner, _currentEnergy, _maxEnergy);
    }

    // Hàm tiêu thụ Energy cho Skill (sẽ dùng sau)
    public bool TryConsumeEnergy(int amount)
    {
        if (_currentEnergy >= amount)
        {
            _currentEnergy -= amount;
            _battleEvents.RaiseEnergyChanged(_owner, _currentEnergy, _maxEnergy);
            return true;
        }
        return false;
    }
}