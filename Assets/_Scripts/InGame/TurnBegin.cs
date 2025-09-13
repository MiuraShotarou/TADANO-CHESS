using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurnBegin : MonoBehaviour
{
    InGameManager _inGameManager;
    Squere[][] _squereArrays;
    SquereID[] _shortSquereIds;
    SquereID[] _longSquereIds;
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _squereArrays = _inGameManager._SquereArrays;
    }

    public void StartTurn()
    {
        //ショートキャスリングを判定
        //ロングキャスリングを判定 →
        //判定基準 → K が動いているかいないか R が動いているかいないか 間に駒があるかないか 間が敵の攻撃範囲に該当するか（ここをなるべく調べたくない）
        // for (int i = 0; i )
        Initialize();
        FilterCastling();
        _inGameManager._AnimatorController.Play("TurnBegin");
    }

    void Initialize()
    {
        _shortSquereIds = _inGameManager._IsWhite? new[] { SquereID.b1, SquereID.c1 }: //w_s_c
                                                    new[] { SquereID.b8, SquereID.c8 };//b_s_c
        _longSquereIds = _inGameManager._IsWhite? new [] { SquereID.f1 , SquereID.g1, SquereID.h1}://w_l_c 
                                                    new []{SquereID.f8, SquereID.g8, SquereID.h8};//B_l_c
    }
    void FilterCastling()
    {
        //_inGameManager.IsWhiteCastling == メソッド名 ですぐにreturnが可能
        //攻撃範囲まで見る必要があるのかどうかを判断する → 自陣のキャスリングだけ判断できれば良い
        
        //条件① R / K が動いていた場合（省略：内実はTurnDeside.csで判断している）
        //条件② 移動経路になにかしらの駒がある場合 → b1, c1, / f1, g1, h1 / b8, c8, / f8, g8, h8 のどこかに駒がある場合は該当のキャスリングができない
        bool isShortPossible = _shortSquereIds.Select(id => (int)id).ToArray().All(id => _squereArrays[id / 8][id % 8]._IsOnPieceObj == null);
        bool isLongPossible = _longSquereIds.Select(id => (int)id).ToArray().All(id => _squereArrays[id / 8][id % 8]._IsOnPieceObj == null);
        //条件③ 移動経路 ・ R / K がチェックされていないならキャスリングが可能な状態である
        if (isShortPossible)
        {
            FilterShortCastling();
        }
        if (isLongPossible)
        {
            FilterLongCastling();
        }
    }

    void FilterShortCastling()
    {
        
    }

    void FilterLongCastling()
    {
        
    }
}
