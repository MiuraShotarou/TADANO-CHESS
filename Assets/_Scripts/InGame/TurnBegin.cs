using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurnBegin : MonoBehaviour
{
    InGameManager _inGameManager;
    AddPieceFunction _addPieceFunction;
    Squere[][] _squereArrays;
    SquereID[] _shortCastlingConditionIds;
    SquereID[] _longcastlingConditionIds;
    bool _isShortCastlingPossible;
    bool _isLongCastlingPossible;
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
        FilterCheck();
        FilterCheckMate();
        //Checkの場合はキャスリング出来ないようにだけ書くこと
        FilterCastling();
        _inGameManager._AnimatorController.Play("TurnBegin");
    }
    /// <summary>
    /// 移動経路の間にあるマスだけを入れている
    /// </summary>
    void Initialize()
    {
        _shortCastlingConditionIds = _inGameManager._IsWhite? new[] { SquereID.b1, SquereID.c1 }: //w_s_c
                                                    new[] { SquereID.b8, SquereID.c8 };//b_s_c
        _longcastlingConditionIds = _inGameManager._IsWhite? new [] { SquereID.f1 , SquereID.g1, SquereID.h1}://w_l_c 
                                                    new []{SquereID.f8, SquereID.g8, SquereID.h8};//B_l_c
    }

    void FilterCheck()
    {
        
    }

    void FilterCheckMate()
    {
        
    }
    void FilterCastling()
    {
        //_inGameManager.IsWhiteCastling == メソッド名 ですぐにreturnが可能
        //攻撃範囲まで見る必要があるのかどうかを判断する → 自陣のキャスリングだけ判断できれば良い
        
        //条件① R / K が動いていた場合 → 省略：実際はTurnDeside.csで判断している
        //条件② 両キャスリング時の移動経路になにかしらの駒がある場合、攻撃範囲の検索を中断する
        MinimalFilter();
        if (!_isLongCastlingPossible && !_isShortCastlingPossible) //isCheck
        {
            return;
        }
        //条件③ 両キャスリング時の移動経路が敵の攻撃範囲内なら
        HashSet<SquereID> resultAttackRangeIds = FinalFilter();
        _isShortCastlingPossible = _shortCastlingConditionIds.All(condition => !resultAttackRangeIds.Contains(condition));
        _isLongCastlingPossible = _longcastlingConditionIds.All(condition => !resultAttackRangeIds.Contains(condition));
        
        _inGameManager._isWhiteShortCastlingSwitch = _isShortCastlingPossible;
        _inGameManager._isWhiteLongCastlingSwitch = _isLongCastlingPossible;
    }
    /// <summary>
    /// 条件② 両キャスリング時の移動経路になにかしらの駒がある場合、攻撃範囲の検索を中断する
    /// 補足：b1, c1, / f1, g1, h1 / b8, c8, / f8, g8, h8 のどこかに駒がある場合は該当のキャスリングができない
    /// </summary>
    /// <returns></returns>
    void MinimalFilter()
    {
        _isShortCastlingPossible = _shortCastlingConditionIds.Select(id => (int)id).ToArray().All(id => _squereArrays[id / 8][id % 8]._IsOnPieceObj == null);
        _isLongCastlingPossible = _longcastlingConditionIds.Select(id => (int)id).ToArray().All(id => _squereArrays[id / 8][id % 8]._IsOnPieceObj == null);
    }

    HashSet<SquereID> FinalFilter()
    {
        string allyGroup = _inGameManager._IsWhite ? "W" : "B";
        string enemyGroup = _inGameManager._IsWhite ? "B" : "W";
        Squere[] enemyPieceSqueres = _squereArrays.SelectMany(flatSqueres =>
                flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.name.Contains(enemyGroup)))
            .ToArray();
        HashSet<SquereID> canAttackAreaID = new HashSet<SquereID>();
        //for すべての駒で 
        for (int i = 0; i < enemyPieceSqueres.Length; i++)
        {
            string[] search = enemyPieceSqueres[i]._IsOnPieceObj.name.Split('_');
            Piece attackPiece = Instantiate(_inGameManager._PieceDict[search[0]]);
            Vector3Int[] attackAreas = Enumerable
                .Repeat(enemyPieceSqueres[i]._SquereTilePos, attackPiece._AttackAreas().Length).ToArray();
            if (search[0] == "P" && !_inGameManager._IsWhite)
            {
                //ポーンの攻撃方向を修正している
                attackPiece = _addPieceFunction.UpdatePoneGroup(attackPiece);
            }

            //駒が攻撃できる範囲を検索する回数。ここから先の処理はPieceが固定されている
            for (int c = 0; c < attackPiece._MoveCount(); c++)
            {
                //for 駒が攻撃できる全方位のSquereを検索。何かの駒にぶつかったら検索しなくて良い
                for (int d = 0; d < attackAreas.Length; d++)
                {
                    if (attackAreas[d].z == -1)
                    {
                        continue;
                    }

                    attackAreas[d] += attackPiece._AttackAreas()[d]; //d == 方角
                    int alphabet = attackAreas[d].y;
                    int number = attackAreas[d].x;
                    Squere attackSquere = _inGameManager._SquereArrays[alphabet][number];
                    //駒がある || 盤外である　は二度と検索しなくて良い条件
                    if (attackSquere._IsOnPieceObj
                        ||
                        !(-1 < alphabet && 8 > alphabet && -1 < number && 8 > number)) //盤外のマスであるならば
                    {
                        attackAreas[d].z = -1;
                        if (attackSquere._IsOnPieceObj && attackSquere._IsOnPieceObj.name.Contains(enemyGroup))
                        {
                            //敵の駒が見つかった
                            canAttackAreaID.Add(attackSquere._SquereID);
                        }
                    }
                    else
                    {
                        canAttackAreaID.Add(attackSquere._SquereID);
                    }
                }
            }
        }
        return canAttackAreaID;
    }

    /// <summary>
    /// すべての敵の駒からAttackAreaを広げていったとき、b1, c1, / f1, g1, h1 / b8, c8, / f8, g8, h8 / d1 / d8 に一度でもヒットしたら該当のキャスリングはできない
    /// </summary>
    void FilterShortCastling()
    {

        
            // SqureID[] 
            //最後に欲しいもの → SqureID
    }

    void FilterLongCastling()
    {
        
    }
}
