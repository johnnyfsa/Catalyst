using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Catalyst.UI.Presentation.Audio;

public class EndGameButton : MonoBehaviour
{
    [SerializeField]
    private BasicAudioPresenter audioPresenter;

    public void ExitGame()
    {
        audioPresenter?.PlayCardClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
