using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockTapSwing : MonoBehaviour {

	public void Swing()
    {
        if (!LeanTween.isTweening(gameObject))
        {
            LeanTween.rotateLocal(gameObject, Vector3.forward * _swingAngle, _swingTime).setEase(_swingCurve);
            AudioController.Play("aud_locked");
        }
    }

    [SerializeField]
    private float _swingAngle, _swingTime;
    [SerializeField]
    private AnimationCurve _swingCurve;
}
