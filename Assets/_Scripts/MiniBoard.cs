using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBoard : MonoBehaviour //InGamemanagerを継承しても良いかもしれない
{
    InGameManager _inGameManager;

    private Squere[][] _squereArrays; //s or InstanceClass
    //同じ方式で考える
    //miniBordの強いところは敵AIで利用可能な新しいアーキテクチャを持てること
    //どこになんの駒があるかをどうやって判断するのか → GameObjectにInstanceClassを持たせどこのマスに誰がいるのかを監視する → TurnDesideからの通知で毎回判断することはできる
    //
    //IsOnPieceで確実にBit == 1は判断できる → SquereのプロパティにEventを追加する
    //AttackAreasで攻撃範囲を正確に判断できる →
    ulong _miniBoardW = 0UL; //攻撃範囲内を判断するだけの変数
    ulong _miniBoardB = 0UL;

    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _squereArrays = _inGameManager._SquereArrays; //miniBord上でのPos 攻撃範囲が入っている（？）
    }
    void Bin(MiniSquere bitNum)
    {
        _miniBoardW |= 1UL << (int)bitNum;
    }
    void Ben(MiniSquere bitNum)
    {
        _miniBoardW &= 1Ul << (int)bitNum;
    }
}
