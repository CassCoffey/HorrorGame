using UnityEngine;
using System.Collections;

public class NetworkCamera : MonoBehaviour {

    // Variables for managing synced movement.
    private Vector3 syncPosition = Vector3.zero;
    private Quaternion syncRotation = Quaternion.identity;

    /// <summary>
    /// If the camera is mine, then make sure it and it's audio listener are enabled.
    /// If it is another client's camera, then disable it and it's audio listener to avoid conflicts.
    /// </summary>
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

    /// <summary>
    /// Syncs movement for cameras that aren't mine.
    /// Why are we syncing movement for someone else's camera? 
    /// Weapons are currently parented to the camera, so until we have proper models with head bones we can sync, we'll need to sync the cameras.
    /// </summary>
    void Update()
    {
        if (!networkView.isMine)
        {
            SyncedMovement();
        }
    }

    /// <summary>
    /// Manages the smooth lerping of the camera between points.
    /// </summary>
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

    /// <summary>
    /// Updates the sync position and rotation.
    /// </summary>
    private void SyncedMovement()
    {
        transform.position = syncPosition;
        transform.rotation = syncRotation;
    }
}
