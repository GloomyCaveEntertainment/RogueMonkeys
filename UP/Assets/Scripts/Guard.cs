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
    public enum GUARD_STATE { IDLE = 0, WARNED, WAKING_UP, ASLEEP, PAUSE }

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
                    _currentFrameIndex = (_currentFrameIndex + 1)%2;    //2 frames length
                    _img.sprite = _wakeUpSpList[4+_currentFrameIndex];  //last th and 6th frames from wakeup anim
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
            GameMgr.Instance.SetAlarmWarnPs(transform.position +_alarmPsOffset);
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
        Vector2 initPos = GetComponent<RectTransform>().anchoredPosition;

        SetupTextures();
        _state = GUARD_STATE.IDLE;
        _timer = 0f;
        _frameTimer = Random.Range(0f, _frameTime); //desyn anim between guards
        _isLeft = (transform.position.x < GameMgr.Instance.GetCurrentLevel().FloorHeightRef.position.x);
        //Set scale and canvas order depending on spawned height
        Debug.Log("G comparison: " + transform.position.y + "/" + GameMgr.Instance.GetCurrentLevel().GetSceneLayout().GetStrikerStartPos().transform.position.y);
        if (transform.position.y > GameMgr.Instance.GetCurrentLevel().GetSceneLayout().GetStrikerStartPos().transform.position.y)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * GameMgr._farGuardCanvasScaleRatio, (transform.position.y / (GameMgr.Instance.GetCurrentLevel().GetSceneLayout().GetStrikerStartPos().transform.position.y + 200f)));
            transform.SetParent(GameMgr.Instance.FarGuardCanvas);           
        }
        else
        {
            transform.localScale = Vector3.one;
            transform.SetParent(GameMgr.Instance.FrontGuardCanvas);
        }
        GetComponent<RectTransform>().anchoredPosition = initPos;
        Debug.Log("Parented to: " + transform.parent);
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
            GameMgr.Instance.SetAlarmPs(transform.position + _alarmPsOffset + Vector3.up*_alarmPsOffset.y*.5f);  //1.5 times y_offset to keep on top while waking up anim
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pause"></param>
    public void Pause(bool pause)
    {
        if (pause && _state != GUARD_STATE.PAUSE)
        {
            _lastState = _state;
            _state = GUARD_STATE.PAUSE;
        }
        else if (!pause && _state == GUARD_STATE.PAUSE)
        {
            _state = _lastState;
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

    /// <summary>
    /// 
    /// </summary>
    private void SetupTextures()
    {
        if (_idleSpList == null)
            _idleSpList = new List<Sprite>();
        else
        {
            Debug.Log("guard" + GameMgr.Instance.GetCurrentStage().GetStageIndex().ToString("00") + "_sleep00");
            int guardStageIndex = GameMgr.Instance.GetCurrentStage().GetStageIndex();

            Sprite aux =  Resources.Load("guard" + guardStageIndex.ToString("00") + "_sleep00",typeof(Sprite)) as Sprite;
            if (aux == null)
                Debug.LogError("No texture found!");
            else
            {
                //Check if need to load new sprites
                if (_idleSpList.Count == 0 || (_idleSpList.Count > 0 && _idleSpList[0].name.CompareTo(aux.name)!=0))
                {
                    _idleSpList.Clear();
                    //Idle Anim: 4 frames
                    //first already loaded
                    _idleSpList.Add(aux);
                    for (int i = 1; i < 4; ++i)
                    {
                        _idleSpList.Add(Resources.Load("guard" + guardStageIndex.ToString("00") + "_sleep" + i.ToString("00"), typeof(Sprite)) as Sprite);
                        if (i == 1 || i == 3)   //2nd and 4th frame are duplicated on sleep animation
                            _idleSpList.Add(Resources.Load("guard" + guardStageIndex.ToString("00") + "_sleep" + i.ToString("00"), typeof(Sprite)) as Sprite);
                    }
                        
                    _idleSpList.Add(Resources.Load("guard" + guardStageIndex.ToString("00") + "_sleep03", typeof(Sprite)) as Sprite);//last frame duplicated
                    //Wake up anim: 6 frames
                    _wakeUpSpList.Clear();
                    for (int i = 0; i < 6; ++i)
                        _wakeUpSpList.Add(Resources.Load("guard" + guardStageIndex.ToString("00") + "_wake" + i.ToString("00"), typeof(Sprite)) as Sprite);
                }
            }
        }
        _img.sprite = _idleSpList[0];
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

    [SerializeField]
    private Sprite _wanrAlmSprite, awakeAlmSprite;
    #endregion

    #region Private Non-serialized Fields
    private GUARD_STATE _state, _lastState;

    private Image _img;
    private float _frameTimer, _timer;
    private int _currentFrameIndex;
    private bool _isLeft;
	#endregion
}
