/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsMgr : MonoBehaviour {

	#region Public Data
    public static AnalyticsMgr Instance;
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
    void Start()
    {
        _eventTimer = 0f;
        _dict = new Dictionary<string, object>();
    }
	// Update is called once per frame
	/*void Update () {
        _eventTimer += Time.deltaTime;
        if (_eventTimer >= _eventPushTimeInterval)
            PushAds();
	}*/
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void PushAds(bool adsForGold)
    {
        _dict.Clear();
        if (adsForGold)
        {
            _dict["attempts"] = _rewarAdsAttemptCount;
            _dict["shown"] = _rewardAdsShownCount;
            //_dict["attempts_on_exit"] = _adsAttemptOnExitCount;
            //_dict["shown_on_exit"] = _adsShownOnExitCount;
            _dict["skipped"] = _rewardAdsSkippedCount;
            _dict["failed"] = _rewardAdsFailedCount;
            _dict["goldAds"] = 1;
            Analytics.CustomEvent("Large_Ads", _dict);
            _rewarAdsAttemptCount = _rewardAdsShownCount = _rewardAdsSkippedCount = _rewardAdsFailedCount = 0;
        }
        else
        {
            _dict["attempts"] = _adsAttemptCount;
            _dict["shown"] = _adsShownCount;
            //_dict["attempts_on_exit"] = _adsAttemptOnExitCount;
            //_dict["shown_on_exit"] = _adsShownOnExitCount;
            _dict["skipped"] = _adsSkippedCount;
            _dict["failed"] = _adsFailedCount;
            _dict["goldAds"] = 0;
            Analytics.CustomEvent("Large_Ads", _dict);
            _adsAttemptCount = _adsShownCount = _adsSkippedCount = _adsFailedCount = 0;/*_adsAttemptOnExitCount = _adsShownOnExitCount =*/
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LevelEnded()
    {
        _dict.Clear();
        _dict["stage"] = GameMgr.Instance.StageIndex;
        _dict["lvl"] = GameMgr.Instance.LevelIndex;
        _dict["completed"] = GameMgr.Instance.Win;
        _dict["attempts"] = GameMgr.Instance.LvlAttempts; //
        _dict["gold_itm_dropped"] = GameMgr.Instance.GoldItmLostCount;
        _dict["gold_itm_collected"] = GameMgr.Instance.GoldItmCount;
        _dict["eqp_itm_dropped"] = GameMgr.Instance.EqpItmLostCount;
        _dict["eqp_itm_collected"] = GameMgr.Instance.EqpItmCount;
        Analytics.CustomEvent("Level_Completed", _dict);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void EquipmentItemSpawned(string id)
    {
        _dict.Clear();
        _dict["id"] = id;
        _dict["stage"] = GameMgr.Instance.StageIndex;
        _dict["lvl"] = GameMgr.Instance.LevelIndex;
        Analytics.CustomEvent("Equip_Spwn", _dict);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void GoldItemSpawned(string id)
    {
        _dict.Clear();
        _dict["id"] = id;
        _dict["stage"] = GameMgr.Instance.StageIndex;
        _dict["lvl"] = GameMgr.Instance.LevelIndex;
        Analytics.CustomEvent("G_Itm_Spwn", _dict);
    }
    #endregion


    #region Private Methods

    #endregion


    #region Properties
    public int AdsAttemptCount { get { return _adsAttemptCount; } set { _adsAttemptCount = value; } }
    public int AdsShownCount { get { return _adsShownCount; } set { _adsShownCount = value; } }
    public int AdsAttemptOnExitCount { get { return _adsAttemptOnExitCount; } set { _adsAttemptOnExitCount = value; } }
    public int AdsShownOnExitCount { get { return _adsShownOnExitCount; } set { _adsShownOnExitCount = value; } }
    public int AdsSkippedCount { get { return _adsSkippedCount; } set { _adsSkippedCount = value; } }
    public int AdsFailedCount { get { return _adsFailedCount; } set { _adsFailedCount = value; } }
    public int AdsFinishedCount { get { return _adsFinishedCount; } set { _adsFinishedCount = value; } }
    public int RewarAdsAttemptCount { get { return _rewarAdsAttemptCount; } set { _rewarAdsAttemptCount = value; } }
    public int RewardAdsShownCount { get { return _rewardAdsShownCount; } set { _rewardAdsShownCount = value; } }
    public int RewardAdsSkippedCount { get { return _rewardAdsSkippedCount; } set { _rewardAdsSkippedCount = value; } }
    public int RewardAdsFailedCount { get { return _rewardAdsFailedCount; } set { _rewardAdsFailedCount = value; } }
    public int RewardAdsFinishedCount { get { return _rewardAdsFinishedCount; } set { _rewardAdsFinishedCount = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private float _eventPushTimeInterval;
    #endregion

    #region Private Non-serialized Fields
    private Dictionary<string, object> _dict;
    private float _eventTimer;

    private int _adsAttemptCount, _adsShownCount;
    private int _rewarAdsAttemptCount, _rewardAdsShownCount;
    private int _adsAttemptOnExitCount, _adsShownOnExitCount;
    private int _adsSkippedCount, _adsFailedCount, _adsFinishedCount; //ad result state
    private int _rewardAdsSkippedCount, _rewardAdsFailedCount, _rewardAdsFinishedCount;

	#endregion
}
