using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBegin : MonoBehaviour
{
    InGameManager _inGameManager;
    public Action JudgmentCastling;
    bool _is
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
    }

    public void StartTurn()
    {
        //ショートキャスリングを判定
        //ロングキャスリングを判定 →
        //判定基準 → K が動いているかいないか R が動いているかいないか 間に駒があるかないか 間が敵の攻撃範囲に該当するか（ここをなるべく調べたくない）
        // for (int i = 0; i )
        if ()
        JudgmentCastling = GameObject.Find("JudgmentChasting").GetComponent<Action>();
        _inGameManager._AnimatorController.Play("TurnBegin");
    }
}
