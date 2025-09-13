using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBegin : MonoBehaviour
{
    InGameManager _inGameManager;
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
    }

    public void StartTurn()
    {
        for (int i)
        _inGameManager._AnimatorController.Play("TurnBegin");
    }
}
