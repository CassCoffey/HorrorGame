using UnityEngine;
using System.Collections;

public class NetworkCamera : MonoBehaviour {

    // Makes sure to disable the camera for clients that are not my own.
    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (networkView.isMine)
        {
            GetComponent<Camera>().enabled = true;
        }
        else
        {
            GetComponent<Camera>().enabled = false;
        }
    }
}
