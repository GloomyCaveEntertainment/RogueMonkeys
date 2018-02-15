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
	void Start () {
        _targetBarValue = 0f;
        _barGrowing = false;
        _initGlowColor = Color.white;// _glowSprite.color.a;
        _paused = false;
        _showLevelTime = true;
        _gameMgr = GameMgr.Instance;
        if (_gameMgr == null)
            Debug.LogError("No Game Manager found!");

    }
	
	// Update is called once per frame
	void Update () {
        #region Alarm
        if (_barGrowing)
        {
            _barTimer += Time.deltaTime;
            _alarmSbar.value = _initBarValue + (_targetBarValue-_initBarValue) *_growCurve.Evaluate(_barTimer / _barGrowTime);
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
            if (_targetBarValue >= 0.75f && (_currentFadeTime != _glowFadeTime_75pc) )
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
            else if (_targetBarValue < 0.75f &&  _currentFadeTime != _glowFadeTime_50pc)
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
        _levelText.text = LocalizationService.Instance.GetTextByKey("loc_stage_"+_gameMgr.GetCurrentStage().GetStageIndex()) +" - "+ (level+1).ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowLoseScreen(bool show)
    {
        _loseScreen.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    public void GoToMenu()
    {
        //if (Tutorial.Instance != null)
            //DestroyImmediate(Tutorial.Instance);
        SceneManager.LoadScene("LevelSelection");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Pause()
    {
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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromLoseScreen"></param>
    public void Retry(bool fromLoseScreen)
    {
        GameMgr.Instance.StartCurrentLevel();
        if (fromLoseScreen)
        {
            ++_gameMgr.LvlAttempts;
            ShowLoseScreen(false);
        }
        else
        {
            _lvlFinishedScr.gameObject.SetActive(false);
            _gameMgr.LvlAttempts = 0;
        }
    }

    /// <summary>
    /// Slowmotion feedback
    /// </summary>
    /// <param name="enable"></param>
    public void SetSlowMoGlow(bool enable)
    {
        _slowMoGlow.SetActive(enable);
            if (enable)
            _slowMoGlow.GetComponent<ParticleSystem>().Play();
    }

    /// <summary>
    /// Speed boos / slowmotion UI feedback
    /// </summary>
    /// <param name="value"></param>
    public void UpdateSlowMoFill(float value)
    {
        _slowMoFill.fillAmount = value;
    }

    /// <summary>
    /// Speed boost
    /// </summary>
    /// <param name="enable"></param>
    public void SetSpeedUp(bool enable)
    {
        _slowMoBg.gameObject.SetActive(enable);
        _slowMoFill.gameObject.SetActive(enable);
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
        Debug.Log("------------Rect-------> " + _pauseBtn.transform.position+" and pos: "+pos);
        if ((pos.x >= _pauseBtn.transform.position.x - _pauseBtn.GetComponent<RectTransform>().rect.width * 0.5f) &&
                (pos.x <= _pauseBtn.transform.position.x + _pauseBtn.GetComponent<RectTransform>().rect.width * 0.5f) &&
                (pos.y >= _pauseBtn.transform.position.y - _pauseBtn.GetComponent<RectTransform>().rect.height * 0.5f) &&
                (pos.y >= _pauseBtn.transform.position.y + _pauseBtn.GetComponent<RectTransform>().rect.height * 0.5f))
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
        GameMgr.Instance._CollectorMonkey._Sack.GetComponent<Image>().enabled = !show;
        GameMgr.Instance._CollectorMonkey._Sack.transform.GetChild(4).GetComponent<Image>().enabled = !show;    //sack fill img
        _fakeShack.gameObject.SetActive(show);
    }
    #endregion


    #region Private Methods

    #endregion


    #region Properties
    public Text ScoreText { get { return _scoreText; } set { _scoreText = value; } }
    public bool _ShowLevelTime { get { return _showLevelTime; } set { _showLevelTime = value; } }
    public Text LvlIntroText { get { return _lvlIntroText; } set { _lvlIntroText = value; } }
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
    private Image _slowMoFill, _slowMoBg;
    [SerializeField]
    private GameObject _slowMoGlow;

    [SerializeField]
    private Text _lvlIntroText;

    [SerializeField]
    private GameObject _fakeShack;
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
