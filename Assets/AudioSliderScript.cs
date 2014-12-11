using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioSliderScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Slider>().value = AudioListener.volume;
	}
}
