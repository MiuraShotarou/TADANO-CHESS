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
    HashSet<SquereID> _enemyAttackRange;
    SquereID[] _shortDistanceIDs;
    SquereID[] _longDistanceID;
    Squere _checkedKingSquere;
    List<Squere> _checkAttackerSqueres;
    bool _isCheck;
    bool _isShortCastlingPossible;
    bool _isLongCastlingPossible;
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _addPieceFunction = GetComponent<AddPieceFunction>();
        _squereArrays = _inGameManager._SquereArrays;
    }

    public void StartTurn()
    {
        //判定基準 → K が動いているかいないか R が動いているかいないか 間に駒があるかないか 間が敵の攻撃範囲に該当するか（ここをなるべく調べたくない）
        Initialize();
        CreateEnemyAtackRange(); //戻り値にして、ローカルに保存することを検討
        _inGameManager.Check(_isCheck ,_checkedKingSquere, _checkAttackerSqueres);
        //Checkの場合はキャスリング出来ないようにだけ書くこと
        FilterCastling();
        _inGameManager._AnimatorController.Play("TurnStart");
    }
    /// <summary>
    /// キングとルークの間のマスを登録する
    /// </summary>
    void Initialize()
    {
        _isCheck = false;
        _checkedKingSquere = null;
        _checkAttackerSqueres = new List<Squere>();
        _shortDistanceIDs = _inGameManager._IsWhite? new[] { SquereID.b1, SquereID.c1 }: //w_s_c
                                                    new[] { SquereID.b8, SquereID.c8 };//b_s_c
        _longDistanceID = _inGameManager._IsWhite? new [] { SquereID.f1 , SquereID.g1, SquereID.h1}://w_l_c 
                                                    new []{SquereID.f8, SquereID.g8, SquereID.h8};//B_l_c
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    HashSet<SquereID> CreateEnemyAtackRange()
    {
        string allyGroup = _inGameManager._IsWhite ? "W" : "B";
        string enemyGroup = _inGameManager._IsWhite ? "B" : "W";
        Squere[] enemyPieceSqueres = _squereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.name.Contains(enemyGroup))).ToArray();
        _enemyAttackRange = new HashSet<SquereID>();
        //for すべての駒で 
        for (int i = 0; i < enemyPieceSqueres.Length; i++)
        {
            string[] search = enemyPieceSqueres[i]._IsOnPieceObj.name.Split('_');
            Piece attackerPiece = Instantiate(_inGameManager._PieceDict[search[0]]);
            Vector3Int[] attackAreas = Enumerable.Repeat(enemyPieceSqueres[i]._SquereTilePos, attackerPiece._AttackAreas().Length).ToArray();
            if (search[0] == "P" && !_inGameManager._IsWhite)
            {
                //ポーンの攻撃方向を修正している
                attackerPiece = _addPieceFunction.UpdatePoneGroup(attackerPiece);
            }
            //駒が攻撃できる範囲を検索する回数。ここから先の処理はPieceが固定されている
            for (int c = 0; c < attackerPiece._MoveCount(); c++)
            {
                //for 駒が攻撃できる全方位のSquereを検索。何かの駒にぶつかったら検索しなくて良い
                for (int d = 0; d < attackAreas.Length; d++)
                {
                    if (attackAreas[d].z == -1)
                    {
                        continue;
                    }
                    attackAreas[d] += attackerPiece._AttackAreas()[d]; //d == 方角
                    int alphabet = attackAreas[d].y;
                    int number = attackAreas[d].x;
                    //盤外である　は二度と検索しなくて良い条件 
                    if (!(-1 < alphabet && 8 > alphabet && -1 < number && 8 > number)) //盤外のマスであるならば
                    {
                        attackAreas[d].z = -1;
                        continue;
                    }
                    Squere difendSquere = _inGameManager._SquereArrays[alphabet][number];
                    // 駒がある の場合も二度と検索しなくて良い条件 + アンパッサンは通常のポーンの攻撃範囲と変わらないので条件にかける必要がない
                    if (difendSquere._IsOnPieceObj)
                    {
                        attackAreas[d].z = -1;
                        //敵から見て敵の駒(ally)が見つかった場合の条件
                        if (difendSquere._IsOnPieceObj && difendSquere._IsOnPieceObj.name.Contains(allyGroup))
                        {
                            _enemyAttackRange.Add(difendSquere._SquereID);
                            if (difendSquere._IsOnPieceObj.name.First().ToString().Contains("K"))
                            {
                                //しかもチェックだった
                                _isCheck = true;
                                _checkedKingSquere = difendSquere;
                                _checkAttackerSqueres.Add(enemyPieceSqueres[i]);
                            }
                        }
                    }
                    else
                    {
                        _enemyAttackRange.Add(difendSquere._SquereID);
                    }
                }
            }
        }
        return _enemyAttackRange;
    }
    /// <summary>
    /// キャスリングができるか、出来ないかを判断するメソッド
    /// </summary>
    void FilterCastling()
    {
        //条件⓪ R / K が動いていた場合 → 省略：実際はTurnDeside.csで判断している
        //条件① チェック（チェックメイト）だった場合
        if (_isCheck){return;}
        //条件② 両キャスリング時の移動経路になにかしらの駒がある場合、攻撃範囲の検索を中断する
        if (!MinimalFilter()) //isCheck
        {
            return;
        }
        //条件③ 両キャスリング時の移動経路が敵の攻撃範囲外ならキャスリングの使用を可能にする
        _inGameManager.IsCastlingSwitch[0] = _shortDistanceIDs.All(condition => !_enemyAttackRange.Contains(condition));
        _inGameManager.IsCastlingSwitch[1] = _longDistanceID.All(condition => !_enemyAttackRange.Contains(condition));
    }
    /// <summary>
    /// 条件② 両キャスリング時の移動経路になにかしらの駒がある場合、攻撃範囲の検索を中断する
    /// 補足：b1, c1, / f1, g1, h1 / b8, c8, / f8, g8, h8 のどこかに駒がある場合は該当のキャスリングができない
    /// </summary>
    /// <returns></returns>
    bool MinimalFilter()
    {
        _isShortCastlingPossible = _shortDistanceIDs.Select(id => (int)id).ToArray().All(id => _squereArrays[id / 8][id % 8]._IsOnPieceObj == null);
        _isLongCastlingPossible = _longDistanceID.Select(id => (int)id).ToArray().All(id => _squereArrays[id / 8][id % 8]._IsOnPieceObj == null);
        if (!_isShortCastlingPossible && !_isLongCastlingPossible)
        {
            return false;
        }
        else
        {
            return true;
        }
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
