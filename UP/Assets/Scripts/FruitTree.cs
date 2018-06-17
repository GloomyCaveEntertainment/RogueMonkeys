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

public class FruitTree : MonoBehaviour {

	#region Public Data
    public enum PT_STATE { IDLE = 0, SPAWNING, ENDED, INTRO_SHAKE }

    public const float _shakeAudioCooldown = 0.6f;
    #endregion


    #region Behaviour Methods
    // Use this for initialization
    //void Start () {
        //InitPalmTree();
	//}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case PT_STATE.IDLE:               
                break;

            case PT_STATE.INTRO_SHAKE:
                //Intro animation
                _shakingTimer += Time.deltaTime;
                if (_shaking)
                {
                    if (_shakingTimer >= _shakingTime)
                    {
                        _shaking = false;
                        _img.sprite = _shakeSpList[0];  //idle
                        _shakerImg.sprite = _shakerMonkeySpList[0];
                        _shakerImg.rectTransform.anchoredPosition = _shakerOffsetList[0];
                    }
                    else
                    {
                        _frameTimer += Time.deltaTime;
                        if (_frameTimer >= _gameMgr.FrameTime * 0.5f)
                        {
                            _frameTimer = 0f;
                            _frameIndex = (_frameIndex + 1) % _shakeSpList.Count;
                            _img.sprite = _shakeSpList[_frameIndex];
                            _shakerImg.sprite = _shakerMonkeySpList[_frameIndex];
                            _shakerImg.rectTransform.anchoredPosition = _shakerOffsetList[_frameIndex];
                        }
                    }
                    //SFX
                    _audioTimer += Time.deltaTime;
                    if (_audioTimer >= _shakeAudioCooldown)
                    {
                        _audioTimer = 0f;
                        AudioController.Play("shake_intro", GameMgr.Instance.transform,0.5f);
                        //Leaves particles
                        SetLeavesParticle(new Vector2(Random.Range(_spawnMinXLimit.position.x, _spawnMaxXLimit.position.x), Random.Range(_spawnMinYLimit.position.y, _spawnMaxYLimit.position.y)));
                    }
                }
                break;

            case PT_STATE.SPAWNING:
                //Spawn Logic
                _timer += Time.deltaTime;
                if (_timer >= _currentSpawnTime)
                {
                    SpawnFruit();
                    _timer = 0f;
                }

                //Sprite
                _shakingTimer += Time.deltaTime;
                if (_shaking)
                {
                    //SFX
                    _audioTimer += Time.deltaTime;
                    if (_audioTimer >= _shakeAudioCooldown)
                    {
                        _audioTimer = 0f;
                        AudioController.Play("shake_intro", GameMgr.Instance.transform, 0.5f);
                    }

                    if (_shakingTimer >= _shakingTime)
                    {
                        _shaking = false;
                        _img.sprite = _shakeSpList[0];  //idle;
                        _shakerImg.sprite = _shakerMonkeySpList[0];
                        _shakerImg.rectTransform.anchoredPosition = _shakerOffsetList[0];
                    }
                    else
                    {
                        _frameTimer += Time.deltaTime;
                        if (_frameTimer >= _gameMgr.FrameTime*0.5f)
                        {
                            _frameTimer = 0f;
                            _frameIndex = (_frameIndex + 1) % _shakeSpList.Count;
                            _img.sprite = _shakeSpList[_frameIndex];
                            _shakerImg.sprite = _shakerMonkeySpList[_frameIndex];
                            _shakerImg.rectTransform.anchoredPosition = _shakerOffsetList[_frameIndex];
                        }
                    }
                }
                else
                {
                    _frameTimer += Time.deltaTime;
                    if (_shakingTimer >= _shakeCooldown)
                    {
                        _shaking = true;
                        _frameTimer = 0f;
                        _shakingTimer = 0f;
                        _frameIndex = 0;
                        //AudioController.Play("shake_1");
                        _audioTimer = 0f;
                        AudioController.Play("shake_intro", GameMgr.Instance.transform, 0.5f);
                    }
                }
                //Audio
                /*_treeAudioTimer += Time.deltaTime;
                if (_treeAudioTimer >= _currentAudioTime)
                {
                    _currentAudioTime = Random.Range(_minTreeAudioTime, _maxTreeAudioTime) + AudioController.Play("shake_1").clipLength;
                    _treeAudioTimer = 0f;
                }*/
                break;

            case PT_STATE.ENDED:

                break;
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
        _currentFruitFallTime = _fallFruitTime;
        _currentGoldSpawnChance = _goldSpawnChance;
        _currentEquipmentSpawnChance = _equipmentSpawnChance;
        _currentFruitFlyTime = _fruitFlyTime;
        if (_slotA != null)
            LoadSlotItem(_slotA);
        if (_slotB != null)
            LoadSlotItem(_slotB);

        GenerateFruitPool(_levelFruitSpawnTypeList, _currentGoldSpawnChance, _currentEquipmentSpawnChance);
    }

    /// <summary>
    /// 
    /// </summary>
    public void InitPalmTree()
    {
        Debug.Log("INIT PALM TREE");
        if (_gameMgr == null)
            _gameMgr = GameMgr.Instance;
        if (_img == null)
            _img = GetComponent<Image>();
        if (_shakerImg == null)
            _shakerImg = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        if (_shakerImg == null)
            Debug.LogError("No shaker monkey img found!");
        //init coconut pool
        //if (_fruitList != null)
            //_fruitList.Clear();
        /*_fruitList = new List<Fruit>();
        for (int i = 0; i < _fruitInitCountPool; ++i)
        {
            _fruitList.Add((Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>());
            _fruitList[i].gameObject.SetActive(false);
        }*/
        _frameIndex = 0;
        //_currentPoolIndex = 0;
        //_currentClusterPoolIndex = 0;
        //_currentMultifruitPoolIndex = 0;
        //_currentEggPoolIndex = 0;
        _shaking = true;
        _shakingTimer = 0f;
        _state = PT_STATE.IDLE;
        _img.sprite = _shakeSpList[0];  //idle;
        _shakerImg.rectTransform.anchoredPosition = _shakerOffsetList[0];
        _audioTimer = 0f;
        AudioController.Play("shake_intro", GameMgr.Instance.transform, 0.5f);
        //_shakerImg.sprite = _shakerMonkeySpList[0];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spawnTime"></param>
    /*public void StartPalmTree(float spawnTime)
    {
        _currentSpawnTime = spawnTime;
        _timer = 0f;
        _state = PT_STATE.SPAWNING;
        _currentPoolIndex = 0;
    }*/

    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fruitSpawnTypeList"></param>
    public void SetFruitPool(List<Level.FruitSpawn> fruitSpawnTypeList)
    {
        _levelFruitSpawnTypeList = fruitSpawnTypeList;
    }
    public void SetGoldPool(List<Level.ItemSpawn> probList)
    {
        _goldItemPoolType = probList;
    }
    public void SetEquipmentPool(List<Level.ItemSpawn> probList)
    {
        _equipmentItemPoolType = probList;
    }

    public void SetGoldSpawnChance(float ratio)
    {
        _goldSpawnChance = ratio;
    }

    public void SetEquipmentSpawnChance(float ratio)
    {
        _equipmentSpawnChance = ratio;
    }
    /// <summary>
    /// Create fruit poolusing ratio spawn values with shuffle bags
    /// </summary>
    /// <param name="fruitSpawnTypeList"></param>
    /// <param name="listSize">shuffle bag size: defines randomness</param>
    public void GenerateFruitPool(List<Level.FruitSpawn> fruitSpawnTypeList, float goldSpawnChance, float equipmentSpawnChance, int listSize = 1000)
    {
        float sum = 0;
        float nonValuableProb = 0f;
        Fruit temp = null;
        float rndom = -1;
        List<string> _auxIdList = null; 


        //(1) Check total sum = 1f
        foreach (Level.FruitSpawn fs in fruitSpawnTypeList)
            sum += fs.SpawnRatio;
        if (sum - 1f > 0.0001f)
            Debug.LogError("Fruit prob distribution wrong!"+sum.ToString("0.0000000000"));

        //(2)Create shuffle bag based on chances and setup each one
        if (_fruitList != null)
            _fruitList.Clear();
        else
            _fruitList = new List<Fruit>();

        //Calculate non equipment or gold item spawn chance
        foreach (Level.FruitSpawn fs in fruitSpawnTypeList)
            nonValuableProb += fs.SpawnRatio;
        nonValuableProb -= (goldSpawnChance + equipmentSpawnChance);
        //Add non valuable fruits to pool
        foreach (Level.FruitSpawn fs in fruitSpawnTypeList)
        {
            for (int i = 0; i < Mathf.FloorToInt(nonValuableProb *fs.SpawnRatio * listSize); ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                /*if (fs.FruitTypeIndex == (int)Fruit.F_TYPE.GOLD_ITEM)
                    temp.SetupFruitAsGoldItem((int)_goldItemsTypePool[i]);*/
                temp.SetupFruit(fs.FruitTypeIndex);
                temp.gameObject.SetActive(false);
                _fruitList.Add(temp);
            }
        }
        //Add gold items
        Debug.Log("Generating " + Mathf.FloorToInt(goldSpawnChance * listSize) + " gold items");
        _auxIdList = GetPoolIdList(_goldItemPoolType, goldSpawnChance, listSize);
        for (int i = 0; i < _auxIdList.Count; ++i)
        {

            temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
            temp.SetupFruitAsGoldItem(_auxIdList[i]);
            temp.gameObject.SetActive(false);
            _fruitList.Add(temp);
        }

        //Add equipment items
        Debug.Log("Generating " + Mathf.FloorToInt(goldSpawnChance * listSize) + " equip items");
        _auxIdList = GetPoolIdList(_equipmentItemPoolType,equipmentSpawnChance, listSize);
        for (int i = 0; i < _auxIdList.Count; ++i)
        {
            temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
            temp.SetupFruitAsEquipmentItem(_auxIdList[i]);
            temp.gameObject.SetActive(false);
            _fruitList.Add(temp);
        }

        //(3)Check if we need cluster pool instantiating
        if (fruitSpawnTypeList.Find((fr) => (fr.FruitTypeIndex == (int)Fruit.F_TYPE.CLUSTER_SEED)) != null)
        {
            if (_clusterFruitPool != null)
                _clusterFruitPool.Clear();
            else
                _clusterFruitPool = new List<Fruit>();

            for (int i = 0; i < _maxClusterPoolCount; ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                temp.SetupFruit((int)Fruit.F_TYPE.CLUSTER_UNIT);
                temp.gameObject.SetActive(false);
                _clusterFruitPool.Add(temp);
            }
        }
        else if (_clusterFruitPool != null)
            _clusterFruitPool.Clear();

        //(4)Check if we need multifruit pool instantiating
        if (fruitSpawnTypeList.Find((fr) => (fr.FruitTypeIndex == (int)Fruit.F_TYPE.MULTI_SEED)) != null)
        {
            if (_multiFruitPool != null)
                _multiFruitPool.Clear();
            else
                _multiFruitPool = new List<Fruit>();

            for (int i = 0; i < _maxMultifruitPoolCount; ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                temp.SetupFruit((int)Fruit.F_TYPE.MULTI_UNIT);
                temp.gameObject.SetActive(false);
                _multiFruitPool.Add(temp);
            }
        }
        else if (_multiFruitPool != null)
            _multiFruitPool.Clear();

        //(5)Check if we need egg pool instantiating
        if (fruitSpawnTypeList.Find((fr) => (fr.FruitTypeIndex == (int)Fruit.F_TYPE.CHICKEN)) != null)
        {
            if (_eggFruitPool != null)
                _eggFruitPool.Clear();
            else
                _eggFruitPool = new List<Fruit>();

            for (int i = 0; i < _maxEggFruitPoolCount; ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                temp.SetupFruit((int)Fruit.F_TYPE.EGG);
                temp.gameObject.SetActive(false);
                _eggFruitPool.Add(temp);
            }
            Debug.Log("Added chickens: " + _eggFruitPool.Count);
        }
        else if (_eggFruitPool != null)
            _eggFruitPool.Clear();
        
        //(6)Check if we need kiwi pool instantiating
        if (fruitSpawnTypeList.Find((fr) => (fr.FruitTypeIndex == (int)Fruit.F_TYPE.SBREAKER_CLUSTER_DUO)) != null)
        {
            if (_kiwiFruitPool != null)
                _kiwiFruitPool.Clear();
            else
                _kiwiFruitPool = new List<Fruit>();

            for (int i = 0; i < _maxKiwiFruitPoolCount; ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                temp.SetupFruit((int)Fruit.F_TYPE.KIWI);
                temp.gameObject.SetActive(false);
                _kiwiFruitPool.Add(temp);
            }
            Debug.Log("Added kiwi fruits: " + _kiwiFruitPool.Count);
        }
        else if (_kiwiFruitPool != null)
            _kiwiFruitPool.Clear();

        //(7)Check if we need banan cluster unit pool
        if (fruitSpawnTypeList.Find((fr) => (fr.FruitTypeIndex == (int)Fruit.F_TYPE.BANANA_CLUSTER)) != null)
        {
            if (_bananaUnitFruitPool != null)
                _bananaUnitFruitPool.Clear();
            else
                _bananaUnitFruitPool = new List<Fruit>();

            for (int i = 0; i < _maxBananaClusterPoolCount; ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                temp.SetupFruit((int)Fruit.F_TYPE.BANANA_CLUSTER_UNIT);
                temp.gameObject.SetActive(false);
                _bananaUnitFruitPool.Add(temp);
            }
            Debug.Log("Added banana unit fruits: " + _bananaUnitFruitPool.Count);
        }
        else if (_bananaUnitFruitPool != null)
            _bananaUnitFruitPool.Clear();

        _currentPoolSplashIndex = 0;
        _currentPoolFeatherIndex = 0;
        _currentPoolLeavesIndex = 0;


        Shuffle(_fruitList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spawnRatio"></param>
    /// <param name="fallSpeed"></param>
    public void SetFruitStats(float spawnRatio, float fallSpeed, float flyTime, float goldItemProb, float equipmentItemProb)
    {
        _currentSpawnTime = spawnRatio;
        _fallFruitTime = fallSpeed;
        _fruitFlyTime = flyTime;
        _goldSpawnChance = goldItemProb;
        _equipmentSpawnChance = equipmentItemProb;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fallSpeed"></param>
    public void SetFruitFallSpeed(float fallSpeed)
    {
        _currentFruitFallTime = fallSpeed;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetIdle()
    {
        _state = PT_STATE.IDLE;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="pause"></param>
    public void Pause(bool pause)
    {
        if (pause && _state != PT_STATE.IDLE)
        {
            _lastState = _state;
            _state = PT_STATE.IDLE;

            for (int i = 0; i < _currentPoolIndex; ++i)
                _fruitList[i].Pause(true);
            for (int i = 0; i < _currentMultifruitPoolIndex; ++i)
                _multiFruitPool[i].Pause(true);
            for (int i = 0; i < _currentClusterPoolIndex; ++i)
                _clusterFruitPool[i].Pause(true);
            for (int i = 0; i < _currentEggPoolIndex; ++i)
                _eggFruitPool[i].Pause(true);
            for (int i = 0; i < _currentKiwiPoolIndex; ++i)
                _kiwiFruitPool[i].Pause(true);
        }
        else if (!pause && _state == PT_STATE.IDLE)
        {
            _state = _lastState;

            for (int i = 0; i < _currentPoolIndex; ++i)
                _fruitList[i].Pause(false);
            for (int i = 0; i < _currentMultifruitPoolIndex; ++i)
                _multiFruitPool[i].Pause(false);
            for (int i = 0; i < _currentClusterPoolIndex; ++i)
                _clusterFruitPool[i].Pause(false);
            for (int i = 0; i < _currentEggPoolIndex; ++i)
                _eggFruitPool[i].Pause(false);
            for (int i = 0; i < _currentKiwiPoolIndex; ++i)
                _kiwiFruitPool[i].Pause(false);
        }
        
    }
    /// <summary>
    /// 
    /// </summary>
    public void StartSpawn()
    {
        Debug.Log("Start spawn");
        //_currentSpawnTime = _baseSpawnTime;
        _timer = 0f;
        _state = PT_STATE.SPAWNING;
        _currentPoolIndex = 0;
        _currentClusterPoolIndex = _currentEggPoolIndex = _currentKiwiPoolIndex = _currentMultifruitPoolIndex = _currentBananaClusterIndex = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop(bool destroyFlyingFruits = true)
    {
        if (destroyFlyingFruits)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Fruit"))
            {
                if (go.GetComponent<Fruit>()._FState == Fruit.FRUIT_ST.FALLING_FROM_TREE)
                    DestroyFruit(go.GetComponent<Fruit>());
            }
        }
        _state = PT_STATE.ENDED;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Fruit GetClusterUnit()
    {
        if (_currentClusterPoolIndex >= _clusterFruitPool.Count)
            Debug.LogError("Index out of range!");

        Fruit returnFr = null;
        returnFr =_clusterFruitPool[_currentClusterPoolIndex];
        ++_currentClusterPoolIndex;
        return returnFr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Fruit GetBananaUnit()
    {
        if (_currentBananaClusterIndex >= _bananaUnitFruitPool.Count)
            Debug.LogError("Index out of range!");

        Fruit returnFr = null;
        returnFr = _bananaUnitFruitPool[_currentBananaClusterIndex];
        ++_currentBananaClusterIndex;
        returnFr.Maturity = 1;
        Debug.Log("Banana index is -_______________________________   " + _currentBananaClusterIndex);
        return returnFr;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Fruit GetMultiUnit()
    {
        if (_currentMultifruitPoolIndex >= _multiFruitPool.Count)
            Debug.LogError("Index out of range!");
     
        Fruit returnFr = null;
        returnFr = _multiFruitPool[_currentMultifruitPoolIndex];
        ++_currentMultifruitPoolIndex;
        Debug.Log("Get multi index_________________________: " + _currentMultifruitPoolIndex);
        return returnFr;        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    public Fruit GetEggFruit(int quality)
    {
        if (_currentEggPoolIndex >= _eggFruitPool.Count)
            Debug.LogError("Index out of range!");

        Fruit returnFr = null;
        returnFr = _eggFruitPool[_currentEggPoolIndex];     
        ++_currentEggPoolIndex;
        returnFr.SetupFruitAsEgg(quality);
        Debug.Log("Get egg index_________________________: " + _currentEggPoolIndex+" -- "+returnFr);
        return returnFr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Fruit GetKiwiFruit()
    {
        if (_currentKiwiPoolIndex >= _kiwiFruitPool.Count)
            Debug.LogError("Index out of range!");

        Fruit returnFr = null;
        returnFr = _kiwiFruitPool[_currentKiwiPoolIndex];
        ++_currentKiwiPoolIndex;
        Debug.Log("Get kiwi index_________________________: " + _currentEggPoolIndex + " -- " + returnFr);
        return returnFr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    public void DestroyClusterUnit(Fruit unit, bool fruitMissed = false)
    {       
        int fruitIndex = _clusterFruitPool.FindIndex(((c) => c == unit));
        Debug.Log("DEstroy cluster unit: " + fruitIndex);
        //seek left and place at the end the current unit
        for (int i = fruitIndex; i < _clusterFruitPool.Count - 1; ++i)
            _clusterFruitPool[i] = _clusterFruitPool[i + 1];

        _clusterFruitPool[_clusterFruitPool.Count - 1] = unit;
        if (fruitMissed)
            GameMgr.Instance.AlarmWarnAtPos(unit.transform.position.x, _gameMgr.AlarmIncrease/Fruit._clusterUnits);
        unit.gameObject.SetActive(false);
        --_currentClusterPoolIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    public void DestroyMultiUnit(Fruit unit, bool fruitMissed = false)
    {
        
        int fruitIndex = _multiFruitPool.FindIndex(((c) => c == unit));
        //Debug.Log("Destroy multi unit" + fruitIndex);
        //seek left and place at the end the current unit
        for (int i = fruitIndex; i < _multiFruitPool.Count - 1; ++i)
            _multiFruitPool[i] = _multiFruitPool[i + 1];

        _multiFruitPool[_multiFruitPool.Count - 1] = unit;
        if (fruitMissed)
            GameMgr.Instance.AlarmWarnAtPos(unit.transform.position.x, _gameMgr.AlarmIncrease / Fruit._multiUnits);
        unit.gameObject.SetActive(false);
        --_currentMultifruitPoolIndex;
        Debug.Log("Index after destroying multi unit::___________________::" + _currentMultifruitPoolIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="egg"></param>
    /// <param name="fruitMissed"></param>
    public void DestroyEggFruit(Fruit egg, bool fruitMissed = false)
    {

        int fruitIndex = _eggFruitPool.FindIndex(((c) => c == egg));
        //Debug.Log("Destroy multi unit" + fruitIndex);
        //seek left and place at the end the current unit
        for (int i = fruitIndex; i < _eggFruitPool.Count - 1; ++i)
            _eggFruitPool[i] = _eggFruitPool[i + 1];

        _eggFruitPool[_eggFruitPool.Count - 1] = egg;
        if (fruitMissed)
            GameMgr.Instance.AlarmWarnAtPos(egg.transform.position.x);
        egg.gameObject.SetActive(false);
        --_currentEggPoolIndex;
        Debug.Log("Index after destroying multi unit::___________________::" + _currentEggPoolIndex);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="fruitMissed"></param>
    public void DestroyKiwiFruit(Fruit unit, bool fruitMissed = false)
    {
        int fruitIndex = _kiwiFruitPool.FindIndex(((c) => c == unit));
        //Debug.Log("Destroy multi unit" + fruitIndex);
        //seek left and place at the end the current unit
        for (int i = fruitIndex; i < _kiwiFruitPool.Count - 1; ++i)
            _kiwiFruitPool[i] = _kiwiFruitPool[i + 1];

        _kiwiFruitPool[_kiwiFruitPool.Count - 1] = unit;
        if (fruitMissed)
            GameMgr.Instance.AlarmWarnAtPos(unit.transform.position.x);
        unit.gameObject.SetActive(false);
        --_currentKiwiPoolIndex;
        Debug.Log("Index after destroying multi unit::___________________::" + _currentKiwiPoolIndex);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="fruitMissed"></param>
    public void DestroyBananaUnit(Fruit unit, bool fruitMissed = false)
    {
        int fruitIndex = _bananaUnitFruitPool.FindIndex(((c) => c == unit));
        //Debug.Log("Destroy multi unit" + fruitIndex);
        //seek left and place at the end the current unit
        for (int i = fruitIndex; i < _bananaUnitFruitPool.Count - 1; ++i)
            _bananaUnitFruitPool[i] = _bananaUnitFruitPool[i + 1];

        _bananaUnitFruitPool[_bananaUnitFruitPool.Count - 1] = unit;
        if (fruitMissed)
            GameMgr.Instance.AlarmWarnAtPos(unit.transform.position.x);
        unit.gameObject.SetActive(false);
        --_currentBananaClusterIndex;
        Debug.Log("Index after destroying banana unit::___________________::" + _currentBananaClusterIndex);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fr"></param>
    public void DestroyFruit(Fruit fr, bool fruitMissed = false)
    {
        Debug.Log("Destroy " + fr._Ftype);
        int fruitIndex = -1;

        if (fr._Ftype == Fruit.F_TYPE.CLUSTER_UNIT)
            DestroyClusterUnit(fr, fruitMissed);
        else if (fr._Ftype == Fruit.F_TYPE.MULTI_UNIT)
            DestroyMultiUnit(fr, fruitMissed);
        else if (fr._Ftype == Fruit.F_TYPE.EGG)
            DestroyEggFruit(fr, fruitMissed);
        else if (fr._Ftype == Fruit.F_TYPE.KIWI)
            DestroyKiwiFruit(fr, fruitMissed);
        else if (fr._Ftype == Fruit.F_TYPE.BANANA_CLUSTER_UNIT)
            DestroyBananaUnit(fr, fruitMissed);
        else
        {
            Debug.Log("Attepmting to destroy: "+fr._Ftype);
            fruitIndex = _fruitList.FindIndex(((c) => c == fr));
            Debug.Log("Fruit index found: " + fruitIndex);
            //seek left and place at the end the current disabled fruit
            for (int i = fruitIndex; i < _fruitList.Count - 1; ++i)
                _fruitList[i] = _fruitList[i + 1];

            _fruitList[_fruitList.Count - 1] = fr;
            if (fruitMissed)
                GameMgr.Instance.AlarmWarnAtPos(fr.transform.position.x);
            fr.gameObject.SetActive(false);
            --_currentPoolIndex;
        }
        fr.transform.SetParent(_fruitRoot);     //Reset parent to pool root

        if (fruitMissed)
        {
            
            //Water sound: deprecated
            if (_gameMgr.StageIndex == 0 && fr.transform.position.x < _gameMgr.MinRightFingerRef.position.x && fr.transform.position.x > _gameMgr.MaxLeftFingerRef.position.x)
                AudioController.Play("aud_fr_missed_0");
            if (fr._Ftype == Fruit.F_TYPE.CHICKEN || fr._Ftype == Fruit.F_TYPE.SACK_BREAKER || fr._Ftype == Fruit.F_TYPE.SBREAKER_CLUSTER_DUO)
            {
                //Splash particles: deprecated
                GameMgr.Instance.SetSplashParticle(fr, fr.transform.position);
                AudioController.Play("aud_fr_bird_ground_0");
            }
                
            else if (fr._Ftype == Fruit.F_TYPE.EQUIPMENT)
            {
                switch (fr.EquipmentItem.SlotType)
                {
                    //Sack
                    case EquipmentItem.SLOT_TYPE.COLLECTOR_A:
                        AudioController.Play("aud_fr_missed_sack");
                        break;
                    //Boots
                    case EquipmentItem.SLOT_TYPE.COLLECTOR_B:
                        AudioController.Play("aud_fr_missed_boots");
                        break;
                    //Glasses
                    case EquipmentItem.SLOT_TYPE.SHAKER_A:
                        AudioController.Play("aud_fr_missed_glasses");
                        break;
                    //Gloves
                    case EquipmentItem.SLOT_TYPE.SHAKER_B:
                        AudioController.Play("aud_fr_missed_gloves");
                        break;
                    //Gloves
                    case EquipmentItem.SLOT_TYPE.STRIKER_A:
                        AudioController.Play("aud_fr_missed_gloves");
                        break;
                    //Boots
                    case EquipmentItem.SLOT_TYPE.STRIKER_B:
                        AudioController.Play("aud_fr_missed_boots");
                        break;
                }
                //EquipmentVsGround particles
                GameMgr.Instance.SetItemCollisionParticle(fr, fr.transform.position);
                //Analytics
                ++GameMgr.Instance.EqpItmLostCount;
                    
            }
            else if (fr._Ftype == Fruit.F_TYPE.GOLD_ITEM)
            {
                AudioController.Play("aud_fr_gold_missed_01");
                //Particles
                GameMgr.Instance.SetItemCollisionParticle(fr, fr.transform.position);
                //Analytics
                ++GameMgr.Instance.GoldItmLostCount;
            }
            else
            {
                //Splash particles
                GameMgr.Instance.SetSplashParticle(fr, fr.transform.position);
                AudioController.Play("aud_fr_missed_0");
            }
                
        }
        else
        {
            if (fr._Ftype == Fruit.F_TYPE.SPIKY || fr._Ftype == Fruit.F_TYPE.SACK_BREAKER_LAUNCH)
                fr.StopFallingSound();
        }

        
    }

    /// <summary>
    ///     
    /// </summary>
    public void ResetTree()
    {
        _state = PT_STATE.IDLE;
        _currentAudioTime = Random.Range(_minTreeAudioTime, _maxTreeAudioTime);
        _treeAudioTimer = 0f;
    }

    /// <summary>
    /// Play shake animation
    /// </summary>
    public void Shake()
    {
        if (_gameMgr == null)
            InitPalmTree();
        _shaking = true;
        _shakingTimer = 0f;
        _state = PT_STATE.INTRO_SHAKE;
        _audioTimer = 0f;
        AudioController.Play("shake_intro", GameMgr.Instance.transform);
        //Leaves particles
        SetLeavesParticle(new Vector2(Random.Range(_spawnMinXLimit.position.x, _spawnMaxXLimit.position.x), Random.Range(_spawnMinYLimit.position.y, _spawnMaxYLimit.position.y)));
    }   
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemSpawnList"></param>
    /// <param name="spawnChance"></param>
    /// <param name="listSize"></param>
    /// <returns></returns>
    private List<string> GetPoolIdList(List<Level.ItemSpawn> itemSpawnList, float spawnChance, int listSize)
    {
        List<string> _retList = new List<string>();

        for (int i = 0; i < itemSpawnList.Count; ++i)
        {
            for (int j = 0; j < Mathf.FloorToInt(itemSpawnList[i].SpawnRatio * spawnChance * listSize); ++j)
                _retList.Add(itemSpawnList[i].Id);
        }
        Shuffle(_retList);
        return _retList;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SpawnFruit()
    {
        if (_currentPoolIndex >= _fruitList.Count)
            Debug.LogError("Index out of range!");
        else
        {
            _fruitList[_currentPoolIndex].gameObject.SetActive(true);
            _fruitList[_currentPoolIndex].transform.position = new Vector3(Random.Range(_spawnMinXLimit.position.x, _spawnMaxXLimit.position.x),
                                                                            Random.Range(_spawnMinYLimit.position.y, _spawnMaxYLimit.position.y), transform.position.z);
            _fruitList[_currentPoolIndex].FruitTree = this;
            _fruitList[_currentPoolIndex].StartFruit();

            //Leaves particles
            SetLeavesParticle(_fruitList[_currentPoolIndex].transform.position.x);

            
            ++_currentPoolIndex;
        }

        /*if (_fruitList[_currentPoolIndex]._Ftype == Fruit.F_TYPE.CLUSTER_UNIT)
            ++_currentClusterPoolIndex;     
        else if (_fruitList[_currentPoolIndex]._Ftype == Fruit.F_TYPE.MULTI_UNIT)
            ++_currentMultifruitPoolIndex;
        else*/
        
            //_currentPoolIndex = (_currentPoolIndex + 1) % _fruitList.Count;
    }

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
                case EquipmentItem.MOD_TYPE.ITEM_FIND_PROB:
                    _currentEquipmentSpawnChance += _equipmentSpawnChance * eI.ModValueList[i];
                    //TODO: collider size + sprite?
                    //fixed sacks?-> no
                    break;
                case EquipmentItem.MOD_TYPE.GOLD_FIND_PROB:
                    _currentEquipmentSpawnChance += _equipmentSpawnChance * eI.ModValueList[i];
                    break;

                case EquipmentItem.MOD_TYPE.FALL_SPEED:
                    _currentFruitFallTime += _fallFruitTime * (-eI.ModValueList[i]);
                    //_currentSpeed = _baseSpeed * (1f + eI.ModValue);
                    //_currentRawSpeed = _currentSpeed;
                    break;
            }
        }
    }

    /// <summary>
    /// Fisher-Yates shuffle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    

    /// <summary>
    /// Spawning fruit particles
    /// </summary>
    /// <param name="pos"></param>
    private void SetLeavesParticle(float xPos)
    {
        _poolLeavesParticle[_currentPoolLeavesIndex].transform.position = new Vector2(xPos, _leavesPsHeightRef.position.y);
        _poolLeavesParticle[_currentPoolLeavesIndex].gameObject.SetActive(true);
        _poolLeavesParticle[_currentPoolLeavesIndex].Play();
        _currentPoolLeavesIndex = (_currentPoolLeavesIndex + 1) % _poolLeavesParticle.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    private void SetLeavesParticle(Vector2 pos)
    {
        _poolLeavesParticle[_currentPoolLeavesIndex].transform.position = pos;
        _poolLeavesParticle[_currentPoolLeavesIndex].gameObject.SetActive(true);
        _poolLeavesParticle[_currentPoolLeavesIndex].Play();
        _currentPoolLeavesIndex = (_currentPoolLeavesIndex + 1) % _poolLeavesParticle.Count;
    }
    #endregion


        #region Properties
        //Shaker Monkey
    public float SapwnRatioModifier { get { return _spawnRatioModifier; } set { _spawnRatioModifier = value; } }
    public float BurstChanceOnSpawn { get { return _burstChanceOnSpawn; } set { _burstChanceOnSpawn = value; } }
    public int BurtsAmount { get { return _burtsAmount; } set { _burtsAmount = value; } }
    public float ItemSpawnChance { get { return _itemSpawnChance; } set { _itemSpawnChance = value; } }
    public EquipmentItem SlotA { get { return _slotA; } set { _slotA = value; } }
    public EquipmentItem SlotB { get { return _slotB; } set { _slotB = value; } }
    public float CurrentFallSpeed { get { return _currentFruitFallTime; } set { _currentFruitFallTime = value; } }
    public float CurrentFruitFlyTime { get { return _currentFruitFlyTime; } set { _currentFruitFlyTime = value; } }
    public List<Fruit> _FList { get { return _fruitList; } set { _fruitList = value; } }
    public Transform FruitPoolRoot {  get { return _fruitRoot; } set { _fruitRoot = value; } }
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    private List<Sprite> _shakeSpList;
    [SerializeField]
    private List<Vector2> _shakerOffsetList;    //offset used during shake animation frames
    //[SerializeField]
    //private Sprite _idleSp;
    [SerializeField]
    private List<Sprite> _shakerMonkeySpList;
    [SerializeField]
    private GameObject _fruitPrefab;
    [SerializeField]
    private Transform _fruitRoot;
    [SerializeField]
    private Transform _spawnMinXLimit, _spawnMaxXLimit, _spawnMinYLimit, _spawnMaxYLimit;
    //[SerializeField]
    //private int _fruitInitCountPool;
    //private int _fruitMaxPool;
    //private int _fruitIncrement;
	
    //Shaker monkey stats
    [SerializeField]
    private float _spawnRatioModifier;
    [SerializeField]
    private float _burstChanceOnSpawn;      //chance to spawn a fruit burst when spawning a fruit
    [SerializeField]
    private int _burtsAmount;
    [SerializeField]
    private float _itemSpawnChance;     //chance to spawn an item instead a fruit


    [SerializeField]
    private int _maxClusterPoolCount, _maxMultifruitPoolCount, _maxEggFruitPoolCount, _maxKiwiFruitPoolCount, _maxBananaClusterPoolCount;

    [SerializeField]
    private float _minTreeAudioTime, _maxTreeAudioTime;

    //[SerializeField]
    private List<Level.ItemSpawn> _goldItemPoolType, _equipmentItemPoolType;
    [SerializeField]
    private float _shakingTime;
    [SerializeField]
    private float _shakeCooldown;

    //Particles
    [SerializeField]
    private List<ParticleSystem> _poolSplashParticle, _poolFeatherParticle, _poolLeavesParticle;
    [SerializeField]
    private Transform _leavesPsHeightRef;
    [SerializeField]
    private Material _fruitSplashMaterial, _featherSplashMaterial;
    //[SerializeField]
    //private List<Gradient> _fruitSplashColorGradList;  //ordered by FType
    #endregion

    #region Private Non-serialized Fields
    private GameMgr _gameMgr;
    private Image _img;
    private Image _shakerImg;
    private PT_STATE _state, _lastState;
    private float _currentSpawnTime, _baseSpawnTime;
    private List<Fruit> _fruitList;
    private List<Fruit> _clusterFruitPool; //pool used for instantiating cluster clones for cluster fruits on launch
    private List<Fruit> _multiFruitPool;    //pool used for instantiating multifruit clones for multifruit type on launch
    private List<Fruit.G_TYPE> _goldItemsTypePool;  //poll which stores different golditems type; used on fruitsetup if type==goldiItem
    private List<Fruit> _eggFruitPool; //pool used for instantiating eggs spawned from any chicken on Sack
    private List<Fruit> _kiwiFruitPool; //pool used for instantiating kiwi fruit spawned from any SACK_BREAKER_DUO on Launch
    private List<Fruit> _bananaUnitFruitPool;   //pool used for banana cluster
    private float _timer, _audioTimer;

    private int _currentPoolIndex;
    private int _currentClusterPoolIndex, _currentMultifruitPoolIndex, _currentEggPoolIndex, _currentKiwiPoolIndex,_currentBananaClusterIndex;

    private float _fallFruitTime;   //fall speed set by level stats
    private float _fruitFlyTime;
    private float _currentFruitFallTime;   //final speed calculated after applying item mod
    private float _currentFruitFlyTime;
    private float _goldSpawnChance, _equipmentSpawnChance;
    private float _currentGoldSpawnChance, _currentEquipmentSpawnChance;

    private EquipmentItem _slotA, _slotB;
    private static System.Random rng = new System.Random();

    private float _currentAudioTime, _treeAudioTimer;

    private List<Level.FruitSpawn> _levelFruitSpawnTypeList, _currentFruitSpawnTypeList;

    private float _frameTimer, _shakingTimer;
    private int _frameIndex;
    private bool _shaking;

    private int _currentPoolSplashIndex, _currentPoolFeatherIndex,  _currentPoolLeavesIndex;   //particles pool index 
    #endregion
}
