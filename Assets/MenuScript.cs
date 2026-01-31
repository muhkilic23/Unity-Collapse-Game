using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject settingsPanel;

    public void OnPlayButtonClicked()
    {
        settingsPanel.GetComponent<SettingsPanelScript>().ApplySettings();
        SceneManager.LoadScene(1);
    }
    public void OnSettingsButtonClicked()
    {
        if (!settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(true);
        }
        else {
            settingsPanel.SetActive(false);
        }
    }
}
