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
public class StrikerMonkey : MonoBehaviour {

	#region Public Data
    public enum MONKEY_STATE { IDLE = 0, MOVING_TO_POS, HIT_ANIMATION, RECOVERING_TO_FLOOR, FLEE, STUN }
    public enum S_BOOST_STATE { IDLE = 0, SPEED_UP, SLOWMOTION }
    public enum SHADOWING_STATE { DISABLED = 0, FADE_IN, ENABLED, FADE_OUT }

    public const int _maxSpeedBoostStacks = 3;
    
	#endregion


	#region Behaviour Methods

	// Use this for initialization
	void Start () {
		_img = GetComponent<Image>();
        if (_img == null)
            Debug.LogError("No img!");
        _gameMgr = GameMgr.Instance;
        if (!_gameMgr)
            Debug.LogError("No GameMgr found!");
        _currentJumpSpeed = _jumpSpeed;
        _currentRecoverSpeed = _recoverSpeed;
        _currentHitTime = _hitTime;
        _jumpOffset = -Vector2.up *_img.rectTransform.rect.height*transform.lossyScale.y;

    }
	

	// Update is called once per frame
	void Update () {
        //Monkey Animation
        if (_gameMgr.SlowMoEnabled)
            _frameTimer = Time.deltaTime / GameMgr._slowMoTimeScale;
        else
            _frameTimer += Time.deltaTime;

        //SpeedBoost
        if (_sBoostSt == S_BOOST_STATE.SPEED_UP)
        {
            _speedBoostTimer -= Time.deltaTime;
            if (_speedBoostTimer <= 0f)
                SpeedBoost(false);
            else
                UIHelper.Instance.UpdateSlowMoFill(_speedBoostTimer / _speedBoostDur);
        }

        switch (_state)
        {
            case MONKEY_STATE.IDLE:
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _idleSpList.Count;
                    _img.sprite = _idleSpList[_frameIndex];
                    _img.SetNativeSize();
                }
                
                break;

            case MONKEY_STATE.MOVING_TO_POS:
                //Animation
                if (_frameIndex < _movSpList.Count && _frameTimer >= _jumpFrameTime/*_gameMgr.FrameTime*/)
                {
                    _frameTimer = 0f;
                    ++_frameIndex;// = (_frameIndex + 1) % _movSpList.Count;
                    if (_frameIndex < _movSpList.Count)
                    {
                        _img.sprite = _movSpList[_frameIndex];
                        _img.SetNativeSize();
                    }
                }
                
 
                if (_gameMgr.SlowMoEnabled) //Check again in case we stoped slowmotion
                    _nextPos +=  /*(Vector2)transform.position +*/  (_targetPos - (Vector2)transform.position).normalized * (Time.deltaTime / GameMgr._slowMoTimeScale) * _currentJumpSpeed;
                else
                    _nextPos +=  /*(Vector2)transform.position +*/  (_targetPos - (Vector2)transform.position).normalized * Time.deltaTime * _currentJumpSpeed;

                //Sprite pos, to prevent graphically going beneath floorYRef
                if (_nextPos.y < _gameMgr.FloorYPos)
                    _spritePos = new Vector2(_nextPos.x, _gameMgr.FloorYPos);
                else if (_nextPos.y >= _targetPos.y)
                {
                    //_nextPos = _targetPos;
                    _spritePos = _targetPos;
                }
                else
                {
                    _spritePos = _nextPos;
                }
                //Debug.Log("Sprite pos: " + _spritePos);

                if ((((_targetPos.x - _nextPos.x > 0f) && _movDirVector.x < 0f) || ((_targetPos.x - _nextPos.x < 0f) && _movDirVector.x > 0f)) ||
                    (((_targetPos.y - _nextPos.y > 0f) && _movDirVector.y < 0f) || ((_targetPos.y - _nextPos.y < 0f) && _movDirVector.y > 0f)))
                {
                    PerformHit();
                }
                else
                    transform.position = _spritePos;

                break;

            case MONKEY_STATE.HIT_ANIMATION:
                if (_gameMgr.SlowMoEnabled)
                    _hitTimer += Time.deltaTime * (_currentJumpSpeed/_jumpSpeed)/ GameMgr._slowMoTimeScale;
                else
                    _hitTimer += Time.deltaTime * (_currentJumpSpeed / _jumpSpeed);

                transform.Rotate(Vector3.forward * (360f / _currentHitTime) * Time.deltaTime * (_currentJumpSpeed / _jumpSpeed));
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _hitSpList.Count;
                    _img.sprite = _hitSpList[_frameIndex];
                    _img.SetNativeSize();
                }
                if (_hitTimer >= _currentHitTime)
                    RecoverToFloor();

                break;

            case MONKEY_STATE.RECOVERING_TO_FLOOR:
                //Sprite
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _recoveringSpList.Count;
                    _img.sprite = _recoveringSpList[_frameIndex];
                }

                //_timer += Time.deltaTime;
                if (_gameMgr.SlowMoEnabled)
                    _nextPos = (Vector2)transform.position - _currentRecoverSpeed * Vector2.up * (Time.deltaTime / GameMgr._slowMoTimeScale);
                else
                    _nextPos = (Vector2)transform.position - _currentRecoverSpeed * Vector2.up * Time.deltaTime;

                if (_nextPos.y <= _gameMgr.FloorYPos)
                {
                    transform.position = new Vector2(transform.position.x, _gameMgr.FloorYPos);
                    SetIdle();
                }
                else
                    transform.position = _nextPos;
                break;

            case MONKEY_STATE.FLEE:
                //Sprite
                if (_frameTimer >= _gameMgr.FrameTime*0.5f)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _fleeSpList.Count;
                    _img.sprite = _fleeSpList[_frameIndex];
                }

                transform.position += Vector3.right * _fleeSpeed * Time.deltaTime;
                break;

            case MONKEY_STATE.STUN:

                if (_gameMgr.SlowMoEnabled)
                {
                    _stunTimer += Time.deltaTime / GameMgr._slowMoTimeScale;
                    _frameTimer += Time.deltaTime / GameMgr._slowMoTimeScale;  //2x Speed, (1st before switch)
                }
                else
                {
                    _stunTimer += Time.deltaTime;
                    _frameTimer += Time.deltaTime;
                }
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _stunSpList.Count;
                    _img.sprite = _stunSpList[_frameIndex];
                    _img.SetNativeSize();
                }
                if (_stunTimer >= _currentStunTime)
                    RecoverToFloor();
                break;
        }

        if (_showingHitFeedback)
        {
            _hitFeedbackTimer += Time.deltaTime;
            if (_hitFeedbackTimer >= _hitFeedbackImgTime)
            {
                _showingHitFeedback = false;
                //_hitFeedbackPs.gameObject.SetActive(false);
            }
        }

        //Shadowing
        if (_shadowSt != SHADOWING_STATE.DISABLED)
        {
            _shadowTimer += (Time.deltaTime / GameMgr._slowMoTimeScale);
            if (_shadowTimer >= _shadowUpdateTime)
            {
                _shadowTimer = 0f;
                switch (_shadowSt)
                {
                    case SHADOWING_STATE.FADE_IN:
                        for (int i = _currentShadowCount; i > 0; --i)
                        {
                            _shadowImageList[i].gameObject.transform.position = _shadowImageList[i - 1].gameObject.transform.position;
                            _shadowImageList[i].sprite = _shadowImageList[i - 1].sprite;
                            _shadowImageList[i].SetNativeSize();
                        }
                        _shadowImageList[0].gameObject.transform.position = transform.position;
                        _shadowImageList[0].sprite = _img.sprite;
                        _img.SetNativeSize();

                        //Set new shadow img in the same player render layer, behind him

                        _shadowImageList[_currentShadowCount].gameObject.SetActive(true);

                        if (_currentShadowCount == _shadowMaxCount - 1)
                            _shadowSt = SHADOWING_STATE.ENABLED;
                        else
                            ++_currentShadowCount;

                        UpdateShadowOpacity();

                        break;

                    case SHADOWING_STATE.ENABLED:
                        for (int i = _currentShadowCount; i > 0; --i)
                        {
                            _shadowImageList[i].gameObject.transform.position = _shadowImageList[i - 1].gameObject.transform.position;
                            _shadowImageList[i].sprite = _shadowImageList[i - 1].sprite;
                            _shadowImageList[i].SetNativeSize();
                        }

                        _shadowImageList[0].gameObject.transform.position = transform.position;
                        _shadowImageList[0].sprite = _img.sprite;
                        _shadowImageList[0].SetNativeSize();
                        UpdateShadowOpacity();
                        break;

                    case SHADOWING_STATE.FADE_OUT:
                        for (int i = _currentShadowCount; i > 0; --i)
                        {
                            _shadowImageList[i].gameObject.transform.position = _shadowImageList[i - 1].gameObject.transform.position;
                            _shadowImageList[i].sprite = _shadowImageList[i - 1].sprite;
                            _shadowImageList[i].SetNativeSize();
                        }

                        _shadowImageList[0].gameObject.transform.position = transform.position;
                        _shadowImageList[0].sprite = _img.sprite;
                        _shadowImageList[0].SetNativeSize();

                        _shadowImageList[_currentShadowCount].gameObject.SetActive(false);
                        --_currentShadowCount;

                        UpdateShadowOpacity();
                        if (_currentShadowCount == -1)
                            _shadowSt = SHADOWING_STATE.DISABLED;
                        break;

                    case SHADOWING_STATE.DISABLED:
                        //Do nothing
                        break;
                }
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
        _currentCollisionRadius = _collisionRadius;
        _currentRecoverSpeed = _recoverSpeed;
        _currentJumpSpeed = _jumpSpeed;
        _currentHitTime = _hitTime;
        _currentAccuracy = _accuracy;

        if (_slotA != null)
            LoadSlotItem(_slotA);
        if (_slotB != null)
            LoadSlotItem(_slotB);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    public void MoveToHit(Vector2 position)
    {
        float angle = 0f, rotAngle = 0f;
        if (_state != MONKEY_STATE.IDLE)
            return;
        Debug.Log("mov to pos: " + position);
        _frameTimer = 0f;
        _frameIndex = 0;
        _img.sprite = _movSpList[0];
        _img.SetNativeSize();
        _state = MONKEY_STATE.MOVING_TO_POS;
        //if (position.y + _jumpOffset.y <= _gameMgr.FloorYPos)
        //_targetPos = new Vector2(position.x + _jumpOffset.x, _gameMgr.FloorYPos);
        //else
        if (position.x > transform.position.x)
        {
            rotAngle = -Vector2.Angle(Vector2.up, (position - (Vector2)transform.position).normalized);
            angle = Vector2.Angle(Vector2.right, (position - (Vector2)transform.position).normalized);
            _jumpOffset = new Vector2(-_img.rectTransform.rect.height * transform.lossyScale.y * Mathf.Cos(Mathf.Abs(angle) * Mathf.PI / 180f), -_img.rectTransform.rect.height * transform.lossyScale.y * Mathf.Sin(Mathf.Abs(angle) * Mathf.PI / 180f));
        }
        else
        {
            rotAngle = Vector2.Angle(Vector2.up, (position - (Vector2)transform.position).normalized);
            angle = Vector2.Angle(Vector2.left, (position - (Vector2)transform.position).normalized);
            _jumpOffset = new Vector2(_img.rectTransform.rect.height * transform.lossyScale.y * Mathf.Cos(Mathf.Abs(angle) * Mathf.PI / 180f), -_img.rectTransform.rect.height * transform.lossyScale.y * Mathf.Sin(Mathf.Abs(angle) * Mathf.PI / 180f));
        }
        _hitPos = position;
        _targetPos = position + _jumpOffset;
        _movDirVector = (_targetPos - (Vector2)transform.position).normalized;
        _jumpFrameTime = Mathf.Abs(_targetPos.x - transform.position.x) / (_jumpSpeed*_movSpList.Count);
        //Debug.Log("Rot before: " + transform.rotation.eulerAngles);
        Debug.Log("ROT: " +rotAngle+"movDirector:"+ _movDirVector);
        //if (_movDirVector.x > 0f)
            transform.Rotate(new Vector3(0f, 0f, rotAngle));
        //else
           // transform.Rotate(new Vector3(0f, 0f, Vector2.Angle(Vector2.up, _movDirVector)));
        //Debug.Log("Rot after: " + transform.rotation.eulerAngles);
        _nextPos = transform.position;
        //Debug.DrawLine(transform.position, position, Color.red, 3f);
        //Debug.DrawLine(transform.position, _targetPos, Color.yellow, 3f);

        //transform.position = position;

    }

    /// <summary>
    /// 
    /// </summary>
    public void PerformHit()
    {
        _state = MONKEY_STATE.HIT_ANIMATION;
        //transform.position = position;    
        _frameTimer = 0f;
        _frameIndex = 0;
        _hitTimer = 0f;
        _img.sprite = _hitSpList[0];
        _img.SetNativeSize();
        _img.rectTransform.pivot = Vector2.one * 0.5f;  //center pivot
        transform.position = _hitPos - Vector2.up * _img.rectTransform.rect.height * 0.5f;
        if (transform.position.y < GameMgr.Instance.FloorYPos + _img.rectTransform.rect.height * 0.5f)
            transform.position = new Vector3(transform.position.x, _gameMgr.FloorYPos + _img.rectTransform.rect.height * 0.5f, transform.position.z);
        //transform.position += (Vector3)_jumpOffset;// Vector3.up * _img.rectTransform.rect.height*transform.lossyScale.y;   //new pivot offset compensation
        _hitFeedbackPs.transform.position = _hitPos;// (Vector3)_targetPos - (Vector3)_jumpOffset;// _hitOffset;
        //_hitFeedbackPs.transform.localScale = (float)(_currentCollisionRadius / _collisionRadius) * Vector3.one;
        //_hitFeedbackPs.gameObject.SetActive(true);
        _hitFeedbackPs.Play();
        _hitFeedbackTimer = 0f;
        _showingHitFeedback = true;

        _hit2DArray = Physics2D.CircleCastAll(_hitFeedbackPs.transform.position/*transform.position*/, _currentCollisionRadius, Vector2.up, 0f, LayerMask.GetMask("Fruit"));
        Debug.DrawLine(_hitFeedbackPs.transform.position/*transform.position*/, transform.position + Vector3.right * _collisionRadius, Color.red, 2f);
        //Debug.DrawRay(GameCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.up * 20f, Color.red, 5f);
        foreach (RaycastHit2D hit2D in _hit2DArray)
        {
            Debug.Log("H I T !");
            if (hit2D != null && hit2D.collider != null)
            {
                //Set monkey hitting coco on position + throw coco
                hit2D.collider.GetComponent<Fruit>().Launch();
            }
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void Flee()
    {
        //TODO
        Debug.Log("Flee! - striker");
        _frameTimer = 0f;
        _state = MONKEY_STATE.FLEE;
        _img.sprite = _fleeSpList[0];
        transform.position = new Vector3(transform.position.x, _gameMgr.FloorYPos, transform.position.z);
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetMonkey()
    {
        _state = MONKEY_STATE.IDLE;
        _frameTimer = 0f;
        _frameIndex = 0;
        _img.sprite = _idleSpList[0];
        //_hitFeedbackPs.gameObject.SetActive(false);
        _hitFeedbackTimer = 0f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetAccuracy()
    {
        return _currentAccuracy;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stun()
    {
        if (_gameMgr.RaiseAlarm(Fruit._spikyAlarmLvl))
            GameMgr.Instance.WakeUpGuards();

        _state = MONKEY_STATE.STUN;
        _frameIndex = 0;
        _frameTimer = 0f;
        _stunTimer = 0f;
        _currentStunTime = _stunTime / (_currentJumpSpeed / _jumpSpeed);
        _img.sprite = _stunSpList[0];
        AudioController.Play("aud_fr_stun");
        if (DataMgr.Instance.Vibration == 1)
            Vibration.Vibrate((long)_currentStunTime);
    }

    /// <summary>
    /// Power up effect when hitting guarana. If reahces Max lvl, Slowmotion gets enabled.
    /// </summary>
    /// <param name="raise"></param>
    public void SpeedBoost(bool raise)
    {
        Debug.Log("Speed Boost! raise: "+raise);
        switch ( _sBoostSt)
        {
            case S_BOOST_STATE.IDLE:
                ++_currentSpeedStacks;
                _currentJumpSpeed += _speedBoostStackVal;
                _currentRecoverSpeed += _speedBoostStackVal;
                _speedBoostTimer += _speedBoostStackTime;
                _sBoostSt = S_BOOST_STATE.SPEED_UP;
                EnableShadowing(_shadowsPerStack);
                UIHelper.Instance.SetSpeedUp(true);
                UIHelper.Instance.UpdateSlowMoFill(_speedBoostTimer / _speedBoostDur);
                AudioController.Play("aud_fr_SpeedUp");
                break;

            case S_BOOST_STATE.SPEED_UP:
                if (raise)
                {
                    ++_currentSpeedStacks;
                    _currentJumpSpeed += _speedBoostStackVal;
                    _currentRecoverSpeed += _speedBoostStackVal;
                    EnableShadowing(_shadowsPerStack*_currentSpeedStacks);
                    if (_currentSpeedStacks == _maxSpeedBoostStacks)
                    {
                        //Disable previous speed stacks on slowmotion; Striker standar speed & World slowed down
                        _currentJumpSpeed -= (_currentSpeedStacks * _speedBoostStackVal);
                        _currentRecoverSpeed -= (_currentSpeedStacks * _speedBoostStackVal);
                        _currentSpeedStacks = 0;
                        _sBoostSt = S_BOOST_STATE.SLOWMOTION;
                        _speedBoostTimer = _speedBoostDur;
                        _gameMgr.SetSlowMotion(true);
                    }
                    else
                    {
                        _speedBoostTimer += _speedBoostStackTime;
                        AudioController.Play("aud_fr_SpeedUp");
                    }
                }
                else
                {
                    
                    --_currentSpeedStacks;
                    _currentJumpSpeed -= _speedBoostStackVal;
                    _currentRecoverSpeed -= _speedBoostStackVal;
                    
                    if (_currentSpeedStacks == 0)
                    {
                        _sBoostSt = S_BOOST_STATE.IDLE;
                        UIHelper.Instance.SetSpeedUp(false);
                        DisableShadowing();
                    }
                    else
                        EnableShadowing(_shadowsPerStack * _currentSpeedStacks);
                }
                break;

            case S_BOOST_STATE.SLOWMOTION:
                _speedBoostTimer += _speedBoostStackVal * 0.5f;
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void StopSpeedBoost()
    {
        _sBoostSt = S_BOOST_STATE.IDLE;
        //_gameMgr.SetSlowMotion(false);
        //_currentJumpSpeed -= (_maxSpeedBoostStacks * _speedBoostStackVal);
        //_currentRecoverSpeed -= (_maxSpeedBoostStacks * _speedBoostStackVal);
        _currentSpeedStacks = 0;
        _speedBoostTimer = 0f;
        DisableShadowing();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="n"></param>
    public void EnableShadowing(int n)
    {
        Debug.Log("Enable shadowing, currentSt: " + _shadowSt);
        switch (_shadowSt)
        {
            case SHADOWING_STATE.DISABLED:
                _shadowSt = SHADOWING_STATE.FADE_IN;
                _shadowMaxCount = n;
                _currentShadowCount = 0;
                _shadowTimer = 0f;
                break;

            case SHADOWING_STATE.FADE_IN:
                _shadowMaxCount = n;
                break;

            case SHADOWING_STATE.FADE_OUT:
                _shadowSt = SHADOWING_STATE.FADE_IN;
                for (int i = 0; i < _currentShadowCount; ++i)
                    _shadowImageList[i].gameObject.SetActive(false);
                _shadowMaxCount = n;
                _currentShadowCount = 0;
                _shadowTimer = 0f;
                break;

            case SHADOWING_STATE.ENABLED:
                if (n > _shadowMaxCount)
                {
                    _shadowSt = SHADOWING_STATE.FADE_IN;
                    _shadowMaxCount = n;

                }
                else if (n < _shadowMaxCount)
                {
                    for (int i = n; i < _shadowMaxCount; ++i)
                        _shadowImageList[i].gameObject.SetActive(false);
                    if (n > 0)
                        _currentShadowCount = n - 1;
                    else
                        _currentShadowCount = 0;
                    _shadowMaxCount = n;
                }
                
                break;
        }
        Debug.Log("Enable shadowing, after logic currentSt: " + _shadowSt+"current, max: "+_currentShadowCount + "/"+_shadowMaxCount);
    }

    /// <summary>
    /// 
    /// </summary>
    public void DisableShadowing()
    {
        if (_shadowSt == SHADOWING_STATE.ENABLED || _shadowSt == SHADOWING_STATE.FADE_IN)
            _shadowSt = SHADOWING_STATE.FADE_OUT;

    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    private void RecoverToFloor()
    {
        transform.rotation = Quaternion.identity;
        _state = MONKEY_STATE.RECOVERING_TO_FLOOR;
        _frameIndex = 0;
        _frameTimer = 0f;
        _img.sprite = _recoveringSpList[0];
        _img.SetNativeSize();
        _img.rectTransform.pivot = Vector2.right * 0.5f;    //recover bot pivot
        transform.position += Vector3.up * _img.rectTransform.rect.height*0.5f;   //recover pivot offset compensation
        transform.rotation = Quaternion.Euler(Vector3.forward * 180f);
        //_initPos = transform.position;
        //Calculate time to get pos ecuation
        _recoveringTime = Mathf.Sqrt((-2f * (transform.position.y - GameMgr.Instance.FloorYPos)) / _recoveringGravity);
        _timer = 0f;
        Debug.Log("Recovering time is: " + _recoveringTime);
     }

    /// <summary>
    /// 
    /// </summary>
    private void SetIdle()
    {
        _state = MONKEY_STATE.IDLE;
        _frameIndex = 0;
        _frameTimer = 0f;
        _img.sprite = _idleSpList[0];
        _img.SetNativeSize();
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eI"></param>
    private void LoadSlotItem(EquipmentItem eI)
    {
        for (int i = 0; i < eI.ModTypeList.Count; ++i)
        //foreach (EquipmentItem.MOD_TYPE mt in eI.ModTypeList)
        {
            switch (eI.ModTypeList[i])
            {
                case EquipmentItem.MOD_TYPE.STRIKER_HIT_SIZE:
                    _currentCollisionRadius += _collisionRadius * eI.ModValueList[i];
                    ParticleSystem.MainModule main = _hitFeedbackPs.main;
                    main.startSize = 160f * _currentCollisionRadius / _collisionRadius; //160f default ps size
                    break;

                case EquipmentItem.MOD_TYPE.STRIKER_SPEED:
                    _currentJumpSpeed += _jumpSpeed *  eI.ModValueList[i];
                    _currentRecoverSpeed += _recoverSpeed * eI.ModValueList[i];
                    _currentHitTime -= _hitTime * eI.ModValueList[i];
                    break;

                case EquipmentItem.MOD_TYPE.ACURRACY:
                    //TODO
                    _currentAccuracy += _accuracy * eI.ModValueList[i];
                    break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateShadowOpacity()
    {
        for (int i = 0; i < _shadowImageList.Count; ++i)
        {
            _shadowImageList[i].color = new Color(_shadowImageList[i].color.r, _shadowImageList[i].color.g, _shadowImageList[i].color.b, _minShadowOpacity + (_maxShadowOpacity - _minShadowOpacity) * (_shadowMaxCount - i) / _shadowMaxCount);
        }
    }
    #endregion


    #region Properties
    public EquipmentItem SlotA { get { return _slotA; } set { _slotA = value; } }
    public EquipmentItem SlotB { get { return _slotB; } set { _slotB = value; } }
    public float BaseAccuracy { get { return _accuracy; } set { _accuracy = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private List<Sprite> _idleSpList;
    [SerializeField]
    private List<Sprite> _movSpList;
    [SerializeField]
    private List<Sprite> _hitSpList;
    [SerializeField]
    private List<Sprite> _recoveringSpList;
    [SerializeField]
    private List<GameObject> _shadowSpriteList; //sprites used as trail****
    [SerializeField]
    private List<Sprite> _fleeSpList;
    [SerializeField]
    private List<Sprite> _stunSpList;
    [SerializeField]
    private float _recoveringGravity;
    [SerializeField]
    private float _jumpSpeed;
    [SerializeField]
    private float _recoverSpeed;
    [SerializeField]
    private float _hitTime;
    [SerializeField]
    private float _stunTime;
    [SerializeField]
    private float _minArrivalOffset;
    //TODO: precision
    [SerializeField]
    private float _collisionRadius;
    [SerializeField]
    private float _accuracy;
    [SerializeField]
    private float _fleeSpeed;
    [SerializeField]
    private ParticleSystem _hitFeedbackPs;
    [SerializeField]
    private float _hitFeedbackImgTime;
    [SerializeField]
    private Vector3 _hitOffset;//offset from striker center to  hit position
    [SerializeField]
    private Vector2 _jumpOffset;

    [SerializeField]
    private float _speedBoostDur;
    [SerializeField]
    private float _speedBoostStackVal;
    [SerializeField]
    private float _speedBoostStackTime;

    [SerializeField]
    private List<Image> _shadowImageList;

    [SerializeField]
    private int _shadowsPerStack;
    
    [SerializeField]
    private float _minShadowOpacity, _maxShadowOpacity;
    [SerializeField]
    private float _shadowUpdateTime;
    #endregion

    #region Private Non-serialized Fields
    private MONKEY_STATE _state;

    //Items
    private EquipmentItem _slotA, _slotB;

    private GameMgr _gameMgr;
    private Image _img; 
    private int _frameIndex;
    private float _frameTimer;
    private float _hitTimer;
    private float _stunTimer;
    private float _currentStunTime;

    private float _currentJumpSpeed, _currentRecoverSpeed, _currentHitTime;
    private float _currentCollisionRadius;
    private float _currentAccuracy;

    private Vector2 _initPos;
    private float _recoveringTime;
    private float _timer;
    private Vector2 _targetPos;
    private RaycastHit2D[] _hit2DArray;

    private Vector2 _nextPos, _spritePos, _movDirVector;

    private float _hitFeedbackTimer;
    private bool _showingHitFeedback;

    private int _currentSpeedStacks;
    private float _speedBoostTimer;

    private S_BOOST_STATE _sBoostSt;
    private SHADOWING_STATE _shadowSt;
    private int _currentShadowCount;
    private int _shadowMaxCount;
    private float _shadowTimer;

    private float _jumpFrameTime;
    private Vector2 _hitPos;
    #endregion
}
