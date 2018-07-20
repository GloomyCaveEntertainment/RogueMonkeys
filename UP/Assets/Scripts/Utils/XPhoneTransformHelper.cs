using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPhoneTransformHelper : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX && _parent == null)
        {
            RectTransform rt = GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(_left, _bottom);
            rt.offsetMax = new Vector2(-_right, _top);
        }
        
    }
    void Start()
    {
        if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX && _parent != null)
        {
            RectTransform rt = GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(_left, _bottom);
            rt.offsetMax = new Vector2(-_right, _top);
        }
    }

    [SerializeField]
    private float _left, _top, _right, _bottom;
    [SerializeField]
    private XPhoneTransformHelper _parent;
}
