using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SunCubeStudio.Localization;

public enum STATE { LANDING = 0, MAIN, TUTORIAL, LOADING_TUTORIAL, OPTIONS, CREDITS, EXIT_POPUP }

public class MainMenu : MonoBehaviour {

    void Awake()
    {
        _st = STATE.LANDING;
        if (!_landingPage.activeSelf)
            _landingPage.SetActive(true);
    }

    void Start()
    {
        //TODO: preferences language, English by efault loaded first time

        /*if (PlayerPrefs.GetString("Language") != "")
        {
            Debug.Log("localization loaded : " + PlayerPrefs.GetString("Language"));
            LocalizationService.Instance.Localization = PlayerPrefs.GetString("Language");
        }
        else
            LocalizationService.Instance.Localization = "English";*/
    }

    void Update()
    {
        switch (_st)
        {
            case STATE.LANDING:
//Mouse input
#if UNITY_EDITOR
                if (Input.GetMouseButtonDown(0))
                {
                    _st = STATE.MAIN;
                    _landingPage.SetActive(false);
                    AudioController.Play("aud_menu_accept");
                    if (!AudioController.IsPlaying("Menu_Theme"))
                        AudioController.Play("Menu_Theme");
                }
#endif             

//Touch input
#if UNITY_ANDROID || UNITY_IPHONE
                foreach (Touch t in Input.touches)
                {
                    if (t.phase == TouchPhase.Began)
                    {
                        AudioController.Play("aud_menu_accept");
                        if (!AudioController.IsPlaying("Menu_Theme"))
                            AudioController.Play("Menu_Theme");
                        _st = STATE.MAIN;
                        _landingPage.SetActive(false);
                        //AudioController.Play("click_0");
                    }
                }
#endif
                if (_st == STATE.LANDING)   //we didnt found any touch
                {
                    _fadingTimer += Time.deltaTime;
                    //fading text
                    _startText.color = Color.Lerp(_fromAlphaColor, _toAlphaColor, _fadingTimer / _startTextFadingTime);
                    //switch color lerp values
                    if (_fadingTimer >= _startTextFadingTime)
                    {
                        Color aux = _fromAlphaColor;
                        _fromAlphaColor = _toAlphaColor;
                        _toAlphaColor = aux;
                        _fadingTimer = 0f;
                    }
                }
                break;
        }          
    }

    /// <summary>
    /// 
    /// </summary>
    public void StartGame()
    {
        AudioController.Play("aud_menu_accept");
        if (Tutorial.Instance!=null)
            DestroyImmediate(Tutorial.Instance.gameObject);
        SceneManager.LoadScene("LevelSelection");
    }
 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowButtons(bool show)
    {
        _buttonsPanel.SetActive(show);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowTutorial(bool show)
    {
        AudioController.Play("aud_menu_accept");
        if (show)
        {
            //_tutorialPanel.GetComponent<Tutorial>().StartTutorial();
            //_st = STATE.TUTORIAL;

            //GameMgr.Instance.StageIndex = 0;
            //GameMgr.Instance.LevelIndex = 0;
            PlayerPrefs.SetInt("Current_Stage", 0);
            PlayerPrefs.SetInt("Current_Level", 0);
            SceneManager.LoadScene("Game");
            GameMgr.Instance.LoadAndStartCurrentLevel();
            _st = STATE.LOADING_TUTORIAL;
            

        }
        else
        {
            _st = STATE.MAIN;
            _inputTimer = 0f;
            AudioController.Play("Menu_Theme");
        }

        ShowButtons(!show);
    }

    public void TutorialReady()
    {
        _st = STATE.TUTORIAL;
        //Tutorial.Instance.InitTutorial();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowOptions(bool show)
    {
        _optionsPanel.SetActive(show);
        //Cursor.visible = show;
        ShowButtons(!show);
        if (show)
        {
            SetupOptions();
            _st = STATE.OPTIONS;
            AudioController.Play("aud_menu_accept");
        }
        else
        {
            AudioController.Play("aud_menu_back");
            //GameMgr.Instance.LoadPlayerPrefs();
            _st = STATE.MAIN;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vol"></param>
    public void UpdateMasterVolume()
    {
        AudioController.SetGlobalVolume(_masterSlider.value);
        //_masterSlider.value
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateMusicVolume()
    {
        AudioController.SetCategoryVolume("Music",_musicSlider.value);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateSfxVolume()
    {
        AudioController.SetCategoryVolume("SFX",_sfxSlider.value);
        //AudioController.SetCategoryVolume("BloodSFX", _sfxSlider.value);
        //AudioController.SetCategoryVolume("Menu", _sfxSlider.value);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
   public void ShowCredits(bool show)
   {
       _creditsPanel.SetActive(show);
       ShowButtons(!show);
       _st = show? STATE.CREDITS : STATE.MAIN;
        if (show)
            AudioController.Play("aud_menu_accept");
        else
            AudioController.Play("aud_menu_back");
    }



   /// <summary>
   /// 
   /// </summary>
   /// <param name="enabled"></param>
   public void SetVibration()
   {
       if (_vibrationToggle.isOn)
       {
           PlayerPrefs.SetInt("Vib", 1);
           DataMgr.Instance.Vibration = 1;
           Vibration.Vibrate(GameMgr._missFruitVibrationTime);
            AudioController.Play("aud_menu_accept");
        }
       else
       {
           DataMgr.Instance.Vibration = 0;
           PlayerPrefs.SetInt("Vib", 0);
            AudioController.Play("aud_menu_back");
        }
   }

   public void SetLanguage(string lang)
   {
       LocalizationService.Instance.Localization = lang;
       PlayerPrefs.SetString("Lang", lang);
       AudioController.Play("aud_menu_accept");
   }

   public void ShowLanguagePanel(bool show)
   {
       _languagePanel.SetActive(show);
        if (show)
            AudioController.Play("aud_menu_accept");
        else
            AudioController.Play("aud_menu_back");
    }

   public void ShowExitPopUp(bool show)
   {
        _exitPopUp.SetActive(show);
   }

   /// <summary>
   /// 
   /// </summary>
   public void ExitGame()
   {
        //AdsMgr.Instance.ShowAdsOnExit();
        Application.Quit();
   }

   /// <summary>
   /// 
   /// </summary>
   private void SetupOptions()
   {
       _masterSlider.value = AudioController.GetGlobalVolume();
       _musicSlider.value = AudioController.GetCategory("Music").Volume;
       _sfxSlider.value = AudioController.GetCategory("SFX").Volume;
       _vibrationToggle.isOn = PlayerPrefs.GetInt("Vib") == 1;
   }



   [SerializeField]
   private GameObject _landingPage;

   [SerializeField]
   private GameObject _buttonsPanel;
   [SerializeField]
   private GameObject _tutorialPanel;
   [SerializeField]
   private GameObject _optionsPanel;
   [SerializeField]
   private GameObject _languagePanel;
   [SerializeField]
   private GameObject _creditsPanel;
   [SerializeField]
   private GameObject _exitPopUp;
   [SerializeField]
   private List<Image> _mainMenuButtonList;
   [SerializeField]
   private Text _startText;
   [SerializeField]
   private float _startTextFadingTime;
   [SerializeField]
   private float _minAlphaValue;
   [SerializeField]
   private Toggle _vibrationToggle;

   [SerializeField]
   private Slider _masterSlider, _musicSlider, _sfxSlider;

    

    private STATE _st;

    private float _timer;
    private float _inputTimer;
    private float _fadingTimer;
    [SerializeField]
    private Color _fromAlphaColor, _toAlphaColor;
    private int _currentIndex;
    private int _animationIndex;

    private bool _animating;

    private LTDescr _lt;
}
