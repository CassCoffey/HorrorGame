using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ScreenManager : MonoBehaviour {

    public Camera mainCamera;
    public float totalTime;

    private Vector3 locationTo;
    private Quaternion rotationTo;
    private Quaternion startRotation;
    private Vector3 startLocation;
    private Canvas previousCanvas;
    private Canvas nextCanvas;
    private bool move = false;
    private float curTime = 0;

    /// <summary>
    /// Disables all other canvases except the current one. If the current canvas has an animation, play it.
    /// </summary>
    void Start()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            canvas.enabled = false;
        }
        mainCamera.transform.parent.GetComponentInChildren<Canvas>().enabled = true;
        mainCamera.transform.parent.GetComponentInChildren<Canvas>().worldCamera = mainCamera;
        mainCamera.transform.parent.GetComponentInChildren<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        mainCamera.transform.parent.GetComponentInChildren<Canvas>().planeDistance = 6;
        mainCamera.transform.parent.GetComponentInChildren<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        if (mainCamera.transform.parent.GetComponent<Animator>() != null)
        {
            mainCamera.transform.parent.GetComponent<Animator>().enabled = true;
        }
    }

    /// <summary>
    /// Moves the camera to the designated location.
    /// </summary>
    /// <param name="Location">The location to move the camera to.</param>
	public void MoveCameraTo(GameObject Location)
    {
        // Enable the next canvas, and turn off the current one.
        if (mainCamera.transform.parent.GetComponent<Animator>() != null)
        {
            mainCamera.transform.parent.GetComponent<Animator>().enabled = false;
        }
        previousCanvas = mainCamera.transform.parent.GetComponentInChildren<Canvas>();
        previousCanvas.renderMode = RenderMode.WorldSpace;
        nextCanvas = Location.transform.GetComponentInChildren<Canvas>();
        nextCanvas.enabled = true;
        mainCamera.transform.SetParent(Location.transform);

        locationTo = Location.transform.position;
        rotationTo = Location.transform.rotation;
        startLocation = mainCamera.transform.position;
        startRotation = mainCamera.transform.rotation;
        move = true;
    }

    /// <summary>
    /// Sends a message to all objects when the camera arrives.
    /// </summary>
    public IEnumerator SendMessage()
    {
        yield return 0;
        foreach (var go in FindObjectsOfType<GameObject>())
        {
            go.SendMessage("OnCameraArrive", nextCanvas.transform.parent.gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// Manages lerping the camera between locations.
    /// </summary>
    void Update()
    {
        if (move)
        {
            float time = curTime / totalTime;
            if (time >= 1)
            {
                previousCanvas.enabled = false;
                time = 1;
                mainCamera.transform.position = Vector3.Lerp(startLocation, locationTo, Mathf.SmoothStep(0, 1, time));
                mainCamera.transform.rotation = Quaternion.Lerp(startRotation, rotationTo, Mathf.SmoothStep(0, 1, time));
                curTime = 0;
                move = false;
                nextCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                nextCanvas.planeDistance = 6;
                nextCanvas.transform.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                if (mainCamera.transform.parent.GetComponent<Animator>() != null)
                {
                    mainCamera.transform.parent.GetComponent<Animator>().enabled = true;
                }
                StartCoroutine(SendMessage());
            }
            else
            {
                mainCamera.transform.position = Vector3.Lerp(startLocation, locationTo, Mathf.SmoothStep(0, 1, time));
                mainCamera.transform.rotation = Quaternion.Lerp(startRotation, rotationTo, Mathf.SmoothStep(0, 1, time));
                curTime += Time.deltaTime;
            }
        }
    }
}