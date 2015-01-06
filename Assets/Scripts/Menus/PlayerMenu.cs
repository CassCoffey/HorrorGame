using UnityEngine;
using System.Collections;

public class PlayerMenu : MonoBehaviour {

    /// <summary>
    /// Literally just quits. That's it.
    /// This should be merged with another script.
    /// </summary>
	public void Quit()
    {
        Network.Disconnect();
        Application.LoadLevel(0);
    }
}
