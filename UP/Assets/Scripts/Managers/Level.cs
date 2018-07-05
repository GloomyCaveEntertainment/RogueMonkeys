/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal, Gloomy Cave Entertainment
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {

	#region Public Data
    public enum L_STATE { IDLE = 0, LOADING, START, RUN, WAITNG_FLYING_FRUITS, RELOADING_SACK_END, FINISHED }
    public enum NET_SIZE { SMALL = 0, MEDIUM, LARGE, EXTRA_LARGE}
    public enum GUARD_POSITION { RIGHT = 0, LEFT, ALL }
    public enum INTRO_STATE { READY = 0, READY_TO_TREE, STEADY_TREE, STEADY_RETURN, GO }
    /// <summary>
    /// LOCKED : info shwon but cant play
    /// UNAVAILABLE : info hidden
    /// </summary>
    public enum AVAILABILITY_STATE { UNLOCKED = 0, LOCKED, COMPLETED, FAILED, UNAVAILABLE }
    public enum RANK { S = 0, A, B, C, D, E, F }
    public const float _endDelayTime = 2.5f;
    public const float _reloadingSackEndTimeOffset = 1f;
    public const float _readyIdleTime = 0.5f;
    public const float _readyIntroTime = 1f;   
    public const float _steadyIntroTime = 1.5f;
    public const float _goIIntroTime = 0.5f;
    //public const float _shakeAudioCooldown = 0.5f;                


    [System.Serializable]
    public class FruitSpawn
    {
        public int FruitTypeIndex;
        public float SpawnRatio;

        //Ctor
        public FruitSpawn(int fType, float sRatio)
        {
            FruitTypeIndex = fType;
            SpawnRatio = sRatio;
        }

    }

    [System.Serializable]
    public class ItemSpawn
    {
        public string Id;
        public float SpawnRatio;

        //Ctor
        public ItemSpawn(string id, float sRatio)
        {
            Id = id;
            SpawnRatio = sRatio;
        }
    }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	/*void Start () {
		
	}*/
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case L_STATE.IDLE:

                break;

            case L_STATE.LOADING:
                if (_asynOp.isDone)
                {
                    GameMgr.Instance.GetManagerReferences();
                    LoadReferences();
                    GameMgr.Instance.StartCurrentLevel();
                }
                break;
            
            case L_STATE.START:
                _timer += Time.deltaTime;
                switch (_introSt)
                {
                    case INTRO_STATE.READY:
                        if (_timer >= _readyIntroTime)
                        {
                            _timer = 0f;
                            _introSt = INTRO_STATE.READY_TO_TREE;
                        }
                            break;
                    case INTRO_STATE.READY_TO_TREE:
                        GameMgr.Instance.GameCamera.transform.position = new Vector3(Mathf.Lerp(_introCamStartPos.x, _introCamShakerPos.x, _timer / _readyIntroTime), Mathf.Lerp(_introCamStartPos.y, _introCamShakerPos.y, _timer / _readyIntroTime), GameMgr.Instance.GameCamera.transform.position.z);
                        GameMgr.Instance._CollectorMonkey._Sack.transform.position = GameMgr.Instance.GameCamera.transform.position - _sakCamRelativeOffset;
                        if (_timer >= _readyIntroTime)
                        {
                            _timer = 0f;
                            _introSt = INTRO_STATE.STEADY_TREE;
                            //No "Steady" on Japanese
                            if (LocalizationService.Instance.Localization.CompareTo("Japanese")!=0)
                                UIHelper.Instance.LvlIntroText.text = LocalizationService.Instance.GetTextByKey("loc_steady");
                            //Shaker animation
                            _fruitTree.Shake();
                        }
                        break;
                    case INTRO_STATE.STEADY_TREE:
                        /*_audioTimer += Time.deltaTime;
                        if (_audioTimer >= _shakeAudioCooldown)
                        {
                            _audioTimer = 0f;
                            AudioController.Play("shake_intro", GameMgr.Instance.GameCamera.transform);
                        }*/
                        if (_timer >= _steadyIntroTime*0.75f)
                        {
                            _timer = 0f;
                            _introSt = INTRO_STATE.STEADY_RETURN;
                        }
                        break;

                    case INTRO_STATE.STEADY_RETURN:
                        GameMgr.Instance.GameCamera.transform.position = new Vector3(Mathf.Lerp(_introCamShakerPos.x, _introCamStartPos.x, _timer / (_steadyIntroTime * 0.25f)), Mathf.Lerp(_introCamShakerPos.y, _introCamStartPos.y, _timer / (_steadyIntroTime * 0.25f)), GameMgr.Instance.GameCamera.transform.position.z);
                        GameMgr.Instance._CollectorMonkey._Sack.transform.position = GameMgr.Instance.GameCamera.transform.position - _sakCamRelativeOffset;
                        //GameMgr.Instance.GameCamera.transform.position = Vector3.Lerp(_introCamShakerPos, _introCamStartPos, );
                        if (_timer >= _steadyIntroTime * 0.25f)
                        {
                            _timer = 0f;
                            _introSt = INTRO_STATE.GO;
                            UIHelper.Instance.LvlIntroText.text = LocalizationService.Instance.GetTextByKey("loc_go");
                        }
                        break;

                    case INTRO_STATE.GO:
                        if (_timer >= _steadyIntroTime)
                        {
                            _timer = 0f;
                            _state = L_STATE.RUN;
                            UIHelper.Instance.LvlIntroText.gameObject.SetActive(false);
                            _fruitTree.InitPalmTree();
                            _fruitTree.StartSpawn();
                            //UIHelper.Instance.ShowFakeShack(false);
                        }
                        break;
                }
                break;

            case L_STATE.RUN:
                if (UIHelper.Instance._ShowLevelTime && Tutorial.Instance == null)
                {
                    _timer += Time.deltaTime;
                    UIHelper.Instance.SetLvlScreenTime(_levelTime - _timer);
                    if (_timer >= _levelTime)
                    {
                        _fruitTree.Stop(false);
                        _timer = 0f;
                        _state = L_STATE.WAITNG_FLYING_FRUITS;
                        
                        //TODO: Show end screen, whatever
                    }
                }
                
                break;

            case L_STATE.WAITNG_FLYING_FRUITS:
                _timer += Time.deltaTime;
                if (_timer >= _endDelayTime)
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
                    {
                        _timer = 0f;
                        //GameMgr.Instance.LevelEnded();
                        GameMgr.Instance._CollectorMonkey._Sack.Reload();
                        GameMgr.Instance._CollectorMonkey.SetTick(true);
                        _state = L_STATE.RELOADING_SACK_END;
                        _reloadingSackEndTime = GameMgr.Instance._CollectorMonkey._ReloadTime + _reloadingSackEndTimeOffset;
                        GameMgr.Instance.WaitForFruits();
                        //Stop Level audio
                        if (AudioController.IsPlaying(_themeAudioId))
                            AudioController.Stop(_themeAudioId);
                        GameMgr.Instance.ResetCombo();  //end combo immediately since there aren't more fruits to collect
                    }
                }
                break;

            case L_STATE.RELOADING_SACK_END:
                _timer += Time.deltaTime;
                if (_timer >= _reloadingSackEndTime)
                {
                    _state = L_STATE.FINISHED;
                    GameMgr.Instance.LevelEnded();
                }
                break;

            case L_STATE.FINISHED:

                break;

        }
	}
	#endregion

	#region Public Methods

    /// <summary>
    /// Setup level environment as well as level logic stats
    /// </summary>
    public AsyncOperation SetupLevel()
    {
        Debug.Log("SetupLEvel");
        //Load scene references
        return LoadLevelLayout(); 
    }

    public void StartLevel()
    {
        _timer = 0f;

        if (Tutorial.Instance != null /*&& Tutorial.Instance._State != Tutorial.TUT_STATE.IDLE*/)
        {
            _state = L_STATE.RUN;
            UIHelper.Instance.LvlIntroText.gameObject.SetActive(false);
            _fruitTree.InitPalmTree();
            _fruitTree.StartSpawn();
        }
        else
        {
            _state = L_STATE.START;
            _introSt = INTRO_STATE.READY;
            //UIHelper.Instance.ShowFakeShack(true);
            UIHelper.Instance.LvlIntroText.gameObject.SetActive(true);
            UIHelper.Instance.LvlIntroText.text = LocalizationService.Instance.GetTextByKey("loc_ready");
            //TOCHECK
            _introCamStartPos = GameMgr.Instance.GameCamera.transform.position;
            _sakCamRelativeOffset = _introCamStartPos - GameMgr.Instance._CollectorMonkey._Sack.transform.position;


            if (AudioController.IsPlaying("Menu_Theme"))
                AudioController.Stop("Menu_Theme");
            /*_musicAudioRef = */AudioController.Play(_themeAudioId);//, GameMgr.Instance.GameCamera.transform);
        }  
    }

    public void EndLevel()
    {
        _state = L_STATE.FINISHED;
        if (AudioController.IsPlaying(_themeAudioId))
            AudioController.Stop(_themeAudioId);
    }
    public void UnloadLevelLayout()
    {
        SceneManager.UnloadSceneAsync(_sceneLayoutId);
    }

    public int GetMaxScore()
    {
        return _maxScore;
    }

    public bool CheckForHighScore(int newScore)
    {
        if (newScore > _maxScore)
        {
            _maxScore = newScore;
            return true;
        }
        else
            return false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public SceneLayout GetSceneLayout()
    {
        return _currentSL;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public AVAILABILITY_STATE GetAvState()
    {
        return _availabilitySt;
    }
    public L_STATE GetLevelState()
    {
        return _state;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<FruitSpawn> GetFruitTypeSpawnList()
    {
        return _fruitPoolType;
    }

    /// <summary>
    /// Get layout references and pass some of them to GameMgr
    /// </summary>
    public void LoadReferences()
    {
        Debug.Log("Load References");
        //Get references from loaded scene
        GameObject sceneLO = null;
        sceneLO = GameObject.FindGameObjectWithTag("SceneLayout");
        if (sceneLO == null)
            Debug.LogError("No Layout found!");
        _currentSL = sceneLO.GetComponent<SceneLayout>();
        if (_currentSL == null)
            Debug.LogError("No layout comp attached!");

        _netObjectList = _currentSL.GetNetList();
        _fruitTree = _currentSL.GetFruitTree();
        _background = _currentSL.GetBackground();
        _skyBg = _currentSL.GetSkyBackground();
        _netHeightRef = _currentSL.GetNetHeightRef();
        _floorHeightRef = _currentSL.GetFloorHeightRef();
        _leftBotSpawnLimit = _currentSL.GetLeftBotspawnLimit();
        _rightTopSpawnLimit = _currentSL.GetRightTopSpawnLimit();
        _leftCollectorLimit = _currentSL.GetLeftCollectorLimit();
        _rightCollectorLimit = _currentSL.GetRightCollectorLimit();
        _minLeftFingerRef = _currentSL.GetMinLeftFingerRef();
        _maxLeftFingerRef = _currentSL.GetMaxLeftFingerRef();
        _minRightFingerRef = _currentSL.GetMinRightFingerRef();
        _maxRightFingerRef = _currentSL.GetMaxRightFingerRef();
        _introCamShakerPos = _currentSL.GetCamShakerPos();
        _introCamStartPos = _currentSL.GetCamInitPos();
        //Sprite setup
        //Deprecated: already baked in scenes
        //_background.GetComponent<Image>().sprite = Resources.Load(_backgroundId, typeof(Sprite)) as Sprite;
        //_netObject.GetComponent<Image>().sprite = Resources.Load(_netSpriteId, typeof(Sprite)) as Sprite;
        //_fruitTree.GetComponent<Image>().sprite = Resources.Load(_treeId, typeof(Sprite)) as Sprite;

        //Background
        //_background.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);//world canvas so setup according to current resolution
        //_skyBg.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, 400f * (Screen.height/1080f));
        Debug.Log(GameMgr.Instance.GameCamera.aspect);
        GameMgr.Instance.GameCamera.aspect = 16f/9f;
        //Setup Tree
        _fruitTree.SetGoldPool(_goldItemPoolType);
        _fruitTree.SetEquipmentPool(_equipmentItemPoolType);
        _fruitTree.SetFruitPool(_fruitPoolType);
        _fruitTree.SetFruitStats(_fSpawnTime, _fFallSpeed, _fFlyTime, _goldItemSpawnChance, _equipmentItemSpawnChance);
        
        _fruitTree.SetIdle();
        GameMgr.Instance._FruitTree = _fruitTree;

        //Monkeys start position
        //GameMgr.Instance._StrikerMonkey.transform.position = _currentSL.GetStrikerStartPos().position;
        //GameMgr.Instance._CollectorMonkey.transform.position = _currentSL.GetCollectorStartPos().position;
        GameMgr.Instance.MaxAlarmLevel = _maxAlarmLvl;
        GameMgr.Instance.AlarmIncrease = _missedFruitAlarm;
        GameMgr.Instance.CurrentAlarmDepletion = _alarmDepletion;
        //Guard setup
        //Copy position list to avoid repetition on random selection
        if (_auxGuardRightPosList == null)
            _auxGuardRightPosList = new List<Transform>();
        else
            _auxGuardRightPosList.Clear();
        if (_auxGuardLeftPosList == null)
            _auxGuardLeftPosList = new List<Transform>();
        else
            _auxGuardLeftPosList.Clear();

        //Save left/right positions depending on current level guard distribution list
        if (_guardDistribution.Contains(true))
        {
            foreach (Transform gPos in _currentSL.GetGuardPositionList(true))
                _auxGuardRightPosList.Add(gPos);
        }
        if (_guardDistribution.Contains(false))
        {
            foreach (Transform gPos in _currentSL.GetGuardPositionList(false))
                _auxGuardLeftPosList.Add(gPos);
        }
        /*
        switch (_guardDistribution)
        {
            case GUARD_POSITION.RIGHT:
                foreach (Transform gPos in _currentSL.GetGuardPositionList(true))
                    _auxGuardRightPosList.Add(gPos);
                break;

            case GUARD_POSITION.LEFT:
                foreach (Transform gPos in _currentSL.GetGuardPositionList(false))
                    _auxGuardLeftPosList.Add(gPos);
                break;

            case GUARD_POSITION.ALL:
                foreach (Transform gPos in _currentSL.GetGuardPositionList(true))
                    _auxGuardRightPosList.Add(gPos);
                foreach (Transform gPos in _currentSL.GetGuardPositionList(false))
                    _auxGuardLeftPosList.Add(gPos);
                break;
        }
        */
        
        if (_guardList == null)
            _guardList = new List<Guard>();
        else
            _guardList.Clear();

        //Select random positions wihtout repetition
        //bool isRightPos = _guardDistribution != GUARD_POSITION.LEFT;
        for (int i = 0; i < GameMgr.Instance._GuardPool.Count; ++i)
        {
            if (i < _guardDistribution.Count)
            {
                //Right guard
                if (_guardDistribution[i])
                {
                    _auxSelectedGuardPosIndex = Random.Range(0, _auxGuardRightPosList.Count);
                    GameMgr.Instance._GuardPool[i].transform.position/*GetComponent<RectTransform>().position*/ = _auxGuardRightPosList[_auxSelectedGuardPosIndex].transform.position;
                }
                else
                {
                    _auxSelectedGuardPosIndex = Random.Range(0, _auxGuardLeftPosList.Count);
                    GameMgr.Instance._GuardPool[i].transform.position/*GetComponent<RectTransform>().position*/ = _auxGuardLeftPosList[_auxSelectedGuardPosIndex].transform.position;
                }
                Debug.Log("Selected inddex " + _auxSelectedGuardPosIndex + "   and [i] " + i);
                
                //GameMgr.Instance._GuardPool[i].SetupGuard(); //Done later from GameMgr since we haven't already canvases references
                //GameMgr.Instance._GuardPool[i].gameObject.SetActive(true);
                //GameMgr.Instance._GuardPool[i].GetComponent<Image>().enabled = true;
                _guardList.Add(GameMgr.Instance._GuardPool[i]);
                if (_guardDistribution[i])
                    _auxGuardRightPosList.RemoveAt(_auxSelectedGuardPosIndex);
                else
                    _auxGuardLeftPosList.RemoveAt(_auxSelectedGuardPosIndex);

                //switch list in next iteration if guards are disributed in both sides
                /*if (_guardDistribution == GUARD_POSITION.ALL)
                    isRightPos = !isRightPos;*/
            }
            else
                GameMgr.Instance._GuardPool[i].gameObject.SetActive(false);
        }
        //canvas render sort order
        GameMgr.Instance._GuardPool.Sort((a, b) => (b.transform.position.y.CompareTo(a.transform.position.y)));  
        for (int i = 0; i < GameMgr.Instance._GuardPool.Count; ++i)
            GameMgr.Instance._GuardPool[i].transform.SetSiblingIndex(i);


        //Set timer and state
        UIHelper.Instance.SetLvlScreenTime(_levelTime);
        UIHelper.Instance.SetAlarmLevel();
        UIHelper.Instance.ResetAlarmUI();
        _state = L_STATE.IDLE;
        if (_musicAudioRefList == null)
            _musicAudioRefList = new List<AudioObject>();
    }

    public void Pause(bool pause)
    {
        if (pause && _state != L_STATE.IDLE)
        {
            _lastState = _state;
            _state = L_STATE.IDLE;
        }
        else if (!pause && _state == L_STATE.IDLE)
        {
            _state = _lastState;
        }
        if (pause)
        {
            if (_musicAudioRefList.Count !=0)
                _musicAudioRefList.Clear();
            foreach (AudioObject ao in GameObject.FindObjectsOfType<AudioObject>())
            {
                ao.Pause(0.2f);
                _musicAudioRefList.Add(ao);
            }              
        }
        else
        {
            foreach (AudioObject ao in _musicAudioRefList)
                ao.Play();
        }        
    }
	#endregion


	#region Private Methods
    /// <summary>
    /// Load Scene references
    /// </summary>
    private AsyncOperation LoadLevelLayout()
    {
        //Debug.Log(transform.parent+"___"+transform.name + "____" + _sceneLayoutId);
        _state = L_STATE.LOADING;
        //Load Scene
        _asynOp = SceneManager.LoadSceneAsync(_sceneLayoutId, LoadSceneMode.Additive);
        return _asynOp;
        

    }
	#endregion


	#region Properties
    public List<GameObject> NetObject { get { return _netObjectList; } private set { _netObjectList = value; } }
    public Transform NetHeightRef { get { return _netHeightRef; } set { _netHeightRef = value; } }
    public Transform FloorHeightRef { get { return _floorHeightRef; } set { _floorHeightRef = value; } }
    public Transform LeftBotSpawnLimit { get { return _leftBotSpawnLimit; } set { _leftBotSpawnLimit = value; } }
    public Transform RightTopSpawnLimit { get { return _rightTopSpawnLimit; } set { _rightTopSpawnLimit = value; } }
    public Transform LeftCollectorLimit { get { return _leftCollectorLimit; } set { _leftCollectorLimit = value; } }
    public Transform RightCollectorLimit { get { return _rightCollectorLimit; } set { _rightCollectorLimit = value; } }
    public Transform MinLeftFingerRef { get { return _minLeftFingerRef; } set { _minLeftFingerRef = value; } }
    public Transform MaxLeftFingerRef { get { return _maxLeftFingerRef; } set { _maxLeftFingerRef = value; } }
    public Transform MinRightFingerRef { get { return _minRightFingerRef; } set { _minRightFingerRef  = value; } }
    public Transform MaxRightFingerRef { get { return _maxRightFingerRef; } set { _maxRightFingerRef = value; } }
    public AVAILABILITY_STATE AvailabilitySt { get { return _availabilitySt; } set { _availabilitySt = value; } }
    public string SceneLayoutId { get { return _sceneLayoutId; } set { _sceneLayoutId = value; } }
    public Vector3 IntroCamStartPos {  get { return _introCamStartPos; } set { _introCamStartPos = value; } }
    public Vector3 IntroCamShakerPos {  get { return _introCamShakerPos; } set { _introCamShakerPos = value; } }
    public List<Guard> GuardList { get { return _guardList; } set { _guardList = value; } }

    public int MaxScore { get { return _maxScore; } set { _maxScore = value; } }
    public float LevelTime { get { return _levelTime; } set { _levelTime = value; } }
    public int MinRankSuperScore { get { return _minRankSuperScore; } set { _minRankSuperScore = value; } }
    public float PenaltyPerAlarmPt { get { return _penaltyPerAlarmPt; } set { _penaltyPerAlarmPt = value; } }
    public RANK Rank { get { return _rank; } set { _rank = value; } }

    public float FruitSpawnTime { get { return _fSpawnTime; } private set { _fSpawnTime = value; } }
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    private string _sceneLayoutId;   //scene id with base level layout
    [SerializeField]
    private string _backgroundId;
    [SerializeField]
    private string _treeId;
    [SerializeField]
    private string _netSpriteId;
    [SerializeField]
    private string _themeAudioId;
    [SerializeField]
    private NET_SIZE _netSize;

    
    //Fruit Tree stats
    [SerializeField]
    private float _fSpawnTime;
    [SerializeField]
    private float _fFallSpeed;
    [SerializeField]
    private float _fFlyTime;    //time to reach highest height
    [SerializeField]
    private List<FruitSpawn> _fruitPoolType;    //defines the fruit types and its chance to be spawned
    [SerializeField]
    private List<ItemSpawn> _goldItemPoolType, _equipmentItemPoolType;
    [SerializeField]
    private float _goldItemSpawnChance;
    [SerializeField]
    private float _equipmentItemSpawnChance;
    //Level stats
    [SerializeField]
    private float _levelTime;
    [SerializeField]
    private float _maxAlarmLvl;
    [SerializeField]
    private float _missedFruitAlarm;
    [SerializeField]
    private float _alarmDepletion;  //over second
    [SerializeField]
    private int _guardCount;
    [SerializeField]
    private List<bool> _guardDistribution; //true = right, false = left

    [SerializeField]
    private float _penaltyPerAlarmPt;   //penalty over score per current alarm point 
    [SerializeField]
    private int _minRankSuperScore; //min score to get S rank, 
	#endregion

	#region Private Non-serialized Fields
    private RANK _rank;
    private L_STATE _state, _lastState;
    private SceneLayout _currentSL;
    private Transform _minLeftFingerRef, _maxLeftFingerRef, _minRightFingerRef, _maxRightFingerRef;

    private List<GameObject> _netObjectList;
    private Transform _netHeightRef;     //Transform which defines fruit max trayectory height
    private Transform _floorHeightRef;
    private Transform _leftBotSpawnLimit, _rightTopSpawnLimit;  //defines fruit spawn area
    private Transform _leftCollectorLimit, _rightCollectorLimit;    //defines fruit left fall area (Collector monkey)
    private FruitTree _fruitTree;
    private GameObject _background;
    private GameObject _skyBg;

    [SerializeField]
    private int _maxScore;
    private float _timer;
    private AsyncOperation _asynOp;
    [SerializeField]
    private AVAILABILITY_STATE _availabilitySt;
    

    private List<Guard> _guardList;
    private List<Transform> _auxGuardRightPosList, _auxGuardLeftPosList;
    private int _auxSelectedGuardPosIndex;

    private bool _flyngFruits;
    private float _reloadingSackEndTime;

    private INTRO_STATE _introSt;
    private Vector3 _introCamStartPos, _introCamShakerPos;

    private List<AudioObject> _musicAudioRefList;

    private Vector3 _sakCamRelativeOffset;
    #endregion
}
