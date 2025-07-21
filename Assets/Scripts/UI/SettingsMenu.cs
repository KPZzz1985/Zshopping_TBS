using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ZShopping.UI
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("UI References")]
        public Dropdown qualityDropdown;
        public Dropdown resolutionDropdown;
        public Toggle fullscreenToggle;
        public Button applyButton;
        public Button startButton;

        private Resolution[] resolutions;

        void Awake()
        {

            qualityDropdown.ClearOptions();
            List<string> qualities = new List<string>(QualitySettings.names);
            qualityDropdown.AddOptions(qualities);
            qualityDropdown.value = QualitySettings.GetQualityLevel();


            resolutions = Screen.resolutions;
            List<string> resOptions = new List<string>();
            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height + " @" + resolutions[i].refreshRate + "Hz";
                resOptions.Add(option);
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height &&
                    resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
                {
                    currentResolutionIndex = i;
                }
            }
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resOptions);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();


            fullscreenToggle.isOn = Screen.fullScreen;


            applyButton.onClick.AddListener(OnApply);
            startButton.onClick.AddListener(OnStartGame);
        }




        public void OnApply()
        {

            QualitySettings.SetQualityLevel(qualityDropdown.value, true);

            Resolution res = resolutions[resolutionDropdown.value];
            Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn, res.refreshRate);
        }




        public void OnStartGame()
        {
            OnApply();

            SceneManager.LoadScene("SampleScene");
        }
    }
} 
