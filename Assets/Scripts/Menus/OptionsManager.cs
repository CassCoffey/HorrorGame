using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class OptionsManager : MonoBehaviour {

    public GameObject currentPanel;
    public UnityEngine.UI.Button currentButton;
    public GameObject buttonPrefab;
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
                AddListener(buttonScript, resolutions[i]);
                button.GetComponentInChildren<Text>().font = font;
                button.GetComponentInChildren<Text>().text = (resolutions[i].width + "x" + resolutions[i].height);
                button.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)resolutions.Length) * i));
                button.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)resolutions.Length)) - ((1.0f / (float)resolutions.Length) * i));
                button.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                button.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                resolutionButtons.Add(button);
            }
        }
        manager.ClosePanel(resolutionPanel.transform.parent.gameObject);
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
