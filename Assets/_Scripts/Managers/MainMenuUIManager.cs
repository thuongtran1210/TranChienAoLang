using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _difficultyPanel;
    [SerializeField] private AIDifficultyConfigSO _aiDifficultyConfig;
    [SerializeField] private AIDifficultyEventChannelSO _aiDifficultyEvents;

    public void OnPlayButtonClicked()
    {
        if (_difficultyPanel != null)
            _difficultyPanel.SetActive(true);
    }

    public void OnSettingsButtonClicked()
    {
        if (_settingsPanel != null)
        {
            bool isActive = _settingsPanel.activeSelf;
            _settingsPanel.SetActive(!isActive);
        }
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    public void OnDifficultyEasy()
    {
        SetDifficulty(AIDifficulty.Easy);
    }

    public void OnDifficultyNormal()
    {
        SetDifficulty(AIDifficulty.Normal);
    }

    public void OnDifficultyHard()
    {
        SetDifficulty(AIDifficulty.Hard);
    }

    public void OnDifficultyReset()
    {
        if (_aiDifficultyConfig != null)
        {
            _aiDifficultyConfig.ResetToDefault();
            RaiseDifficultyChanged(_aiDifficultyConfig.CurrentDifficulty);
        }
    }

    public void OnDifficultyConfirm()
    {
        if (_difficultyPanel != null)
            _difficultyPanel.SetActive(false);

        if (_gameManager != null)
        {
            _gameManager.StartGameFromUI();
        }
    }

    private void SetDifficulty(AIDifficulty difficulty)
    {
        if (_aiDifficultyConfig != null)
        {
            _aiDifficultyConfig.CurrentDifficulty = difficulty;
        }
        RaiseDifficultyChanged(difficulty);
    }

    private void RaiseDifficultyChanged(AIDifficulty difficulty)
    {
        if (_aiDifficultyEvents != null)
        {
            _aiDifficultyEvents.RaiseDifficultyChanged(difficulty);
        }
    }
}

