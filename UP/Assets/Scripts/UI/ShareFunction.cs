using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShareFunction : MonoBehaviour {
    //Store Free version Android and iOS urls
    public const string RogueMonkeysFreeGooglePlayUrl = "https://play.google.com/store/apps/details?id=com.GloomyCaveEntertainment.RogueMonkeys_Free";
    public const string RogueMonkeysFreeAppStoreUrl = "https://itunes.apple.com/us/app/rogue-monkeys-ads/id1412100415?mt=8";

    private bool _nativeShare, _facebookShare;

    /*
    private void LateUpdate()
    {
        if (_nativeShare)
        {
            StartCoroutine(NativeShareScreenCoroutine());
        }
    }*/

    /// <summary>
    /// Share Screen through Native Share
    /// </summary>
    public void NativeShareScreen()
    {
        //_nativeShare = true;
        //StartCoroutine(NativeShareScreenCoroutine());

        string fileUrl = "";

        //fileUrl = Path.Combine(Application.persistentDataPath, "RMScoreTemp.png");
        //ScreenCapture.CaptureScreenshot(fileUrl);


        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        fileUrl = Path.Combine(Application.temporaryCachePath, "RMScoreTemp.png");
        File.WriteAllBytes(fileUrl, ss.EncodeToPNG());

        new NativeShare().AddFile(fileUrl).
            SetTitle("Check my score! (Rogue Monkeys)").
            SetSubject("Rogue Monkeys score").
            SetText("I scored " + GameMgr.Instance.GetCurrentLevel().GetMaxScore() + " on level " + (GameMgr.Instance.LastStagePlayed + 1).ToString() + "-" + (GameMgr.Instance.LastLevelPlayed + 1).ToString() + "Rank[" + GetRankString() + "]! "
            + "Download Rogue Monkeys at https://play.google.com/store/apps/details?id=com.GloomyCaveEntertainment.RogueMonkeys_Free")
            .Share();

    }
    /*
    private IEnumerator NativeShareScreenCoroutine()
    {
        yield return new WaitForEndOfFrame();
        string fileUrl = "";

        //fileUrl = Path.Combine(Application.persistentDataPath, "RMScoreTemp.png");
        //ScreenCapture.CaptureScreenshot(fileUrl);
            

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        fileUrl = Path.Combine(Application.temporaryCachePath, "RMScoreTemp.png");
        File.WriteAllBytes(fileUrl, ss.EncodeToPNG());

        new NativeShare().AddFile(fileUrl).
            SetTitle("Check my score! (Rogue Monkeys)").
            SetSubject("Rogue Monkeys score").
            SetText("-(" + GetRankString() + ")- " + "I scored " + GameMgr.Instance.GetCurrentLevel().GetMaxScore() + " on level " + (GameMgr.Instance.StageIndex + 1).ToString() + "-" + (GameMgr.Instance.LevelIndex + 1).ToString() + "!")
            .Share();
    }*/


    /// <summary>
    /// Rank from enum to string
    /// </summary>
    /// <returns>Rank as a string</returns>
    private string GetRankString()
    {
        string ret = "";

        switch (GameMgr.Instance.GetCurrentLevel().Rank)
        {
            case Level.RANK.A:
                ret = "A";
                break;

            case Level.RANK.B:
                ret = "B";
                break;

            case Level.RANK.C:
                ret = "C";
                break;

            case Level.RANK.D:
                ret = "D";
                break;

            case Level.RANK.E:
                ret = "E";
                break;

            case Level.RANK.F:
                ret = "F";
                break;

            case Level.RANK.S:
                ret = "S";
                break;
        }
        return ret;
    }
}
