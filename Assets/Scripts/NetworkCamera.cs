using UnityEngine;
using System.Collections;

public class NetworkCamera : MonoBehaviour {

    private Vector3 syncPosition = Vector3.zero;
    private Quaternion syncRotation = Quaternion.identity;

    // Makes sure to disable the camera for clients that are not my own.
    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (networkView.isMine)
        {
            GetComponent<Camera>().enabled = true;
            GetComponent<AudioListener>().enabled = true;
        }
        else
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
        }
    }

    void Update()
    {
        if (!networkView.isMine)
        {
            SyncedMovement();
        }
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 tempSyncPosition = Vector3.zero;
        Quaternion tempSyncRotation = Quaternion.identity;
        if (stream.isWriting)
        {
            tempSyncPosition = transform.position;
            stream.Serialize(ref tempSyncPosition);

            tempSyncRotation = transform.rotation;
            stream.Serialize(ref tempSyncRotation);
        }
        else
        {
            stream.Serialize(ref tempSyncPosition);
            stream.Serialize(ref tempSyncRotation);

            syncPosition = tempSyncPosition;
            syncRotation = tempSyncRotation;
        }
    }

    private void SyncedMovement()
    {
        transform.position = syncPosition;
        transform.rotation = syncRotation;
    }
}
