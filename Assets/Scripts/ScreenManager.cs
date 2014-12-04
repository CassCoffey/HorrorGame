using UnityEngine;
using System.Collections;

public class ScreenManager : MonoBehaviour {

    public Camera camera;
    public float totalTime;
    Vector3 locationTo;
    Quaternion rotationTo;
    Quaternion startRotation;
    Vector3 startLocation;
    bool move = false;
    float curTime = 0;

	public void MoveCameraTo(GameObject Location)
    {
        // Enable the next canvas, and turn off the current one.
        Location.transform.GetComponentInChildren<Canvas>().enabled = true;
        camera.transform.parent.GetComponentInChildren<Canvas>().enabled = false;
        camera.transform.SetParent(Location.transform);

        locationTo = Location.transform.position;
        rotationTo = Location.transform.rotation;
        startLocation = camera.transform.position;
        startRotation = camera.transform.rotation;
        move = true;
    }

    void Update()
    {
        if (move)
        {
            float time = curTime / totalTime;
            if (time >= 1)
            {
                time = 1;
                camera.transform.position = Vector3.Lerp(startLocation, locationTo, Mathf.SmoothStep(0, 1, time));
                camera.transform.rotation = Quaternion.Lerp(startRotation, rotationTo, Mathf.SmoothStep(0, 1, time));
                curTime = 0;
                move = false;
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