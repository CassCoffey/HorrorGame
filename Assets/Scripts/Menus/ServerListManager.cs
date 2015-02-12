using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ServerListManager : MonoBehaviour {

    public GameObject serverPrefab;
    public GameObject serverListPanel;
    public Font font;
    public GameObject lobby;
    public GameObject screenManager;
    public float pingUpdate;
    public GameObject passwordPanel;

    private HostData[] hostList;
    private List<GameObject> servers = new List<GameObject>();
    private HostData host;
    private UnityEngine.UI.Button selectedButton;
    private Dictionary<Text, Ping> serverPings = new Dictionary<Text,Ping>();
    private string password = "";
    private bool cameraChild = false;

    /// <summary>
    /// Gets a new host list from the master server.
    /// </summary>
    public void RefreshHostList()
    {
        host = null;
        for (int i = 0; i < servers.Count; i++)
        {
            Destroy(servers[i]);
        }
        servers.Clear();
        MasterServer.RequestHostList(NetworkManager.AppId);
    }

    /// <summary>
    /// Adds a listener to the host buttons.
    /// </summary>
    /// <param name="button">The button to add a listener to.</param>
    /// <param name="data">The host to associate that button with.</param>
    public void AddListener(UnityEngine.UI.Button button, HostData data)
    {
        button.onClick.AddListener(() => setHost(data, button));
    }

    /// <summary>
    /// Checks the pings of the servers.
    /// </summary>
    public string CheckPing(Text text)
    {
        Ping ping = serverPings[text];
        if (ping.isDone)
        {
            string pingTime = ping.time.ToString();
            return pingTime;
        }
        else
        {
            return "???";
        }
    }

    /// <summary>
    /// Called when the master server returns the host list.
    /// Populates the server list.
    /// </summary>
    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
        {
            hostList = MasterServer.PollHostList();
            if (hostList != null)
            {
                float panelHeight = serverPrefab.GetComponent<RectTransform>().rect.height * hostList.Length;
                float currentHeight = serverListPanel.transform.parent.GetComponent<RectTransform>().rect.height;
                serverListPanel.GetComponentInParent<ScrollRect>().enabled = (panelHeight > currentHeight);
                serverListPanel.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                serverListPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, currentHeight - panelHeight);
                for (int i = 0; i < hostList.Length; i++)
                {
                    if (hostList[i].comment != "Closed" && hostList[i].comment == NetworkManager.Version)
                    {
                        GameObject button = (GameObject)Instantiate(serverPrefab);
                        button.transform.SetParent(serverListPanel.transform, false);
                        UnityEngine.UI.Button buttonScript = button.GetComponent<UnityEngine.UI.Button>();
                        foreach (Text text in button.GetComponentsInChildren<Text>())
                        {
                            text.font = font;
                        }
                        button.transform.FindChild("NameText").GetComponent<Text>().text = hostList[i].gameName;
                        button.transform.FindChild("PlayersText").GetComponent<Text>().text = hostList[i].connectedPlayers + "/" + hostList[i].playerLimit;
                        button.transform.FindChild("PasswordText").GetComponent<Text>().text = hostList[i].passwordProtected.ToString();
                        Ping serverPing = new Ping(hostList[i].ip[0]);
                        serverPings.Add(button.transform.FindChild("PingText").GetComponent<Text>(), serverPing);
                        button.transform.FindChild("PingText").GetComponent<Text>().text = "???";
                        button.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)hostList.Length) * i));
                        button.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)hostList.Length)) - ((1.0f / (float)hostList.Length) * i));
                        button.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                        button.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                        HostData data = hostList[i];
                        AddListener(buttonScript, data);
                        servers.Add(button);
                        if (servers.Count % 2 == 0)
                        {
                            button.GetComponent<Image>().color = new UnityEngine.Color(0.4f, 0.4f, 0.4f, 0.7f);
                        }
                        else
                        {
                            button.GetComponent<Image>().color = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 0.7f);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when the camera arrives at this menu.
    /// Refreshes the list.
    /// </summary>
    public void OnCameraArrive(Object canvas)
    {
        if ((GameObject)canvas == gameObject)
        {
            RefreshHostList();
        }
    }

    /// <summary>
    /// Joins the currently selected host.
    /// </summary>
    public void JoinServer()
    {
        if (host != null)
        {
            Debug.Log("Attempting to join server.");
            if (host.passwordProtected)
            {
                passwordPanel.SetActive(true);
            }
            else
            {
                Network.Connect(host);
            }
        }
        else
        {
            Debug.Log("No host selected.");
        }
    }

    /// <summary>
    /// Joins a server and submits a password.
    /// </summary>
    public void JoinPasswordedServer()
    {
        Network.Connect(host, password);
        passwordPanel.SetActive(false);
    }

    /// <summary>
    /// When connected to the server, move to the lobby.
    /// </summary>
    void OnConnectedToServer()
    {
        screenManager.GetComponent<ScreenManager>().MoveCameraTo(lobby);
    }

    /// <summary>
    /// Debug logs for possible connection errors.
    /// </summary>
    void OnFailedToConnect(NetworkConnectionError error)
    {
        if (error == NetworkConnectionError.InvalidPassword)
        {
            Debug.Log("Invalid Password");
        }
        else
        {
            Debug.Log(error);
        }
    }

    /// <summary>
    /// Sets the current selected host.
    /// </summary>
    public void setHost(HostData data, UnityEngine.UI.Button button)
    {
        if (selectedButton != null && selectedButton != button)
        {
            UnityEngine.Color oldColor = selectedButton.image.color;
            selectedButton.image.color = new UnityEngine.Color(oldColor.r + 0.1f, oldColor.g + 0.1f, oldColor.b + 0.1f);
        }
        selectedButton = button;
        UnityEngine.Color newColor = button.image.color;
        if (host == data)
        {
            host = null;
            button.image.color = new UnityEngine.Color(newColor.r + 0.1f, newColor.g + 0.1f, newColor.b + 0.1f);
        }
        else
        {
            host = data;
            button.image.color = new UnityEngine.Color(newColor.r - 0.1f, newColor.g - 0.1f, newColor.b - 0.1f);
        }
    }

    /// <summary>
    /// Called when the user enters a password.
    /// </summary>
    public void setPassword(string newPassword)
    {
        password = newPassword;
    }

    /// <summary>
    /// Checks ping.
    /// </summary>
    void Update()
    {
        if (cameraChild && Camera.main.transform.parent != this.transform)
        {
            cameraChild = false;
        }
        else if (!cameraChild && Camera.main.transform.parent == this.transform)
        {
            cameraChild = true;
            RefreshHostList();
        }
        if (servers.Count > 0)
        {
            foreach (GameObject server in servers)
            {
                server.transform.FindChild("PingText").GetComponent<Text>().text = CheckPing(server.transform.FindChild("PingText").GetComponent<Text>());
            }
        }
    }
}
