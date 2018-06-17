/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectorMonkey : MonoBehaviour {

	#region Public Data
    public enum C_STATE { IDLE = 0, MOVING, RELOADING, FLEE, STUN, TICK, PAUSE }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        _state = C_STATE.IDLE;
        _gameMgr = GameMgr.Instance;
        _currentSpeed = _baseSpeed;
        _sack._Collector = this;
        _img = transform.GetChild(1).GetComponent<Image>();
        _leftPupilCentre = _leftPupil.transform.localPosition;
        _rightPupilCentre = _rightPupil.transform.localPosition;
        if (!_img)
            Debug.LogError("No image found!");
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case C_STATE.IDLE:
                //Sprite
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _idleSpList.Count;
                    _img.sprite = _idleSpList[_frameIndex];
                }
                break;

            case C_STATE.MOVING:
                if (_targetPosTouch.phase == TouchPhase.Canceled || _targetPosTouch.phase == TouchPhase.Ended)
                {
                    Debug.LogError("Wrong phase");
                }else
                {
                    //Sprite
                    _frameTimer += Time.deltaTime;
                    if (_frameTimer >= _gameMgr.FrameTime)
                    {
                        _frameTimer = 0f;
                        _frameIndex = (_frameIndex + 1) % _moveSpList.Count;
                        _img.sprite = _moveSpList[_frameIndex];
                    }

                    //Move left
                    if (_gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x < transform.position.x)
                    {
                        //Calculate next pos to avoid go over it
                        _nextXpos = transform.position.x - _currentSpeed * Time.deltaTime;
                        if (_nextXpos < _gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x)
                        {
                            transform.position = new Vector3(_gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x, _gameMgr.FloorYPos);
                            Stop();
                        }
                        else
                            transform.position = new Vector3(transform.position.x - _currentSpeed * Time.deltaTime, _gameMgr.FloorYPos);
                    }
                    else//Right
                    {
                        _nextXpos = transform.position.x + _currentSpeed * Time.deltaTime;
                        if (_nextXpos > _gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x)
                        {
                            transform.position = new Vector3(_gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x, _gameMgr.FloorYPos);
                            Stop();
                        }
                        else
                            transform.position = new Vector3(transform.position.x + _currentSpeed * Time.deltaTime, _gameMgr.FloorYPos);
                    }
                }
                break;

            case C_STATE.RELOADING:
                //Time
                _rTimer += Time.deltaTime;
                if (_rTimer >= _reloadTime)
                {
                    _state = C_STATE.IDLE;
                    _frameTimer = 0f;
                    _frameIndex = 0;
                    _img.sprite = _idleSpList[0];
                }
                else
                {
                    //Sprite
                    _frameTimer += Time.deltaTime;
                    if (_frameTimer >= _gameMgr.FrameTime)
                    {
                        _frameTimer = 0f;
                        _frameIndex = (_frameIndex + 1) % _reloadSpList.Count;
                        _img.sprite = _reloadSpList[_frameIndex];
                    }                    
                }
                break;

            case C_STATE.FLEE:
                //Sprite
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime*0.5f)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _fleeSpList.Count;
                    _img.sprite = _fleeSpList[_frameIndex];
                }
                transform.position -= Vector3.right * _fleeSpeed * Time.deltaTime;
                break;

            case C_STATE.STUN:
                _frameTimer += Time.deltaTime * 2f; //TOCHECK
                _stunTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _stunSpList.Count;
                    _img.sprite = _stunSpList[_frameIndex];
                }
                if (_stunTimer >= _currentStunTime)
                {
                    SetIdle();
                }
                break;

            case C_STATE.TICK:
                //Sprite
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime*0.5f) //2x frame speed
                {
                    _frameTimer = 0f;
                    ++_frameIndex;
                    if (_frameIndex == _tickSpList.Count)
                        SetTick(false);
                    else
                        _img.sprite = _tickSpList[_frameIndex];
                }
                break;
        }
        //Pupils
        if (_state != C_STATE.FLEE && _state != C_STATE.TICK && _state != C_STATE.STUN)
        {
            GetPupilPos();
            _leftPupil.transform.localPosition = _leftPupilCentre + _pupilOffset;
            _rightPupil.transform.localPosition = _rightPupilCentre + _pupilOffset;
        }
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter2D(Collider2D col)
    {
        if (_gameMgr.GetCurrentLevel().GetLevelState() == Level.L_STATE.FINISHED)
            return;

        if (col.CompareTag("Fruit"))
        {
            if (col.GetComponent<Fruit>()._FState == Fruit.FRUIT_ST.LAUNCHING && !_sack.TryToPushToSack(col.GetComponent<Fruit>()))
            {
                col.GetComponent<Fruit>().Dissmiss();
            }

        }
    }
	#endregion

	#region Public Methods

    /// <summary>
    /// Apply equipped items mods
    /// </summary>
    public void LoadItemsStats()
    {
        //reset to default values
        _reloadTime = _sack.ReloadTime;
        _currentReloadTime = _reloadTime;
        _currentRawSpeed = _baseSpeed;
        _currentSpeed = _baseSpeed;
        _sack.ResetMods();
        _currentCapacity =  _sack.Capacity;
        Debug.Log("RELOADING before loading items " + _currentReloadTime);
        if (_slotA != null)
            LoadSlotItem(_slotA);
        if (_slotB != null)
            LoadSlotItem(_slotB);

        //Set Sack Pt List + Capacity
        if (_slotA != null) //&&_slotA.ModTypeList.Contains(EquipmentItem.MOD_TYPE.SACK_SIZE))
            _sack.SetCapacity(_currentCapacity, _slotA.IdName);
        /*else if (_slotB != null && _slotB.ModTypeList.Contains(EquipmentItem.MOD_TYPE.SACK_SIZE))
            _sack.SetCapacity(_currentCapacity, _slotB.IdName);*/
        else
            _sack.SetCapacity(_currentCapacity, "default");
        _sack.CurrentReloadTime = _currentReloadTime;
        _sack.InitSack();
        _leftPupil.SetActive(true);
        _rightPupil.SetActive(true);
        Debug.Log("RELOADING after loading items " + _currentReloadTime);
        
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="speedLoss"></param>
    public void ReduceSpeed(float speedLoss)
    {
        _currentSpeed = _currentRawSpeed - speedLoss;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop()
    {
        _state = C_STATE.IDLE;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetTouchPos"></param>
    public void Move(Touch targetTouchPos)
    {
        if (_state == C_STATE.RELOADING || _state == C_STATE.STUN)
            return;

        _state = C_STATE.MOVING;
        _targetPosTouch = targetTouchPos;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reload(/*float reloadTime*/)
    {
        _state = C_STATE.RELOADING;
        _frameTimer = 0f;
        _frameIndex = 0;
        //_reloadTime = reloadTime;
    }

    public void Flee()
    {
        //TODO
        Debug.Log("Flee! - collector");
        _frameTimer = 0f;
        _state = C_STATE.FLEE;
        _frameIndex = 0;
        _img.sprite = _fleeSpList[0];
        _leftPupil.SetActive(false);
        _rightPupil.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void _Reset()
    {
        _currentRawSpeed = _baseSpeed;
        _state = C_STATE.IDLE;
        _frameTimer = 0f;
        _frameIndex = 0;
        _img.sprite = _idleSpList[0];
        _sack.ResetSack();
        
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stun()
    {
        if (_state == C_STATE.STUN || _state == C_STATE.FLEE)
            return;

        if (_gameMgr.RaiseAlarm(Fruit._sBreakerAlarmLvl))
            GameMgr.Instance.WakeUpGuards();

        _state = C_STATE.STUN;
        _frameIndex = 0;
        _frameTimer = 0f;
        _stunTimer = 0f;
        _currentStunTime = _stunTime / (_currentSpeed / _baseSpeed);
        _img.sprite = _stunSpList[0];

        _sack.SackBroken();
        AudioController.Play("aud_fr_stun");
        if (DataMgr.Instance.Vibration == 1)
            Vibration.Vibrate((long)_currentStunTime);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enabled"></param>
    public void SetTick(bool enabled)
    {
        _leftPupil.SetActive(!enabled);
        _rightPupil.SetActive(!enabled);
        if (enabled)
        {
            _state = C_STATE.TICK;
            _frameTimer = 0f;
            _frameIndex = 0;
            _img.sprite = _tickSpList[0];
        }
        else
            SetIdle();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pause"></param>
    public void Pause(bool pause)
    {
        if (pause && _state != C_STATE.PAUSE)
        {
            _lastState = _state;
            _state = C_STATE.PAUSE;
        }
        else if (!pause && _state == C_STATE.PAUSE)
        {
            _state = _lastState;
        }
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="eI"></param>
    private void LoadSlotItem(EquipmentItem eI)
    {
        for (int i=0; i< eI.ModTypeList.Count; ++i)
        //foreach (EquipmentItem.MOD_TYPE mt in eI.ModTypeList)
        {
            switch (eI.ModTypeList[i])
            {
                case EquipmentItem.MOD_TYPE.SACK_SIZE:
                    _currentCapacity += Mathf.RoundToInt(eI.ModValueList[i]);
                    //TODO: collider size + sprite?
                    //fixed sacks?-> no
                    break;

                case EquipmentItem.MOD_TYPE.RELOAD_SPEED:
                    _currentReloadTime += _reloadTime * eI.ModValueList[i];
                    //_sack.CurrentReloadTime -= _sack.ReloadTime * eI.ModValueList[i];
                    break;

                case EquipmentItem.MOD_TYPE.COLLECTOR_SPEED:
                    _currentSpeed += _baseSpeed * eI.ModValueList[i];
                    //_currentSpeed = _baseSpeed * (1f + eI.ModValueList[i]);
                    _currentRawSpeed = _currentSpeed;
                    break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetPupilPos()
    {
        Vector2 minPos = Vector2.one * Mathf.Infinity;
        float  tempDist;
        Fruit closestFr = null;

        _minPupilDist = Mathf.Infinity;
        //Get closest fruit
        foreach (GameObject fO in GameObject.FindGameObjectsWithTag("Fruit"))
        {
            if (fO.GetComponent<Fruit>()._FState == Fruit.FRUIT_ST.LAUNCHING)
            {
                tempDist = Vector2.Distance(_pupilCentreRef.position, fO.transform.position);
                if (tempDist < _minPupilDist)
                {
                    _minPupilDist = tempDist;
                    closestFr = fO.GetComponent<Fruit>();   
                }
            }
        }
        if (_minPupilDist != Mathf.Infinity)
            _pupilOffset =  ((Vector2)closestFr.transform.position - (Vector2)_pupilCentreRef.position).normalized * _pupilRadius;
        else
            _pupilOffset = Vector2.zero;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetIdle()
    {
        _frameIndex = 0;
        _frameTimer = 0f;
        _img.sprite = _idleSpList[0];
        _collectorSack.transform.localPosition = new Vector3(_collectorSack.transform.localPosition.x, _sack.InitSackPos.y, _collectorSack.transform.localPosition.z);
        _state = C_STATE.IDLE;
    }

	#endregion


	#region Properties
    public float _ReloadTime { get { return _reloadTime; } set { _reloadTime = value; } }
    public float _StunTime { get { return _stunTime; } set { _stunTime = value; } }
    public Sack _Sack { get { return _sack; } set { _sack = value; } }
    public EquipmentItem SlotA { get { return _slotA; } set { _slotA = value; } }
    public EquipmentItem SlotB { get { return _slotB; } set { _slotB = value; } }
    public GameObject _CollectorSack { get { return _collectorSack; } set { _collectorSack = value; } }
    public C_STATE _State {  get { return _state; } private set { _state = value; } }

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private Sack _sack;
    [SerializeField]
    private float _baseSpeed;

    //Animation
    [SerializeField]
    private List<Sprite> _reloadSpList;
    [SerializeField]
    private List<Sprite> _idleSpList;
    [SerializeField]
    private List<Sprite> _moveSpList;
    [SerializeField]
    private List<Sprite> _fleeSpList;
    [SerializeField]
    private List<Sprite> _stunSpList;
    [SerializeField]
    private List<Sprite> _tickSpList;

    [SerializeField]
    private float _fleeSpeed;

    [SerializeField]
    private GameObject _leftPupil, _rightPupil;
    [SerializeField]
    private float _pupilRadius, _pupilMovTime;
    [SerializeField]
    private Transform _pupilCentreRef;

    [SerializeField]
    private float _stunTime;

    [SerializeField]
    private GameObject _collectorSack;
    #endregion

    #region Private Non-serialized Fields
    private C_STATE _state, _lastState;
    private GameMgr _gameMgr;
    private Image _img;
    private Touch _targetPosTouch;      //the sliding thumb the collector follows
    private float _currentSpeed;
    private float _currentRawSpeed;     //current sppeed with equipped items. Used to calculate _currentSpeed when applying SpeedLoss
    private float _nextXpos;
    

    //Reload
    private float _frameTimer;
    private int _frameIndex;

    private float _rTimer;
    private float _reloadTime;
    private float _currentReloadTime;
    private float _currentCapacity;
    //Items
    private EquipmentItem _slotA, _slotB;

    private Vector2 _pupilOffset;
    private float _minPupilDist;
    private Vector2 _leftPupilCentre,_rightPupilCentre;

    private float _stunTimer;
    private float _currentStunTime;
    #endregion
}
