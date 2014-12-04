using UnityEngine;
using System.Collections;

public class PlayerMenu : MonoBehaviour {

	public void Quit()
    {
        Network.Disconnect();
        Application.LoadLevel(0);
    }
}
