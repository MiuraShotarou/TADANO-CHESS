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
    bool[] _isCastling;
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
        _inGameManager.
        IsCastling = _inGameManager._IsWhite? new []{_inGameManager.IsWhiteShortCastling, _inGameManager.IsWhiteLongCastling}:
                                                new []{_inGameManager.IsBlackShortCastling, _inGameManager.IsBlackLongCastling};
        _isCastling = _inGameManager._IsWhite? new []{_inGameManager._isWhiteShortCastling, _inGameManager._isWhiteLongCastling }:
                                                new []{_inGameManager._isBlackShortCastling, _inGameManager._isBlackLongCastling };
        _shortSquereIds = _inGameManager._IsWhite? new[] { SquereID.b1, SquereID.c1 }: //w_s_c
                                                    new[] { SquereID.b8, SquereID.c8 };//b_s_c
        _longSquereIds = _inGameManager._IsWhite? new [] { SquereID.f1 , SquereID.g1, SquereID.h1}://w_l_c 
                                                    new []{SquereID.f8, SquereID.g8, SquereID.h8};//B_l_c
    }
    void FilterCastling()
    {
        //_inGameManager.IsWhiteCastling == メソッド名 ですぐにreturnが可能
        //攻撃範囲まで見る必要があるのかどうかを判断する → 自陣のキャスリングだけ判断できれば良い
        //条件の駒が動いていた場合
        // bool isShortPossible = _inGameManager.IsCastling[0]() == _isCastling[0];
        // bool isLongPossible = _inGameManager.IsCastling[1]() == _isCastling[1];
        //間に駒がある場合 → b1, c1, / f1, g1, h1 / b8, c8, / f8, g8, h8 のどこかに駒がある場合、そこのキャスリングは不可
        bool isShortPossible = _shortSquereIds.Select(id => (int)id).ToArray().All(id => _squereArrays[id / 8][id % 8]._IsOnPieceObj == null);
        bool isLongPossible = _inGameManager.IsCastling[1]() == _isCastling[1];
        // Array.ForEach(Enumerable.Range(0, 2).ToArray(), i =>
        // {
        //     if (_inGameManager._IsWhite)
        //     {
        //         Func<bool>[] checks = new[]
        //             { _inGameManager.IsWhiteShortCastling, _inGameManager.IsWhiteLongCastling };
        //         if (_inGameManager.IsCastling[i] != checks[i])
        //         {
        //
        //         }
        //     }
        //     else
        //     {
        //         Func<bool>[] checks = new[]
        //             { _inGameManager.IsBlackShortCastling, _inGameManager.IsBlackLongCastling };
        //         if (_inGameManager.IsCastling[i] != checks[i])
        //         {
        //             _inGameManager.IsCastling[i] = () => false;
        //         }
        //     }
        // });
    }
}
