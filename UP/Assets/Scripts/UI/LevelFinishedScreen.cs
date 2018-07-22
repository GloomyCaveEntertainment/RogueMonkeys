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

public class LevelFinishedScreen : MonoBehaviour {

    #region Public Data
    /// <summary>
    /// Ratios over S rank score
    /// </summary>
    public const float _rankSRatio = .95f;
    public const float _rankARatio = .9f;
    public const float _rankBRatio = .8f;
    public const float _rankCRatio = .7f;
    public const float _rankDRatio = .6f;
    public const float _rankERatio = .5f;
    public const float _rankFRatio = 0f;

    public enum LF_MENU_STATE { IDLE = 0, COUNTING, BONUS, TOTAL, RANK, FINISHED }

    public class FruitIndex
    {
        public Fruit.F_TYPE _Ftype;
        public Fruit.RIPEN_STATE _Rstate;   //used to have different entries for Ripen type Fruit
        public int _Index;       //index in image list
        public int EggQuality;
        public int _Count;
        
        public FruitIndex(Fruit.F_TYPE t, int index)
        {
            _Ftype = t;
            this._Index = index;
            _Count = 1;
        }
        public FruitIndex(Fruit.F_TYPE t, Fruit.RIPEN_STATE s, int index)
        {
            _Ftype = t;
            _Rstate = s;
            this._Index = index;
            _Count = 1;
        }

        public FruitIndex(Fruit.F_TYPE t, int eggQuality, int index)
        {
            _Ftype = t;
            EggQuality = eggQuality;
            this._Index = index;
            _Count = 1;
        }
    }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        _fruitIndex = new List<FruitIndex>();
        EnableIconButtons(false);
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case LF_MENU_STATE.IDLE:

                break;

            case LF_MENU_STATE.COUNTING:
                _timer += Time.deltaTime;
                if (_timer >= _countingTimePerFruit)
                {
                    _timer = 0f;
                    //Item - Gold
                    if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.EQUIPMENT ||
                        GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.GOLD_ITEM ||
                        ( GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.EGG && GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._EggQuality == Fruit._maxEggQuality-1))
                    {
                        _itemImageList[_collectedItemIndex].sprite = GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Sprite;
                        _itemImageList[_collectedItemIndex].gameObject.SetActive(true);
                        if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.GOLD_ITEM ||
                            GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.EGG)
                            _tempGold += GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Gold;
                        else if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.EQUIPMENT)
                        {
                            //public EquipmentItem(string id, string spriteId, int value, SLOT_TYPE sT, List<MOD_TYPE> mtList, List<float> modValList, int count = -1) 
                            DataMgr.Instance.AddInventoryItem(GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._E_Item, false);
                            //GameMgr.Instance.ShowInvFb = true;
                        }

                        _goldText.text = _tempGold.ToString();
                        ++_collectedItemIndex;
                        //Overriden
                        if (_overridenItemsCount != 0 || _collectedItemIndex == _itemImageList.Count)
                        {
                            ++_overridenItemsCount;
                            if (_collectedItemIndex == _itemImageList.Count)
                                _collectedItemIndex = 0;
                            _overridenCountText.text = "+"+_overridenItemsCount.ToString();
                        }
                        if (!_goldIcon.activeSelf)
                            _goldIcon.SetActive(true);
                        
                    }
                    else//Fruit
                    {
                        //Search for fruit type on index and if its Ripen, Ripen state must be equal too
                        if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.RIPEN)
                            _temp = _fruitIndex.Find((fi) => (fi._Ftype == Fruit.F_TYPE.RIPEN && fi._Rstate == GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._R_State));
                        else if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.EGG)
                            _temp = _fruitIndex.Find((fi) => (fi._Ftype ==Fruit.F_TYPE.EGG && fi.EggQuality == GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._EggQuality));
                        else if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.BANANA_CLUSTER_UNIT || GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.BANANA)
                            _temp = _fruitIndex.Find((fi) => (fi._Ftype == Fruit.F_TYPE.BANANA || fi._Ftype == Fruit.F_TYPE.BANANA_CLUSTER_UNIT));
                        else
                            _temp = _fruitIndex.Find((fi) => (fi._Ftype == GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type));

                        //add this fruit as new list element
                        if (_temp == null)
                        {
                            if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.RIPEN)
                                _fruitIndex.Add(new FruitIndex(GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type, GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._R_State, _maxFruitDisplayedIndex));
                            else if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type == Fruit.F_TYPE.EGG)
                                _fruitIndex.Add(new FruitIndex(GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type, GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._EggQuality, _maxFruitDisplayedIndex));
                            else
                                _fruitIndex.Add(new FruitIndex(GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._F_Type, _maxFruitDisplayedIndex));

                            _fruitImageList[_maxFruitDisplayedIndex].sprite = GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Sprite;
                            _fruitTextCountList[_maxFruitDisplayedIndex].text = "1";
                            _fruitImageList[_maxFruitDisplayedIndex].gameObject.SetActive(true);
                            ++_maxFruitDisplayedIndex;
                        }
                        //increment count if fruit is already set on UI
                        else
                        {
                            //Add one element to fruit count and update screen counter
                            ++_temp._Count;
                            Debug.Log("Coun augmented so: "+_temp._Count);
                            _fruitTextCountList[_temp._Index].text = _temp._Count.ToString("0");
                        }
                        _tempScore += GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Score;
                        if (LeanTween.isTweening(_scoreText.gameObject))
                            LeanTween.cancel(_scoreText.gameObject);
                        LeanTween.value(_tempScore - GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Score, _tempScore, _countingTimePerFruit).setOnUpdate((v) => _scoreText.text = ((float)v).ToString("0"));

                        
                    }
                    //Update collected index and check if list ended to show rank
                    ++_collectedFruitIndex;
                    //End count
                    if (_collectedFruitIndex == GameMgr.Instance.StageCollectedFruits.Count)
                    {
                        _state = LF_MENU_STATE.BONUS;
                        _timer = 0f;
                    }
                }
                break;

            case LF_MENU_STATE.BONUS:             
                _bonusScoreText.gameObject.SetActive(true);
                _bonusScoreTextLabel.gameObject.SetActive(true);
                _timer += Time.deltaTime;
                if (_timer / _bonusTextTime > 1f)
                    _bonusScoreText.text = (GameMgr.Instance.Score - _tempScore).ToString("0");
                else
                    _bonusScoreText.text = ((GameMgr.Instance.Score - _tempScore) * (_timer / _bonusTextTime)).ToString("0");
                if (_timer >= _bonusTextTime)
                {
                    _state = LF_MENU_STATE.TOTAL;
                    _timer = 0f;
                }
                break;

            case LF_MENU_STATE.TOTAL:               
                _totalScoreText.gameObject.SetActive(true);
                _totalScoreTextLabel.gameObject.SetActive(true);
                _timer += Time.deltaTime;
                if (_timer / _bonusTextTime > 1f)
                    _totalScoreText.text = GameMgr.Instance.Score.ToString("0");
                else
                    _totalScoreText.text = (GameMgr.Instance.Score * (_timer / _bonusTextTime)).ToString("0");
                if (_timer >= _bonusTextTime)
                {
                    AudioController.Stop("aud_score");
                    _state = LF_MENU_STATE.RANK;
                    _timer = 0f;
                    //Rank letter + stamp
                    SetRank();

                    if (GameMgr.Instance.GetCurrentLevel().CheckForHighScore(GameMgr.Instance.Score))
                        _newScoreFb.SetActive(true);
                    
                }
                break;

            case LF_MENU_STATE.RANK:
                _timer += Time.deltaTime;
                if (!_rankAudioPlayed /*AudioController.IsPlaying("rank_stamp")*/ && _timer >= _rankShowTime*0.75f)
                {
                    _rankAudioPlayed = true;
                    AudioController.Play("aud_stamp");
                }
                    
                if (_timer >= _rankShowTime)
                {
                    //Add gold
                    GameMgr.Instance.AddGold(_tempGold);
                    GameMgr.Instance.SaveProgress();
                    _state = LF_MENU_STATE.FINISHED;
                    if (GameMgr.Instance.GetCurrentLevel().GetAvState() != Level.AVAILABILITY_STATE.FAILED)
                    {
                        if (GameMgr.Instance.LevelIndex < GameMgr.Instance.GetCurrentStage().GetLevelList().Count - 1)
                            ++GameMgr.Instance.LevelIndex;
                        else
                        {
                            if (GameMgr.Instance.StageIndex < GameMgr.Instance._StageList.Count)
                            {
                                //not last stage, last level
                                if (GameMgr.Instance.StageIndex < GameMgr.Instance._StageList.Count - 1)
                                {
                                    ++GameMgr.Instance.StageIndex;
                                    GameMgr.Instance.LevelIndex = 0;
                                }
                                _stageFinishedFb.gameObject.SetActive(true);
                                LeanTween.scale(_stageFinishedFb.gameObject, Vector3.one * 1.1f, 1f).setLoopPingPong();
                            }
                            else
                            {
                                //LAST LEVEL, LAST STAGE COMPLETED
                            }
                        }
                        //GameMgr.Instance.LevelUnlocked = true;
                        //GameMgr.Instance.UpdateAndSaveProgression();
                    }
                    _nextButton.SetActive(true);    //always enabled since it goes to Selection Menu
                    //}

                    _retryButton.SetActive(true);
                    EnableGoldFeedback(GameMgr.Instance.GoldCollected);
                    //EnableItemFeedback(GameMgr.Instance.ItemCollected);
                    //GameMgr.Instance.ShowInvFb = true;
                    EnableIconButtons(true);
                    //Inventory Feedback
                    if (GameMgr.Instance.ItemCollected/*GameMgr.Instance.ShowInvFb*/)
                    {
                        GameMgr.Instance.ShowInvFb = false;
                        _inventoryIconFx.SetActive(true);
                        //LeanTween.color(_inventoryIconFx, Color.green, 1f).setLoopPingPong();
                    }
                    
                }
                break;

            case LF_MENU_STATE.FINISHED:
                
                
                break;
        }
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void SetupMenu()
    {
        if (_fruitIndex == null)
            _fruitIndex = new List<FruitIndex>();
        else
            if (_fruitIndex.Count != 0)
                _fruitIndex.Clear();
        _state = LF_MENU_STATE.IDLE;
        EnableIconButtons(false);
        foreach (Image fI in _fruitImageList)
            fI.gameObject.SetActive(false);
        foreach (Text t in _fruitTextCountList)
            t.text = "0";
        foreach (Image _itemImg in _itemImageList)
            _itemImg.gameObject.SetActive(false);

        _goldText.text = "";
        _rankLetterImg.gameObject.SetActive(false);
        _rankStampSuccess.gameObject.SetActive(false);
        _rankStampFail.gameObject.SetActive(false);
        _newScoreFb.SetActive(false);
        _rankAudioPlayed = false;
        _scoreText.gameObject.SetActive(false);
        _scoreLabel.gameObject.SetActive(false);
        _bonusScoreText.gameObject.SetActive(false);
        _bonusScoreTextLabel.gameObject.SetActive(false);
        _totalScoreText.gameObject.SetActive(false);
        _totalScoreTextLabel.gameObject.SetActive(false);

    }

    /// <summary>
    /// 
    /// </summary>
    public void StartScreen()
    {
        Debug.Log("tartscreeen");
        if (GameMgr.Instance.StageCollectedFruits.Count == 0)
            _state = LF_MENU_STATE.RANK;
        else
        {
            _state = LF_MENU_STATE.COUNTING;
            _scoreText.gameObject.SetActive(true);
            _scoreLabel.gameObject.SetActive(true);
            AudioController.Play("aud_score");
        }
        _timer = 0f;
        _temp = null;
        _tempScore = 0;
        _tempGold = 0;
        _goldText.text = "";
        _goldIcon.SetActive(false);
        if (LeanTween.isTweening(_stageFinishedFb.gameObject))
            LeanTween.cancel(_stageFinishedFb.gameObject);
        _stageFinishedFb.gameObject.SetActive(false);
        _nextButton.SetActive(false);
        _retryButton.SetActive(false);
        _rankLetterImg.gameObject.SetActive(false);
        _rankStampSuccess.gameObject.SetActive(false);
        _maxFruitDisplayedIndex = 0;
        _collectedFruitIndex = 0;
        _collectedItemIndex = 0;
        _countingTimePerFruit = _countingTime/GameMgr.Instance.StageCollectedFruits.Count;
        _overridenItemsCount = 0;
        _overridenCountText.text = "";
        _inventoryIconFx.SetActive(false);
        EnableGoldFeedback(false);

        //Inventory Feedback
        /*if (GameMgr.Instance.ShowInvFb)
        {
            GameMgr.Instance.ShowInvFb = false;
            _inventoryIconFx.SetActive(true);
            LeanTween.color(_inventoryIconFx, Color.green, 1f).setLoopPingPong();
        }*/
    }


    public void EnableIconButtons(bool enable)
    {
        foreach (Button b in _iconButtonList)
            b.interactable = enable;
        
    }

    public void GoToNextLevel()
    {
        GameMgr.Instance.LoadAndStartNextLevel();
        gameObject.SetActive(false);
    }

    public void GoToMenuSelection()
    {
        SceneManager.LoadScene("LevelSelection");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Enable Inventory feedback if needed
    /// </summary>
    public void CheckIventoryFeedback()
    {
        if (GameMgr.Instance.ShowInvFb)
        {
            GameMgr.Instance.ShowInvFb = false;
            _inventoryIconFx.SetActive(true);
            //LeanTween.color(_inventoryIconFx, Color.green, 1f).setLoopPingPong();
        }
        else if (_inventoryIconFx.activeSelf)
            _inventoryIconFx.SetActive(false);
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    private void SetRank()
    {
        SetRankRatio();
        //Rank stamp scale animation
        _rankStampInitScale = _currentStamp.transform.localScale;
        _currentStamp.transform.localScale = new Vector3(_rankStampInitScale.x, _rankStampInitScale.y) * 2f;
        _currentStamp.gameObject.SetActive(true);
        LeanTween.scale(_currentStamp.gameObject, _rankStampInitScale, _rankShowTime).setEase(_stampAC);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private void SetRankRatio()
    {
        //Score ratio = (collected * 2 + dismissed * 1) / (total spawned fruit * 2)
        float currentScoreRatio = (GameMgr.Instance.StageSucceededFruitCount * 2f + GameMgr.Instance.StageDismissedFruitCount) / (GameMgr.Instance.StageSpawnedFruitCount * 2f);//( GameMgr.Instance.Score - GameMgr.Instance.CurrentAlarmLevel * GameMgr.Instance.GetCurrentLevel().PenaltyPerAlarmPt)/ GameMgr.Instance.GetCurrentLevel().MinRankSuperScore;
        Debug.Log("Ratio calculated: " + currentScoreRatio + "score: " + GameMgr.Instance.Score + "CALvl: " + GameMgr.Instance.CurrentAlarmLevel);
        //TODO: Chinese letters?
        if (currentScoreRatio >= _rankSRatio)
        {
            _rankLetterImg.sprite = _rankLetterSpriteList[0];
            _currentStamp = _rankStampSuccess;
            GameMgr.Instance.GetCurrentLevel().Rank = Level.RANK.S;
            //Check if new level has completed  to unlock next one
            /*if (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.UNLOCKED || (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.FAILED))
            {
                GameMgr.Instance.LevelUnlocked = true;
                //GameMgr.Instance.UpdateAndSaveProgression();
            }*/
            GameMgr.Instance.GetCurrentLevel().AvailabilitySt = Level.AVAILABILITY_STATE.COMPLETED;

        }
        else if (currentScoreRatio >= _rankARatio)
        {
            _rankLetterImg.sprite = _rankLetterSpriteList[1];
            _currentStamp = _rankStampSuccess;
            if (GameMgr.Instance.GetCurrentLevel().Rank > Level.RANK.A)
                GameMgr.Instance.GetCurrentLevel().Rank = Level.RANK.A;
            //Check if new level has completed  to unlock next one
            /*if (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.UNLOCKED || (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.FAILED))
            {
                GameMgr.Instance.LevelUnlocked = true;
                //GameMgr.Instance.UpdateAndSaveProgression();
            }*/
            GameMgr.Instance.GetCurrentLevel().AvailabilitySt = Level.AVAILABILITY_STATE.COMPLETED;
        }
        else if (currentScoreRatio >= _rankBRatio)
        {
            _rankLetterImg.sprite = _rankLetterSpriteList[2];
            _currentStamp = _rankStampSuccess;
            if (GameMgr.Instance.GetCurrentLevel().Rank > Level.RANK.B)
                GameMgr.Instance.GetCurrentLevel().Rank = Level.RANK.B;
            //Check if new level has completed  to unlock next one
            /*if (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.UNLOCKED || (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.FAILED))
            {
                GameMgr.Instance.LevelUnlocked = true;
                //GameMgr.Instance.UpdateAndSaveProgression();
            }*/
            GameMgr.Instance.GetCurrentLevel().AvailabilitySt = Level.AVAILABILITY_STATE.COMPLETED;
        }
        else if (currentScoreRatio >= _rankCRatio)
        {
            _rankLetterImg.sprite = _rankLetterSpriteList[3];
            _currentStamp = _rankStampSuccess;
            if (GameMgr.Instance.GetCurrentLevel().Rank > Level.RANK.C)
                GameMgr.Instance.GetCurrentLevel().Rank = Level.RANK.C;
            //Check if new level has completed  to unlock next one
            /*if (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.UNLOCKED || (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.FAILED))
            {
                GameMgr.Instance.LevelUnlocked = true;
                //GameMgr.Instance.UpdateAndSaveProgression();
            }*/
            GameMgr.Instance.GetCurrentLevel().AvailabilitySt = Level.AVAILABILITY_STATE.COMPLETED;
        }
        else if (currentScoreRatio >= _rankDRatio)
        {
            _rankLetterImg.sprite = _rankLetterSpriteList[4];
            _currentStamp = _rankStampSuccess;
            if (GameMgr.Instance.GetCurrentLevel().Rank > Level.RANK.D)
                GameMgr.Instance.GetCurrentLevel().Rank = Level.RANK.D;
            //Check if new level has completed  to unlock next one
            /*if (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.UNLOCKED || (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.FAILED))
            {
                GameMgr.Instance.LevelUnlocked = true;
                //GameMgr.Instance.UpdateAndSaveProgression();
            }*/
            GameMgr.Instance.GetCurrentLevel().AvailabilitySt = Level.AVAILABILITY_STATE.COMPLETED;
        }
        else if (currentScoreRatio >= _rankERatio)
        {
            _rankLetterImg.sprite = _rankLetterSpriteList[5];
            _currentStamp = _rankStampSuccess;
            if (GameMgr.Instance.GetCurrentLevel().Rank > Level.RANK.E)
                GameMgr.Instance.GetCurrentLevel().Rank = Level.RANK.E;
            //Check if new level has completed  to unlock next one
            /*if (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.UNLOCKED || (GameMgr.Instance.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.FAILED))
            {
                GameMgr.Instance.LevelUnlocked = true;
                //GameMgr.Instance.UpdateAndSaveProgression();
            }*/
            GameMgr.Instance.GetCurrentLevel().AvailabilitySt = Level.AVAILABILITY_STATE.COMPLETED;
        }
        //if (currentScoreRatio >= _rankFRatio)
        //return _rankLetterSpriteList[6];
        else
        {
            _rankLetterImg.sprite = _rankLetterSpriteList[6];
            _currentStamp = _rankStampFail;
            if (GameMgr.Instance.GetCurrentLevel().Rank == Level.RANK.F)
            {
                //GameMgr.Instance.GetCurrentLevel().Rank = Level.RANK.F;
                GameMgr.Instance.GetCurrentLevel().AvailabilitySt = Level.AVAILABILITY_STATE.FAILED;
            }              
        }
        //Check for stage completion
        if (GameMgr.Instance.LevelIndex == GameMgr.Instance.GetCurrentStage().GetLevelList().Count -1 && 
            GameMgr.Instance.GetCurrentLevel().AvailabilitySt != Level.AVAILABILITY_STATE.FAILED)
        {
            GameMgr.Instance.GetCurrentStage().State = Stage.STAGE_STATE.COMPLETED;

        }
        _rankLetterImg.gameObject.SetActive(true);
        _currentStamp.gameObject.SetActive(true);
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    private void EnableGoldFeedback(bool show)
    {

        _goldFeedback.SetActive(show);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    private void EnableItemFeedback(bool show)
    {
        //_itemFeedback.SetActive(show);
        _iconButtonList[3].GetComponent<Outline>().enabled = true;
  
    }
    #endregion


    #region Properties

    #endregion

    #region Private Serialized Fields
    [SerializeField]
    private List<Image> _fruitImageList;
    [SerializeField]
    private List<Text> _fruitTextCountList;
    [SerializeField]
    private List<Image> _itemImageList;
    [SerializeField]
    private Image _rankLetterImg, _rankStampSuccess, _rankStampFail;
    [SerializeField]
    private Text _scoreText, _scoreLabel;
    [SerializeField]
    private GameObject _newScoreFb;

    [SerializeField]
    private GameObject _stageFinishedFb;        //displayed when last stage level has finished
    [SerializeField]
    private GameObject _retryButton, _nextButton;
    [SerializeField]
    private List<Button> _iconButtonList;
    [SerializeField]
    private GameObject _inventoryIconFx;    //feedback used when new items are on inventory

    [SerializeField]
    private float _countingTime;
    [SerializeField]
    private float _rankShowTime;

    [SerializeField]
    private GameObject _goldIcon;
    [SerializeField]
    private Text _goldText;
    [SerializeField]
    private List<Sprite> _rankLetterSpriteList;
    [SerializeField]
    private GameObject _goldFeedback;

    [SerializeField]
    private Text _overridenCountText;

    [SerializeField]
    private AnimationCurve _stampAC;

    [SerializeField]
    private Text _bonusScoreText, _totalScoreText;
    [SerializeField]
    private Text _bonusScoreTextLabel, _totalScoreTextLabel;
    [SerializeField]
    private float _bonusTextTime;
    #endregion

    #region Private Non-serialized Fields
    private LF_MENU_STATE _state;

    private float _timer;
    private float _countingTimePerFruit;
    private int _maxFruitDisplayedIndex;     //determines the last fruit type shown on collected fruit list
    private int _collectedFruitIndex;       //index sued to go through level collected uirt list
    private List<FruitIndex> _fruitIndex;
    private int _collectedItemIndex;

    private FruitIndex _temp;       //used to go through list
    private int _tempScore;
    private int _tempGold;

    private Vector3 _rankStampInitScale;
    private Image _currentStamp;

    private int _overridenItemsCount;

    private bool _rankAudioPlayed;
	#endregion
}
