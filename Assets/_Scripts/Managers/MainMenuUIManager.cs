using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObject _settingsPanel;

    public void OnPlayButtonClicked()
    {
        if (_gameManager != null)
        {
            _gameManager.StartGameFromUI();
        }
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
}

