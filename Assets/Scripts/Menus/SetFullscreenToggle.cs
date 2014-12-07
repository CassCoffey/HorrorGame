using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SetFullscreenToggle : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.GetComponent<Toggle>().isOn = Screen.fullScreen;
	}
}
