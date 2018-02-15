/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal
 * @Date: Date
 * @Brief: Guard NPC
 * @Description: Guard animation and alarm behaviour management
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guard : MonoBehaviour {

	#region Public Data
    public enum GUARD_STATE { IDLE = 0, WARNED, WAKING_UP, ASLEEP }

	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        _img = GetComponent<Image>();
        if (_img == null)
            Debug.LogError("No img found!");
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case GUARD_STATE.IDLE:
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _frameTime)
                {
                    _frameTimer = 0f;
                    _currentFrameIndex = (_currentFrameIndex + 1) % _idleSpList.Count;
                    _img.sprite = _idleSpList[_currentFrameIndex];
                }
                break;

            case GUARD_STATE.WARNED:
                _timer += Time.deltaTime;
                if (_timer >= _warnTime)
                {
                    _state = GUARD_STATE.IDLE;
                    _frameTimer = 0f;
                }
                break;

            case GUARD_STATE.WAKING_UP:
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _frameTime)
                {
                    _frameTimer = 0f;
                    if (_currentFrameIndex < _wakeUpSpList.Count - 2)
                    {
                        ++_currentFrameIndex;
                        _img.sprite = _wakeUpSpList[_currentFrameIndex];
                    }
                }

                _timer += Time.deltaTime;
                if (_timer >= _wakeUpTime)
                {
                    GameMgr.Instance.GuardWokenUp();
                    _state = GUARD_STATE.ASLEEP;
                    _frameTimer = Random.Range(0f, _frameTime*5f); //desyn anim between guards
                    _timer = 0f;
                }
                break;

            case GUARD_STATE.ASLEEP:
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _frameTime*5f)
                {
                    _frameTimer = 0f;
                    _currentFrameIndex = (_currentFrameIndex + 1)%_asleepSpList.Count;
                    _img.sprite = _asleepSpList[_currentFrameIndex];
                }
                break;
        }
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="noiseHorizontalPt"></param>
    /// <param name="alarm"></param>
    /// <returns></returns>
    public bool CheckAlarm(float noiseHorizontalPt, float alarm = -1f)
    {
        if ((noiseHorizontalPt < GameMgr.Instance.MidFloorPos && _isLeft) ||
            (noiseHorizontalPt > GameMgr.Instance.MidFloorPos && !_isLeft))
        {
            /*if (alarm == -1f)
                Alarm();
            else
                Alarm(alarm);*/
            GameMgr.Instance.SetAlarmPs(transform.position +_alarmPsOffset);
            _state = GUARD_STATE.WARNED;
            _timer = 0f;
            _currentFrameIndex = 3;
            _img.sprite = _idleSpList[3];//freeze this sprite during warn feedback time
            return true;
        }

        return false;
        /*if (noseHorizontalPt > transform.position.x)
        {
            if (noseHorizontalPt - transform.position.x <= _alarmRadius)
            {
                Alarm();
                return true;
            }
            return false;
        }
        else
        {
            if ((transform.position.x - noseHorizontalPt) <= _alarmRadius)
            {
                Alarm();
                return true;
            }
            return false;
        }*/
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetupGuard()
    {
        _state = GUARD_STATE.IDLE;
        _timer = 0f;
        _frameTimer = Random.Range(0f, _frameTime); //desyn anim between guards
        _isLeft = (transform.position.x < GameMgr.Instance.GetCurrentLevel().FloorHeightRef.position.x);
        //Set scale and canvas order depending on spawned height
        if (transform.position.y > GameMgr.Instance._CollectorMonkey.transform.position.y)
        {
            transform.localScale = Vector3.one * GameMgr._farGuardCanvasScaleRatio;
            transform.parent = GameMgr.Instance.FarGuardCanvas;
        }
        else
        {
            transform.localScale = Vector3.one;
            transform.parent = GameMgr.Instance.FrontGuardCanvas;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void WakeUp()
    {
        if (_state != GUARD_STATE.WAKING_UP)
        {
            _state = GUARD_STATE.WAKING_UP;
            _frameTimer = Random.Range(0f, _frameTime); //desyn anim between guards
            _currentFrameIndex = 0;
        }
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /*private void Alarm()
    {
        ShowAlarmFeedback();
        if (GameMgr.Instance.RaiseAlarm(_alarmLvlIncrement))
            WakeUp();
    }

    private void Alarm(float alm)
    {
        ShowAlarmFeedback();
        if (GameMgr.Instance.RaiseAlarm(alm))
            WakeUp();
    }*/

    private void ShowAlarmFeedback()
    {

    }

    
	#endregion


	#region Properties

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private float _alarmLvlIncrement;       //amount incremented when a fruit is missed while falling from tree

    //Animation
    [SerializeField]
    private List<Sprite> _idleSpList;
    [SerializeField]
    private List<Sprite> _wakeUpSpList;
    [SerializeField]
    private List<Sprite> _asleepSpList;
    [SerializeField]
    private float _frameTime;
    [SerializeField]
    private float _alarmRadius; //to check if a fallen fruit alarms the guard

    [SerializeField]
    private float _wakeUpTime;

    [SerializeField]
    private Vector3 _alarmPsOffset;
    [SerializeField]
    private float _warnTime;
    #endregion

    #region Private Non-serialized Fields
    private GUARD_STATE _state;

    private Image _img;
    private float _frameTimer, _timer;
    private int _currentFrameIndex;
    private bool _isLeft;
	#endregion
}
