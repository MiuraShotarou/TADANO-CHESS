using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MoveScene : AnimationRelay
{
    void InGame()
    {
        SceneManager.LoadScene("InGameScene");
    }

    void ActiveOff()
    {
        gameObject.SetActive(false);
    }
}
