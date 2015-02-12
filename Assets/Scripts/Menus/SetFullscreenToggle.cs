using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SetFullscreenToggle : MonoBehaviour {

	/// <summary>
	/// Initializes the fullscreen toggle to match whether or not the window is fullscreen.
	/// </summary>
	void Start () 
    {
        this.GetComponent<Toggle>().isOn = Screen.fullScreen;
	}
}
