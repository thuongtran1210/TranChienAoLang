using UnityEngine;

public enum AIDifficulty
{
    Easy,
    Normal,
    Hard
}

[CreateAssetMenu(fileName = "AIDifficultyConfig", menuName = "Duck Battle/Settings/AI Difficulty Config")]
public class AIDifficultyConfigSO : ScriptableObject
{
    [System.Serializable]
    public class DifficultySettings
    {
        public AIDifficulty Level;
        public float EnemyTurnDelay = 1.0f;
        public float DecisionRandomness = 0.0f;
    }

    [Header("Default")]
    public AIDifficulty DefaultDifficulty = AIDifficulty.Normal;

    [Header("Runtime State")]
    public AIDifficulty CurrentDifficulty = AIDifficulty.Normal;

    [Header("Per Level Settings")]
    public DifficultySettings EasySettings = new DifficultySettings { Level = AIDifficulty.Easy, EnemyTurnDelay = 1.5f, DecisionRandomness = 0.4f };
    public DifficultySettings NormalSettings = new DifficultySettings { Level = AIDifficulty.Normal, EnemyTurnDelay = 1.0f, DecisionRandomness = 0.2f };
    public DifficultySettings HardSettings = new DifficultySettings { Level = AIDifficulty.Hard, EnemyTurnDelay = 0.6f, DecisionRandomness = 0.0f };

    public DifficultySettings GetSettings(AIDifficulty level)
    {
        switch (level)
        {
            case AIDifficulty.Easy:
                return EasySettings;
            case AIDifficulty.Hard:
                return HardSettings;
            default:
                return NormalSettings;
        }
    }

    public void ResetToDefault()
    {
        CurrentDifficulty = DefaultDifficulty;
    }
}

