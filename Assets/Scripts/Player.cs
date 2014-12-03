using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float speed = 10f;
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;
    private Vector3 mousePosPrev;

    void Update()
    {
        if (networkView.isMine)
        {
            InputMovement();
            InputColorChange();
        }
        else
        {
            SyncedMovement();
        }
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncVelocity = Vector3.zero;
        if (stream.isWriting)
        {
            syncPosition = rigidbody.position;
            stream.Serialize(ref syncPosition);

            syncVelocity = rigidbody.velocity;
            stream.Serialize(ref syncVelocity);
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncStartPosition = rigidbody.position;
        }
    }

    void InputMovement()
    {
        if (Input.GetKey(KeyCode.W))
            rigidbody.MovePosition(rigidbody.position + rigidbody.transform.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.S))
            rigidbody.MovePosition(rigidbody.position - rigidbody.transform.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
            rigidbody.MovePosition(rigidbody.position + rigidbody.transform.right * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.A))
            rigidbody.MovePosition(rigidbody.position - rigidbody.transform.right * speed * Time.deltaTime);
    }

    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;
        rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    }

    private void InputColorChange()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ChangeColorTo(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
    }

    [RPC]
    void ChangeColorTo(Vector3 color)
    {
        renderer.material.color = new Color(color.x, color.y, color.z, 1f);

        if (networkView.isMine)
            networkView.RPC("ChangeColorTo", RPCMode.OthersBuffered, color);
    }
}