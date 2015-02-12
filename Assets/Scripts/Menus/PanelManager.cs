using UnityEngine;
using System.Collections;

public class PanelManager : MonoBehaviour {

    /// <summary>
    /// Handles dropdown panels.
    /// </summary>
    /// <param name="panel">The panel in question.</param>
    /// <param name="isOpen">Whether to open or close it.</param>
    public void SetPanel(GameObject panel, bool isOpen)
    {
        panel.GetComponent<Animator>().SetBool("Open", isOpen);
        foreach (UnityEngine.UI.Button button in panel.GetComponentsInChildren<UnityEngine.UI.Button>())
        {
            button.enabled = isOpen;
        }
    }

    /// <summary>
    /// Toggles a panel.
    /// </summary>
    /// <param name="panel">The panel to toggle.</param>
    public void TogglePanel(GameObject panel)
    {
        SetPanel(panel, !panel.GetComponent<Animator>().GetBool("Open"));
    }

    /// <summary>
    /// Closes a panel.
    /// </summary>
    /// <param name="panel">The panel to close.</param>
    public void ClosePanel(GameObject panel)
    {
        SetPanel(panel, false);
    }
}
