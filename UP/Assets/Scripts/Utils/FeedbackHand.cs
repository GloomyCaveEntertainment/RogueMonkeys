using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackHand : MonoBehaviour {
    [SerializeField]
    private float _alphaTime, _minAlpha;

    private Image _hand, _glow;
    private float _timer;
    private bool _ping;


    private void Start()
    {
        _timer = 0f;
        _ping = true;
        _hand = transform.GetChild(0).GetComponent<Image>();
        _glow = transform.GetChild(1).GetComponent<Image>();
    }
    private void OnEnable()
    {
        _timer = 0f;
    }
    // Update is called once per frame
    void Update () {
        transform.rotation = Quaternion.identity;
        _timer += Time.deltaTime;
        if (_timer >= _alphaTime)
        {
            _timer = 0f;
            _ping = !_ping;
        }
        _hand.color = new Color(1f, 1f, 1f, _ping ? Mathf.Lerp(_minAlpha, 1f, _timer / _alphaTime) : Mathf.Lerp(1f, _minAlpha, _timer / _alphaTime));

	}
}
