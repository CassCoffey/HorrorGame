using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ChatScript : MonoBehaviour {

    public GameObject chatPrefab;
    public GameObject chatPanel;
    public InputField chatInput;
    public Font font;

    private List<GameObject> chatMessages = new List<GameObject>();
    private float maxChatHeight;
    private float currentChatHeight = 0;
    private int totalMessages = 0;
    private float opacity = 0.0f;
    private bool visible = false;
    private const float chatTime = 8.0f;
    private float curTime = 0.0f;

    public void Start()
    {
        RefreshChat();
        chatPanel.GetComponentInChildren<Scrollbar>().value = 0;
        chatPanel.GetComponentInChildren<Scrollbar>().size = 0.2f;
    }

    public void Update()
    {
        if (visible && opacity != 1.0f)
        {
            opacity += Time.deltaTime;
            if (opacity > 1.0f)
            {
                opacity = 1.0f;
            }
            chatPanel.transform.parent.GetComponent<CanvasGroup>().alpha = opacity;
        }
        else if (!visible && opacity != 0.0f)
        {
            opacity -= Time.deltaTime;
            if (opacity < 0.0f)
            {
                opacity = 0.0f;
            }
            chatPanel.transform.parent.GetComponent<CanvasGroup>().alpha = opacity;
        }
        if (visible && opacity == 1.0f && !chatInput.IsInteractable())
        {
            curTime += Time.deltaTime;
            if (curTime >= chatTime)
            {
                curTime = 0.0f;
                visible = false;
            }
        }
    }

    public void ToggleActive()
    {
        if (chatInput.IsInteractable())
        {
            chatInput.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);
            chatInput.text = "";
            Screen.showCursor = false;
            Screen.lockCursor = true;
        }
        else
        {
            chatInput.interactable = true;
            EventSystem.current.SetSelectedGameObject(chatInput.gameObject);
            chatInput.text = "";
            curTime = 0.0f;
            visible = true;
            Screen.showCursor = true;
            Screen.lockCursor = false;
        }
    }

    public void SetChat(bool chat)
    {
        visible = chat;
    }

    public void SetInactive()
    {
        if (chatInput.IsInteractable())
        {
            chatInput.text = "";
            chatInput.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);
            visible = false;
        }
    }

    //Removes all chat messages and resizes the chat panel
    void RefreshChat()
    {
        for (int i = 0; i < chatMessages.Count; i++)
        {
            Destroy(chatMessages[i]);
        }
        chatMessages.Clear();
        float panelHeight = chatPrefab.GetComponent<RectTransform>().rect.height * 20;
        maxChatHeight = panelHeight;
        currentChatHeight = 0;
        chatPanel.transform.FindChild("ChatScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, panelHeight);
        chatPanel.transform.FindChild("ChatScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
    }

    public void SendChatMessage()
    {
        if (Input.GetButtonUp("Chat") && chatInput.text != "" && chatInput.IsInteractable())
        {
            foreach (var go in FindObjectsOfType<ChatScript>())
            {
                go.networkView.RPC("RetrieveChatMessage", RPCMode.All, GameObject.Find("SpawnManager").GetComponent<SpawnManager>().randomName, chatInput.text, 0.2f, 0.2f, 0.2f);
            }
            chatInput.text = "";
        }
    }

    //Adds the chat message to the chat message array and moves all chat messages up in the scrolling box.
    //Destroys any chat messages past the maximum chat height
    [RPC] void RetrieveChatMessage(string player, string message, float r, float g, float b)
    {
        GameObject chat = (GameObject)Instantiate(chatPrefab);
        chat.transform.SetParent(chatPanel.transform.FindChild("ChatScrolling"), false);
        chat.GetComponentInChildren<Text>().font = font;
        chat.transform.FindChild("NameText").GetComponent<Text>().text = player;
        chat.transform.FindChild("ChatText").GetComponent<Text>().color = new Color(r, g, b);
        chat.transform.FindChild("ChatText").GetComponent<Text>().text = message;
        chat.GetComponent<RectTransform>().anchorMax = new Vector2(1, chat.transform.FindChild("ChatText").GetComponent<Text>().preferredHeight / maxChatHeight);
        chat.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        chat.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        chat.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        totalMessages++;
        if ((totalMessages) % 2 == 0)
        {
            chat.GetComponent<Image>().color = new UnityEngine.Color(0.4f, 0.4f, 0.4f, 0.7f);
        }
        else
        {
            chat.GetComponent<Image>().color = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 0.7f);
        }
        chatMessages.Add(chat);
        for (int i = chatMessages.Count - 2; i >= 0; i--)
        {
            chatMessages[i].GetComponent<RectTransform>().anchorMax = new Vector2(1, chatMessages[i + 1].GetComponent<RectTransform>().anchorMax.y + (chatMessages[i].transform.FindChild("ChatText").GetComponent<Text>().preferredHeight / maxChatHeight));
            chatMessages[i].GetComponent<RectTransform>().anchorMin = new Vector2(0, chatMessages[i + 1].GetComponent<RectTransform>().anchorMax.y);
        }
        currentChatHeight += chat.transform.FindChild("ChatText").GetComponent<Text>().preferredHeight;
        float panelHeight = chatPanel.GetComponent<RectTransform>().rect.height;
        chatPanel.GetComponentInChildren<Scrollbar>().value = 0;
        chatPanel.GetComponent<ScrollRect>().enabled = (currentChatHeight > panelHeight);
        chatPanel.GetComponentInChildren<Scrollbar>().enabled = (currentChatHeight > panelHeight);
        while (currentChatHeight > maxChatHeight)
        {
            currentChatHeight -= chatMessages[0].transform.FindChild("ChatText").GetComponent<Text>().preferredHeight;
            Destroy(chatMessages[0]);
            chatMessages.RemoveAt(0);
        }
        curTime = 0.0f;
        visible = true;
    }
}
