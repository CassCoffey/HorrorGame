using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ScreenManager : MonoBehaviour {

    public Camera camera;
    public float totalTime;
    Vector3 locationTo;
    Quaternion rotationTo;
    Quaternion startRotation;
    Vector3 startLocation;
    private Canvas previousCanvas;
    private Canvas nextCanvas;
    bool move = false;
    float curTime = 0;

    void Start()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            canvas.enabled = false;
        }
        camera.transform.parent.GetComponentInChildren<Canvas>().enabled = true;
        camera.transform.parent.GetComponentInChildren<Canvas>().worldCamera = camera;
        camera.transform.parent.GetComponentInChildren<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        camera.transform.parent.GetComponentInChildren<Canvas>().planeDistance = 6;
        if (camera.transform.parent.GetComponent<Animator>() != null)
        {
            camera.transform.parent.GetComponent<Animator>().enabled = true;
        }
    }

	public void MoveCameraTo(GameObject Location)
    {
        // Enable the next canvas, and turn off the current one.
        if (camera.transform.parent.GetComponent<Animator>() != null)
        {
            camera.transform.parent.GetComponent<Animator>().enabled = false;
        }
        previousCanvas = camera.transform.parent.GetComponentInChildren<Canvas>();
        previousCanvas.renderMode = RenderMode.WorldSpace;
        nextCanvas = Location.transform.GetComponentInChildren<Canvas>();
        nextCanvas.enabled = true;
        camera.transform.SetParent(Location.transform);

        locationTo = Location.transform.position;
        rotationTo = Location.transform.rotation;
        startLocation = camera.transform.position;
        startRotation = camera.transform.rotation;
        move = true;
    }

    public void SetPanel(GameObject panel, bool isOpen)
    {
        panel.GetComponent<Animator>().SetBool("Open", isOpen);
        foreach (UnityEngine.UI.Button button in panel.GetComponentsInChildren<UnityEngine.UI.Button>())
        {
            button.enabled = isOpen;
        }
    }

    public void TogglePanel(GameObject panel)
    {
        SetPanel(panel, !panel.GetComponent<Animator>().GetBool("Open"));
    }

    public void ClosePanel(GameObject panel)
    {
        SetPanel(panel, false);
    }

    public IEnumerator SendMessage()
    {
        yield return 0;
        foreach (var go in FindObjectsOfType<GameObject>())
        {
            go.SendMessage("OnCameraArrive", nextCanvas.transform.parent.gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }

    void Update()
    {
        if (move)
        {
            float time = curTime / totalTime;
            if (time >= 1)
            {
                previousCanvas.enabled = false;
                time = 1;
                camera.transform.position = Vector3.Lerp(startLocation, locationTo, Mathf.SmoothStep(0, 1, time));
                camera.transform.rotation = Quaternion.Lerp(startRotation, rotationTo, Mathf.SmoothStep(0, 1, time));
                curTime = 0;
                move = false;
                nextCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                nextCanvas.planeDistance = 6;
                if (camera.transform.parent.GetComponent<Animator>() != null)
                {
                    camera.transform.parent.GetComponent<Animator>().enabled = true;
                }
                StartCoroutine(SendMessage());
            }
            else
            {
                camera.transform.position = Vector3.Lerp(startLocation, locationTo, Mathf.SmoothStep(0, 1, time));
                camera.transform.rotation = Quaternion.Lerp(startRotation, rotationTo, Mathf.SmoothStep(0, 1, time));
                curTime += Time.deltaTime;
            }
        }
    }
}