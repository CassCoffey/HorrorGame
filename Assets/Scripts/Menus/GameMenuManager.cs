﻿using UnityEngine;
using System.Collections;

public class GameMenuManager : MonoBehaviour {

    private Canvas previousCanvas;
    private Canvas nextCanvas;


    public void SetPanel(GameObject panel, bool isOpen)
    {
        if (networkView.isMine)
        {
            panel.GetComponent<Animator>().SetBool("Open", isOpen);
            foreach (UnityEngine.UI.Button button in panel.GetComponentsInChildren<UnityEngine.UI.Button>())
            {
                button.enabled = isOpen;
            }
        }
    }

    public void TogglePanel(GameObject panel)
    {
        if (networkView.isMine)
        {
            SetPanel(panel, !panel.GetComponent<Animator>().GetBool("Open"));
        }
    }

    public void ClosePanel(GameObject panel)
    {
        if (networkView.isMine)
        {
            SetPanel(panel, false);
        }
    }
}