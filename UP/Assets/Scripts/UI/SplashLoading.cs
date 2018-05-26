using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashLoading : MonoBehaviour {

    [SerializeField]
    private Image _loadingImg;

    private AsyncOperation _asynOp;

    private void OnEnable()
    {
        _asynOp = SceneManager.LoadSceneAsync(1);
    }
	
	// Update is called once per frame
	void Update () {
        Debug.Log("_asynOp.progress : " + _asynOp.progress);
        _loadingImg.fillAmount = _asynOp.progress*1.15f;    //15% gain to avoid losing the fill from last iteration, ending in full filled sprite
	}
}
