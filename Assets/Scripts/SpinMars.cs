using UnityEngine;
using System.Collections;

public class SpinMars : MonoBehaviour {

    public float speed = 4.0f;

	// Update is called once per frame
	void Update () {
        transform.Rotate(0f, speed, 0f);
	}
}
