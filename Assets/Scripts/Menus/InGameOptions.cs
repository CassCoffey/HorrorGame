using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class InGameOptions : MonoBehaviour
{

    public GameObject currentPanel;
    public UnityEngine.UI.Button currentButton;
    public GameObject buttonPrefab;
    public GameObject resolutionPanel;
    public GameObject audioPanel;
	public GameObject gameplayPanel;
    public Font font;

    private Resolution[] resolutions;
    private List<GameObject> resolutionButtons = new List<GameObject>();
    private Resolution CurrentResolution;

    /// <summary>
    /// Initializes resolution options and volume sliders.
    /// </summary>
    void Start()
    {
        CurrentResolution = Screen.currentResolution;
        CreateResolutionList();
        SetVolumeSliders();
		SetSensitivitySlider();
    }

    /// <summary>
    /// Manages which tab is open.
    /// </summary>
    public void SetTabButton(UnityEngine.UI.Button button)
    {
        switch (button.name)
        {
            case "GraphicsTab":
                ChangeTab("GraphicsPanel", button);
                break;
            case "AudioTab":
                ChangeTab("AudioPanel", button);
                break;
            case "GameplayTab":
                ChangeTab("GameplayPanel", button);
                break;
        }
    }

    /// <summary>
    /// Changes which tab is active.
    /// </summary>
    private void ChangeTab(string panel, UnityEngine.UI.Button button)
    {
        currentPanel.SetActive(false);
        currentButton.image.color = Color.white;
        currentPanel = button.transform.parent.FindChild(panel).gameObject;
        currentButton = button;
        currentButton.image.color = Color.grey;
        currentPanel.SetActive(true);
        if (panel == "AudioPanel")
        {
            SetVolumeSliders();
        }
    }

    /// <summary>
    /// Makes the game fullscreen.
    /// </summary>
    /// <param name="fullscreen">Fullscreen or windowed?</param>
    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    /// <summary>
    /// Generates a list of possible resolutions based on your monitor.
    /// </summary>
    private void CreateResolutionList()
    {
        Resolution[] resolutions = Screen.resolutions;
        resolutionPanel.transform.parent.GetComponent<Animator>().SetBool("Open", true);
        if (resolutions != null)
        {
            float panelHeight = buttonPrefab.GetComponent<RectTransform>().rect.height * resolutions.Length;
            float currentHeight = resolutionPanel.transform.parent.GetComponent<RectTransform>().rect.height;
            resolutionPanel.GetComponentInParent<ScrollRect>().enabled = (panelHeight > currentHeight);
            resolutionPanel.transform.parent.GetComponentInChildren<Scrollbar>().enabled = (panelHeight > currentHeight);
            resolutionPanel.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            resolutionPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, currentHeight - panelHeight);
            for (int i = 0; i < resolutions.Length; i++)
            {
                GameObject button = (GameObject)Instantiate(buttonPrefab);
                button.transform.SetParent(resolutionPanel.transform, false);
                UnityEngine.UI.Button buttonScript = button.GetComponent<UnityEngine.UI.Button>();
                if (resolutions[i].height == CurrentResolution.height && resolutions[i].width == CurrentResolution.width)
                {
                    buttonScript.image.color = Color.grey;
                }
                else
                {
                    AddListener(buttonScript, resolutions[i]);
                }
                button.GetComponentInChildren<Text>().font = font;
                button.GetComponentInChildren<Text>().text = (resolutions[i].width + "x" + resolutions[i].height);
                button.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)resolutions.Length) * i));
                button.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)resolutions.Length)) - ((1.0f / (float)resolutions.Length) * i));
                button.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                button.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                resolutionButtons.Add(button);
            }
        }
        // Close the panel and disable all buttons.
        resolutionPanel.transform.parent.gameObject.GetComponent<Animator>().SetBool("Open", false);
        foreach (UnityEngine.UI.Button button in resolutionPanel.transform.parent.gameObject.GetComponentsInChildren<UnityEngine.UI.Button>())
        {
            button.enabled = false;
        }
    }

    /// <summary>
    /// Makes it so that clicking on a resolution changes your resolution.
    /// </summary>
    private void AddListener(UnityEngine.UI.Button button, Resolution resolution)
    {
        button.onClick.AddListener(() =>
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            CurrentResolution = resolution;
            for (int i = 0; i < resolutionButtons.Count; i++)
            {
                Destroy(resolutionButtons[i]);
            }
            resolutionButtons.Clear();
            CreateResolutionList();
        });
    }

    /// <summary>
    /// Updates volume sliders to match your preferences.
    /// </summary>
    public void SetVolumeSliders()
    {
        if (PlayerPrefs.HasKey("Master"))
        {
            audioPanel.transform.FindChild("MasterSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("Master");
            audioPanel.transform.FindChild("MusicSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("Music");
            audioPanel.transform.FindChild("VoiceSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("Voice");
        }
        else
        {
            PlayerPrefs.SetFloat("Master", 1f);
            PlayerPrefs.SetFloat("Music", 1f);
            PlayerPrefs.SetFloat("Voice", 1f);
        }
    }
	public void SetSensitivitySlider()
	{
		if (PlayerPrefs.HasKey("Sensitivity"))
		{
			gameplayPanel.transform.FindChild("MouseSensitivity").GetComponent<Slider>().value = PlayerPrefs.GetFloat("Sensitivity");
		}
		else
		{
			PlayerPrefs.SetFloat("Sensitivity", 15f);
		}
	}

    /// <summary>
    /// Volume slider controls.
    /// </summary>
    
    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Master", value);
        PlayerPrefs.Save();
    }

    public void ChangeMusicVolume(float value)
    {
        Camera.main.transform.FindChild("Music").GetComponent<AudioSource>().volume = value;
        PlayerPrefs.SetFloat("Music", value);
        PlayerPrefs.Save();
    }
    public void ChangeVoiceVolume(float value)
    {
        Camera.main.transform.FindChild("Voice").GetComponent<AudioSource>().volume = value;
        PlayerPrefs.SetFloat("Voice", value);
        PlayerPrefs.Save();
    }
	public void ChangeMouseSensitivity(float value)
	{
		PlayerPrefs.SetFloat("Sensitivity", value);
		PlayerPrefs.Save();
	}
}
