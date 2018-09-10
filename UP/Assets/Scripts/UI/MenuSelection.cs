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

public class MenuSelection : MonoBehaviour {

	#region Public Data
    public enum MENU_STATE { IDLE = 0, LOADING_BASE, LOADED, SHOWING_PADLOCKC }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        InitSelectionMenu();
	}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CenterLevelPanel(10);
            //SetOpeningPadlock();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            CenterLevelPanel(2);
            //SetOpeningPadlock();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            CenterLevelPanel(0);
            //SetOpeningPadlock();
        }
    }

    // Update is called once per frame
    /*void Update () {
        switch (_state)
        {
            case MENU_STATE.LOADING_BASE:
                if (_asyincOp.isDone)
                {
                    _state = MENU_STATE.LOADED;
                    //Set level values
                    Debug.Log("Setup lvl " + _currentLevelIndex + " . . .");
                    
                }
                break;
        }
	}*/
    #endregion

    #region Public Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void ShowStageAndCenter(int index)
    {
        /*if (_currentStageIndex == index)
            return;*/

        //center stage content panel
        //_stageScrollRect.content.Translate(new Vector3(_stageMiniList[index].transform.position.x - _stageCenterRef.transform.position.x, 0f, 0f));
        Debug.Log("Moving index "+index+" : " + _contentPanelInitPos + Vector2.right * _distanceBetweenStageMinis * index);
        _stageScrollRect.content.position =  new Vector2(_contentPanelInitPos.x - _distanceBetweenStageMinis * index, _contentPanelInitPos.y);// new Vector3(transform.position.x + _stageCenterRef.transform.position.x - _stageMiniList[index].transform.position.x, 0f, 0f);// new Vector3(_stageMiniList[index].transform.position.x - _stageCenterRef.transform.position.x, 0f, 0f);
        _stageScrollRect.velocity = Vector2.zero;
        ShowStage(index);
        //switch level panel
        /*if (_currentStageIndex!= -1)
            _stageLevelPanelList[_currentStageIndex].SetActive(false);
        _currentStageIndex = index;
        _stageLevelPanelList[_currentStageIndex].SetActive(true);
        _stageText.text = LocalizationService.Instance.GetTextByKey("loc_stage") + " " + _currentStageIndex.ToString();
        UpdateStageButtons();*/

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    public void CenterLevelPanel(int level)
    {
        Debug.Log("Center to lvl: " + level);
        //3 =  levels per row
        _levelListScrollRect.content.position = new Vector2(_contentLevelPanelInitPos.x, _contentLevelPanelInitPos.y +_distanceBetweenLevelMinis * Mathf.FloorToInt(level/3));// new Vector3(transform.position.x + _stageCenterRef.transform.position.x - _stageMiniList[index].transform.position.x, 0f, 0f);// new Vector3(_stageMiniList[index].transform.position.x - _stageCenterRef.transform.position.x, 0f, 0f);
        _stageScrollRect.velocity = Vector2.zero;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void ShowStage(int index, bool btnClicked = true)
    {
        /*if (_currentStageIndex == index)
            return;*/
        Debug.Log("Show stage index: " + index);
        
        //switch level panel
        _stageMiniList[_currentStageIndex].transform.GetChild(0).GetComponent<Image>().sprite = _stageFrame;    //unselect previous miniature
        if (_currentStageIndex != -1)
            _stageLevelPanelList[_currentStageIndex].SetActive(false);
        _currentStageIndex = index;
        _stageMiniList[_currentStageIndex].transform.GetChild(0).GetComponent<Image>().sprite = _selectedStageFrame;     //select current miniature
        if (GameMgr.Instance._StageList[_currentStageIndex].State == Stage.STAGE_STATE.UNLOCKED || GameMgr.Instance._StageList[_currentStageIndex].State == Stage.STAGE_STATE.COMPLETED)
            _currentAvailableStageIndex = _currentStageIndex;
        _levelListScrollRect.content = _stageLevelPanelList[_currentStageIndex].GetComponent<RectTransform>();
        _stageLevelPanelList[_currentStageIndex].SetActive(true);
        _stageScrollRect.transform.GetChild(2).GetComponent<Text>().text += " " + (_currentStageIndex + 1).ToString();
        _stageText.text = LocalizationService.Instance.GetTextByKey("loc_stage") + " " + (_currentStageIndex + 1).ToString();
        UpdateStageButtons();

        if (btnClicked)
            AudioController.Play("aud_menu_accept");
        else
            AudioController.Play("aud_woosh_01");
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckScrollStage()
    {
        float xDist = Mathf.Infinity;
        float stageDist = -1f;
        int closestStageIndex = -1;

        for (int i = 0; i < _stageMiniList.Count; ++i)
        {
            stageDist = Vector2.Distance(_stageMiniList[i].transform.position, _stageCenterRef.transform.position);
            //Debug.Log("Stage dist: " + stageDist);
            if (stageDist < xDist)
            {
                xDist = stageDist;
                closestStageIndex = i;
            }
        }
        //Debug.Log("Closest index is: " + closestStageIndex);
        if (_currentStageIndex != closestStageIndex)
            ShowStage(closestStageIndex, false);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void SelectLevel(int index)
    {
        //if (_currentLevelIndex == index)
            //return;
        Debug.Log("lvl index: " + _currentLevelIndex);
        //Disable previous
        if (_currentLevelIndex != -1)
            _stageLevelPanelList[_currentStageIndex].transform.GetChild(_currentLevelIndex).GetChild(_frameIndex).gameObject.SetActive(false);
        _currentLevelIndex = index;
        Debug.Log("child count: " + _stageLevelPanelList[_currentStageIndex].transform.GetChild(_currentLevelIndex).transform.childCount);
        //TODO: Feedback
        _stageLevelPanelList[_currentStageIndex].transform.GetChild(_currentLevelIndex).GetChild(_frameIndex).GetComponent<Image>().sprite = _levelFrameSelected;
        _stageLevelPanelList[_currentStageIndex].transform.GetChild(_currentLevelIndex).GetChild(_frameIndex).gameObject.SetActive(true);
        //Play Button
        if (!_playButton.activeSelf)
            _playButton.SetActive(true);
    }
    /// <summary>
    /// 
    /// </summary>
    public void BakckToMenu()
    {
        AudioController.Play("aud_menu_back");
        SceneManager.LoadScene("Menu");
    }

    public void PlayLevel()
    {
        AudioController.Play("aud_menu_accept");
        AudioController.Stop("Menu_Theme", 0.5f);
        Debug.Log("Play Level");
       // DataMgr.Instance.StageIndex = _currentStageIndex;
        //DataMgr.Instance.LevelIndex = _currentLevelIndex;
        PlayerPrefs.SetInt("Current_Stage", _currentAvailableStageIndex);
        PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
        SceneManager.LoadScene("Game");
        GameMgr.Instance.LoadAndStartCurrentLevel();
   
    }

    /// <summary>
    /// 
    /// </summary>
    public void InitSelectionMenu()
    {
        _currentStageIndex = GameMgr.Instance.StageIndex;
        _currentAvailableStageIndex = _currentStageIndex;
        _currentLevelIndex = GameMgr.Instance.LevelIndex;// - 1;
        _contentPanelInitPos = _stageScrollRect.content.transform.position;
        _contentLevelPanelInitPos = _levelListScrollRect.content.transform.position;
        _distanceBetweenStageMinis = _stageMiniList[1].transform.position.x - _stageMiniList[0].transform.position.x;
        _distanceBetweenLevelMinis = _stageLevelPanelList[0].transform.GetChild(0).transform.position.y - _stageLevelPanelList[0].transform.GetChild(4).transform.position.y;
        SetupLevelList();
        //UpdateStageButtons();
        //ShowStage(_currentStageIndex);
        ShowStageAndCenter(_currentStageIndex);
        CenterLevelPanel(_currentLevelIndex);
        //UpdateStageButtons();
        _playButton.SetActive(false);
        EnableRewardAdsButton(AdsMgr.Instance.RewardAdsReady);
        _goldText.text = GameMgr.Instance.Gold.ToString("0");

        CheckIventoryFeedback();

        if (!AudioController.IsPlaying("Menu_Theme"))
            AudioController.Play("Menu_Theme");

        if (GameMgr.Instance.LevelUnlocked)
        {
            GameMgr.Instance.LevelUnlocked = false;
            _state = MENU_STATE.SHOWING_PADLOCKC;
            SetOpeningPadlock();
        }
        else
            _state = MENU_STATE.LOADED;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowShop(bool show)
    {
        if (show)
        {
            _shopScreen.InitShop();          
            AudioController.Play("aud_menu_accept");
        }
        else
            AudioController.Play("aud_menu_back");

        _shopScreen.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowInventory(bool show)
    {
        if (show)
        {
            AudioController.Play("aud_menu_accept");
            _invScreen.InitScreen();
            if (LeanTween.isTweening(_inventoryIconFx))
            {
                LeanTween.cancel(_inventoryIconFx);
                _inventoryIconFx.SetActive(false);
            }
        }
        else
            AudioController.Play("aud_menu_back");
        _invScreen.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowAdRewardPopup(bool show)
    {
        _adPopup.SetActive(show);
        if (show)
            AudioController.Play("aud_menu_accept");
        else
            AudioController.Play("aud_menu_back");
    }

    public void ShowRewardAd()
    {
        AdsMgr.Instance.ShowRewardAd();
        _adPopup.SetActive(false);
        AudioController.Play("aud_menu_accept");
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateMoney()
    {
        _goldText.text = GameMgr.Instance.Gold.ToString("0");
        //AudioController.Play("aud_money_01");
        LeanTween.scale(_goldText.gameObject, _goldText.transform.localScale * 1.2f, .5f).setEase(_goldFbCurve);//.setLoopPingPong(1);
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
    private void SetupLevelList()
    {
        int enabledIndex = 1;

        Debug.Log("________________________________Setup Level List");
        _stageList = GameMgr.Instance._StageList;
        //if (GameMgr.Instance.GetCurrentLevel() != null)
            enabledIndex = GameMgr.Instance.StageIndex;
        //else
            //enabledIndex = 0;

        for (int i = 0; i < _stageList.Count; ++i)
        {
            //Stage setup
            switch (_stageList[i].GetAvState())
            {
                case Stage.STAGE_STATE.COMPLETED:
                    _stageMiniList[i].transform.GetChild(1).gameObject.SetActive(false);
                    _stageMiniList[i].transform.GetChild(2).gameObject.SetActive(false);
                    _stageMiniList[i].transform.GetChild(3).gameObject.SetActive(true);
                    _stageMiniList[i].transform.GetChild(3).GetChild(0).GetComponent<Image>().sprite = GetStageRank(i);
                    _stageMiniList[i].GetComponent<Image>().color = Color.white;
                    break;

                case Stage.STAGE_STATE.UNLOCKED:
                    _stageMiniList[i].transform.GetChild(1).gameObject.SetActive(false);
                    _stageMiniList[i].transform.GetChild(2).gameObject.SetActive(false);
                    _stageMiniList[i].transform.GetChild(3).gameObject.SetActive(false);
                    _stageMiniList[i].GetComponent<Image>().color = Color.white;
                    break;

                case Stage.STAGE_STATE.LOCKED:
                    _stageMiniList[i].transform.GetChild(1).gameObject.SetActive(true);
                    _stageMiniList[i].transform.GetChild(2).gameObject.SetActive(false);
                    _stageMiniList[i].transform.GetChild(3).gameObject.SetActive(false);
                    _stageMiniList[i].GetComponent<Image>().color = Color.white;
                    break;

                case Stage.STAGE_STATE.UNAVAILABLE:
                    _stageMiniList[i].transform.GetChild(1).gameObject.SetActive(false);
                    _stageMiniList[i].transform.GetChild(2).gameObject.SetActive(true);
                    _stageMiniList[i].transform.GetChild(3).gameObject.SetActive(false);
                    _stageMiniList[i].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f); //almost black
                    break;
            }
            _stageLevelPanelList[i].SetActive(i == enabledIndex);
            for (int j = 0; j < _stageList[i].GetLevelList().Count; ++j)
            {
                switch (_stageList[i].GetLevelList()[j].GetAvState())
                {
                    case Level.AVAILABILITY_STATE.UNLOCKED:
                    case Level.AVAILABILITY_STATE.FAILED:
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_bgIndex).GetComponent<Image>().sprite = _levelUnlockedBg;
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_unavailableIndex).gameObject.SetActive(false);
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_padlockIndex).gameObject.SetActive(false);
                        //_stageLevelPanelList[i].transform.GetChild(j).GetComponent<Image>().color = Color.white;
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_bgIndex).GetComponent<Button>().interactable = true;
                        break;

                    case Level.AVAILABILITY_STATE.LOCKED:
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_bgIndex).GetComponent<Image>().sprite = _levelUnlockedBg;
                        //TODO: 
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_unavailableIndex).gameObject.SetActive(false);
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_padlockIndex).GetComponent<Image>().sprite = _levelPadlock_closed;
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_padlockIndex).gameObject.SetActive(true);
                        //_stageLevelPanelList[i].transform.GetChild(j).GetComponent<Image>().color = Color.red;
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_bgIndex).GetComponent<Button>().interactable = false;
                        break;

                    case Level.AVAILABILITY_STATE.COMPLETED:
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_bgIndex).GetComponent<Image>().sprite = _levelCompletedBg;
                        //_stageLevelPanelList[i].transform.GetChild(j).GetComponent<Image>().color = Color.green;
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_unavailableIndex).gameObject.SetActive(false);
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_padlockIndex).gameObject.SetActive(false);
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_bgIndex).GetComponent<Button>().interactable = true;
                        break;

                    case Level.AVAILABILITY_STATE.UNAVAILABLE:
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_bgIndex).GetComponent<Image>().sprite = _levelUnavailableBg;
                        //TODO: label
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_unavailableIndex).GetComponent<Image>().sprite = _levelUnavailableLabel;
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_unavailableIndex).gameObject.SetActive(true);
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_padlockIndex).gameObject.SetActive(false);
                        //_stageLevelPanelList[i].transform.GetChild(j).GetComponent<Image>().color = Color.black;
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_bgIndex).GetComponent<Button>().interactable = false;
                        break;
                }
                //Frame
                _stageLevelPanelList[i].transform.GetChild(j).GetChild(_frameIndex).gameObject.SetActive(false);

                //Enable+ setup or Disable rank miniatures
                if (_stageList[i].GetLevelList()[j].GetAvState() != Level.AVAILABILITY_STATE.COMPLETED && _stageList[i].GetLevelList()[j].GetAvState() != Level.AVAILABILITY_STATE.FAILED)
                {
                    _stageLevelPanelList[i].transform.GetChild(j).GetChild(_rankIndex)./*GetChild(0).*/gameObject.SetActive(false);
                    //_stageLevelPanelList[i].transform.GetChild(j).GetChild(3).GetChild(1).gameObject.SetActive(false);

                }
                else
                {
                    _stageLevelPanelList[i].transform.GetChild(j).GetChild(_rankIndex).GetChild(0).GetComponent<Image>().sprite = _rankLetterSpList[(int)_stageList[i].GetLevelList()[j].Rank];
                    if (_stageList[i].GetLevelList()[j].GetAvState() == Level.AVAILABILITY_STATE.FAILED)
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_rankIndex).GetChild(1).GetComponent<Image>().sprite = _failStamp;
                    else
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_rankIndex).GetChild(1).GetComponent<Image>().sprite = _successStamp;
                    _stageLevelPanelList[i].transform.GetChild(j).GetChild(_rankIndex).GetChild(0).gameObject.SetActive(true);
                    _stageLevelPanelList[i].transform.GetChild(j).GetChild(_rankIndex).GetChild(1).gameObject.SetActive(true);
                }

                //Setup fruit miniatures
                if (_stageList[i].GetLevelList()[j].GetAvState() == Level.AVAILABILITY_STATE.UNAVAILABLE)
                {
                    //Disable fruits root
                    _stageLevelPanelList[i].transform.GetChild(j).GetChild(_fruitMiniRootIndex).gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("Stage ::: " + i);
                    //Set fruit types on miniature
                    _auxfruitTypeList = _stageList[i].GetLevelList()[j].GetFruitTypeSpawnList();
                    for (int k = 0; k < _auxfruitTypeList.Count; ++k)
                    {
                        if (_auxfruitTypeList[k].FruitTypeIndex != (int)Fruit.F_TYPE.GOLD_ITEM)
                        {
                            Debug.Log("j"+j+"_auxLvlMiniFruitListIndex" + _auxLvlMiniFruitListIndex+ "_auxfruitTypeList[k]"+ _auxfruitTypeList[k].FruitTypeIndex);
                            _stageLevelPanelList[i].transform.GetChild(j).GetChild(_fruitMiniRootIndex).GetChild(_auxLvlMiniFruitListIndex).GetComponent<Image>().sprite = _spriteFruitList[(int)_auxfruitTypeList[k].FruitTypeIndex];
                            _stageLevelPanelList[i].transform.GetChild(j).GetChild(_fruitMiniRootIndex).GetChild(_auxLvlMiniFruitListIndex).gameObject.SetActive(true);
                            ++_auxLvlMiniFruitListIndex;
                        }

                    }

                    //Disable non used fruit iniatures
                    for (int k = _auxLvlMiniFruitListIndex; k < _stageLevelPanelList[i].transform.GetChild(j).GetChild(_fruitMiniRootIndex).childCount; ++k)
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(_fruitMiniRootIndex).GetChild(k).gameObject.SetActive(false);
                }

                _stageLevelPanelList[i].transform.GetChild(j).GetChild(_scoreContainerIndex).GetComponentInChildren<Text>().text = _stageList[i].GetLevelList()[j].GetMaxScore().ToString("0");
                

                
                //reset iindex for next lvl iteration
                _auxLvlMiniFruitListIndex = 0;
                //text
                _stageLevelPanelList[i].transform.GetChild(j).GetChild(_numContainerIndex).GetComponentInChildren<Text>().text = string.Format(LocalizationService.Instance.GetTextByKey("loc_level") + (j+1).ToString());

            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateStageButtons()
    {
        for (int i = 0; i < _stageSelectionButtonList.Count; ++i)
        {
            if (_currentStageIndex == i)
            {
                _stageSelectionButtonList[i].GetComponent<RectTransform>().sizeDelta = _selectedStageButtonScale;
                //_stageSelectionButtonList[i].Select();
                _stageSelectionButtonList[i].GetComponent<Image>().sprite = _selectedStageBtnSp;
            }
            else
            {
                _stageSelectionButtonList[i].GetComponent<RectTransform>().sizeDelta = _unselectedStageButtonScale;
                _stageSelectionButtonList[i].GetComponent<Image>().sprite = _unselectedStageBtnSp;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enable"></param>
    private void EnableRewardAdsButton(bool enable)
    {
        _rewardAdsButton.interactable = enable;
        _rewardAdsButton.GetComponent<Outline>().enabled = enable;

    }

    /// <summary>
    /// 
    /// </summary>
    private void PadlockAnimCompleted()
    {
        _state = MENU_STATE.LOADED;
        _padlock_open_object.SetActive(false);
        _padlock_open_object.transform.parent =  _initPadlockParent;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetOpeningPadlock()
    {
        if (_initPadlockParent == null)
            _initPadlockParent = _padlock_open_object.transform.parent;
        //New stage unlocked
        if (GameMgr.Instance.LevelIndex == 0)
        {
            ShowStageAndCenter(GameMgr.Instance.StageIndex);
            _padlock_open_object.transform.parent = _stageMiniList[GameMgr.Instance.StageIndex].transform;//.GetChild(GameMgr.Instance.LevelIndex).GetChild(_stageLevelPanelList[GameMgr.Instance.StageIndex].transform.GetChild(GameMgr.Instance.LevelIndex).transform.childCount - 2);
            _padlock_open_object.transform.localPosition = Vector3.zero;
            _padlock_open_object.SetActive(true);
            //LeanTween.rotate(_padlock_open_object, Vector3.forward * _padlockAnimRot, _padlockAnimTime);
            LeanTween.moveLocalX(_padlock_open_object, _padlock_open_object.transform.localPosition.x + _padlockAnimMoveDir.x, _padlockAnimTime);
            LeanTween.moveLocalY(_padlock_open_object, _padlock_open_object.transform.localPosition.y + _padlockAnimMoveDir.y, _padlockAnimTime).setEase(_padlockAnimC).setOnComplete(() => { PadlockAnimCompleted(); });

        }
        //level unlocked (except if game has been completed)
        else if (GameMgr.Instance.StageIndex != GameMgr.Instance._StageList.Count-1 && GameMgr.Instance.LevelIndex != GameMgr.Instance.GetCurrentStage().GetLevelList().Count-1)//new level unlocked
        {
            //_padlock_open_object.transform.position = _stageLevelPanelList[GameMgr.Instance.StageIndex].transform.GetChild(GameMgr.Instance.LevelIndex).GetChild(_stageLevelPanelList[GameMgr.Instance.StageIndex].transform.GetChild(GameMgr.Instance.LevelIndex).transform.childCount - 2).position;
            _padlock_open_object.transform.parent = _stageLevelPanelList[GameMgr.Instance.StageIndex].transform.GetChild(GameMgr.Instance.LevelIndex);
            _padlock_open_object.transform.localPosition = Vector3.zero;
            _padlock_open_object.SetActive(true);
            //LeanTween.rotate(_padlock_open_object, Vector3.forward * _padlockAnimRot, _padlockAnimTime);
            LeanTween.moveLocalX(_padlock_open_object, _padlock_open_object.transform.localPosition.x + _padlockAnimMoveDir.x, _padlockAnimTime);
            LeanTween.moveLocalY(_padlock_open_object, _padlock_open_object.transform.localPosition.y + _padlockAnimMoveDir.y, _padlockAnimTime).setEase(_padlockAnimC).setOnComplete(() => { PadlockAnimCompleted(); });
        }

    }

    /// <summary>
    /// Calculate average rank and return its letter sprite
    /// </summary>
    /// <param name="stageIndex"></param>
    /// <returns></returns>
    private Sprite GetStageRank(int stageIndex)
    {
        int sum = 0;
        Sprite ret = null;

        foreach (Level lvl in GameMgr.Instance._StageList[stageIndex].GetLevelList())
        {
            switch (lvl.Rank)
            {
                case Level.RANK.S:
                    sum += 6;
                    break;

                case Level.RANK.A:
                    sum += 5;
                    break;

                case Level.RANK.B:
                    sum += 4;
                    break;

                case Level.RANK.C:
                    sum += 3;
                    break;

                case Level.RANK.D:
                    sum += 2;
                    break;

                case Level.RANK.E:
                    sum += 1;
                    break;

            }
        }
        sum = Mathf.RoundToInt((float)sum / GameMgr.Instance._StageList[stageIndex].GetLevelList().Count);
        switch (sum)
        {
            case 1:
                ret = _rankLetterSpList[5];
                break;

            case 2:
                ret = _rankLetterSpList[4];
                break;

            case 3:
                ret = _rankLetterSpList[3];
                break;

            case 4:
                ret = _rankLetterSpList[2];
                break;

            case 5:
                ret = _rankLetterSpList[1];
                break;

            case 6:
                ret = _rankLetterSpList[0];
                break;
        }
        return ret;
    }
    #endregion


    #region Properties
    public List<Stage> _StageList { get { return _stageList; } set { _stageList = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private ScrollRect _stageScrollRect;
    [SerializeField]
    private Transform _stageCenterRef;      //reference position to freeze selected stage
    [SerializeField]
    private List<GameObject>_stageMiniList;
    [SerializeField]
    private ScrollRect _levelListScrollRect;
    [SerializeField]
    private List<GameObject> _stageLevelPanelList;
    [SerializeField]
    private List<Button> _stageSelectionButtonList;
    [SerializeField]
    private GameObject _playButton;
    [SerializeField]
    private List<Stage> _stageList;
    [SerializeField]
    private float _distanceBetweenStageMinis;   //distance between stage miniature; used to seek content panel on stage select
    [SerializeField]
    private float _distanceBetweenLevelMinis;
    [SerializeField]
    private Vector2 _selectedStageButtonScale, _unselectedStageButtonScale;
    [SerializeField]
    private Sprite _selectedStageBtnSp, _unselectedStageBtnSp;
    [SerializeField]
    private List<Sprite> _spriteFruitList;      //used to set lvl miniatures fruits

    [SerializeField]
    private ShopMenu _shopScreen;
    [SerializeField]
    private InventoryScreen _invScreen;
    [SerializeField]
    private GameObject _adPopup;
    [SerializeField]
    private Text _stageText;

    [SerializeField]
    private Button _rewardAdsButton;

    [SerializeField]
    private Sprite _successStamp, _failStamp;
    [SerializeField]
    private List<Sprite> _rankLetterSpList;
    [SerializeField]
    private Text _goldText;
    [SerializeField]
    private AnimationCurve _goldFbCurve;

    [SerializeField]
    private GameObject _inventoryIconFx;

    [SerializeField]
    private Sprite _selectedStageFrame, _stageFrame;

    [SerializeField]
    private Sprite _levelCompletedBg, _levelFailedBg, _levelPadlock_closed, _levelPadlock_open, _levelUnavailableBg, _levelUnavailableLabel, _levelUnlockedBg, _levelLockedBg;
    [SerializeField]
    private Sprite _levelFrameSelected;

    [SerializeField]
    private GameObject _padlock_open_object;
    [SerializeField]
    private Vector2 _padlockAnimMoveDir;
    [SerializeField]
    private float _padlockAnimTime;
    [SerializeField]
    private float _padlockAnimRot;
    [SerializeField]
    private AnimationCurve _padlockAnimC;

    //Children indexes level selection miniatures
    [SerializeField]
    private int _scoreContainerIndex, _fruitMiniRootIndex, _numContainerIndex, _frameIndex, _rankIndex, _padlockIndex, _unavailableIndex, _bgIndex;
    #endregion

    #region Private Non-serialized Fields
    private AsyncOperation _asyincOp;
    private MENU_STATE _state;

    private int _currentStageIndex, _currentAvailableStageIndex;
    private int _currentLevelIndex;
    private Vector2 _contentPanelInitPos;
    private Vector2 _contentLevelPanelInitPos;
    private List<Level.FruitSpawn> _auxfruitTypeList;
    private int _auxLvlMiniFruitListIndex;      //index used to track the fruit list over the level miniature on setup
    private Transform _initPadlockParent;
    #endregion
}
