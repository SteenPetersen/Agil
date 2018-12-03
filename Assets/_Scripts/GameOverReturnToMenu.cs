using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverReturnToMenu : MonoBehaviour {

    public void ReturnToMenu()
    {
        Camera.main.GetComponent<Animator>().SetTrigger("gametomenu");
    }
}
