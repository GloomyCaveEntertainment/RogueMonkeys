using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward * _rotSpeed * Time.deltaTime);
	}

    [SerializeField]
    private float _rotSpeed;
}
