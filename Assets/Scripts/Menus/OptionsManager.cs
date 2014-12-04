using UnityEngine;
using System.Collections;

public class OptionsManager : MonoBehaviour {

	public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
