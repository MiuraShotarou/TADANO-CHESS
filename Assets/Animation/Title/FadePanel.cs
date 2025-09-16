using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadePanel : MonoBehaviour
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
