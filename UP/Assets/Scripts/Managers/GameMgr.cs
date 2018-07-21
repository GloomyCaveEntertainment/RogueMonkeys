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
using UnityEngine.SceneManagement;
using SunCubeStudio.Localization;

public class GameMgr : MonoBehaviour {

	#region Public Data
    /// <summary>
    /// Helper class used to store information about collected fruits, used later on LevelFinishedScreen
    /// </summary>
    public class CollectedFruit
    {
        public Fruit.F_TYPE _F_Type;
        public Fruit.RIPEN_STATE _R_State;
        public int _EggQuality;
        public Sprite _Sprite;
        public EquipmentItem _E_Item;
        public int _Gold;
        public int _Score;

        //Ctor
        public CollectedFruit(Fruit.F_TYPE fType, Fruit.RIPEN_STATE rState, int eggQuality, Sprite sp, EquipmentItem eI, int gold, int score)
        {
            _F_Type = fType;
            _R_State = rState;
            _EggQuality = eggQuality;
            _Sprite = sp;
            _E_Item = eI;
            _Gold = gold;
            _Score = score;
        }
    }

    public static GameMgr Instance;

    public enum GAME_STATE { IDLE = 0, LOADING_LEVEL, RUN_LEVEL, WAITING_FOR_GUARD, WAITING_MONKEYS_FLEE, WAITING_FRUITS, LEVEL_ENDED }

    public const float _monkeysFleeTime = 2f;
    public const long _fruitHitVibrationTime = 50;
    public const long _fruitOnSackVibrationTime = 30;
    public const long _fruitCollectedVibrationTime = 20;
    public const long _fruitFullSackVibrationTime = 100;
    public const long _missFruitVibrationTime = 200;
    public const long _loseVibrationTime = 750;
    public const float _slowMoTimeScale = 0.5f;
    public const float _slowMoDuration = 2f;
    public const float _maxAccuracy = 2f;
    public const float _farGuardCanvasScaleRatio = 0.8f;
    public const float _marginSaverDist = 50f;

    public delegate void OnAlarmRaisedEvt();


    public event OnAlarmRaisedEvt AlarmRaisedEvt;

	#endregion


	#region Behaviour Methods
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        
    }
	// Use this for initialization
	void Start () {
        
        
        _rightFingerTouch.fingerId = -1;
        _leftFingerTouch.fingerId = -1;
        Input.multiTouchEnabled = true;
        
        
        _stageCollectedFruits = new List<CollectedFruit>();
        _stageDismissedFruitCount = _stageSpawnedFruitCount = _stageSucceededFruitCount = 0;
        _gameState = GAME_STATE.IDLE;

        
        //_invScreen = GameObject.FindGameObjectWithTag("InventoryScreen").GetComponent<InventoryScreen>();
        //_shopScreen = GameObject.FindGameObjectWithTag("ShopScreen").GetComponent<ShopMenu>(); ;

        //GameMgr.Instance.SetLevelIndex(_currentLevelIndex);


        //_cocoFlyTime = .5f;
        //_gravity = -9.8f;
    }
	
	// Update is called once per frame
	void Update ()
    {

        /*if (Input.GetKeyDown(KeyCode.K))
        {
            if (_stageCollectedFruits == null)
                _stageCollectedFruits = new List<Fruit>();
            _stageCollectedFruits.Add(_fruitTree._FList[0]);
            _stageCollectedFruits.Add(_fruitTree._FList[1]);
            _fruitTree.DestroyFruit(_fruitTree._FList[0]);
        }*/

        //if (Input.GetKeyDown(KeyCode.S))
            //SetSlowMotion(true);
        //Slow Motion
        if (_slowMoEnabled)
        {
            _slowMoTimer -= (Time.deltaTime / _slowMoTimeScale);
            if (_slowMoTimer <= 0f)
                SetSlowMotion(false);
            else
                UIHelper.Instance.UpdateSlowMoFill(_slowMoTimer / _slowMoDuration, true);
        }
        switch (_gameState)
        {
            case GAME_STATE.IDLE:

                break;

            case GAME_STATE.LOADING_LEVEL:
                /*if (_levelLoaded.isDone)
                {
                    
                }*/
                break;
            case GAME_STATE.RUN_LEVEL:

                if (_currentLevel.GetLevelState() == Level.L_STATE.RUN || _currentLevel.GetLevelState() == Level.L_STATE.IDLE || _currentLevel.GetLevelState() == Level.L_STATE.WAITNG_FLYING_FRUITS) //IDLE for tutorial scenario
                {
//#if UNITY_EDITOR
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!UIHelper.Instance.ContainsPauseBtn(Input.mousePosition))
                            _strikerMonkey.MoveToHit(GameCamera.ScreenToWorldPoint(Input.mousePosition));
                    }
                    if (Input.GetAxis("Horizontal") != 0f)
                    {
                        _collectorMonkey.transform.position += Input.GetAxis("Horizontal") * Vector3.right * 700f * Time.deltaTime;
                    }
                    if (Input.GetKeyDown(KeyCode.Space))
                        _collectorMonkey._Sack.Reload(); //collect sack fruits
//#endif
                //    #region Touch management
                //    foreach (Touch t in Input.touches)
                //    {
                //        //Debug.Log("Touch::: " + t.fingerId);
                //        switch (t.phase)
                //        {
                //            case TouchPhase.Began:

                //                //Check either rightFinger isnt init or it has ended
                //                if (//_rightFingerTouch.fingerId == -1 
                //                    /*|| (_rightFingerTouch.fingerId == t.fingerId))*/
                //                    /*&&*/ IsOnRightFingerZone(t, out _rightFingerTouch))

                //                {
                //                    //DEPRECATED _rightFingerTouch = t;
                //                    _strikerMonkey.MoveToHit(GameCamera.ScreenToWorldPoint(t.position));

                //                }
                //                else if (/*(_leftFingerTouch.fingerId == -1
                //                || (_leftFingerTouch.phase == TouchPhase.Ended || _leftFingerTouch.phase == TouchPhase.Canceled))
                //                &&*/ IsOnLeftFingerZone(t))
                //                {
                //                    //TOCHEC: rBody moveposition
                //                    _leftFingerTouch = t;
                //                    _collectorMonkey.Move(t);
                //                    //_leftMonkey.transform.position = new Vector3(GameCamera.ScreenToWorldPoint(t.position).x, transform.position.y, transform.position.z);
                //                }
                //                break;

                //            case TouchPhase.Canceled | TouchPhase.Ended:
                //                if (t.fingerId == _rightFingerTouch.fingerId)
                //                {

                //                    //TODO: send right monkey to floor
                //                }
                //                else if (t.fingerId == _leftFingerTouch.fingerId)
                //                {
                //                    //TODO: leftmonkey animation stop
                //                    _collectorMonkey.Stop();
                //                }

                //                break;

                //            case TouchPhase.Moved:
                //                if (IsOnLeftFingerZone(t))
                //                {
                //                    //TOCHEC: rBody moveposition
                //                    _leftFingerTouch = t;
                //                    _collectorMonkey.Move(t);
                //                    //_leftMonkey.transform.position = new Vector3(GameCamera.ScreenToWorldPoint(t.position).x, FloorYPos, _leftMonkey.transform.position.z);
                //                }
                //                break;

                //            case TouchPhase.Stationary:

                //                break;
                //        }
                //    }
                //    #endregion
                }





                if (_currentAlarmLvl > 0f && _currentLevel.GetLevelState() == Level.L_STATE.RUN)
                { 
                    _currentAlarmLvl -= _currentAlarmDepletion * Time.deltaTime;
                    UIHelper.Instance.AlarmDepleted(_currentAlarmDepletion * Time.deltaTime/_maxAlarmLvl);
                }

                //_currentLevelTime -= Time.deltaTime;
                //UIHelper.Instance.SetLvlScreenTime(_currentLevelTime);
                //UpdateScreenTime();//TODO: change to change it sec by sec
                /*if (Input.GetKeyDown(KeyCode.Space))
                    _fruitTree.StartSpawn();*/
                break;

            case GAME_STATE.WAITING_MONKEYS_FLEE:
                _fleeTimer += Time.deltaTime;
                if (_fleeTimer >= _monkeysFleeTime)
                {
                    //Check for remaining fruits on screen
                    _flyngFruits = false;
                    foreach (GameObject fO in GameObject.FindGameObjectsWithTag("Fruit"))
                    {
                        if (fO.GetComponent<Fruit>()._FState == Fruit.FRUIT_ST.FALLING_FROM_TREE || fO.GetComponent<Fruit>()._FState == Fruit.FRUIT_ST.LAUNCHING)
                        {
                            _flyngFruits = true;
                            break;
                        }
                    }
                    if (!_flyngFruits)
                        Lose();
                    /*UIHelper.Instance.ShowLoseScreen(true);
                    _gameState = GAME_STATE.IDLE;*/
                    //ShowLoseScreen();
                }
                break;
        }
        
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pts"></param>
    public void AddScore(int pts)
    {
        
        LeanTween.value(gameObject,_score, _score + pts, _scoreTimeCount).setOnUpdate((float val) =>
        {
            UIHelper.Instance.ScoreText.text = val.ToString("000");
            //_scoreText.text = val.ToString("0000");
        });
        _score += pts;
    }
    public void StopScoreTween()
    {
        if (LeanTween.isTweening(gameObject))
            LeanTween.cancel(gameObject);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gAmount"></param>
    public void AddGold(int gAmount)
    {
        _gold += gAmount;
        if (UIHelper.Instance == null)
            GameObject.FindGameObjectWithTag("SelectionMenu").GetComponent<MenuSelection>().UpdateMoney();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fr"></param>
    public void CollectFruit(Fruit fr)
    {
        _stageCollectedFruits.Add(new CollectedFruit(fr._Ftype, fr._RState, fr.CurrentEggQuality, fr.GetFruitSprite(), fr.EquipmentItem, fr.GetGold(), fr.GetScore()));
        ++_stageSucceededFruitCount;
        ++_currentCollectedAudioIndex;
        if (_currentCollectedAudioIndex > 4)
            _currentCollectedAudioIndex = 4;    //TODO: magic numbers
        AudioController.Play("aud_fr_collect_" + GameMgr.Instance.GetCollectedAudioIndex());
        //AudioController.Play("aud_fr_collect_0");
        if (fr._Ftype == Fruit.F_TYPE.GOLD_ITEM)
        {
            ++_goldItmCount;
            if (!_goldCollected)
                _goldCollected = true;
        }
        else if (fr._Ftype == Fruit.F_TYPE.EQUIPMENT)
        {
            ++_eqpItmCount;
            if (!_itemCollected)
                _itemCollected = true;
        }
    }

    

    /// <summary>
    /// 
    /// </summary>
    public void UpdateScreenTime()
    {
        _levelTimeText.text = _currentLevelTime.ToString("00");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void SetStageIndex(int index)
    {
        _currentStageIndex = index;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /*public void SetLevelIndex(int index)
    {
        _currentLevelIndex = index;
    }*/

    /// <summary>
    /// 
    /// </summary>
    public void LoadAndStartCurrentLevel()
    {
        _currentStageIndex = PlayerPrefs.GetInt("Current_Stage");
        _currentLevelIndex = PlayerPrefs.GetInt("Current_Level");
        Debug.Log("Stage, Level: " + _currentStageIndex + "_" + _currentLevelIndex);
        _currentStage = _stageList[_currentStageIndex];
        _currentLevel = _stageList[_currentStageIndex].GetLevelList()[_currentLevelIndex];

        Debug.Log("Load and start level. . . ." + _currentStageIndex+"_"+_currentLevelIndex);
        _stageCollectedFruits.Clear();
        _stageDismissedFruitCount = _stageSpawnedFruitCount = _stageSucceededFruitCount = 0;
        //_currentLevel = _stageList[_currentStageIndex].GetLevelList()[_currentLevelIndex];
        _levelLoaded = _currentLevel.SetupLevel();
        _gameState = GAME_STATE.LOADING_LEVEL;
        _lastHitTime = Mathf.Infinity;
        _lvlAttempts = 0;
        //ResetCombo(false);
        ClearCombo();
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadAndStartNextLevel()
    {
        //stage cmompleted
        if (_currentLevelIndex == _currentStage.GetLevelList().Count - 1)
        {
            if (_currentStageIndex == _stageList.Count - 1)
            {
                //TODO: WIN!!
            }
            else
            {
                _currentLevel.UnloadLevelLayout();
                ++_currentStageIndex;
                _currentLevelIndex = 0;
                PlayerPrefs.SetInt("Current_Stage", _currentStageIndex);
                PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
                LoadAndStartCurrentLevel();
            }
        }
        else
        {
            //check if we have to load a new layout and unload previous one
            if (_currentLevel.SceneLayoutId.CompareTo(_currentStage.GetLevelList()[_currentLevelIndex + 1].SceneLayoutId) != 0)
            {
                _currentLevel.UnloadLevelLayout();
                ++_currentLevelIndex;
                PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
                LoadAndStartCurrentLevel();
            }
            else
            {
                ++_currentLevelIndex;
                PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
                _currentLevel = _currentStage.GetLevelList()[_currentLevelIndex];
                _currentLevel.LoadReferences();
                StartCurrentLevel();
            }
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateAndSaveProgression()
    {
        //stage cmompleted
        if (_currentLevelIndex == _currentStage.GetLevelList().Count - 1)
        {
            if (_currentStageIndex == _stageList.Count - 1)
            {
                //TODO: WIN!!
            }
            else
            {
                ++_currentStageIndex;
                _currentLevelIndex = 0;
                PlayerPrefs.SetInt("Current_Stage", _currentStageIndex);
                PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
            }
        }
        else
        {

            ++_currentLevelIndex;
            PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void StartCurrentLevel()
    {
        Debug.Log("Start cLevel");
        //Show Adv
        //Once ad ended, LEvelReady is called from AdsMgr
        //if (!AdsMgr.Instance.ShowAd())
            LevelReady();
        //_levelText.text = "Level "+_currentLevelIndex;
    }

    public void LevelReady()
    {
        Debug.Log("Level Ready");
        if (_stageCollectedFruits == null)
            _stageCollectedFruits = new List<CollectedFruit>();
        else
            _stageCollectedFruits.Clear();
        _stageDismissedFruitCount = _stageSpawnedFruitCount = _stageSucceededFruitCount = 0;
        GetLevelReferences();
        _itemCollected = _goldCollected = false;
        //Reset Monkeys
        _strikerMonkey.transform.position = _currentLevel.GetSceneLayout().GetStrikerStartPos().position;
        _strikerMonkey.ResetMonkey();
        _collectorMonkey.transform.position = _currentLevel.GetSceneLayout().GetCollectorStartPos().position;
        _collectorMonkey._Reset();
        _fruitTree.ResetTree();

        //Set previous session equipped items
        foreach (EquipmentItem eI in DataMgr.Instance.GetInventoryItems())
        {
            if (eI.Equipped)
            {
                switch (eI.SlotType)
                {
                    case EquipmentItem.SLOT_TYPE.COLLECTOR_A:
                        _collectorSlotA = eI;
                        break;

                    case EquipmentItem.SLOT_TYPE.COLLECTOR_B:
                        _collectorSlotB = eI;
                        break;

                    case EquipmentItem.SLOT_TYPE.SHAKER_A:
                        _shakerSlotA = eI;
                        break;

                    case EquipmentItem.SLOT_TYPE.SHAKER_B:
                        _shakerSlotB = eI;
                        break;

                    case EquipmentItem.SLOT_TYPE.STRIKER_A:
                        _strikerSlotA = eI;
                        break;

                    case EquipmentItem.SLOT_TYPE.STRIKER_B:
                        _strikerSlotB = eI;
                        break;
                }
            }
        }

        //Load equipped items
        if (_strikerSlotA != null)
        {
            _strikerMonkey.SlotA = _strikerSlotA;
            _strikerSlotA = null;
        }
        if (_strikerSlotB != null)
        {
            _strikerMonkey.SlotB = _strikerSlotB;
            _strikerSlotB = null;
        }
        if (_collectorSlotA != null)
        {
            _collectorMonkey.SlotA = _collectorSlotA;
            _collectorSlotA = null;
        }
        if (_collectorSlotB!= null)
        {
            _CollectorMonkey.SlotB = _collectorSlotB;
            _collectorSlotB = null;
        }
        if (_shakerSlotA != null)
        {
            _fruitTree.SlotA= _shakerSlotA;
            _shakerSlotA = null;
        }
        if (_shakerSlotB != null)
        {
            _fruitTree.SlotB = _shakerSlotB;
            _shakerSlotB = null;
        }
        _strikerMonkey.LoadItemsStats();
        _fruitTree.LoadItemsStats();
        _collectorMonkey.LoadItemsStats();

        UIHelper.Instance.SetLevelText(_currentLevelIndex);
        _currentLevelTime = _currentLevel.LevelTime;
        UIHelper.Instance.SetLvlScreenTime(_currentLevelTime);
        _currentAlarmLvl = 0f;
        _score = 0;
        UIHelper.Instance.ScoreText.text = _score.ToString("000");
        UIHelper.Instance.SetAlarmLevel();
        UIHelper.Instance.ResetAlarmUI();
        _goldItmCount = _eqpItmCount = 0;
        //Start Level
        _currentLevel.StartLevel();
        _gameState = GAME_STATE.RUN_LEVEL;
        /*if (GameObject.FindGameObjectWithTag("MainMenu") != null)
        {
            Tutorial.Instance.InitTutorial();
        }*/
        if (Tutorial.Instance != null)
            Tutorial.Instance.InitTutorial();
        //Guard setup
        _farGuardCanvas = GameObject.FindGameObjectWithTag("FarGuardCanvas").transform;
        _frontGuardCanvas = GameObject.FindGameObjectWithTag("FrontGuardCanvas").transform;
        foreach (Guard g in _guardPool)
            if (g.gameObject.activeSelf)
                g.SetupGuard();
        _comboCount = 0;
        _currentLaunchAudioIndex = -1;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LevelEnded()
    {
        //Debug.Log("________________________>>Level Edned, saving data...");
        _gameState = GAME_STATE.LEVEL_ENDED;
        /*if (_stageCollectedFruits.Find((fr) => (fr._Ftype == Fruit.F_TYPE.GOLD_ITEM)) != null)
            DataMgr.Instance.SaveGold();
        if (_stageCollectedFruits.Find((fr) => (fr._Ftype == Fruit.F_TYPE.EQUIPMENT)) != null)
            DataMgr.Instance.SaveInventoryItems();
        DataMgr.Instance.SaveLevelData(_currentStageIndex, _currentLevelIndex);*/
        //DataMgr.Instance.SaveData();
        //collect sack fruits
        AnalyticsMgr.Instance.LevelEnded();
        UIHelper.Instance.ShowLevelFinishedScreen();
        //_collectorMonkey._Sack.ResetToInitPos();
        _resetShop = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveProgress()
    {
        if (_stageCollectedFruits.Find((fr) => (fr._F_Type == Fruit.F_TYPE.GOLD_ITEM)) != null)
            DataMgr.Instance.SaveGold();
        if (_stageCollectedFruits.Find((fr) => (fr._F_Type == Fruit.F_TYPE.EQUIPMENT)) != null)
            DataMgr.Instance.SaveInventoryItems();
        DataMgr.Instance.SaveLevelData(_currentStageIndex, _currentLevelIndex);

    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Level GetCurrentLevel()
    {
        return _currentLevel;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Stage GetCurrentStage()
    {
        return _currentStage;
    }

    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="alarm"></param>
    /// <returns>if game is lost due to alarm lvl</returns>
    public bool RaiseAlarm(float alarm)
    { 
        _currentAlarmLvl += alarm;
        if (AlarmRaisedEvt != null)
            AlarmRaisedEvt();
        //_alarmText.text = _currentAlarmLvl + " / " + _maxAlarmLvl;
        UIHelper.Instance.SetAlarmLevel();
        if (_currentAlarmLvl >= _maxAlarmLvl)
        {
            if (DataMgr.Instance.Vibration == 1)
                Vibration.Vibrate(_loseVibrationTime);

            _fruitTree.Stop();
            WaitForGuardWakeUp();
            _currentLevel.EndLevel();
            WakeUpGuards();
            return true;
        }
        else
        {
            //Vibration feedback
            if (DataMgr.Instance.Vibration == 1)
                Vibration.Vibrate(_missFruitVibrationTime);
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool RaiseAlarm()
    {
        return RaiseAlarm(_alarmIncrease);
    }

    /// <summary>
    /// 
    /// </summary>
    public void GuardWokenUp()
    {
        if (_gameState != GAME_STATE.WAITING_MONKEYS_FLEE)
        {
            _collectorMonkey.Flee();
            _strikerMonkey.Flee();
            _gameState = GAME_STATE.WAITING_MONKEYS_FLEE;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void WakeUpGuards()
    {
        foreach (Guard g in _guardPool)
            if (g.gameObject.activeSelf)
                g.WakeUp();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Lose()
    {
        //TOOO: analytics
        UIHelper.Instance.ShowLoseScreen(true);
        _collectorMonkey._Sack.Stop();
        _gameState = GAME_STATE.IDLE;
        Debug.Log("LOSE_______________________________________LOSE");
    }

    /// <summary>
    /// 
    /// </summary>
    public void WaitForGuardWakeUp()
    {
        _gameState = GAME_STATE.WAITING_FOR_GUARD;
    }

    /// <summary>
    /// 
    /// </summary>
    public void WaitForMonkeysFlee()
    {
        _gameState = GAME_STATE.WAITING_MONKEYS_FLEE;
    }

    /// <summary>
    /// Check if any guard get alarmed without repetition
    /// </summary>
    /// <param name="xPos"></param>
    public void AlarmWarnAtPos(float xPos, float alarm = -1f)
    {
        int awardedGuards = 0;
        float alarmRaised = 0f;

        if (_currentAlarmLvl >= _maxAlarmLvl)
            return;

        ResetCombo();
        foreach (Guard g in _currentLevel.GuardList)
        {
            if (g.CheckAlarm(xPos, alarm))
                ++awardedGuards;              
        }

        //level alarm raise
        if (alarm == -1f)
        {
            //Get total alarm amount
            //alarmRaised = _alarmIncrease;
            alarmRaised = _alarmIncrease * awardedGuards;
            /*for (int i = 0; i < awardedGuards; ++i)
                alarmRaised += _alarmIncrease;// * (1f / (2f * i));*/
            if (awardedGuards != 0)
                RaiseAlarm(alarmRaised);
        }
        else
        {
            //alarmRaised = alarm;
            for (int i = 0; i < awardedGuards; ++i)
                alarmRaised += alarm;// * (1f / (2f * i));
            if (awardedGuards != 0)
                RaiseAlarm(alarmRaised);
        }


    }

    

    /// <summary>
    /// 
    /// </summary>
    /*public void Retry()
    {
        //SceneManaetfrio
        SceneManager.LoadScene("Game");
        StartCurrentLevel();
    }*/

    /// <summary>
    /// 
    /// </summary>
    public void GetManagerReferences()
    {
        GameObject returnGO = null;

        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (_mainCamera == null)
            Debug.LogError("No main cam found!");

        //Collector
        if (_collectorMonkey == null)
        {
            returnGO = GameObject.FindGameObjectWithTag("Collector");
            if (returnGO == null)
                Debug.LogError("No Collector go found");
            _collectorMonkey = returnGO.GetComponent<CollectorMonkey>();
            if (_collectorMonkey == null)
                Debug.LogError("No Collector comp attached!");
        }

        //Striker
        if (_strikerMonkey == null)
        {
            returnGO = GameObject.FindGameObjectWithTag("Striker");
            if (returnGO == null)
                Debug.LogError("No Striker go found");
            _strikerMonkey = returnGO.GetComponent<StrikerMonkey>();
            if (_strikerMonkey == null)
                Debug.LogError("No Striker comp attached!");

        }
        //Guards
        if (_guardPool == null)
            _guardPool = new List<Guard>();
        else
        {
            _guardPool.Clear();
            foreach (GameObject goGuard in GameObject.FindGameObjectsWithTag("Guard"))
                _guardPool.Add(goGuard.GetComponent<Guard>());
    
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pause"></param>
    public void Pause(bool pause)
    {
        if (pause && _gameState != GAME_STATE.IDLE)
        {
            _lastState = _gameState;
            _gameState = GAME_STATE.IDLE;
            LeanTween.pauseAll();
        }
        else if (!pause &&_gameState == GAME_STATE.IDLE)
        {
            _gameState = _lastState;
            LeanTween.resumeAll();
        }
        _currentLevel.Pause(pause);
        _fruitTree.Pause(pause);
        _strikerMonkey.Pause(pause);
        _collectorMonkey.Pause(pause);
        foreach (Guard g in _guardPool)
            if (g.gameObject.activeSelf)
                g.Pause(pause);
        
            //AudioController.PauseMusic(_currentStage.au);   //0.2s fade out
        
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void FruitHit()
    {
        if ((Time.time - _lastHitTime)  <= _maxcomboTime)
        {
            _lastHitTime = Time.time;
            ++_comboCount;
            if (_comboCount > 1)
                UIHelper.Instance.ShowComboCount(_comboCount);
            ++_currentLaunchAudioIndex;
            if (_comboCount > 9)    //TODO: magic numbers!
                _currentLaunchAudioIndex = 9;
            else
                _currentLaunchAudioIndex = _comboCount;
        }
        else
        {
            _lastHitTime = Time.time;
            _currentLaunchAudioIndex = -1;   //reset index if there's no combo
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetLaunchAudioIndex()
    {
        return _currentLaunchAudioIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetCombo()
    {
        if (_comboCount > 1)
        {
            _scoreResult = Mathf.FloorToInt(Mathf.Log(_comboCount * _comboCount, (float)System.Math.E) * _comboCount); // log(x^2)*x
            //round it to closest 5
            _restToFive = (_scoreResult % 5);
            if (_restToFive < 3)
                _scoreResult -= _restToFive;
            else
                _scoreResult += (5 - _restToFive);
            //sum 5 to have a min value of 5 (combo x2)
            _scoreResult += 5;
            UIHelper.Instance.ShowComboResult(_scoreResult);
            AddScore(_scoreResult);   
        }
        
        _comboCount = 0;
        _currentLaunchAudioIndex = -1;

    }

    /// <summary>
    /// 
    /// </summary>
    public void ClearCombo()
    {
        _comboCount = 0;
        _currentLaunchAudioIndex = -1;
        //UIHelper.Instance.HideComboCount();
    }
    public int GetCollectedAudioIndex()
    {
        return _currentCollectedAudioIndex;
    }

    public void ResetCollectedIndex()
    {
        _currentCollectedAudioIndex = 0;
    }


    /// <summary>
    /// Enable / Disable Game in Slow Motion mode
    /// </summary>
    /// <param name="enabled"></param>
    public void SetSlowMotion(bool enabled)
    {
        if (_slowMoEnabled && enabled)
            Debug.LogWarning("Attemtping to trigger slowmo twice");
        else if (!!_slowMoEnabled && !enabled)
            Debug.LogWarning("Attempting to disable slowmo twice");
        _slowMoEnabled = enabled;
        
        if (enabled)
        {
            Time.timeScale = _slowMoTimeScale;
            _slowMoTimer = _slowMoDuration;
            AudioController.Play("aud_fr_SlowMoEnabled");
        }
        else
        {
            Time.timeScale = 1f;
            AudioController.Play("aud_fr_SlowMoDisabled");
            _strikerMonkey.StopSpeedBoost();
        }
        UIHelper.Instance.SetSlowMoGlow(enabled);
        if (!enabled)
            UIHelper.Instance.SetSpeedUp(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void WaitForFruits()
    {
        _gameState = GAME_STATE.WAITING_FRUITS;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    public void SetGoldParticleHit(Vector3 pos)
    {
        _goldHitPsPool[_currentGoldPsIndex].transform.position = pos;
        _goldHitPsPool[_currentGoldPsIndex].Play();
        _currentGoldPsIndex = (_currentGoldPsIndex + 1) % _goldHitPsPool.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    public void SetEquipmentParticleHit(Vector3 pos)
    {
        _equipmentHitPsPool[_currentEquipmentPsIndex].transform.position = pos;
        _equipmentHitPsPool[_currentEquipmentPsIndex].Play();
        _currentGoldPsIndex = (_currentEquipmentPsIndex + 1) % _equipmentHitPsPool.Count;
    }

    /// <summary>
    /// Particles for gold items and equipment items on miss
    /// </summary>
    /// <param name="fr"></param>
    /// <param name="pos"></param>
    public void SetItemCollisionParticle(Fruit fr, Vector3 pos)
    {
        if (fr._Ftype == Fruit.F_TYPE.GOLD_ITEM)
        {
            _goldItemSplashPsPool[_currentGoldItemSplashIndex].transform.position = pos;
            _goldItemSplashPsPool[_currentGoldItemSplashIndex].Play();
            _currentGoldItemSplashIndex = (_currentGoldItemSplashIndex + 1) % _goldItemSplashPsPool.Count;
        }
        else
        {
            _equipmetSplashPsPool[_currentEquipmentSplashIndex].transform.position = pos;
            _equipmetSplashPsPool[_currentEquipmentSplashIndex].Play();
            _currentGoldItemSplashIndex = (_currentEquipmentSplashIndex + 1) % _equipmetSplashPsPool.Count;
        }
    }
    /// <summary>
    /// Particles fruit Vs ground
    /// </summary>
    /// <param name="pos"></param>
    public void SetSplashParticle(Fruit fr, Vector3 pos)
    {
        ParticleSystem.MainModule mainMod;
        ParticleSystem.ColorOverLifetimeModule colorMod;
        int gradIndex = -1;
        switch (fr._Ftype)
        {
            case Fruit.F_TYPE.COCO_L:
            case Fruit.F_TYPE.COCO_M:
            case Fruit.F_TYPE.COCO_S:
                gradIndex = 0;
                break;
            case Fruit.F_TYPE.BANANA:
            case Fruit.F_TYPE.BANANA_CLUSTER_UNIT:
                gradIndex = 1;
                break;
            case Fruit.F_TYPE.CLUSTER_SEED:
                gradIndex = 2;
                break;
            case Fruit.F_TYPE.CLUSTER_UNIT:
                gradIndex = 3;
                break;
            case Fruit.F_TYPE.CACAO:
            case Fruit.F_TYPE.MULTI_SEED:
                gradIndex = 4;
                break;
            case Fruit.F_TYPE.MULTI_UNIT:
                gradIndex = 5;
                break;
            
            case Fruit.F_TYPE.RIPEN:
                gradIndex = 6;
                break;
            case Fruit.F_TYPE.KIWI:
                gradIndex = 7;
                break;
            case Fruit.F_TYPE.GUARANA:
                gradIndex = 8;
                break;
 

            case Fruit.F_TYPE.SPIKY:
                gradIndex = 0;
                break;
            case Fruit.F_TYPE.SACK_BREAKER:
            case Fruit.F_TYPE.SBREAKER_CLUSTER_DUO:
                gradIndex = 1;
                break;
            case Fruit.F_TYPE.CHICKEN:
                gradIndex = 2;
                break;

        }
        if (fr._Ftype != Fruit.F_TYPE.CHICKEN && fr._Ftype != Fruit.F_TYPE.SACK_BREAKER && fr._Ftype != Fruit.F_TYPE.SBREAKER_CLUSTER_DUO && fr._Ftype != Fruit.F_TYPE.SPIKY)
        {
            _poolSplashParticle[_currentPoolSplashIndex].transform.position = pos;
            colorMod = _poolSplashParticle[_currentPoolSplashIndex].colorOverLifetime;
            colorMod.color = _fruitSplashColorGradList[gradIndex];
            _poolSplashParticle[_currentPoolSplashIndex].Play();
            _currentPoolSplashIndex = (_currentPoolSplashIndex + 1) % _poolSplashParticle.Count;
        }
        else
        {
            _poolFeatherParticle[_currentPoolFeatherIndex].transform.position = pos;
            colorMod = _poolFeatherParticle[_currentPoolFeatherIndex].colorOverLifetime;
            colorMod.color = _featherSplashColorGradList[gradIndex];
            _poolFeatherParticle[_currentPoolFeatherIndex].Play();
            _currentPoolSplashIndex = (_currentPoolFeatherIndex + 1) % _poolFeatherParticle.Count;
        }

    }

    /// <summary>
    /// Sprite "!"
    /// </summary>
    /// <param name="pos"></param>
    public void SetAlarmPs(Vector3 pos)
    {
        Debug.Log("Particle pos: " + pos);
        _alarmPsPool[_currentAlarmPsIndex].transform.position = pos;
        _alarmPsPool[_currentAlarmPsIndex].Play();
        _currentAlarmPsIndex = (_currentAlarmPsIndex + 1) % _alarmPsPool.Count;
        //stop any remaining warn
        foreach (ParticleSystem ps in _alarmWarnPsPool)
            ps.Stop();
      
    }

    /// <summary>
    /// Sprite "?"
    /// </summary>
    /// <param name="pos"></param>
    public void SetAlarmWarnPs(Vector3 pos)
    {
        _alarmWarnPsPool[_currentAlarmWarnPsIndex].transform.position = pos;
        _alarmWarnPsPool[_currentAlarmWarnPsIndex].Play();
        _currentAlarmWarnPsIndex = (_currentAlarmWarnPsIndex + 1) % _alarmWarnPsPool.Count;
    }

    /// <summary>
    /// Helper method to reset state when going back to menu during gameplay
    /// </summary>
    public void SetIdle()
    {
        _gameState = GAME_STATE.IDLE;
    }

    #endregion


    #region Private Methods

    private void GetLevelReferences()
    {
        _currentHRef = _currentLevel.NetHeightRef;
        _floorRef = _currentLevel.FloorHeightRef;
        _minHRef = _currentLevel.LeftCollectorLimit;
        _maxHRef = _currentLevel.RightCollectorLimit;
        _minLeftFingerRef = _currentLevel.MinLeftFingerRef;
        _maxLeftFingerRef =  _currentLevel.MaxLeftFingerRef;
        _minRightFingerRef = _currentLevel.MinRightFingerRef;
        _maxRightFingerRef = _currentLevel.MaxRightFingerRef;
    }

    private void GetCurrentCocoHeight()
    {
        _currentHRef = GameObject.FindGameObjectWithTag("HeightRef").transform;
    }

    private void GetCurrentFloorReference()
    {
        
    }

    /// <summary>
    /// Check if Touch t is inside left finger - drag zone
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private bool IsOnLeftFingerZone(Touch t)
    {
        Vector2 tWorldPosition;

        tWorldPosition = _mainCamera.GetComponent<Camera>().ScreenToWorldPoint(t.position);
        if (tWorldPosition.x > _minLeftFingerRef.position.x && tWorldPosition.x < _maxLeftFingerRef.position.x
            && tWorldPosition.y < Screen.height*0.65f)
            return true;
        return false;
    }

    /// <summary>
    /// Check if Touch t is inside right finger - tap zone
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private bool IsOnRightFingerZone(Touch t, out Touch normalizedT)
    {
        Vector2 tWorldPosition;
        //RaycastHit2D[] raycastHitResults = null;
        //if (Physics.Raycast(t.position, Vector2.right, .1f, raycastHitResults))
        normalizedT = t;
        tWorldPosition = GameCamera.ScreenToWorldPoint(t.position);
        if (tWorldPosition.x > (_minRightFingerRef.position.x - _marginSaverDist) && tWorldPosition.x < (_maxRightFingerRef.position.x + _marginSaverDist)
            && !UIHelper.Instance.ContainsPauseBtn(/*tWorldPosition*/t.position))
        {
            //left margin saver
            if (tWorldPosition.x < _minRightFingerRef.position.x)
                normalizedT.position = new Vector2(_minLeftFingerRef.position.x, t.position.y);
            //right margin saver
            else if (tWorldPosition.x > _maxRightFingerRef.position.x)
                normalizedT.position = new Vector2(_minLeftFingerRef.position.x, t.position.y);
            return true;
        }
        else 
            return false;
    }

    

    private void ShowLoseScreen()
    {
        _loseScreen.SetActive(true);
    }
	#endregion


	#region Properties
    public float Gravity { get { return _gravity; } set { _gravity = value; } }
    public Camera GameCamera { get { return _mainCamera.GetComponent<Camera>(); } private set {  } }
    public float FruitFlyTime { get { return _fruitFlyTime; } set { _fruitFlyTime = value; } }
    public float FruitFallTime { get { return _fruitFallTime; } set { _fruitFallTime = value; } }
    public float FloorYPos { get { return _floorRef.position.y; } private set { } }
    public float MidFloorPos { get { return _floorRef.position.x; } private set { } }
    public float MaxCocoHeight { get { return _currentHRef.position.y; } private set { } }
    public float MinHorizontalCocoPos { get { return _minHRef.position.x; } private set { } }
    public float MaxHorizontalCocoPos { get { return _maxHRef.position.x; } private set { } }
    public float FrameTime { get { return _frameTime; } set { _frameTime = value; } }
    public float MaxCocoOffset { get { return _maxFruitOffset; } set { _maxFruitOffset = value; } }

    public List<CollectedFruit> StageCollectedFruits { get { return _stageCollectedFruits; } set { _stageCollectedFruits = value; } }
   
    //stats used to calculate Rank ratio
    public int StageSpawnedFruitCount {  get { return _stageSpawnedFruitCount; } set { _stageSpawnedFruitCount = value; } }
    public int StageDismissedFruitCount { get { return _stageDismissedFruitCount; } set { _stageDismissedFruitCount = value; } }
    public int StageSucceededFruitCount { get { return _stageSucceededFruitCount; } set { _stageSucceededFruitCount = value; } }

    public StrikerMonkey _StrikerMonkey { get { return _strikerMonkey; } set { _strikerMonkey = value; } }
    public CollectorMonkey _CollectorMonkey { get { return _collectorMonkey; } set { _CollectorMonkey = value; } }

    public List<Stage> _StageList { get { return _stageList; } set { _stageList = value; } }

    public List<Guard> _GuardPool { get { return _guardPool; } set { _guardPool = value; } }

    public FruitTree _FruitTree { get { return _fruitTree; } set { _fruitTree = value; } }

    public int StageIndex { get { return _currentStageIndex; } set { _currentStageIndex = value; } }
    public int LevelIndex { get { return _currentLevelIndex; } set { _currentLevelIndex = value; } }
    public int Gold { get { return _gold; } set { _gold = value; } }
    public int Score { get { return _score; } set { _score = value; } }
    public int _Vibration { get { return _vibration; } set { _vibration = value; } }

    public float CurrentAlarmLevel { get { return _currentAlarmLvl; } set { _currentAlarmLvl = value; } }
    public float MaxAlarmLevel { get { return _maxAlarmLvl; } set { _maxAlarmLvl = value; } }
    public float CurrentAlarmDepletion { get { return _currentAlarmDepletion; } set { _currentAlarmDepletion = value; } }
    public float AlarmIncrease { get { return _alarmIncrease; } set { _alarmIncrease = value; } }

    public EquipmentItem ShakerSlotA { get { return _shakerSlotA; } set { _shakerSlotA = value; } }
    public EquipmentItem ShakerSlotB { get { return _shakerSlotB; } set { _shakerSlotB = value; } }
    public EquipmentItem CollectorSlotA { get { return _collectorSlotA; } set { _collectorSlotA = value; } }
    public EquipmentItem CollectorSlotB { get { return _collectorSlotB; } set { _collectorSlotB = value; } }
    public EquipmentItem StrikerSlotA { get { return _strikerSlotA; } set { _strikerSlotA = value; } }
    public EquipmentItem StrikerSlotB { get { return _strikerSlotB; } set { _strikerSlotB = value; } }

    public bool GoldCollected { get { return _goldCollected; } set { _goldCollected = value; } }
    public bool ItemCollected { get { return _itemCollected; } set { _itemCollected = value; } }

    public bool SlowMoEnabled { get { return _slowMoEnabled; } set { _slowMoEnabled = value; } }

    //Level completed analytics data
    public int GoldItmCount {  get { return _goldItmCount; } set { _goldItmCount = value; } }
    public int EqpItmCount {  get { return _eqpItmCount; } set { _eqpItmCount = value; } }
    public int GoldItmLostCount {  get { return _goldItmLostCount; } set { _goldItmLostCount = value; } }
    public int EqpItmLostCount {  get { return _eqpItmLostCount; } set { _eqpItmLostCount = value; } }
    //public bool Win {  get { return _win; } set { _win = value; } }
    public int LvlAttempts {  get { return _lvlAttempts; } set { _lvlAttempts = value; } }

    public bool ShowInvFb {  get { return _showInvFb; } set { _showInvFb = value; } }
    public bool ResetShop {  get { return _resetShop; } set { _resetShop = value; } }
    public bool LevelUnlocked {  get { return _levelUnlocked; } set { _levelUnlocked = value; } }

    //Finger screen limits
    public Transform MinLeftFingerRef {  get { return _minLeftFingerRef; } private set { _minLeftFingerRef = value; } }
    public Transform MaxLeftFingerRef { get { return _maxLeftFingerRef; } private set { _maxLeftFingerRef = value; } }
    public Transform MinRightFingerRef { get { return _minLeftFingerRef; } private set { _minLeftFingerRef = value; } }
    public Transform MaxRightFingerRef { get { return _maxRightFingerRef; } private set { _maxRightFingerRef = value; } }

    //Guard canvases
    public Transform FrontGuardCanvas {  get { return _frontGuardCanvas; } set { _frontGuardCanvas = value; } }
    public Transform FarGuardCanvas { get { return _farGuardCanvas; } set { _farGuardCanvas = value; } }

    public float MaxComboTime {  get { return _maxcomboTime; } private set { } }
    
    #endregion

    #region Private Serialized Fields
        //Level references
        [SerializeField]
    private Transform _floorRef;
    [SerializeField]
    private Transform _minHRef, _maxHRef;
    

    //Coconut
    [SerializeField]
    private float _fruitFlyTime;
    [SerializeField]
    private float _fruitFallTime;
    [SerializeField]
    private FruitTree _fruitTree;
    [SerializeField]
    private float _fruitSpawnTime;
    [SerializeField]
    private float _maxFruitOffset;
    //Monkeys
    [SerializeField]
    private CollectorMonkey _collectorMonkey;
    [SerializeField]
    private StrikerMonkey _strikerMonkey;
    [SerializeField]
    private Image _leftMonkeyBodyImg, _leftMonkeyLegsImg;
    [SerializeField]
    private Image _rightMonkeyImg;
    //Guards
    [SerializeField]
    private Transform _frontGuardCanvas, _farGuardCanvas;

    [SerializeField]
    private float _frameTime;
    [SerializeField]
    private float _maxCollectorAreaHeight;

    

    //UI
    [SerializeField]
    private Text _scoreText;
    [SerializeField]
    private float _scoreTimeCount;
    [SerializeField]
    private Text _alarmText;

    [SerializeField]
    private Text _levelTimeText;

    [SerializeField]
    private List<Stage> _stageList;
    [SerializeField]
    private Level _currentLevel;
    [SerializeField]
    private List<Level> _levelList;
    [SerializeField]
    private LevelFinishedScreen _lvlFinishedScr;
    [SerializeField]
    private GameObject _loseScreen;
    [SerializeField]
    private List<Guard> _guardPool;
    [SerializeField]
    private Text _levelText;

    [SerializeField]
    private float _maxcomboTime;

    [SerializeField]
    private Fruit _fr1, _fr2;

    [SerializeField]
    private List<ParticleSystem> _goldHitPsPool, _equipmentHitPsPool, _poolSplashParticle, _poolFeatherParticle, _goldItemSplashPsPool, _equipmetSplashPsPool, _alarmPsPool, _alarmWarnPsPool;

    [SerializeField]
    private List<Gradient> _fruitSplashColorGradList, _featherSplashColorGradList;

    //Screens
    //[SerializeField]
    private ShopMenu _shopScreen;
    //[SerializeField]
    private InventoryScreen _invScreen;
	#endregion

	#region Private Non-serialized Fields
    private Transform _minLeftFingerRef, _maxLeftFingerRef, _minRightFingerRef, _maxRightFingerRef;
    private float _gravity;
    private Transform _currentHRef;
    private GameObject _mainCamera;

    private List<CollectedFruit> _stageCollectedFruits;
    private int _stageSpawnedFruitCount, _stageDismissedFruitCount, _stageSucceededFruitCount;
    private Touch _leftFingerTouch, _rightFingerTouch;
    

    private int _score, _gold;
    private int _vibration; //0 diabled, 1 enabled
    
    private float _currentLevelTime;
    private int _currentStageIndex, _currentLevelIndex;

    private GAME_STATE _gameState, _lastState;

    private AsyncOperation _levelLoaded;
    private Stage _currentStage;




    private float _currentAlarmLvl, _maxAlarmLvl, _alarmIncrease;
    private float _currentAlarmDepletion;   //alarm depletion over second
    private float _fleeTimer;

    private EquipmentItem _shakerSlotA, _shakerSlotB, _collectorSlotA, _collectorSlotB, _strikerSlotA, _strikerSlotB;

    private bool _itemCollected, _goldCollected;
    private bool _flyngFruits;

    private int _comboCount;
    private int _currentLaunchAudioIndex;
    private int _currentCollectedAudioIndex;
    private float _lastHitTime;

    private bool _slowMoEnabled;
    private float _slowMoTimer;

    private int _goldItmCount, _eqpItmCount;    //collected items
    private int _goldItmLostCount, _eqpItmLostCount;
    private int _lvlAttempts;
    private bool _win;

    private bool _showInvFb;
    private bool _resetShop;
    private bool _levelUnlocked;

    private int _currentGoldPsIndex, _currentEquipmentPsIndex, _currentPoolSplashIndex, _currentPoolFeatherIndex, _currentGoldItemSplashIndex, _currentEquipmentSplashIndex, _currentAlarmPsIndex, _currentAlarmWarnPsIndex;
    private int _scoreResult, _restToFive;   //used to store combo total score
    #endregion

}
