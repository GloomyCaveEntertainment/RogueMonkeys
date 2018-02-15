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
public class Sack : MonoBehaviour {

	#region Public Data
    public enum S_STATE { IDLE = 0, ANIMATING, RELOADING, STOP, BROKEN }

    [System.Serializable]
    public class SackFruit
    {
        public Fruit _Fruit;        //Attached Fruit
        public float _Timer;        //current animation time
        public Vector2 _destPos;    //destination position on UI stack

        public SackFruit(Fruit f, Vector2 sackPos)
        {
            _Fruit = f;
            _Timer = 0f;
            _destPos = sackPos;
        }
    }
    [System.Serializable]
    public class SackPointListEntry
    {
        public string _Id;
        public List<Transform> _PtList;
    }
    [System.Serializable]
    public class SackCountMiniListEntry
    {
        public string _Id;
        public List<Image> _ImgList;
    }
    #endregion


    #region Behaviour Methods
    // Use this for initialization
    void Start () {
        //InitSack();
        _fruitStackHeightOffset = _sackFrPtList[2].position.y - _sackFrPtList[0].position.y;    //fruit size = 1 aprox size on UI sack
        if (_sackOutline == null)
            _sackOutline = GetComponent<Outline>();
        if (_sackOutline.enabled)
            _sackOutline.enabled = false;


    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("Sack y pos:: " + _Collector._CollectorSack.transform.position.y);
        switch (_state)
        {
            case S_STATE.ANIMATING:
                foreach (SackFruit sf in _animatingFruitList)
                {
                    sf._Timer += Time.deltaTime;
                    sf._Fruit.transform.position = Vector2.Lerp((Vector2)_stackEnterPt.position, sf._destPos, _fruitStackAC.Evaluate(sf._Timer / _sackFruitAnimationTime));
                    //Reached its targegt osition. Set time to -1 to delete 
                    if (sf._Timer >= _sackFruitAnimationTime)
                        sf._Timer = -1f;
                }
                //Remove all placed on position fruit
                _animatingFruitList.RemoveAll((sf) => sf._Timer == -1f);
                //Check state
                if (_animatingFruitList.Count == 0)
                    _state = S_STATE.IDLE;
                break;

            case S_STATE.RELOADING:
                _reloadTimer += Time.deltaTime;
                if (_reloadTimer >= _reloadPerFruitTime)
                { 
                    _reloadTimer = 0f;


                    _tempFruit = _fruitList[_currentStackIndex];
                    _fruitList.RemoveAt(_currentStackIndex);
                    --_currentStackIndex;
                    _currentSize -= _tempFruit.Size;
                    UpdateSackCountMiniList();
                    _sackFillImg.fillAmount = _currentSize/_currentCapacity;// (float)_fruitList.Count / _maxCount; //Update UI fill
                    if (_fillingSack)
                        _fillingSack = false;
                    //Collect Fruit
                    _tempFruit.Collect();
                    //Checkk Event (tutorial)
                    if (FruitCollectedEvt!=null)
                        FruitCollectedEvt();
                    //Animate fruit  going out from Sack
                    _tempFruit.SetCollectedAnimation(new Vector2(_tempFruit.transform.position.x + _collectedAnimationOffset.x, _tempFruit.transform.position.y + (_stackEnterPt.transform.position.y - _tempFruit.transform.position.y) + _collectedAnimationOffset.y), _collectedAnimTime);
                    
                    //Update Collector speed
                    _speedLoss -= _lossSpeedPerFruit * _tempFruit.Size;
                    _Collector.ReduceSpeed(_speedLoss);

                    //Reloading finished
                    if (_fruitList.Count == 0)
                    {
                        _state = S_STATE.IDLE;
                        GameMgr.Instance.ResetCollectedIndex();
                    }
                }  
                break;

            case S_STATE.BROKEN:
                _brokenTimer += Time.deltaTime;
                if(_brokenTimer >= _Collector._StunTime)
                {
                    _state = S_STATE.IDLE;
                }
                else if (_brokenFrCount != 0)
                {
                    _throwFrTimer += Time.deltaTime;
                    if (_throwFrTimer >= _Collector._StunTime / _brokenFrCount)
                    {
                        UnstackAndDissmiss();
                        _throwFrTimer = 0f;
                    }
                }
                break;
        }
        //sack image fill animation
        if (_fillingSack)
        {
            _fillTimer += Time.deltaTime;
            _sackFillImg.fillAmount = _initFillValue + (_targetFillValue - _initFillValue) * _fillAnimCurve.Evaluate(_fillTimer / _fillTime);
            //_alarmSbar.value = Mathf.Lerp(_initBarValue, _targetBarValue, _growCurve.Evaluate(_barTimer / _barGrowTime));
            if (_fillTimer >= _fillTime)
            {
                _fillTimer = 0f;
                _fillingSack = false;
            }
        }
        //Full feedback, outline color transition
        if (_fullFeedback)
        {
            _feedbackTimer += Time.deltaTime;
            if (_feedbackTimer >= _fullFbTime)
                EnableFullFeedback(false);
            else
                _sackOutline.effectColor = new Color(1f, 0f, 0f, Mathf.Lerp(0f, 1f, _fullFeedbackAnimC.Evaluate(_feedbackTimer / _fullFbTime)));
        }

        if (_seekingSack)
        {
            _seekTimer += Time.deltaTime;
            _Collector._CollectorSack.transform.localPosition = new Vector3(_Collector._CollectorSack.transform.localPosition.x, Mathf.Lerp(_initSackPos.y, _initSackPos.y + _catchAnimOffset, _catchAnimCurve.Evaluate(_seekTimer / _catchAnimTime)), _Collector._CollectorSack.transform.localPosition.z);// Vector3.Lerp(_initSackPos, _Collector._CollectorSack.transform.position + Vector3.up * _catchAnimOffset, _catchAnimCurve.Evaluate(_seekTimer / _catchAnimTime));
            if (_seekTimer >= _catchAnimTime)
                _seekingSack = false;
        }
	}
	#endregion

	#region Public Methods
    public delegate void OnSackFullEvent();
    public delegate void OnSackReloadEvent();
    public delegate void OnFruitCollectedEvent();

    public event OnSackFullEvent SackFullEvt;
    public event OnSackReloadEvent SackReloadEvt;
    public event OnFruitCollectedEvent FruitCollectedEvt;
    /// <summary>
    /// 
    /// </summary>
    public void InitSack()
    {
        if (_animatingFruitList == null)
            _animatingFruitList = new List<SackFruit>(Mathf.RoundToInt(_currentCapacity*2f)); //x2 because we have size split in 0.5 steps, for smaller fruits 
        else
            _animatingFruitList.Clear();
        
        if (_fruitList == null)
            _fruitList = new List<Fruit>();
        else
            _fruitList.Clear();
        _currentStackIndex = -1;

        _maxCount = _currentCapacity;
        Debug.Log("Max count is : " + _maxCount);
        _speedLoss = 0f;
        _sackFillImg.fillAmount = 0f;
        _fillingSack = false;
        
        _currentSize = 0f;

        _initSackPos = _Collector._CollectorSack.transform.localPosition;
        //_catchAnimOffset = _Collector._Sack.transform.lossyScale.y * 0.1f;
        //Debug.Log("INIT SACK; sack init pos: " + _initSackHeight);
    }
    /// <summary>
    /// 
    /// </summary>
    public void ResetToInitPos()
    {
        //Debug.Log("RESET POS, from " + _Collector._CollectorSack.transform.position + " to " + _initSackPos);
        //_Collector._CollectorSack.transform.localPosition = _initSackPos;// new Vector3(_Collector._CollectorSack.transform.position.x, _initSackPos.y, _Collector._CollectorSack.transform.position.z);
    }
    /// <summary>
    /// 
    /// </summary>
    public void ResetSack()
    {
        Debug.Log("Reset Sack_______________________________________");
        if (_animatingFruitList == null)
            _animatingFruitList = new List<SackFruit>(_initUICapacity);
        else if (_animatingFruitList.Count > 0)
        {
            /*for (int i=0; i< _animatingFruitList.Count; ++i)
            //foreach (SackFruit sf in _animatingFruitList)
            {
                Destroy(_animatingFruitList[i]._Fruit);
                //_animatingFruitList.Remove(sf);
            }
            //_animatingFruitList.Remo*/
            _animatingFruitList.Clear();
        }
        if (_fruitList == null)
            _fruitList = new List<Fruit>();
        else
        {
            foreach (Fruit f in _fruitList)
                f.FruitTree.DestroyFruit(f);
            _fruitList.Clear();
        }
        _currentStackIndex = 0;

        _maxCount = _currentCapacity;
        _speedLoss = 0f;
        _state = S_STATE.IDLE;
    }

    public void Stop()
    {
        Debug.Log("Stopppppp SSSSack");
        _state = S_STATE.STOP;
    }
    /// <summary>
    /// 
    /// </summary>
    public void ResetMods()
    {
        _currentCapacity = _defaultCapacity;
        _currentReloadTime = _reloadTime;
    }

    /// <summary>
    /// Set Sack Capacity and its associated on_sack collected pt list and fruit amount mini list
    /// </summary>
    /// <param name="value"></param>
    /// <param name="id"></param>
    public void SetCapacity(float value, string id)
    {
        _currentCapacity = value;
        SackPointListEntry se = _sackPtListIndex.Find((e) => e._Id.CompareTo(id) == 0);
        if (se != null)
            _sackFrPtList = se._PtList;
        else
            _sackFrPtList = _sackPtListIndex.Find((e) => e._Id.CompareTo("default") == 0)._PtList;
        SackCountMiniListEntry sc = _sackCountMiniListIndex.Find((e) => e._Id.CompareTo(id) == 0);
        if (sc != null)
            _sackCountMiniList = sc._ImgList;
        else
            _sackCountMiniList = _sackCountMiniListIndex.Find((e) => e._Id.CompareTo("default") == 0)._ImgList;
        //enable/disable root object
        foreach (SackCountMiniListEntry scm in _sackCountMiniListIndex)
                scm._ImgList[0].transform.parent.parent.gameObject.SetActive(scm._Id.CompareTo(id) == 0);
        UpdateSackCountMiniList();
        
    
    }

    /// <summary>
    /// Try to push a fruit inside sack
    /// </summary>
    /// <param name="f"></param>
    /// <returns>if operation succeeded</returns>
    public bool TryToPushToSack(Fruit f)
    {
        Debug.Log("Try to push");
        if (f._Ftype == Fruit.F_TYPE.SACK_BREAKER_LAUNCH)
        {
            _Collector.Stun();
            //GameMgr.Instance.AlarmWarnAtPos(f.transform.position.x, Fruit._sBreakerAlarmLvl);
            /*if (GameMgr.Instance.RaiseAlarm(Fruit._sBreakerAlarmLvl))
                GameMgr.Instance.WakeUpGuards();*/
        }
            
        if (_state != S_STATE.RELOADING && _state != S_STATE.BROKEN && _state != S_STATE.STOP)
        {
            if (f._Ftype != Fruit.F_TYPE.SACK_BREAKER_LAUNCH && _currentSize + f.Size <= _currentCapacity)
            {
                PushToSack(f);
                return true;
            }
            else
            {
                if (f._Ftype != Fruit.F_TYPE.SACK_BREAKER_LAUNCH)
                    EnableFullFeedback(true);
                if (SackFullEvt != null)
                    SackFullEvt();
                return false;
            }
        }
        else
        {
            if (SackFullEvt != null)
                SackFullEvt();
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reload()
    {
        if (_state == S_STATE.RELOADING ||_fruitList.Count == 0 ||_state == S_STATE.STOP || _state == S_STATE.BROKEN)
            return;

        //Event call for any subscribed client(ie Tutorial)
        if (SackReloadEvt != null)
            SackReloadEvt();

        _state = S_STATE.RELOADING;
        _reloadPerFruitTime = _currentReloadTime / _fruitList.Count;
        _reloadTimer = _reloadPerFruitTime; //firs fruit pop is instant
        _Collector.Reload(/*_reloadTime*/);
    }

    /// <summary>
    /// 
    /// </summary>
    public void DismissAll()
    {
        foreach (Fruit f in _fruitList)
        {
            f.transform.position = _Collector.transform.position;
            f.Dissmiss();
        }
        _fruitList.Clear();     
    }

    /// <summary>
    /// Sack spins 720º during _stunTime Dismiss() fruits every _stunTime/_fruitCount, 
    /// first 1 at t=0; (n-1) each _stunTime/_fruitCount; Unparent fruit from sack, 
    /// move to world sack spot, remove from stack/animation    
    /// </summary>
    public void SackBroken()
    {
        LeanTween.rotateZ(_Collector._CollectorSack, 720f, _Collector._StunTime);
        //Dismiss first 1 autoatically
        _brokenFrCount = _fruitList.Count;
        
        if (_fruitList.Count != 0)
            UnstackAndDissmiss();

        //_sackFillImg.fillAmount = (float)_fruitList.Count / _maxCount; //Update UI fill
        
        _state = S_STATE.BROKEN;
        _brokenTimer = 0f;
        _throwFrTimer = 0f;
        if (_brokenFrCount !=0)
            _throwFrTime = _Collector._StunTime / _brokenFrCount;
    }

    /// <summary>
    /// Push to Sack an egg under Fruit fr
    /// All fruits from fr to top seeks up.
    /// If Sack is full, Dismiss top fruit
    /// </summary>
    /// <param name="fr"></param>
    public void PushEgg(Fruit fr)
    {
        int frStackIndex = -1;
        Fruit tempEgg = null;
        Fruit pushedFr_a = null;
        Fruit pushedFr_b = null;
        Fruit chicken = null;

        frStackIndex = _fruitList.FindIndex((f) => (f == fr));
        if (frStackIndex == -1)
            Debug.Log("Chicken not found to spawn an egg");
        else
        {
            Debug.Log("Pushing egg from " + frStackIndex);
            Fruit temp = null;

            //(1) Save Chicken ref
            chicken = fr;
            //Debug.Log("MAx count and count is :" + _maxCount + " / " + _fruitList.Count +" and size: "+ _currentSize);
            //(3) If List full,last top will get dismissed, else, duplicate last to seek iteration

            if (_currentSize + 1f > _currentCapacity)   //TODO: MAgic Numbers (egg.size)
            //if (_fruitList.Count == _maxCount)
            {
                pushedFr_a = _fruitList[_fruitList.Count - 1];
                //pushed fruit with size 1
                if (_currentSize + 1f - pushedFr_a.Size <= _currentCapacity)
                    _fruitList.RemoveAt(_fruitList.Count - 1);
                //pushed fruit size 0.5, so need to push another one
                else
                {
                    pushedFr_b = _fruitList[_fruitList.Count - 2];
                    _fruitList.RemoveAt(_fruitList.Count - 1);
                    _fruitList.RemoveAt(_fruitList.Count - 1);
                }
            }
            /*else
                _fruitList.Add(_fruitList[_currentStackIndex]);*/
            
            //(2) Insert Egg at position
            tempEgg = GameMgr.Instance._FruitTree.GetEggFruit(fr.EggSpawnQuality);
            Debug.Log("Spawned egg with quality: " + tempEgg.CurrentEggQuality + "frStackIndex: "+ frStackIndex+ "_fruitList.Count: "+ _fruitList.Count);
            tempEgg.transform.position = _stackEndPt.position + Vector3.up * (frStackIndex * _fruitStackHeightOffset);
            tempEgg.transform.parent = transform;
            tempEgg.gameObject.SetActive(true);
            //if we are trying to spawn an egg on last position, spawn it at end of list (we could have pushed 1 or 2 fruits, depending on size)
            if (frStackIndex >= _fruitList.Count)
                _fruitList.Add(tempEgg);
            else
                _fruitList.Insert(frStackIndex, tempEgg);
           
            //Debug.Log("fruit list count after insert: " + _fruitList.Count);
            for (int i = 0; i < _fruitList.Count; ++i)
                Debug.Log("---------------->>>> " + _fruitList[i]._Ftype);
            //_animatingFruitList.Insert(frStackIndex, new SackFruit(tempEgg, (Vector2)_stackEndPt.position + Vector2.up * (frStackIndex * _fruitStackHeightOffset)));

            int ptListIndex = -1;
            for (int i = 0; i < _fruitList.Count; ++i)
            {
                Debug.Log("pt index: " + ptListIndex);
                //edge case, index out of range
                if ( (ptListIndex >= _sackFrPtList.Count - 2 && _fruitList[i].Size == 1f) || (ptListIndex >= _sackFrPtList.Count - 1 && _fruitList[i].Size == 0.5f))
                    LeanTween.move(_fruitList[i].gameObject, _sackFrPtList[ptListIndex].position + Vector3.up * _fruitStackHeightOffset, _eggSpawnTime);
                else
                {
                    if (i >= frStackIndex + 1)
                    {
                        //Cancel any previous tween
                        if (LeanTween.isTweening(_fruitList[i].gameObject))
                            LeanTween.cancel(_fruitList[i].gameObject);

                        if (_fruitList[i].Size == 1f)
                            LeanTween.move(_fruitList[i].gameObject, _sackFrPtList[ptListIndex + 2], _eggSpawnTime);
                        else
                            LeanTween.move(_fruitList[i].gameObject, _sackFrPtList[ptListIndex + 1], _eggSpawnTime);
                        
                            
                    }
                }
                if (_fruitList[i].Size == 1f)
                    ptListIndex += 2;
                else
                    ++ptListIndex;
                
            }
            //seek animation for fruits in top of chicken
            //for (int i = frStackIndex+1; i < _fruitList.Count-1; ++i)
            //{
                //if (i!=_maxCount-1)
              //  if (_fruitList[i].Size == 1f)
               //     LeanTween.move(_fruitList[i].gameObject, _sackFrPtList[Mathf.RoundToInt((i+1) * 2f)].position/*_fruitList[i].transform.position + Vector3.up * _fruitStackHeightOffset*/, _eggSpawnTime).setEase(_eggSpawnAC);
            //}
            //Edge case: sack full
            //if (_fruitList.Count < _currentSize)
              //  LeanTween.move(_fruitList[_fruitList.Count - 1].gameObject, _sackFrPtList[Mathf.RoundToInt((_fruitList.Count) * 2f)].position/*_fruitList[i].transform.position + Vector3.up * _fruitStackHeightOffset*/, _eggSpawnTime).setEase(_eggSpawnAC);

            //else
                //LeanTween.move(_fruitList[_fruitList.Count - 1].gameObject, _sackFrPtList[Mathf.RoundToInt((_fruitList.Count - 1) * 2f)].position + Vector3.up * _fruitStackHeightOffset/*_fruitList[i].transform.position + Vector3.up * _fruitStackHeightOffset*/, _eggSpawnTime).setEase(_eggSpawnAC);

            //(4) Seek up elements in top of inserted egg

            //LeanTween.move(_fruitList[frStackIndex].gameObject, _fruitList[frStackIndex].transform.position + Vector3.up * _fruitStackHeightOffset, _eggSpawnTime).setEase(_eggSpawnAC);

            //(5) Dissmis element if needed
            if (pushedFr_a != null)
            {
                LeanTween.move(pushedFr_a.gameObject, pushedFr_a.transform.position + Vector3.up * _fruitStackHeightOffset * 3f, _eggSpawnTime * 0.4f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
                    {
                        Debug.Log("Di s s misss by Chicken pushed egg A");
                        pushedFr_a.transform.SetParent(GameMgr.Instance._FruitTree.FruitPoolRoot);   //TODO: egg pool parent
                    pushedFr_a.transform.position = _Collector._CollectorSack.transform.position;
                        pushedFr_a.Dissmiss();
                    });
                _currentSize = _currentSize + tempEgg.Size - pushedFr_a.Size;   //Update size
                //Update Collector speed                                                              
                _speedLoss = _speedLoss - _lossSpeedPerFruit * pushedFr_a.Size + _lossSpeedPerFruit * tempEgg.Size;
                _Collector.ReduceSpeed(_speedLoss);
            }
            if (pushedFr_b != null)
            {
                LeanTween.move(pushedFr_b.gameObject, pushedFr_b.transform.position + Vector3.up * _fruitStackHeightOffset * 3f, _eggSpawnTime * 0.4f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
                {
                    Debug.Log("Di s s misss by Chicken pushed egg B");
                    pushedFr_b.transform.SetParent(GameMgr.Instance._FruitTree.FruitPoolRoot);   //TODO: egg pool parent
                    pushedFr_b.transform.position = _Collector._CollectorSack.transform.position;
                    pushedFr_b.Dissmiss();
                });
                _currentSize = _currentSize  - pushedFr_a.Size; //Update size (egg size already added on fr_a)
                --_currentStackIndex;
                //Update Collector speed                                                              
                _speedLoss = _speedLoss - _lossSpeedPerFruit * pushedFr_b.Size;
                _Collector.ReduceSpeed(_speedLoss);
            }
            else if (pushedFr_a == null && pushedFr_b == null)
            {
                ++_currentStackIndex;
                //UI Sack Fill
                if (_fillingSack)
                    _targetFillValue = (_currentSize + tempEgg.Size) / _currentCapacity;
                else
                {
                    _fillingSack = true;
                    _initFillValue = _sackFillImg.fillAmount;
                    _targetFillValue = (_currentSize + tempEgg.Size) / _currentCapacity;
                }
                _currentSize += tempEgg.Size;
                //Update Collector speed                                                              
                _speedLoss += _lossSpeedPerFruit * tempEgg.Size;
                _Collector.ReduceSpeed(_speedLoss);
            }

            //Sack Animation
            //_Collector._CollectorSack.transform.position = new Vector3(_Collector._CollectorSack.transform.position.x, _initSackPos, _Collector._CollectorSack.transform.position.z);    //reset position in case it was animating
            //LeanTween.moveLocalY(_Collector._CollectorSack, _Collector._CollectorSack.transform.localPosition.y + _catchAnimOffset, _catchAnimTime).setEase(_catchAnimCurve);
            _seekTimer = 0f;
            _seekingSack = true;
            ++GameMgr.Instance.StageSpawnedFruitCount;
            UpdateSackCountMiniList();
            //Audio
            AudioController.Play("aud_fr_egg_0");
            AudioController.Play("aud_fr_egg_spawn");
        }
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    private void UnstackAndDissmiss()
    {
        Fruit temp = null;

        //temp = _fruitList.Pop();
        temp = _fruitList[_currentStackIndex];
        _fruitList.RemoveAt(_currentStackIndex);
        --_currentStackIndex;
        _currentSize -= temp.Size;
        UpdateSackCountMiniList();
        _sackFillImg.fillAmount = _currentSize / _currentCapacity;
        temp.transform.parent = GameMgr.Instance._FruitTree.FruitPoolRoot;   //TODO: check if must returned to pool or gets auto attached on destroy
        temp.transform.position = _Collector._CollectorSack.transform.position;
        temp.Dissmiss();
        //Update Collector speed
        _speedLoss -= _lossSpeedPerFruit * temp.Size;
        _Collector.ReduceSpeed(_speedLoss);
        _sackFillImg.fillAmount = (float)_fruitList.Count / _maxCount; //Update UI fill

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    private void PushToSack(Fruit f)
    {
        Debug.Log("_________pushing to sack");
       
        f.DepositOnSack();
        f.transform.position = _stackEnterPt.position;
        f.transform.SetParent(transform);
        _fruitList.Add(f);
        ++_currentStackIndex;
        UpdateUISack(f);       
        _currentSize += f.Size;
        UpdateSackCountMiniList();
        //Update speed
        _speedLoss += _lossSpeedPerFruit * f.Size;
        _Collector.ReduceSpeed(_speedLoss);

        //Sack Animation
        _seekingSack = true;
        _seekTimer = 0f;
        //if (LeanTween.isTweening(_Collector._CollectorSack))
        //LeanTween.cancel(_Collector._CollectorSack);
        //_Collector._CollectorSack.transform.position = new Vector3(_Collector._CollectorSack.transform.position.x, _initSackHeight, _Collector._CollectorSack.transform.position.z);    //reset position in case it was animating
        //LeanTween.moveY(_Collector._CollectorSack, _Collector._CollectorSack.transform.position.y + _catchAnimOffset, _catchAnimTime).setEase(_catchAnimCurve).setOnComplete(() => { ResetToInitPos(); });
        AudioController.Play("aud_fr_catch_0");
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateUISack(Fruit f)
    {
        _animatingFruitList.Add(new SackFruit(f, (Vector2)_sackFrPtList[Mathf.RoundToInt(_currentSize*2f)].position)); 
        //_animatingFruitList.Add(new SackFruit(f, (Vector2)_stackEndPt.position + Vector2.up * ((_fruitList.Count-1) * _fruitStackHeightOffset)));
        if (_state != S_STATE.ANIMATING)
            _state = S_STATE.ANIMATING;
        _initFillValue = _sackFillImg.fillAmount;
        _targetFillValue = (_currentSize +f.Size) / _currentCapacity;// (float)_fruitList.Count / _maxCount;
        _fillingSack = true;
        
    }

    /// <summary>
    /// Feedback shown when a fruit tries to enter on full sack
    /// </summary>
    private void EnableFullFeedback(bool enable)
    {       
        if (enable)
        {
            _feedbackTimer = 0f;
            _sackOutline.effectColor = new Color(1f, 0f, 0f, 0f);//Red no alpha
        }
        _fullFeedback = enable;
        _sackOutline.enabled = enable;

    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateSackCountMiniList()
    {
        auxFullSlot = Mathf.FloorToInt(_currentSize);
        Debug.Log("auxFSlot: " + auxFullSlot);
        for (int i = 0; i < _sackCountMiniList.Count; ++i)
        {
            if (i < auxFullSlot)
            {
                _sackCountMiniList[i].sprite = _fullSlotMini;
                _sackCountMiniList[i].gameObject.SetActive(true);
            }
            else
                _sackCountMiniList[i].gameObject.SetActive(false);
        }
            
        if (_currentSize % auxFullSlot > 0f)
        {
            _sackCountMiniList[auxFullSlot].sprite = _halfSlotMini;
            _sackCountMiniList[auxFullSlot].gameObject.SetActive(true);
        }
            
    }
    #endregion


    #region Properties
    public CollectorMonkey _Collector;
    public float ReloadTime { get { return _reloadTime; } set { _reloadTime = value; } }
    public float CurrentReloadTime { get { return _currentReloadTime; } set { _currentReloadTime = value; } }
    public float Capacity { get { return _defaultCapacity; } set { _defaultCapacity = value; } }
    public float CurrentCapacity { get { return _currentCapacity; } set { _currentCapacity = value; } } 
    public Vector2 InitSackPos {  get { return _initSackPos; } set { _initSackPos = value; } }
    #endregion

    #region Private Serialized Fields
    //UI
    [SerializeField]
    private AnimationCurve _fruitStackAC;
    [SerializeField]
    private AnimationCurve _eggSpawnAC;
    [SerializeField]
    private float _sackFruitAnimationTime;
    [SerializeField]
    private float _eggSpawnTime;
    [SerializeField]
    private Transform _stackEndPt;
    [SerializeField]
    private Transform _stackEnterPt;
    [SerializeField]
    private int _initUICapacity;
    [SerializeField]
    private List<Transform> _sackFrPtList;

    [SerializeField]
    private float _lossSpeedPerFruit;
    [SerializeField]
    private float _rotModifierSpeed;        //increaes/decreases fruit rot behaviour
    [SerializeField]
    private float _reloadTime;
    [SerializeField]
    private float _defaultCapacity;

    [SerializeField]
    private FruitTree _fTree;

    [SerializeField]
    private Image _sackFillImg;
    [SerializeField]
    private float _fillTime;
    [SerializeField]
    private AnimationCurve _fillAnimCurve;

    [SerializeField]
    private float _catchAnimOffset;
    [SerializeField]
    private float _catchAnimTime;
    [SerializeField]
    private AnimationCurve _catchAnimCurve;

    [SerializeField]
    private AnimationCurve _fullFeedbackAnimC;
    [SerializeField]
    private float _fullFbTime;  //full sack feedback anim time
    #endregion

    #region Private Non-serialized Fields
    private S_STATE _state;
    private List<Fruit> _fruitList;
    private float _maxCount;

    //UI
    [SerializeField]
    private float _fruitStackHeightOffset;  //y offset between stored fruits
    [SerializeField]
    private Vector2 _collectedAnimationOffset;
    [SerializeField]
    private float _collectedAnimTime;

    [SerializeField]
    private List<SackPointListEntry> _sackPtListIndex;
    [SerializeField]
    private List<SackCountMiniListEntry> _sackCountMiniListIndex;
    [SerializeField]
    private Sprite _fullSlotMini, _halfSlotMini;


    private List<SackFruit> _animatingFruitList;   //store all references of fruit entering in the sack's stack and its associated animation time
    private int _animatingFruitCount;

    private float _speedLoss;

    private Fruit _tempFruit;
    private float _currentReloadTime;
    private float _reloadTimer;
    private float _reloadPerFruitTime;

    private float _currentCapacity;
    private float _currentSize; //size used by containesd fruits

    private bool _fillingSack;
    private float _initFillValue, _targetFillValue;
    private float _fillTimer;

    private int _brokenFrCount;
    private float _brokenTimer;
    private float _throwFrTime, _throwFrTimer;

    private int _currentStackIndex; //top of list

    private Vector2 _initSackPos;
    private bool _seekingSack;
    private float _seekTimer;

    private bool _fullFeedback;
    private float _feedbackTimer;
    private Outline _sackOutline;
    private List<Image> _sackCountMiniList;

    private int auxFullSlot;
    #endregion
}
