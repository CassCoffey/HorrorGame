using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class OptionsManager : MonoBehaviour {

    public GameObject currentPanel;
    public UnityEngine.UI.Button currentButton;
    public GameObject dummyButton;
    public GameObject resolutionPanel;
    public Font font;
    public ScreenManager manager;

    private Resolution[] resolutions;
    private List<GameObject> resolutionButtons = new List<GameObject>();

    void Start()
    {
        CreateResolutionList();
    }

    // Tab Management
    public void SetTabButton(UnityEngine.UI.Button button)
    {
        switch(button.name)
        {
            case "GraphicsTab":
                ChangeTab("GraphicsPanel", button);
                break;
            case "AudioTab":
                ChangeTab("AudioPanel", button);
                break;
            case "MultiplayerTab":
                ChangeTab("MultiplayerPanel", button);
                break;
        }
    }

    private void ChangeTab(string panel, UnityEngine.UI.Button button)
    {
        currentPanel.SetActive(false);
        currentButton.image.color = Color.white;
        currentPanel = button.transform.parent.FindChild(panel).gameObject;
        currentButton = button;
        currentButton.image.color = Color.grey;
        currentPanel.SetActive(true);
    }

    // Graphics Options
	public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    private void CreateResolutionList()
    {
        Resolution[] resolutions = Screen.resolutions;
        resolutionPanel.GetComponent<Animator>().SetBool("Open", true);
        if (resolutions != null)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                GameObject button = (GameObject)Instantiate(dummyButton);
                button.transform.SetParent(resolutionPanel.transform, false);
                UnityEngine.UI.Button buttonScript = button.GetComponent<UnityEngine.UI.Button>();
                AddListener(buttonScript, resolutions[i]);
                button.GetComponentInChildren<Text>().font = font;
                button.GetComponentInChildren<Text>().text = (resolutions[i].width + "x" + resolutions[i].height);
                button.GetComponent<RectTransform>().anchorMax = new Vector2(0.8728045f, 1 - (float)(0.155f * i));
                button.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.845f - (float)(0.155f * i));
                button.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                button.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                resolutionButtons.Add(button);
            }
        }
        resolutionPanel.transform.FindChild("ResolutionScroll").GetComponent<RectTransform>().anchorMax = new Vector2(0.8728045f, 1);
        resolutionPanel.transform.FindChild("ResolutionScroll").GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.845f - (float)(0.155f * resolutionButtons.Count));
        resolutionPanel.transform.FindChild("ResolutionScroll").GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        resolutionPanel.transform.FindChild("ResolutionScroll").GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        foreach (GameObject button in resolutionButtons)
        {
            button.transform.SetParent(resolutionPanel.transform.FindChild("ResolutionScroll"), true);
        }
        manager.ClosePanel(resolutionPanel);
    }

    private void AddListener(UnityEngine.UI.Button button, Resolution resolution)
    {
        button.onClick.AddListener(() =>
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        });
    }

    // Audio Options

    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
    }
}
