using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimationController : MonoBehaviour {

    GameManager gm;
    MenuController menu;
    EnterNameController nameControl;
    HighScoreController highScores;

    private void Start()
    {
        gm = GameManager.instance;
        menu = MenuController.instance;
        nameControl = EnterNameController.instance;
        highScores = HighScoreController.instance;
    }

    public void ReadyToStartGame()
    {
        gm.readyToPlay = true;
        menu.ClearMenuElements();
    }

    public void GameToMenu()
    {
        gm.ResetGame();
        menu.leftMenu = false;
    }

    public void LeavingGameArea()
    {
        gm.readyToPlay = false;
    }

    public void ArrivedAtEnterName()
    {
        nameControl.enteringName = true;
        menu.ClearMenuElements();
    }

    public void LeavingEnterName()
    {
        nameControl.enteringName = false;
    }

    public void ResetEnterNameArea()
    {
        nameControl.ResetEnterNameArea();
        menu.leftMenu = false;
    }

    public void LeavingHighScores()
    {
        menu.ClearMenuElements();
        highScores.viewingHighScores = false;
    }

    public void HighToMenu()
    {
        menu.leftMenu = false;
        highScores.ResetHighScoreArea();
    }

    public void ArrivedAtHigh()
    {
        highScores.viewingHighScores = true;
    }
}
