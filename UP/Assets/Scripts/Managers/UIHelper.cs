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

public class UIHelper : MonoBehaviour {

    #region Public Data
    public const int _comboCount_s = 5;
    public const int _comboCount_m = 10;
    public const int _comboCount_l = 15;
    public const int _comboFontSize_s = 35; //[2,5]
    public const int _comboFontSize_m = 40; //[6,10]
    public const int _comboFontSize_l = 45; //[11,15]
    public const int _comboFontSize_xl = 50; //[16,inf)
    public const int _comboFontSize_result = 50;
    public static UIHelper Instance;
    #endregion


    #region Behaviour Methods

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
        else
            DontDestroyOnLoad(this);
    }

    // Use this for initialization
    void Start() {
        _targetBarValue = 0f;
        _barGrowing = false;
        _initGlowColor = Color.white;// _glowSprite.color.a;
        _paused = false;
        _showLevelTime = true;
        _gameMgr = GameMgr.Instance;
        if (_gameMgr == null)
            Debug.LogError("No Game Manager found!");
        //_comboFeedbackTime = _gameMgr.MaxComboTime;
        _slowmotionDecal.SetActive(false);
        _slowmotionStrikerBgFx.SetActive(false);
        _slowMoSand_bot.gameObject.SetActive(false);
        _slowMoSand_top.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update() {
        #region Alarm
        if (_barGrowing)
        {
            _barTimer += Time.deltaTime;
            _alarmSbar.value = _initBarValue + (_targetBarValue - _initBarValue) * _growCurve.Evaluate(_barTimer / _barGrowTime);
            //_alarmSbar.value = Mathf.Lerp(_initBarValue, _targetBarValue, _growCurve.Evaluate(_barTimer / _barGrowTime));
            if (_barTimer >= _barGrowTime)
                _barGrowing = false;
        }
        if (glowEnabled)
        {
            _barGlowTimer += Time.deltaTime;
            _glowSprite.color = Color.Lerp(_initGlowColor, _endGlowColor, _barGlowTimer / _currentFadeTime);// new Color(Mathf.Lerp(_initGlowColor, _endGlowColor, _barGlowTimer / _currentFadeTime),/*_glowSprite.color.r,*/ _glowSprite.color.g, _glowSprite.color.b, 1f/*Mathf.Lerp(_initGlowAlpha, _endGlowAlpha, _barGlowTimer / _currentFadeTime)*/);
            if (_barGlowTimer >= _currentFadeTime)
            {
                //Collor swap
                _auxAlpha = _endGlowColor;
                _endGlowColor = _initGlowColor;
                _initGlowColor = _auxAlpha;
                _barGlowTimer = 0f;
            }
        }
        #endregion


    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    public void SetLvlScreenTime(float time)
    {
        if (time < 0f)
            time = 0f;

        //Format
        if (time < 5f)
            _timeText.text = time.ToString("00.00");
        else if (time < 10f)
            _timeText.text = time.ToString("00.0");
        else
            _timeText.text = time.ToString("00");

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
            _shopScreen.gameObject.SetActive(true);
            if (_stageCompletedFx.isPlaying)
                _stageCompletedFx.Stop();
        }
        else
        {
            _shopScreen.Back();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowInventory(bool show)
    {

        if (show)
        {
            _invScreen.InitScreen();
            if (LeanTween.isTweening(_inventoryIconFx))
            {
                LeanTween.cancel(_inventoryIconFx);
                _inventoryIconFx.SetActive(false);
            }
            if (_stageCompletedFx.isPlaying)
                _stageCompletedFx.Stop();
        }
        _invScreen.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowAdRewardPopup(bool show)
    {
        _adPopup.SetActive(show);
    }

    public void ShowRewardAd()
    {
        AdsMgr.Instance.ShowRewardAd();
        ShowAdRewardPopup(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowLevelFinishedScreen()
    {
        _lvlFinishedScr.SetupMenu();
        _lvlFinishedScr.gameObject.SetActive(true);
        _lvlFinishedScr.StartScreen();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetAlarmLevel()
    {

        //_currentAlarmLvl = 0;
        _alarmText.text = GameMgr.Instance.CurrentAlarmLevel.ToString("0") + " / " + GameMgr.Instance.MaxAlarmLevel;
        _initBarValue = _targetBarValue;
        _targetBarValue = GameMgr.Instance.CurrentAlarmLevel / GameMgr.Instance.MaxAlarmLevel;
        if (_targetBarValue >= 0.5f)
        {
            if (_targetBarValue >= 0.75f && (_currentFadeTime != _glowFadeTime_75pc))
            {
                _currentFadeTime = _glowFadeTime_75pc;
                //Speed up tween animation
                if (LeanTween.isTweening(_alarmIcon))
                {
                    LeanTween.cancel(_alarmIcon);
                    _alarmIcon.transform.localScale = Vector3.one;
                    LeanTween.scale(_alarmIcon, Vector2.one * 1.2f, _currentFadeTime).setLoopPingPong();
                }
            }
            else if (_targetBarValue < 0.75f && _currentFadeTime != _glowFadeTime_50pc)
            {
                _currentFadeTime = _glowFadeTime_50pc;
                //Lower speed
                if (LeanTween.isTweening(_alarmIcon))
                {
                    LeanTween.cancel(_alarmIcon);
                    _alarmIcon.transform.localScale = Vector3.one;
                    LeanTween.scale(_alarmIcon, Vector2.one * 1.2f, _currentFadeTime).setLoopPingPong();
                }
            }

            if (!glowEnabled)
            {
                glowEnabled = true;
                _barGlowTimer = 0f;
                //_glowSprite.gameObject.SetActive(true);
                _endGlowColor = Color.red;
                LeanTween.scale(_alarmIcon, Vector2.one * 1.2f, _currentFadeTime).setLoopPingPong();
            }
        }

        _barGrowing = true;
        _barTimer = 0f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="val"></param>
    public void AlarmDepleted(float val)
    {
        if (_barGrowing)
            _targetBarValue -= val;
        else
            _alarmSbar.value -= val;
        _alarmText.text = GameMgr.Instance.CurrentAlarmLevel.ToString("0") + " / " + GameMgr.Instance.MaxAlarmLevel;
        if (glowEnabled && GameMgr.Instance.CurrentAlarmLevel/*_targetBarValue*/ < 50f)
        {
            glowEnabled = false;
            if (LeanTween.isTweening(_alarmIcon))
            {
                LeanTween.cancel(_alarmIcon);
                _alarmIcon.transform.localScale = Vector3.one;

                _currentFadeTime = _glowFadeTime_50pc;
            }
            _glowSprite.color = Color.white;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void ResetAlarmUI()
    {
        glowEnabled = false;
        _glowSprite.color = Color.white;
        if (LeanTween.isTweening(_alarmIcon))
        {
            LeanTween.cancel(_alarmIcon);
            _alarmIcon.transform.localScale = Vector3.one;
        }

        //_glowSprite.gameObject.SetActive(false);
    }
    public void SetLevelText(int level)
    {
        _levelText.text = LocalizationService.Instance.GetTextByKey("loc_stage_" + _gameMgr.GetCurrentStage().GetStageIndex()) + " - " + (level + 1).ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowLoseScreen(bool show)
    {
        _loseScreen.SetActive(show);
        _slowmotionDecal.SetActive(false);
        _slowmotionStrikerBgFx.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void GoToMenu()
    {
        if (Tutorial.Instance != null)
            DestroyImmediate(Tutorial.Instance.gameObject);
        _gameMgr.SetIdle();
        SceneManager.LoadScene("LevelSelection");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Pause()
    {
        if (_gameMgr.GetCurrentLevel().GetLevelState() == Level.L_STATE.FINISHED)
            return;

        _paused = !_paused;
        GameMgr.Instance.Pause(_paused);
        _pausePanel.SetActive(_paused);
        _pauseBtn.interactable = !_paused;
        // if (_paused)
        AudioController.PauseMusic(0.5f);
        //else
        //AudioController.
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowLevelTime(bool show)
    {
        _showLevelTime = show;
        _timeText.gameObject.SetActive(show);
        _clock.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromLoseScreen"></param>
    public void Retry(bool fromLoseScreen)
    {
        

        
        if (fromLoseScreen)
        {
            ++_gameMgr.LvlAttempts;
            ShowLoseScreen(false);
        }
        else
        {
            _lvlFinishedScr.gameObject.SetActive(false);
            _gameMgr.LvlAttempts = 0;

            //Recover level index, alresdy incremented in FinishedScreen
            //(1)general case
            if (GameMgr.Instance.LevelIndex > 0)
            {
                //edge case: last stage, last level completed, avoid lowering lvl index
                if (_gameMgr.GetCurrentLevel().AvailabilitySt == Level.AVAILABILITY_STATE.UNLOCKED)
                    --GameMgr.Instance.LevelIndex;
            }

            //level = 0, check if we came from previous stage
            else
            {
                if (GameMgr.Instance.StageIndex < 0)
                {
                    --GameMgr.Instance.StageIndex;
                    GameMgr.Instance.LevelIndex = 14;

                }
                else
                {
                    Debug.LogWarning("Trying to retry lvl 0 as if we succeeded, REVIEW");

                }
            }
        }

        GameMgr.Instance.StartCurrentLevel();
    }

    /// <summary>
    /// Slowmotion feedback
    /// </summary>
    /// <param name="enable"></param>
    public void SetSlowMoGlow(bool enable)
    {
        _slowMoGlow.SetActive(enable);
        if (enable)
        {
            _slowMoGlow.GetComponent<ParticleSystem>().Stop();
            _slowMoGlow.GetComponent<ParticleSystem>().Play();

        }
        //_slowmotionDecal.SetActive(enable);
        //Tree, background, net elements and guards to grey
        GameMgr.Instance._FruitTree.GetComponent<Image>().color = enable ? _slowMoDisabledColor : Color.white;
        foreach (Guard g in _gameMgr.GetCurrentLevel().GuardList)
            g.GetComponent<Image>().color = enable ? _slowMoDisabledColor : Color.white;
        _gameMgr.GetCurrentLevel().GetSceneLayout().GetBackground().GetComponent<Image>().color = enable ? _slowMoDisabledColor : Color.white;
        foreach (GameObject net in _gameMgr.GetCurrentLevel().GetSceneLayout().GetNetList())
            net.GetComponent<Image>().color = enable ? _slowMoDisabledColor : Color.white;
        _slowmotionStrikerBgFx.SetActive(enable);
        _slowMoSand_bot.gameObject.SetActive(enable);
        _slowMoSand_top.gameObject.SetActive(enable);
    }

    /// <summary>
    /// Speed boos / slowmotion UI feedback
    /// </summary>
    /// <param name="value"></param>
    public void UpdateSlowMoFill(float value, bool enabled = false)
    {
        if (enabled)
        {
            _slowMoSand_top.fillAmount = value;
            _slowMoSand_bot.fillAmount = 1f - value;
        }
        else
            _slowMoFill_global.fillAmount = value;
    }



    /// <summary>
    /// Speed boost
    /// </summary>
    /// <param name="enable"></param>
    public void SetSpeedUp(bool enable)
    {
        _slowMoBg.gameObject.SetActive(enable);
        _slowMoFill_global.gameObject.SetActive(enable);
        //if (_slowMoGlow.gameObject.activeSelf)
        //_slowMoGlow.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool ContainsPauseBtn(Vector2 pos)
    {
        Debug.Log("------------Rect-------> " + _pauseBtn.transform.position + " and pos: " + pos);
        if ((pos.x >= _pauseBtn.transform.position.x - _pauseBtn.GetComponent<RectTransform>().rect.width * 0.5f) &&
                (pos.x <= _pauseBtn.transform.position.x + _pauseBtn.GetComponent<RectTransform>().rect.width * 0.5f) &&
                (pos.y >= _pauseBtn.transform.position.y - _pauseBtn.GetComponent<RectTransform>().rect.height * 0.5f) &&
                (pos.y <= _pauseBtn.transform.position.y + _pauseBtn.GetComponent<RectTransform>().rect.height * 0.5f))
            return true;
        return false;
        //return _pauseBtn.GetComponent<RectTransform>().rect.Contains(pos);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowFakeShack(bool show)
    {
        //TODO: update with sack img index or serialize the images and set it directly to the sack insetad activating objects
        //_gameMgr._CollectorMonkey._Sack.transform.GetChild(0).GetComponentInChildren<Image>().enabled = !show;
        //_gameMgr._CollectorMonkey._Sack.transform.GetChild(5).GetComponent<Image>().enabled = !show;    //sack fill img
        //_gameMgr._CollectorMonkey._Sack.transform.GetChild(4).gameObject.SetActive(!show);    //sack count miniatures
        //_fakeShack.GetComponent<Image>().sprite = _gameMgr._CollectorMonkey._Sack.transform.GetChild(5).GetComponent<Image>().sprite;
        //_fakeShack.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public void ShowComboCount(int count)
    {
        //Size
        if (count <= _comboCount_s)
        {
            _comboText.fontSize = _comboFontSize_s;
            _comboText.color = _comboColor_s;
        }
        else if (count <= _comboCount_m)
        {
            _comboText.fontSize = _comboFontSize_m;
            _comboText.color = _comboColor_m;
        }
        else if (count <= _comboCount_l)
        {
            _comboText.fontSize = _comboFontSize_l;
            _comboText.color = _comboColor_l;
        }
        else
        {
            _comboText.fontSize = _comboFontSize_xl;
            _comboText.color = _comboColor_xl;
        }
        _comboText.transform.rotation = Quaternion.identity;

        _comboText.text = "x " + count.ToString();
        //Bg
        if (!_comboBg.gameObject.activeSelf)
            _comboBg.gameObject.SetActive(true);
        if (_comboTotalBg.gameObject.activeSelf)
            _comboTotalBg.gameObject.SetActive(false);
        LeanTween.cancel(_comboBg.gameObject);
        LeanTween.alpha(_comboBg.gameObject, .01f, _comboFeedbackTime);

        //_comboText.color = Color.white;
        if (!_comboText.transform.parent.gameObject.activeInHierarchy)
            _comboText.transform.parent.gameObject.SetActive(true);
        LeanTween.cancel(_comboText.gameObject);
        LeanTween.cancel(_comboText.transform.parent.gameObject);
        _comboText.transform.parent.localScale = Vector3.one;
        _comboText.transform.parent.gameObject.transform.localPosition = Vector3.zero;
        LeanTween.scale(_comboText.transform.parent.gameObject, Vector3.one * 1.5f, _comboFeedbackTime*0.5f);
        LeanTween.moveLocalY(_comboText.transform.parent.gameObject, _comboOffset, _comboFeedbackTime);
        LeanTween.textAlpha(_comboText.rectTransform, .01f, _comboFeedbackTime).setOnComplete(() => { _comboText.transform.parent.gameObject.SetActive(false); });

        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="comboCount"></param>
    public void ShowComboResult(int comboCount)
    {
        //TODO: localization
        _comboText.text = "+ " + comboCount.ToString();
        _comboText.color = _comboResultColor;
        _comboText.fontSize = _comboFontSize_result;
        _comboText.transform.rotation = Quaternion.Euler(Vector3.forward * 25f);

        if (!_comboText.transform.parent.gameObject.activeInHierarchy)
            _comboText.transform.parent.gameObject.SetActive(true);
        //Bg
        if (_comboBg.gameObject.activeSelf)
            _comboBg.gameObject.SetActive(false);
        if (!_comboTotalBg.gameObject.activeSelf)
            _comboTotalBg.gameObject.SetActive(true);
        LeanTween.cancel(_comboTotalBg.gameObject);
        LeanTween.alpha(_comboTotalBg.gameObject, .01f, _comboFeedbackTime);

        if (!_comboText.gameObject.activeInHierarchy)
            _comboText.gameObject.SetActive(true);
        LeanTween.cancel(_comboText.transform.parent.gameObject);
        LeanTween.cancel(_comboText.gameObject);
        LeanTween.moveLocalY(_comboText.transform.parent.gameObject, _comboOffset, _comboFeedbackTime);
        _comboText.transform.parent.localScale = Vector3.one;
        LeanTween.scale(_comboText.transform.parent.gameObject, Vector3.one * 1.2f, _comboFeedbackTime*0.5f);
        LeanTween.textAlpha(_comboText.rectTransform, .01f, _comboFeedbackTime).setOnComplete(() => {  _comboText.transform.parent.gameObject.SetActive(false); });

        
    }

    public void HideComboCount()
    {
        _comboText.gameObject.SetActive(false);
        _comboText.transform.parent.gameObject.SetActive(false);
    }
    #endregion


    #region Private Methods

    #endregion


    #region Properties
    public Text ScoreText { get { return _scoreText; } set { _scoreText = value; } }
    public bool _ShowLevelTime { get { return _showLevelTime; } set { _showLevelTime = value; } }
    public Text LvlIntroText { get { return _lvlIntroText; } set { _lvlIntroText = value; } }
    public GameObject SlowmotionStrikerBgFx { get { return _slowmotionStrikerBgFx;} private set { }}
    public GameObject FakeSack {  get { return _fakeShack; }  set { _fakeShack = value; } }
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    private InventoryScreen _invScreen;
    [SerializeField]
    private ShopMenu _shopScreen;
    [SerializeField]
    private LevelFinishedScreen _lvlFinishedScr;
    [SerializeField]
    private GameObject _loseScreen;
    [SerializeField]
    private GameObject _pausePanel;
    [SerializeField]
    private Button _pauseBtn;
    [SerializeField]
    private GameObject _inventoryIconFx;

    [SerializeField]
    private Text _alarmText;
    [SerializeField]
    private Text _levelText;
    [SerializeField]
    private Text _scoreText;
    [SerializeField]
    private Text _timeText;
    [SerializeField]
    private GameObject _adPopup;
    [SerializeField]
    private Scrollbar _alarmSbar;
    [SerializeField]
    private GameObject _alarmIcon;
    [SerializeField]
    private float _barGrowTime;
    [SerializeField]
    private AnimationCurve _growCurve;
    [SerializeField]
    private Image _glowSprite;
    [SerializeField]
    private float _glowFadeTime_50pc, _glowFadeTime_75pc;

    [SerializeField]
    private Image _slowMoFill_global, _slowMoSand_top, _slowMoSand_bot, _slowMoBg;
    [SerializeField]
    private GameObject _slowMoGlow;

    [SerializeField]
    private Text _lvlIntroText;

    [SerializeField]
    private GameObject _fakeShack;

    [SerializeField]
    private Text _comboText;
    [SerializeField]
    private Image _comboBg, _comboTotalBg;
    [SerializeField]
    private float _comboOffset;
    [SerializeField]
    private float _comboFeedbackTime;

    [SerializeField]
    private Color _comboColor_s, _comboColor_m, _comboColor_l, _comboColor_xl;
    [SerializeField]
    private Color _comboResultColor;

    [SerializeField]
    private GameObject _slowmotionDecal;
    [SerializeField]
    private GameObject _slowmotionStrikerBgFx;

    [SerializeField]
    private GameObject _clock;

    [SerializeField]
    private Color _slowMoDisabledColor;
    [SerializeField]
    private ParticleSystem _stageCompletedFx;
    #endregion

    #region Private Non-serialized Fields
    private GameMgr _gameMgr;
    private float _initBarValue, _targetBarValue;   //used to lerp bar values
    private float _barTimer;
    private float _barGlowTimer, _currentFadeTime;
    private Color _initGlowColor, _endGlowColor, _auxAlpha;
    private bool _barGrowing, glowEnabled;
    private bool _paused;
    private bool _showLevelTime;
	#endregion
}
