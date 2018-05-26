using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToTargetPos : MonoBehaviour {

	// Update is called once per frame
	void LateUpdate () {
        transform.position = _target.transform.position;
	}

    [SerializeField]
    private Transform _target;
}
