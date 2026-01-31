using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelScript : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private Slider rowSlider;
    [SerializeField] private Slider colSlider;
    [SerializeField] private Slider colorSlider;
    [SerializeField] private Text rowText; 
    [SerializeField] private Text colText;
    [SerializeField] private Text colorText;

    void Start()
    {
        // slider setup
        rowSlider.minValue = 2; rowSlider.maxValue = 10; rowSlider.wholeNumbers = true;
        colSlider.minValue = 2; colSlider.maxValue = 10; colSlider.wholeNumbers = true;
        colorSlider.minValue = 1; colorSlider.maxValue = 6; colorSlider.wholeNumbers = true;

        // mevcut ayarları UI'a bas
        rowSlider.value = SettingsHolder.Rows;
        colSlider.value = SettingsHolder.Cols;
        colorSlider.value = SettingsHolder.colors;

        UpdateTexts();

        rowSlider.onValueChanged.AddListener(_ => UpdateTexts());
        colSlider.onValueChanged.AddListener(_ => UpdateTexts());
        colorSlider.onValueChanged.AddListener(_ => UpdateTexts());

        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        // her açışta güncel değerleri göster
        rowSlider.value = SettingsHolder.Rows;
        colSlider.value = SettingsHolder.Cols;
        UpdateTexts();
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void ApplySettings()
    {
        SettingsHolder.Rows = (int)rowSlider.value;
        SettingsHolder.Cols = (int)colSlider.value;
        SettingsHolder.colors = (int)colorSlider.value;
        settingsPanel.SetActive(false);
    }

    private void UpdateTexts()
    {
        rowText.text = $"Rows: {(int)rowSlider.value}";
        colText.text = $"Cols: {(int)colSlider.value}";
        colorText.text = $"Colors: {(int)colorSlider.value}";
    }
}
