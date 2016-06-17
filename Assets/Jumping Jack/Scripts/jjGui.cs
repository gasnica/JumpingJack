using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class jjGui : MonoBehaviour {

    public bool debug = false;
    public Rect rectLives = new Rect(0.12f,0.825f,0.2f,0.1f);
    public Rect rectScore = new Rect(0.7f, 0.825f, 0.2f, 0.1f);
    public GUISkin skin = new GUISkin();

    void OnGUI()
    {
        if (debug || Application.isPlaying)
        {

            Rect rectLivesPixel = new Rect(rectLives.x * Screen.width, rectLives.y * Screen.height, rectLives.width * Screen.width, rectLives.height * Screen.height);
            Rect rectScorePixel = new Rect(rectScore.x * Screen.width, rectScore.y * Screen.height, rectScore.width * Screen.width, rectScore.height * Screen.height);
            // Formatting https://msdn.microsoft.com/en-us/library/0c899ak8(v=vs.110).aspx
            GUI.Label(rectLivesPixel, string.Format("Lives: {0}", jjMain.levelMgr.numLives), skin.label);
            GUI.Label(rectScorePixel, string.Format("HI{0:00000} SC{1:00000}", jjMain.levelMgr.highScore, jjMain.levelMgr.score), skin.label);
        }
    }
}
