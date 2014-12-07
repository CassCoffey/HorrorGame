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
    private float pingUpdateTime = 0;
    private string password = "";

    // Refreshes the list of hosts.
    public void RefreshHostList()
    {
        host = null;
        for (int i = 0; i < servers.Count; i++)
        {
            Destroy(servers[i]);
        }
        servers.Clear();
        MasterServer.RequestHostList(NetworkManager.TypeName);
    }

    public void AddListener(UnityEngine.UI.Button button, HostData data)
    {
        button.onClick.AddListener(() => setHost(data, button));
    }

    public string CheckPing(Text text)
    {
        Debug.Log("Checking Ping.");
        Ping ping = serverPings[text];
        if (serverPings[text].isDone)
        {
            return ping.time.ToString();
        }
        else
        {
            return "???";
        }
    }

    // Called by the master server.
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
                    button.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f/(float)hostList.Length) * i));
                    button.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)hostList.Length)) - ((1.0f / (float)hostList.Length) * i));
                    button.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                    button.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                    HostData data = hostList[i];
                    AddListener(buttonScript, data);
                    if (i % 2 == 0)
                    {
                        button.GetComponent<Image>().color = new UnityEngine.Color(0.4f, 0.4f, 0.4f, 0.7f);
                    }
                    else
                    {
                        button.GetComponent<Image>().color = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 0.7f);
                    }
                    servers.Add(button);
                }
            }
        }
    }

    // Joins the specified host.
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
                screenManager.GetComponent<ScreenManager>().MoveCameraTo(lobby);
            }
        }
        else
        {
            Debug.Log("No host selected.");
        }
    }

    public void JoinPasswordedServer()
    {
        Network.Connect(host, password);
        screenManager.GetComponent<ScreenManager>().MoveCameraTo(lobby);
    }

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

    public void setPassword(string newPassword)
    {
        password = newPassword;
    }

    void Update()
    {
        if (servers.Count > 0 && pingUpdateTime > 1f)
        {
            foreach (GameObject server in servers)
            {
                server.transform.FindChild("PingText").GetComponent<Text>().text = CheckPing(server.transform.FindChild("PingText").GetComponent<Text>());
            }
            pingUpdateTime = 0;
        }
        pingUpdateTime += Time.deltaTime;
    }
}
