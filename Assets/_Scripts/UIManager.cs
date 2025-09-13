using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    protected InGameManager _inGameManager;

    void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
    }
    // protected string ActivePromotionPanel()
    // {
    //     
    // }
}
