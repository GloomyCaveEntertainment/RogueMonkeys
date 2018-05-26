/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal
 * @Date: 2017/05/15
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Fruit : MonoBehaviour {

	#region Public Data
    public enum FRUIT_ST { IDLE = 0, FALLING_FROM_TREE, LAUNCHING, ON_SACK, DISMISSED, COLLECTED, WAITING_FOR_LAUNCH, EGG }
    /// <summary>
    /// COCO = 0
    /// BANANA = 1
    /// CACAO = 2
    /// CLUSTER_SEED = 3
    /// CLUSTER_UNIT = 4
    /// MULTI_SEED = 5
    /// MULTI_UNIT = 6
    /// SPIKY = 7
    /// COCO_M = 8
    /// COCO_S = 9
    /// RIPEN = 10
    /// SACK_BREAKER = 11
    /// SACK_BREAKER_LAUNCH = 12
    /// CHICKEN = 13
    /// SBREAKER_CLUSTER_DUO = 14
    /// GUARANA = 15
    /// BANANA_CLUSTER = 16
    /// KIWI = 17
    /// GOLD_ITEM = 18
    /// EQUIPMENT = 19                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
    /// </summary>
    public enum F_TYPE { COCO_L = 0, BANANA, CACAO, CLUSTER_SEED, CLUSTER_UNIT, MULTI_SEED, MULTI_UNIT, SPIKY, COCO_M, COCO_S, RIPEN, SACK_BREAKER, SACK_BREAKER_LAUNCH, CHICKEN, SBREAKER_CLUSTER_DUO, GUARANA, BANANA_CLUSTER, KIWI, BANANA_CLUSTER_UNIT, GOLD_ITEM, EQUIPMENT, EGG }
    public enum G_TYPE { XS = 0, S, M, L, XL, XXL, RELIC }
    public enum RIPEN_STATE { LITTLE = 0, MELLOW, MATURE, STALE}
    public const float _floorCollisionOffset = 5f;  //distance below floor reference to destroy missed fruits
    public const float _multiLaunchDelay = .2f;
    public const float _bananaUnitLaunchDelay = 0.1f;
    public const float _spikyAlarmLvl = 15f;
    public const float _sBreakerAlarmLvl = 15f;
    public const float _ripenTime_mellow = 1f;
    public const float _ripenTime_mature = 1.5f;
    public const float _ripentTime_stale = 2f;
    public const int _multiUnits = 3;
    public const int _clusterUnits = 3;
    public const int _bananaClusterUnits = 3;
    public const float _clusterUnitFlyTimeBonus = 0.2f;
    public const float _kiwiBreakerFruitFlyTimeBonus = 0.25f;
    public const float _bananaUnitFlyTimeBonus = 0.25f;
    public const float _maxClusterUnitDeviation = 0.25f;

    //Fruit score
    public const int _cocoScore_L = 5;
    public const int _cocoScore_M = 10;
    public const int _cocoScore_S = 15;
    public const int _bananaScore = 15;
    public const int _cacaoScore = 5;
    public const int _littleRipenScore = 10;
    public const int _mellowRipenScore = 20;
    public const int _matureRipenScore = 5;
    public const int _staleRipenScore = 0;
    public const int _chickenScore = 5;

    public const int _eggScoreQuality_0 = 10;
    public const int _eggScoreQuality_1 = 15;
    public const int _eggScoreQuality_2 = 20;
    public const float _eggTierOneChance = 0.5f;
    public const float _eggTierTwoChance = 0.35f;
    public const float _eggTierGoldChance = 0.15f;

    public const int _slowMoScore = 15;
    public const int _clusterScore = 10;
    public const int _kiwiScore = 20;
    public const int _goldItemScore = 25;
    public const int _equipmentItemScore = 75;

    //Egg
    public const int _maxSpawnedEggs = 4;
    public const int _maxEggQuality = 4;
    public const int _goldenEggGoldValue = 15;

    [System.Serializable]
    public class PathIndexEntry
    {
        public List<F_TYPE> _TypeList;
        public Vector2[] _Path;
    }
    #endregion


    #region Behaviour Methods
    // Use this for initialization
    void Start () {
        _img = GetComponent<Image>();
        if (_img == null)
            Debug.LogError("No attached img found");
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case FRUIT_ST.WAITING_FOR_LAUNCH:
                _waitingTimer += Time.deltaTime;
                if (_waitingTimer >= _waitCooldown)
                {
                    _state = FRUIT_ST.LAUNCHING;
                    AudioController.Play("fruit_1b");
                    //_flyingTime = 0f;
                }
                break;

            case FRUIT_ST.FALLING_FROM_TREE:
                _flyingTime += Time.deltaTime;
                transform.position = new Vector2(_initPos.x + _initSpeed.x * _flyingTime,
                                            _initPos.y + _initSpeed.y * _flyingTime + 0.5f * _virtualGravity * _flyingTime * _flyingTime);
                //Check if floor collision
                if (transform.position.y <=  GameMgr.Instance.FloorYPos - _floorCollisionOffset)
                   _fTree.DestroyFruit(this, _type != F_TYPE.SPIKY);
                break;

            case FRUIT_ST.LAUNCHING:
                _flyingTime += Time.deltaTime;
                transform.position = new Vector2(_initPos.x + _initSpeed.x * _flyingTime,
                                            _initPos.y  + _initSpeed.y * _flyingTime + 0.5f * _virtualGravity * _flyingTime * _flyingTime);
                //Check if floor collision
                if (transform.position.y <= GameMgr.Instance.FloorYPos - _floorCollisionOffset)
                    _fTree.DestroyFruit(this, _type != F_TYPE.SACK_BREAKER_LAUNCH);
                break;

            case FRUIT_ST.DISMISSED:
                _flyingTime += Time.deltaTime;
                if (_flyingTime >= _dissmissAnimationTime)
                {
                    GameMgr.Instance._FruitTree.DestroyFruit(this);
                }
                /*_flyingTime += Time.deltaTime;
                transform.position = new Vector2(_initPos.x + _initSpeed.x * _flyingTime,
                                            _initPos.y  + _initSpeed.y * _flyingTime + 0.5f * _virtualGravity * _flyingTime * _flyingTime);*/
                //Check if floor collision
                //if (transform.position.y <= GameMgr.Instance.FloorYPos - _floorCollisionOffset)
                    //_fTree.DestroyFruit(this);//
                break;

            case FRUIT_ST.ON_SACK:
                if (_type == F_TYPE.RIPEN && _ripenSt != RIPEN_STATE.STALE && (GameMgr.Instance.GetCurrentLevel().GetLevelState() == Level.L_STATE.RUN || GameMgr.Instance.GetCurrentLevel().GetLevelState() == Level.L_STATE.WAITNG_FLYING_FRUITS))
                {
                    _timer += Time.deltaTime;

                    switch (_ripenSt)
                    {
                        case RIPEN_STATE.LITTLE:
                            //Animation
                            if (_ripenTransAnim)
                                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, _ripenAnimCurve.Evaluate(_timer - _ripenTime_mellow * 0.75f) / (_ripenTime_mellow * 0.25f));
                            else if (_timer >= _ripenTime_mellow * 0.75f)
                            {
                                AudioController.Play("fr_ripen_maturity_good");
                                _ripenTransAnim = true;
                            }
                                
                            if (_timer >= _ripenTime_mellow)
                            {
                                _timer = 0f;
                                _ripenSt = RIPEN_STATE.MELLOW;
                                _img.sprite = _mellowRipenSp;
                                _currentScore = _mellowRipenScore;
                                _currentRipenTime = _ripenTime_mature * GameMgr.Instance.GetCurrentLevel().FruitSpawnTime;
                                _ripenTransAnim = false;
                            }

                            break;
                        case RIPEN_STATE.MELLOW:
                            //Animation
                            if (_ripenTransAnim)
                                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, _ripenAnimCurve.Evaluate(_timer - _ripenTime_mature * 0.75f) / (_ripenTime_mature * 0.25f));
                            else if (_timer >= _ripenTime_mature * 0.75f)
                            {
                                AudioController.Play("fr_ripen_maturity_good");
                                _ripenTransAnim = true;
                                Debug.Log("Play Audio!!");
                            }                               

                            if (_timer >= _ripenTime_mature)
                            {
                                _timer = 0f;
                                _ripenSt = RIPEN_STATE.MATURE;
                                _img.sprite = _matureRipenSp;
                                _currentScore = _matureRipenScore;
                                _currentRipenTime = _ripentTime_stale * GameMgr.Instance.GetCurrentLevel().FruitSpawnTime;
                                _ripenTransAnim = false;
                            }
                            break;
                        case RIPEN_STATE.MATURE:
                            //Animation
                            if (_ripenTransAnim)
                                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, _ripenAnimCurve.Evaluate(_timer - _ripentTime_stale * 0.75f) / (_ripentTime_stale * 0.25f));
                            else if (_timer >= _ripentTime_stale * 0.75f)
                                _ripenTransAnim = true;

                            if (_timer >= _ripentTime_stale)
                            {
                                _ripenSt = RIPEN_STATE.STALE;
                                _img.sprite = _staleRipenSp;
                                _currentScore = _staleRipenScore;
                            }
                            break;
                    }
                    
                }
                else if (_type == F_TYPE.CHICKEN && (_eggSpawnQuality < _maxSpawnedEggs) && (GameMgr.Instance.GetCurrentLevel().GetLevelState() == Level.L_STATE.RUN || GameMgr.Instance.GetCurrentLevel().GetLevelState() == Level.L_STATE.WAITNG_FLYING_FRUITS) 
                    && (GameMgr.Instance._CollectorMonkey._State != CollectorMonkey.C_STATE.STUN  && GameMgr.Instance._CollectorMonkey._State != CollectorMonkey.C_STATE.RELOADING))
                {
                    _timer += Time.deltaTime;
                    if (_chickenEggAnimation)
                    {
                        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, _ripenAnimCurve.Evaluate(_timer - _eggSpawnTime * 0.75f) / (_eggSpawnTime * 0.25f));
                    }
                    else if (_timer >= _eggSpawnTime*0.75f)
                    {
                        _chickenEggAnimation = true;
                    }
                    if (_timer >= _eggSpawnTime)
                    {
                        _chickenEggAnimation = false;
                        _timer = 0f;
                        transform.localScale = Vector3.one;
                        //Fruit newEgg =_fTree.GetEggFruit(_currentEggQuality);
                        if (_eggSpawnQuality < _maxSpawnedEggs)
                        {  
                            GameMgr.Instance._CollectorMonkey._Sack.PushEgg(this);
                            ++_eggSpawnQuality;
                            //increase golden egg time
                            if (_eggSpawnQuality == _maxSpawnedEggs-1)
                                _eggSpawnTime *= 2f;// 2f;
                            if (_eggSpawnQuality == _maxSpawnedEggs)
                                LeanTween.rotateAroundLocal(gameObject, Vector3.forward, 120f, 0.5f);
                        }
                        
                    }
                }
                else if (_type == F_TYPE.EGG && _eggSpawnAnimation)
                {
                    _timer += Time.deltaTime;
                    if (_timer >= _eggAnimTime)
                    {
                        transform.localScale = Vector3.one;
                        _eggSpawnAnimation = false;
                    }
                    else
                        transform.localScale = Vector3.one * _eggPushEggAnimCurve.Evaluate(_timer / _eggAnimTime);
                }
                break;

            case FRUIT_ST.COLLECTED:
                _flyingTime += Time.deltaTime;
                if (_flyingTime >= _collectedAnimTime)
                    GameMgr.Instance._FruitTree.DestroyFruit(this);
                break;
        }
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void StartFruit()
    {
        GetFruitFallSpeed(_fTree.CurrentFallSpeed);
        _state = FRUIT_ST.FALLING_FROM_TREE;
        if (_type != F_TYPE.SPIKY && _type != F_TYPE.SACK_BREAKER && _type != F_TYPE.SBREAKER_CLUSTER_DUO)
            LeanTween.rotateZ(gameObject, -540f, _fTree.CurrentFallSpeed/*GameMgr.Instance.FruitFallTime*/);
        else
        {
            //transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            if (_type == F_TYPE.SPIKY)
                _fallSfxAO = AudioController.Play("aud_fr_spikySackBreaker");
        }
        if (_type != F_TYPE.SPIKY)
            ++GameMgr.Instance.StageSpawnedFruitCount;
        //Analytics
        if (_type == F_TYPE.EQUIPMENT)
            AnalyticsMgr.Instance.EquipmentItemSpawned(equipmentItemRef.IdName);
        else if (_type == F_TYPE.GOLD_ITEM)
            AnalyticsMgr.Instance.GoldItemSpawned(_img.sprite.ToString());
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void Launch()
    {
        if (_state == FRUIT_ST.FALLING_FROM_TREE /*|| (_state == FRUIT_ST.LAUNCHING && _maturityLevel > 0)*/)
        {

            _initPos = new Vector2(transform.position.x, transform.position.y);
            --_maturityLevel;
            GameMgr.Instance.FruitHit();
            switch (_type)
            {
                case F_TYPE.COCO_L: case F_TYPE.COCO_M : case F_TYPE.COCO_S : case F_TYPE.RIPEN:
                    GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);
                    _state = FRUIT_ST.LAUNCHING;
                    
                    AudioController.Play("aud_fr_launch_"+GameMgr.Instance.GetLaunchAudioIndex());
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    break;

                case F_TYPE.BANANA:
                case F_TYPE.BANANA_CLUSTER_UNIT:
                    if (_maturityLevel == 0)
                    {
                        GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);
                        _state = FRUIT_ST.LAUNCHING;
                    }
                    else
                    {
                        //keep falling state
                        GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime, false);  
                        //switch sprite
                        _img.sprite = _yellowBananaSp;
                    }
                    AudioController.Play("aud_fr_launch_" + GameMgr.Instance.GetLaunchAudioIndex());
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    break;

                case F_TYPE.CACAO:
                    GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);
                    _state = FRUIT_ST.LAUNCHING;
                    AudioController.Play("fruit_1b");
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    break;

                case F_TYPE.CLUSTER_SEED:
                    Fruit clusterUnit = _fTree.GetClusterUnit();
                    clusterUnit.LaunchClusterUnitFruit(transform.position, _initPos, 0f);
                    float targetXPos = clusterUnit._targetXPos;
                    for (int i = 1; i < _clusterUnits; ++i)
                    {
                        /*Fruit*/ clusterUnit =_fTree.GetClusterUnit();
                        clusterUnit.LaunchClusterUnitFruit(transform.position, _initPos, i*_clusterUnitFlyTimeBonus, targetXPos);

                        AudioController.Play("aud_fr_launch_" + GameMgr.Instance.GetLaunchAudioIndex());
                        if (DataMgr.Instance.Vibration == 1)
                            Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    }
                    //Destroy seed
                    _fTree.DestroyFruit(this);
                    //Add the additional spawned fruit to count (1 already count on seed spawn)
                    GameMgr.Instance.StageSpawnedFruitCount+=(_clusterUnits-1);
                    break;

                case F_TYPE.MULTI_SEED:
                    //Launch 1st seed and then create others with same speed
                    
                    Fruit multiUnit = _fTree.GetMultiUnit();
                    multiUnit.LaunchMultiUnitFruit(transform.position, _initPos, _virtualGravity, _initSpeed,/*_initSpeed,*/ 0f);
                    AudioController.Play("aud_fr_launch_" + GameMgr.Instance.GetLaunchAudioIndex());
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    //Get speed and gravity from frist to apply the same to others
                    Vector2 initSpeed = multiUnit._InitSpeed;
                    float vGrav = multiUnit._virtualGravity;
                    float fHeight = multiUnit.CurrentFruitHeight;
                    float targetPos = multiUnit._targetXPos;
                    Debug.Log("init speeeeeeeeeeeeed: " + _virtualGravity);
                    for (int i = 0; i < _multiUnits - 1; ++i)
                    {
                        multiUnit = _fTree.GetMultiUnit();
                        multiUnit.LaunchMultiUnitFruit(transform.position, _initPos, vGrav, initSpeed,/*unitsSpeed,*/ _multiLaunchDelay *(i+1), fHeight, targetPos);

                    }
                    //Destroy seed
                    _fTree.DestroyFruit(this);
                    //Add the additional spawned fruit to count (1 already count on seed spawn)
                    GameMgr.Instance.StageSpawnedFruitCount += (_multiUnits - 1);
                    break;

                case F_TYPE.SPIKY:
                    GameMgr.Instance._StrikerMonkey.Stun();
                    GameMgr.Instance.AlarmWarnAtPos(transform.position.x, _spikyAlarmLvl);
                    /*if (GameMgr.Instance.RaiseAlarm(_spikyAlarmLvl))
                        GameMgr.Instance.WakeUpGuards();*/
                    ThrowOut();
                    break;

                case F_TYPE.SACK_BREAKER:
                    _img.sprite = _kiwiLaunchSp;
                    _state = FRUIT_ST.LAUNCHING;
                    _type = F_TYPE.SACK_BREAKER_LAUNCH;
                    GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);                    
                    _fallSfxAO = AudioController.Play("aud_fr_spikySackBreaker");
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    GetComponent<PolygonCollider2D>().SetPath(0, _pathIndexList.Find((e) => (e._TypeList.Contains(_type)))._Path);
                    //breaker gets succeeded on launch hit instead on collect
                    ++GameMgr.Instance.StageSucceededFruitCount;
                    break;

                case F_TYPE.CHICKEN:
                    //_img.sprite = _chickenLaunchSp;
                    GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);
                    _eggSpawnQuality = 0;
                    _state = FRUIT_ST.LAUNCHING;
                    AudioController.Play("aud_fr_chicken_launch_0");
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    break;

                case F_TYPE.SBREAKER_CLUSTER_DUO:
                    Fruit kiwiSpawned = null;

                    //(1)Sack Breaker
                    _img.sprite = _kiwiLaunchSp;
                    _state = FRUIT_ST.LAUNCHING;
                    _type = F_TYPE.SACK_BREAKER_LAUNCH;
                    
                    GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);    //sack breaker moves slower than kiwi                              
                    _fallSfxAO = AudioController.Play("aud_fr_spikySackBreaker");
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    GetComponent<PolygonCollider2D>().SetPath(0, _pathIndexList.Find((e) => (e._TypeList.Contains(F_TYPE.SACK_BREAKER_LAUNCH)))._Path);

                    //(2)Kiwi Fruit
                    kiwiSpawned = _fTree.GetKiwiFruit();
                    kiwiSpawned.transform.position = transform.position;
                    kiwiSpawned.LaunchKiwiFruit(transform.position, _initPos, _fTree.CurrentFruitFlyTime * (1f + _kiwiBreakerFruitFlyTimeBonus) , GameMgr.Instance.MaxCocoHeight*1.2f); //TODO: Magic numbers, 20% slower to avoid collision with s_breaker

                    //breaker duo gets succeeded on launch hit instead on collect (and spawned kiwi gets succeeded on collect)
                    ++GameMgr.Instance.StageSucceededFruitCount;
                    break;

                case F_TYPE.GUARANA:
                    GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);
                    AudioController.Play("aud_fr_guarana");
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    _state = FRUIT_ST.LAUNCHING;
                    GameMgr.Instance._StrikerMonkey.SpeedBoost(true);
                    
                    break;

                case F_TYPE.BANANA_CLUSTER:
                    for (int i = 0; i < _bananaClusterUnits; ++i)
                    {

                        Fruit bananaUnit = _fTree.GetBananaUnit();
                        bananaUnit.LaunchBananaClusterUnitFruit(transform.position, _initPos, (i+1) * _bananaUnitFlyTimeBonus);
                        bananaUnit._FState = FRUIT_ST.FALLING_FROM_TREE;
                        /*Fruit bananaUnit = _fTree.GetBananaUnit();
                        bananaUnit.transform.position = transform.position;
                        Vector2 initSpeedb = bananaUnit._InitSpeed;
                        float vGravb = bananaUnit._virtualGravity;
                        Debug.Log("init speeeeeeeeeeeeed: " + _virtualGravity);*/
                        //bananaUnit.LaunchBananaUnitFruit(transform.position, _initPos, vGravb, initSpeedb,/*unitsSpeed,*/ _bananaUnitLaunchDelay * (i + 1), _bananaUnitFlyTimeBonus*(i+1));

                        //bananaUnit.LaunchBananaUnit(transform.position, _initPos, _fTree.CurrentFruitFlyTime * (1f + _bananaUnitFlyTimeBonus));

                        //switch sprite
                        //_img.sprite = _yellowBananaSp;
                        //bananaUnit.LaunchClusterUnitFruit(transform.position, _initPos, i * _clusterUnitFlyTimeBonus);

                        AudioController.Play("aud_fr_launch_" + GameMgr.Instance.GetLaunchAudioIndex());
                        if (DataMgr.Instance.Vibration == 1)
                            Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    }
                    //Destroy seed
                    _fTree.DestroyFruit(this);
                    //Add the additional spawned fruit to count (1 already count on seed spawn)
                    GameMgr.Instance.StageSpawnedFruitCount += (_bananaClusterUnits - 1);
                    break;

                case F_TYPE.GOLD_ITEM:
                    //TODO: Faster speed? Special Sound
                    GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);
                    _state = FRUIT_ST.LAUNCHING;
                    GameMgr.Instance.SetGoldParticleHit(transform.position);
                    AudioController.Play("ding");
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    break;

                case F_TYPE.EQUIPMENT:
                    AudioController.Play("aud_fr_item_"+(2-_maturityLevel)); //TODO: magic numbers
                    if (DataMgr.Instance.Vibration == 1)
                        Vibration.Vibrate(GameMgr._fruitHitVibrationTime);
                    if (_maturityLevel == 0)
                    {
                        GameMgr.Instance.SetEquipmentParticleHit(transform.position);
                        GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime);
                        _state = FRUIT_ST.LAUNCHING;
                    }
                    else
                    {
                        GameMgr.Instance.SetGoldParticleHit(transform.position);
                        GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime, false);
                    }
                    
                    break;
            }
            Debug.Log("Launch!");
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void DepositOnSack()
    {
        LeanTween.cancel(gameObject);

        //_pTree.DestroyCoconut(this);
        //AudioController.Play("fruit_collect");
        _state = FRUIT_ST.ON_SACK;
        _img.raycastTarget = false;     //Avoid collision hit with Sack
        if (_type == F_TYPE.RIPEN)
            _timer = 0f;
        else if (_type == F_TYPE.CHICKEN)
            transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Collect()
    {
        _state = FRUIT_ST.COLLECTED;
        GameMgr.Instance.CollectFruit(this);
        GameMgr.Instance.AddScore(_currentScore);

        //reset scaling from animation
        if (_type == F_TYPE.CHICKEN)
        {
            transform.localScale = Vector3.one;
            _chickenEggAnimation = false;
        }
        else if (_type == F_TYPE.EGG)
        {
            transform.localScale = Vector3.one;
            _eggSpawnAnimation = false;
        }
        //TODO: fix gold amount
        //if (_type == F_TYPE.GOLD_ITEM)
        //GameMgr.Instance.AddGold(_baseGold);
        if (DataMgr.Instance.Vibration == 1)
            Vibration.Vibrate((long)GameMgr._fruitCollectedVibrationTime);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="animTime"></param>
    public void SetCollectedAnimation(Vector2 pos, float animTime)
    {
        //Animation
        LeanTween.cancel(gameObject);
        LeanTween.move(gameObject, pos, animTime);
        LeanTween.rotateZ(gameObject, 360f, animTime);/* setOnComplete(() =>
        {
            _fTree.DestroyFruit(this);
        });*/
        _collectedAnimTime = animTime;
        _flyingTime = 0f;
    }

    /// <summary>
    /// Move, rotate and destroy the fruit
    /// </summary>
    public void Dissmiss()
    {
        //reset scaling from animation
        if (_type == F_TYPE.CHICKEN)
        {
            transform.localScale = Vector3.one;
            _chickenEggAnimation = false;
        }
        else if(_type == F_TYPE.EGG)
        {
            transform.localScale = Vector3.one;
            _eggSpawnAnimation = false;
        }
            

        GameMgr.Instance.ResetCombo();
        ++GameMgr.Instance.StageDismissedFruitCount;
        _state = FRUIT_ST.DISMISSED;
        _flyingTime = 0f;
        LeanTween.cancel(gameObject);
        LeanTween.move(gameObject, (Vector2)transform.position + _dissmissAnimationOffset*Random.Range(0.8f,1.2f), _dissmissAnimationTime);
        LeanTween.rotateZ(gameObject, 1080f, _dissmissAnimationTime);// setOnComplete(() =>
         //{


         //});
         if (_type == F_TYPE.BANANA_CLUSTER_UNIT || _type == F_TYPE.CLUSTER_UNIT || _type == F_TYPE.MULTI_UNIT )
            AudioController.Play("aud_fr_dismissed_1");
         else
            AudioController.Play("aud_fr_dismissed_0");

        //Analytics
        if (_type == F_TYPE.EQUIPMENT)
            ++GameMgr.Instance.EqpItmLostCount;
        else if (_type == F_TYPE.GOLD_ITEM)
            ++GameMgr.Instance.GoldItmLostCount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fruitTypeIndex"></param>
    public void SetupFruit(int fruitTypeIndex)
    {
        PathIndexEntry pe = null;

        _type = (F_TYPE)fruitTypeIndex;
        if (_img == null)
            _img = GetComponent<Image>();
        if (_type != F_TYPE.GOLD_ITEM && _type != F_TYPE.EQUIPMENT && _type != F_TYPE.EGG)
            _img.sprite = _fruitSpriteList[fruitTypeIndex];
        _img.raycastTarget = true;
        if (_type != F_TYPE.CLUSTER_UNIT && _type != F_TYPE.MULTI_UNIT)
            _size = 1f;
        else
            _size = 0.5f;

        switch (_type)
        {
            case F_TYPE.COCO_L:
                _currentScore = _cocoScore_L;
                _maturityLevel = 1;
                break;

            case F_TYPE.COCO_M:
                _currentScore = _cocoScore_M;
                _maturityLevel = 1;
                break;

            case F_TYPE.COCO_S:
                _currentScore = _cocoScore_S;
                _maturityLevel = 1;
                break;

            case F_TYPE.BANANA:
                _currentScore = _bananaScore;
                _maturityLevel = 2; //TODO: MAGIC NUMBER
                break;

            case F_TYPE.CACAO:
                _currentScore = _cacaoScore;
                _maturityLevel = 1;
                break;

            case F_TYPE.SPIKY:
                _currentScore = 0;
                _maturityLevel = 1;
                break;

            case F_TYPE.RIPEN:
                _currentScore = _littleRipenScore;
                _maturityLevel = 1;
                _ripenSt = RIPEN_STATE.LITTLE;
                _currentRipenTime = _ripenTime_mellow * GameMgr.Instance.GetCurrentLevel().FruitSpawnTime;
                break;

            case F_TYPE.CHICKEN:
                _currentScore = _chickenScore;
                _maturityLevel = 1;
                _eggSpawnTime = GameMgr.Instance.GetCurrentLevel().FruitSpawnTime;
                break;

            case F_TYPE.GUARANA:
                _currentScore = _slowMoScore;
                _maturityLevel = 1;
                break;

            case F_TYPE.SACK_BREAKER:
                _currentScore = 0;
                _maturityLevel = 1;
                break;

            case F_TYPE.KIWI:
                _currentScore = _kiwiScore;
                _maturityLevel = 1;
                break;

            case F_TYPE.BANANA_CLUSTER_UNIT:
                _currentScore = _bananaScore;
                _maturityLevel = 1;
                break;

            /*case F_TYPE.GOLD_ITEM:
                _currentScore = _baseGold;
                _maturityLevel = 1;
                _img.sprite = _goldSpriteList[(int)_goldItemType];
                break;

            case F_TYPE.EQUIPMENT:
                _currentScore = _baseScore;
                _maturityLevel =  GetEquipmentMaturity();
                _img.sprite = _goldSpriteList[(int)_goldItemType];
                break;*/
        }

        //Set Path
        pe = _pathIndexList.Find((e) => (e._TypeList.Contains(_type)));
        if (pe != null)
            GetComponent<PolygonCollider2D>().SetPath(0, pe._Path);
        else
            GetComponent<PolygonCollider2D>().SetPath(0, _pathIndexList[0]._Path);  //TOREMOVE: COCO BIG if no entry found.

        if (_fTree == null)
            _fTree = GameMgr.Instance._FruitTree;

        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="goldTypeIndex"></param>
    public void SetupFruitAsGoldItem(string id)
    {
        PathIndexEntry pe = null;

        Debug.Log("___________" + id + "____________________>>>>Fruit set as gold: ");
        //_goldItemType = (G_TYPE)goldTypeIndex;
        if (_img == null)
            _img = GetComponent<Image>();
        _img.sprite = DataMgr.Instance.GetGoldItems().Find((itm) => (itm.IdName.CompareTo(id) == 0))._Sprite;
        
        _currentScore = _goldItemScore;
        _maturityLevel = 1;
        _type = F_TYPE.GOLD_ITEM;
        _goldValue = DataMgr.Instance.GetGoldItems().Find((itm) => (itm.IdName.CompareTo(id) == 0)).Value;

        //Set Path
        pe = _pathIndexList.Find((e) => (e._TypeList.Contains(_type)));
        if (pe != null)
            GetComponent<PolygonCollider2D>().SetPath(0, pe._Path);
        else
            GetComponent<PolygonCollider2D>().SetPath(0, _pathIndexList[0]._Path);  //TOREMOVE: COCO BIG if no entry found.
        _size = 1f;

        if (_fTree == null)
            _fTree = GameMgr.Instance._FruitTree;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="goldTypeIndex"></param>
    public void SetupFruitAsEquipmentItem(string id)
    {
        PathIndexEntry pe = null;
            
        //_goldItemType = (G_TYPE)goldTypeIndex;
        if (_img == null)
            _img = GetComponent<Image>();
        equipmentItemRef = DataMgr.Instance.GetGameItems().Find((itm) => (itm.IdName.CompareTo(id) == 0));
        Debug.Log("EI::::: id " + id+"///"+ equipmentItemRef);
        _img.sprite = equipmentItemRef._Sprite;// DataMgr.Instance.GetGameItems().Find((itm) => (itm.IdName.CompareTo(id) == 0))._Sprite;
        _currentScore = _equipmentItemScore;
        _maturityLevel = 2; 
        _type = F_TYPE.EQUIPMENT;
        
        //Set Collider Path
        pe = _pathIndexList.Find((e) => (e._TypeList.Contains(_type)));
        if (pe != null)
            GetComponent<PolygonCollider2D>().SetPath(0, pe._Path);
        else
            GetComponent<PolygonCollider2D>().SetPath(0, _pathIndexList[0]._Path);  //TOREMOVE: COCO BIG if no entry found.
        _size = 1f;

        if (_fTree == null)
            _fTree = GameMgr.Instance._FruitTree;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="quality"></param>
    public void SetupFruitAsEgg(int quality)
    {
        Debug.Log("Setup egg Q: " + quality);
        //_goldItemType = (G_TYPE)goldTypeIndex;
        if (_img == null)
            _img = GetComponent<Image>();
        _currentEggQuality = quality;
        //2nd egg is random
        /*if (quality == _maxSpawnedEggs-1)
        {
            float result = Random.Range(0f, 1f);
            if (result <= _eggTierOneChance)
            {
                _currentEggQuality = 1;
                _currentScore = _eggScoreQuality_1;
            }
                
            else if (result <= _eggTierTwoChance)
            {
                _currentEggQuality = 2;
                _currentScore = _eggScoreQuality_2;
            }
                
            else
            {
                _currentEggQuality = 3;
                _currentScore = _goldItemScore;
            }
                
        }
        else
        {
            _currentEggQuality = 0;
            _currentScore = _eggScoreQuality_0;
        }*/
        _img.sprite = _qualityEggSpList[_currentEggQuality];// DataMgr.Instance.GetGameItems().Find((itm) => (itm.IdName.CompareTo(id) == 0))._Sprite;
        if (quality == 0)
            _currentScore = _eggScoreQuality_0;
        else if (quality == 0)
            _currentScore = _eggScoreQuality_1;
        else if (quality == 0)
            _currentScore = _eggScoreQuality_2;
        else
            _currentScore = _goldItemScore;
        _maturityLevel = 1;
        _type = F_TYPE.EGG;
        _state = FRUIT_ST.ON_SACK;
        _size = 0.5f;// 1f;
        if (_currentEggQuality == _maxEggQuality - 1)
            _goldValue = _goldenEggGoldValue;

        if (_fTree == null)
            _fTree = GameMgr.Instance._FruitTree;

        _eggSpawnAnimation = true;
        _timer = 0f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Sprite GetFruitSprite()
    {
        Sprite ret = null;

        if (_type == F_TYPE.RIPEN)
        {
            switch (_ripenSt)
            {
                case RIPEN_STATE.LITTLE:
                    ret = _img.sprite;
                    break;

                case RIPEN_STATE.MELLOW:
                    ret = _mellowRipenSp;
                    break;

                case RIPEN_STATE.MATURE:
                    ret = _matureRipenSp;
                    break;

                case RIPEN_STATE.STALE:
                    ret = _staleRipenSp;
                    break;
            }
        }
        else
            ret = _img.sprite;

        return ret;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetScore()
    {
        return _currentScore;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetGold()
    {
        if (_type == F_TYPE.GOLD_ITEM || (_type == F_TYPE.EGG && _currentEggQuality == _maxEggQuality-1))
            return _goldValue;
        return 0;
    }

    /// <summary>
    /// Calculates variables to use position ecuation when throwing the fruit to left side oaccording to its landing time(from maxH to floor)
    /// </summary>
    /// <param name="flyTime"></param>
    public void GetFruitLaunchSpeed(float flyTime, bool toLeftSide = true, float fruitHeight = -1f, float targetXPos = -1f)
    {
        float xSpeed, ySpeed, timeToH;
        float targetMidPos = 0f;
        float accuracy, baseAccuracy;

        if (fruitHeight != -1f)
            _currentFruitHeight = fruitHeight;
        else
            _currentFruitHeight = GameMgr.Instance.MaxCocoHeight + Random.Range(0f, GameMgr.Instance.MaxCocoOffset);
        //(1) Set virtual gravity given the time from maxHeight to floor
        _virtualGravity = -2f * (_currentFruitHeight - GameMgr.Instance.FloorYPos) / (flyTime * flyTime);
        if (!toLeftSide)    //20% bonus time for bananas
            _virtualGravity *= 0.8f;
        //Debug.Log("FRUIT: " + _type + "  toLEftSide" + toLeftSide);

        //(2) Get init speed. We need first to calculate the time it needs to reach maxHeight, when its y_speed = 0f
        if (transform.position.y > _currentFruitHeight)
            timeToH = 0f;
        else
            timeToH = Mathf.Sqrt(2f * (transform.position.y - _currentFruitHeight) / _virtualGravity);
        
        //Debug.Log("Time to H: " + timeToH);
        ySpeed = -_virtualGravity * timeToH;

        //check throwing side
        if (targetXPos == -1f)
        {
            //Dont apply accuracy to sack breakers
            if (_type == F_TYPE.SACK_BREAKER_LAUNCH)
            {
                targetXPos = Random.Range(GameMgr.Instance.MinHorizontalCocoPos, GameMgr.Instance.MaxHorizontalCocoPos);
            }
            else
            {
                accuracy = GameMgr.Instance._StrikerMonkey.GetAccuracy();
                baseAccuracy = GameMgr.Instance._StrikerMonkey.BaseAccuracy;
                if (toLeftSide)
                {
                    targetMidPos = (GameMgr.Instance.MaxHorizontalCocoPos - GameMgr.Instance.MinHorizontalCocoPos) * 0.5f;
                    targetXPos = Random.Range(GameMgr.Instance.MinHorizontalCocoPos, GameMgr.Instance.MaxHorizontalCocoPos);
                    //Apply accuracy 
                    if ((accuracy - baseAccuracy) >= 0f)
                    {
                        //targetMidPos = (GameMgr.Instance.MaxHorizontalCocoPos - GameMgr.Instance.MinHorizontalCocoPos) * 0.5f;
                        targetXPos = Mathf.Lerp(targetXPos, targetMidPos, (accuracy - baseAccuracy) / (GameMgr._maxAccuracy - baseAccuracy));
                    }
                    else
                    {
                        
                        if (targetXPos < targetMidPos)
                            targetXPos = Mathf.Lerp(targetXPos, GameMgr.Instance.MinHorizontalCocoPos, -(accuracy - baseAccuracy) / GameMgr._maxAccuracy);
                        else
                            targetXPos = Mathf.Lerp(targetXPos, GameMgr.Instance.MaxHorizontalCocoPos, -(accuracy - baseAccuracy) / GameMgr._maxAccuracy);
                    }
                }
                else //Right side
                {
                    targetMidPos = transform.position.x;
                    targetXPos = Random.Range(GameMgr.Instance.GetCurrentLevel().LeftBotSpawnLimit.position.x, GameMgr.Instance.GetCurrentLevel().RightTopSpawnLimit.position.x);
                    if (accuracy >= 0f)
                    {
                        
                        targetXPos = Mathf.Lerp(targetXPos, targetMidPos, (accuracy - baseAccuracy) / (GameMgr._maxAccuracy - baseAccuracy));

                    }
                    else
                    {
                        //targetXPos = Random.Range(GameMgr.Instance.MinHorizontalCocoPos, GameMgr.Instance.MaxHorizontalCocoPos);
                        if (targetXPos < targetMidPos)
                            targetXPos = Mathf.Lerp(targetXPos, GameMgr.Instance.GetCurrentLevel().LeftBotSpawnLimit.position.x, -(accuracy - baseAccuracy) / GameMgr._maxAccuracy);
                        else
                            targetXPos = Mathf.Lerp(targetXPos, GameMgr.Instance.GetCurrentLevel().RightTopSpawnLimit.position.x, -(accuracy - baseAccuracy) / GameMgr._maxAccuracy);
                    }
                }
            }
        }
        _targetXPos = targetXPos;
        xSpeed = (targetXPos - transform.position.x) / (timeToH + flyTime);
        _initSpeed = new Vector2(xSpeed, ySpeed);
        LeanTween.cancel(gameObject);
        if (_type != F_TYPE.SACK_BREAKER_LAUNCH)
            LeanTween.rotateZ(gameObject, 1080f, timeToH + flyTime);
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 315f);
            LeanTween.rotateZ(gameObject, 85f, timeToH + flyTime);
        }

        _flyingTime = 0f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="initSpeed"></param>
    public void LaunchClusterUnitFruit(Vector2 position, Vector2 initPos, float flyTimeBonus, float targetXPos = -1f)
    {
        float   targetMidPos = (GameMgr.Instance.MaxHorizontalCocoPos - GameMgr.Instance.MinHorizontalCocoPos) * 0.5f;
        float   targetPos = Random.Range(GameMgr.Instance.MinHorizontalCocoPos, GameMgr.Instance.MaxHorizontalCocoPos);
        //Apply accuracy 
        if ((GameMgr.Instance._StrikerMonkey.GetAccuracy() - GameMgr.Instance._StrikerMonkey.BaseAccuracy) >= 0f)
        {
            //targetMidPos = (GameMgr.Instance.MaxHorizontalCocoPos - GameMgr.Instance.MinHorizontalCocoPos) * 0.5f;
            targetPos = Mathf.Lerp(targetPos, targetMidPos, (GameMgr.Instance._StrikerMonkey.GetAccuracy() - GameMgr.Instance._StrikerMonkey.BaseAccuracy) / (GameMgr._maxAccuracy - GameMgr.Instance._StrikerMonkey.BaseAccuracy));
        }
        else
        {

            if (targetPos < targetMidPos)
                targetPos = Mathf.Lerp(targetPos, GameMgr.Instance.MinHorizontalCocoPos, -(GameMgr.Instance._StrikerMonkey.GetAccuracy() - GameMgr.Instance._StrikerMonkey.BaseAccuracy) / GameMgr._maxAccuracy);
            else
                targetPos = Mathf.Lerp(targetPos, GameMgr.Instance.MaxHorizontalCocoPos, -(GameMgr.Instance._StrikerMonkey.GetAccuracy() - GameMgr.Instance._StrikerMonkey.BaseAccuracy) / GameMgr._maxAccuracy);
        }
        //Check max deviation between cluster units if it is not the first
        if (targetXPos != -1f)
        {
            if (Mathf.Abs(targetPos - targetXPos) > (GameMgr.Instance.MaxHorizontalCocoPos - GameMgr.Instance.MinHorizontalCocoPos) * _maxClusterUnitDeviation)
            {
                if (targetPos > targetXPos)
                    targetPos = targetXPos + (GameMgr.Instance.MaxHorizontalCocoPos - GameMgr.Instance.MinHorizontalCocoPos) * _maxClusterUnitDeviation;
                else
                    targetPos = targetXPos - (GameMgr.Instance.MaxHorizontalCocoPos - GameMgr.Instance.MinHorizontalCocoPos) * _maxClusterUnitDeviation;
            }
        }
        
        transform.position = position;
        _initPos = initPos;
        //_initSpeed = initSpeed;
        
        _state = FRUIT_ST.LAUNCHING;
        //_fTree = GameMgr.Instance._FruitTree;
        GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime*(1f+flyTimeBonus),true , -1f, targetPos);
        _flyingTime = 0f;
        gameObject.SetActive(true);
       
    }

    public void LaunchBananaClusterUnitFruit(Vector2 position, Vector2 initPos, float flyTimeBonus)
    {
        transform.position = position;
        _initPos = initPos;
        //_initSpeed = initSpeed;

        _state = FRUIT_ST.LAUNCHING;
        //_fTree = GameMgr.Instance._FruitTree;
        GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime * (1f + flyTimeBonus), false);
        _flyingTime = 0f;
        gameObject.SetActive(true);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="initPos"></param>
    /// <param name="gravity"></param>
    /// <param name="initSpeed"></param>
    /// <param name="delayTime"></param>
    public void LaunchMultiUnitFruit(Vector2 position, Vector2 initPos, float gravity, Vector2 initSpeed, float delayTime, float fruitHeight = -1f, float targetXPos = -1f)
    {
        transform.position = position;
        _initPos = initPos;
        _initSpeed = initSpeed;
        //_fTree = GameMgr.Instance._FruitTree;
        GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime, true, fruitHeight, targetXPos);
        _state = FRUIT_ST.WAITING_FOR_LAUNCH;
        
        //_virtualGravity = gravity;
        //_initSpeed = initSpeed;
        _waitCooldown = delayTime;
        _waitingTimer = 0f;
        _flyingTime = 0f;
        gameObject.SetActive(true);  
    }

    public void LaunchBananaUnitFruit(Vector2 position, Vector2 initPos, float gravity, Vector2 initSpeed, float delayTime, float flyBonusTime, float fruitHeight = -1f)
    {
        transform.position = position;
        _initPos = initPos;
        _initSpeed = initSpeed;
        //_fTree = GameMgr.Instance._FruitTree;
        GetFruitLaunchSpeed(_fTree.CurrentFruitFlyTime+ flyBonusTime, false, fruitHeight);
        _state = FRUIT_ST.WAITING_FOR_LAUNCH;

        //_virtualGravity = gravity;
        //_initSpeed = initSpeed;
        _waitCooldown = delayTime;
        _waitingTimer = 0f;
        _flyingTime = 0f;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="initPos"></param>
    /// <param name="fTime"></param>
    /// <param name="maxHeight"></param>
    public void LaunchKiwiFruit(Vector2 position, Vector2 initPos, float fTime, float maxHeight)
    {
        transform.position = position;
        _initPos = initPos;
        //_initSpeed = initSpeed;

        _state = FRUIT_ST.LAUNCHING;
        //_fTree = GameMgr.Instance._FruitTree;
        GetFruitLaunchSpeed(fTime, true, maxHeight);
        _flyingTime = 0f;
        gameObject.SetActive(true);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="initPos"></param>
    /// <param name="fTime"></param>
    public void LaunchBananaUnit(Vector2 position, Vector2 initPos, float fTime)
    {
        transform.position = position;
        _initPos = initPos;
        //_initSpeed = initSpeed;

        _state = FRUIT_ST.FALLING_FROM_TREE;
        //_fTree = GameMgr.Instance._FruitTree;
        GetFruitLaunchSpeed(fTime, false);
        _flyingTime = 0f;
        gameObject.SetActive(true);
    }
    public void Pause(bool pause)
    {
        if (pause)
        {
            _lastState = _state;
            _state = FRUIT_ST.IDLE;
            
        }
        else
        {
            _state = _lastState;
        }
    }

    /// <summary>
    /// Helper function to stop falling sound (on death)
    /// </summary>
    public void StopFallingSound()
    {
        if (_fallSfxAO != null)
            _fallSfxAO.Stop();
        else
            Debug.LogWarning("No fall sound found!!");
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// Calculates variables to use fall position ecuation according to fall time
    /// </summary>
    /// <param name="fallTime"></param>
    private void GetFruitFallSpeed(float fallTime)
    {
        //(1) Set virtual gravity given the time from spawnPosition to floor
        _virtualGravity = -2f * (transform.position.y-GameMgr.Instance.FloorYPos) / (fallTime * fallTime);
        _initSpeed = new Vector2(0f, _virtualGravity*fallTime);
        _initPos = transform.position;
        _flyingTime = 0f;
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int GetEquipmentMaturity()
    {
        if (_type != F_TYPE.EQUIPMENT)
            Debug.LogError("This is not an equip item!");

        //TODO: check equpment "quality" to determine max Maturity
        return 3;

    }

    /// <summary>
    /// 
    /// </summary>
    private void ThrowOut()
    {
        float xSpeed, ySpeed, timeToH, currentFruitHeight;

        _state = FRUIT_ST.DISMISSED;
        _initPos = transform.position;
        _flyingTime = 0f;

        currentFruitHeight = transform.position.y + _throwOutHeight;
        //(1) Set virtual gravity given the time from maxHeight to floor
        _virtualGravity = -2f * (currentFruitHeight - GameMgr.Instance.FloorYPos) / (_throwOutFallTime * _throwOutFallTime);
        //Debug.Log("FRUIT: " + _type + "  toLEftSide" + toLeftSide);

        //(2) Get init speed. We need first to calculate the time it needs to reach maxHeight, when its y_speed = 0f
        timeToH = Mathf.Sqrt(2f * (transform.position.y - currentFruitHeight) / _virtualGravity);
        /*Debug.Log("Time to H: " + timeToH);
        ySpeed = -_virtualGravity * timeToH;


        xSpeed = (Random.Range(GameMgr.Instance.GetCurrentLevel().LeftBotSpawnLimit.position.x, GameMgr.Instance.GetCurrentLevel().RightTopSpawnLimit.position.x) - transform.position.x) / (timeToH + _throwOutFallSpeed);
        */
        _initSpeed = _throwOutSpeed;
        LeanTween.cancel(gameObject);
        LeanTween.rotateZ(gameObject, -1440f, timeToH + _throwOutFallTime);

        _flyingTime = 0f;
        
    }
	#endregion


	#region Properties
    public FruitTree FruitTree { get { return _fTree; } set { _fTree = value; } }
    public F_TYPE _Ftype { get { return _type; } private set { _type = value; } }
    public FRUIT_ST _FState { get { return _state; } private set { _state = value; } }
    public RIPEN_STATE _RState {  get { return _ripenSt; } set { _ripenSt = value; } }
    public Vector2 _InitSpeed { get { return _initSpeed; } set { _initSpeed = value; } }
    public EquipmentItem EquipmentItem { get { return equipmentItemRef; } set { equipmentItemRef = value; } }
    public float CurrentFruitHeight { get { return _currentFruitHeight; } set { _currentFruitHeight = value; } }
    public int EggSpawnQuality { get { return _eggSpawnQuality; } set { _eggSpawnQuality = value; } }
    public int CurrentEggQuality { get { return _currentEggQuality; } set { _currentEggQuality = value; } }
    public float Size { get { return _size; } set { _size = value; } }
    public int Maturity {  get { return _maturityLevel; } set { _maturityLevel = value; } }
    #endregion

    #region Private Serialized Fields
    //TODO: unerialie and add score based on fruit type
    [SerializeField]
    private int _baseScore;
    [SerializeField]
    private int _goldValue;
    [SerializeField]
    private F_TYPE _type;//TODO: unserialize both fields
    [SerializeField]
    private G_TYPE _goldItemType;   

    [SerializeField]
    private Transform _minLandingPt, _maxLandingPt;
    [SerializeField]
    private Transform _maxHeightPt;
    [SerializeField]
    private float _maxHeightOffset;

    [SerializeField]
    private Vector2 _dissmissAnimationOffset;
    
    [SerializeField]
    private float _dissmissAnimationTime;

    [SerializeField]
    private List<Sprite> _fruitSpriteList;
    //[SerializeField]
    //private List<Sprite> _goldSpriteList;
    [SerializeField]
    private List<Sprite> _qualityEggSpList;
    [SerializeField]
    private Sprite _yellowBananaSp;
    [SerializeField]
    private Sprite _mellowRipenSp;
    [SerializeField]
    private Sprite _matureRipenSp;
    [SerializeField]
    private Sprite _staleRipenSp;
    [SerializeField]
    private Sprite _kiwiFallSp, _kiwiLaunchSp;

    [SerializeField]
    private Vector2 _throwOutSpeed;
    [SerializeField]
    private float _throwOutHeight;
    [SerializeField]
    private float _throwOutFallTime;
    [SerializeField]
    private float _eggSpawnTime;

    [SerializeField]
    private List<PathIndexEntry> _pathIndexList;

    [SerializeField]
    private AnimationCurve _chickenPushEggAnimCurve, _eggPushEggAnimCurve, _ripenAnimCurve;
    [SerializeField]
    private float _eggAnimTime;
    #endregion

    #region Private Non-serialized Fields
    private FRUIT_ST _state, _lastState;
    private RIPEN_STATE _ripenSt;

    private FruitTree _fTree;
    private Vector2 _initSpeed;
    private Vector2 _initPos;
    private float _currentMaxHeight;
    private float _flyingTime;
    private float _virtualGravity;
    private float _waitingTimer, _waitCooldown;
    private float _timer; //timer, general purposes

    private Image _img;
    private int _currentScore;
    private int _maturityLevel; //if 0 then goes to left; otherwise it get launched upwards again**

    private float _currentFruitHeight;
    private float _targetXPos;

    private float _collectedAnimTime;

    private EquipmentItem equipmentItemRef;     //reference to attached equipment item if fruit type is Equipment Item

    private int _eggSpawnQuality;   //quality for new spawned egg
    private int _currentEggQuality; //current quality for Egg fruit

    private AudioObject _fallSfxAO;

    private float _size;
    private float _currentRipenTime;

    private bool _eggSpawnAnimation, _chickenEggAnimation, _ripenTransAnim;
    #endregion
}
