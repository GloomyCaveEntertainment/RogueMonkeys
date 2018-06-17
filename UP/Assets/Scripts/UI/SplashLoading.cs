using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashLoading : MonoBehaviour {

    [SerializeField]
    private Image _loadingImg;

    private AsyncOperation _asynOp;

    private void OnEnable()
    {
        //Screen autorotation setup
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.orientation =  ScreenOrientation.AutoRotation;
        //Loading
        _asynOp = SceneManager.LoadSceneAsync(1);
    }
	
	// Update is called once per frame
	void Update () {
        _loadingImg.fillAmount = _asynOp.progress*1.15f;    //15% gain to avoid losing the fill from last iteration, ending in full filled sprite
	}
}
