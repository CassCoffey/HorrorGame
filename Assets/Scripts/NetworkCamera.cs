using UnityEngine;
using System.Collections;

public class NetworkCamera : MonoBehaviour {

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
