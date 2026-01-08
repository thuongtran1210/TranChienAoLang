using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Duck Battle/Battle Event Channel",order =1)]
public class BattleEventChannelSO : ScriptableObject
{
    // Sự kiện bắn: (Người bắn, Kết quả, Tọa độ)
    public UnityAction<Owner, ShotResult, Vector2Int> OnShotFired;

    // Sự kiện thay đổi Energy: (Chủ sở hữu, Energy hiện tại, Max Energy)
    public UnityAction<Owner, int, int> OnEnergyChanged;

    public void RaiseShotFired(Owner shooter, ShotResult result, Vector2Int pos)
    {
        OnShotFired?.Invoke(shooter, result, pos);
    }

    public void RaiseEnergyChanged(Owner owner, int currentEnergy, int maxEnergy)
    {
        OnEnergyChanged?.Invoke(owner, currentEnergy, maxEnergy);
    }
}