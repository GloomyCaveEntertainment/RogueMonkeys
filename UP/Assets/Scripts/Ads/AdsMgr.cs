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
using UnityEngine.Advertisements;


public class AdsMgr : MonoBehaviour {

	#region Public Data
    public static AdsMgr Instance;

    public const int _minGoldRewardSuccess = 10, _maxGoldRewardSuccess = 25;
    public const int _skippedGoldReward = 1, _failedGoldReward = 1;
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Awake () {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
	}

    void Start()
    {
        _regularTimer = 0f;
        _rewardAdTimer = 0f;
        _regularAdReady = false;
        _rewardAdReady = false;
        _adOptionsOnLevelStart = new ShowOptions { resultCallback = HandleShowResultOnStartLevel };
    }
    void Update()
    {
        if (!_regularAdReady)
        {
            _regularTimer += Time.deltaTime;
            if (_regularTimer >= _regularAdsMinTime)
            {
                _regularTimer = 0f;
                _regularAdReady = true;
            }
        }

        if (!_rewardAdReady)
        {
            _rewardAdTimer += Time.deltaTime;
            if (_rewardAdTimer >= _rewardAdsMinTime)
            {
                _rewardAdTimer = 0f;
                _rewardAdReady = true;
            }
        }
    }
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bypassTimer"></param>
    /// <returns></returns>
    public bool ShowAd(bool bypassTimer = false)
    {
        if (bypassTimer || _regularAdReady)
        {
            ++AnalyticsMgr.Instance.AdsAttemptCount;
            if (Advertisement.IsReady())
            {
                Advertisement.Show(_adOptionsOnLevelStart);
                ++AnalyticsMgr.Instance.AdsShownCount;
                _regularAdReady = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowAdsOnExit()
    {
        ++AnalyticsMgr.Instance.AdsAttemptCount;
        ++AnalyticsMgr.Instance.AdsAttemptOnExitCount;
        if (Advertisement.IsReady())
        {
            var options = new ShowOptions { resultCallback = ExitAfterAds  };
            Advertisement.Show(options);
            ++AnalyticsMgr.Instance.AdsShownCount;
            ++AnalyticsMgr.Instance.AdsShownOnExitCount;
        }
        else
        {
            Application.Quit();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowRewardAd()
    {
        ++AnalyticsMgr.Instance.RewarAdsAttemptCount;
        if (Advertisement.IsReady())
        {
            ShowOptions options = new ShowOptions { resultCallback = HandleShowResultOnRewardAds };
            Advertisement.Show(options);
            ++AnalyticsMgr.Instance.RewardAdsShownCount;
            _rewardAdReady = false;
        }
    }

#if UNITY_ADS
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    private void ExitAfterAds(ShowResult result)
    {
        /*
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                //
                // YOUR CODE TO REWARD THE GAMER
                // Give coins etc.
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
         * */
        Application.Quit();
    }

#endif
	#endregion


	#region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    private void HandleShowResultOnStartLevel(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                //
                // YOUR CODE TO REWARD THE GAMER
                // Give coins etc.
                Debug.Log("ADS::: Ad Success");
                ++AnalyticsMgr.Instance.AdsFailedCount;
                break;
            case ShowResult.Skipped:
                Debug.Log("ADS::: Ad Skipped");
                ++AnalyticsMgr.Instance.AdsSkippedCount;
                break;
            case ShowResult.Failed:
                Debug.Log("ADS::: Ad Failed");
                ++AnalyticsMgr.Instance.AdsFailedCount;
                break;
        }
        GameMgr.Instance.LevelReady();
        AnalyticsMgr.Instance.PushAds(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    private void HandleShowResultOnRewardAds(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                //
                // YOUR CODE TO REWARD THE GAMER
                // Give coins etc.
                Debug.Log("ADS::: Ad Success");
                ++AnalyticsMgr.Instance.RewardAdsSkippedCount;
                GameMgr.Instance.AddGold(Random.Range(_minGoldRewardSuccess, _maxGoldRewardSuccess));
                break;
            case ShowResult.Skipped:
                Debug.Log("ADS::: Ad Skipped");
                ++AnalyticsMgr.Instance.RewardAdsSkippedCount;
                GameMgr.Instance.AddGold(_skippedGoldReward);
                break;
            case ShowResult.Failed:
                Debug.Log("ADS::: Ad Failed");
                ++AnalyticsMgr.Instance.RewardAdsFailedCount;
                GameMgr.Instance.AddGold(_failedGoldReward);
                break;
        }
        //TODO: gold animation + sound feedback

        //TODO: habdle to disable popup button
        GameObject.FindGameObjectWithTag("RewardPopupBtn").GetComponent<Button>().interactable = false;
        GameObject.FindGameObjectWithTag("RewardPopupBtn").GetComponent<Outline>().enabled = false;
        AnalyticsMgr.Instance.PushAds(true);
    }
	#endregion


	#region Properties
    public bool RewardAdsReady { get { return _rewardAdReady; } set { _rewardAdReady = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private float _regularAdsMinTime;  //min time between being able to show ad
    [SerializeField]
    private float _rewardAdsMinTime;  //min time between giving the chance to get a reward by seeing an ad
	#endregion

	#region Private Non-serialized Fields
    private float _regularTimer;
    private float _rewardAdTimer;
    private bool _regularAdReady;
    private bool _rewardAdReady;
    private ShowOptions _adOptionsOnLevelStart;
	#endregion
}
