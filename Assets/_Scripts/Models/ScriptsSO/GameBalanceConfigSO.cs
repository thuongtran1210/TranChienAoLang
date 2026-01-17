using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Duck Battle/Settings/Game Balance Config")]
public class GameBalanceConfigSO : ScriptableObject
{
    [Header("Energy Settings")]
    [Min(0)] public int EnergyGainOnMiss = 10;
    [Min(0)] public int EnergyGainOnHit = 20;
    [Min(0)] public int EnergyGainOnSunk = 30;

    [Header("Starting Stats")]
    public int DefaultMaxEnergy = 100;
    public int DefaultStartingEnergy = 0;

    [Header("Grid Settings")]
    public float GridCellSize = 1.0f;
    public float SkillExecutionDelay = 1f;
    public float EnemyTurnDelay = 1f;

    [Header("Turn Timer Settings")]
    public float PlayerTurnTimeSeconds = 30f;
}
