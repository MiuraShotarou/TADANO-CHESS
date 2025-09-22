using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class TurnBegin : MonoBehaviour
{
    InGameManager _inGameManager;
    AddPieceFunction _addPieceFunction;
    Squere[][] _squereArrays;
    HashSet<SquereID> _enemyAttackRange;
    SquereID[] _shortDistanceIDs;
    SquereID[] _longDistanceIDs;
    Squere _checkedKingSquere;
    List<Squere> _checkAttackerSqueres;
    bool _isCheck;
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _addPieceFunction = GetComponent<AddPieceFunction>();
        _squereArrays = _inGameManager._SquereArrays;
    }
    /// <summary>
    /// ターンが切り替わった時、InGameManagerから一度だけ呼び出される
    /// </summary>
    public void StartTurn()
    {
        //判定基準 → K が動いているかいないか R が動いているかいないか 間に駒があるかないか 間が敵の攻撃範囲に該当するか（ここをなるべく調べたくない）
        Initialize();
        AddTurnCount();
        CreateEnemyAtackRange(); //戻り値にして、ローカルに保存することを検討
        _inGameManager.Check(_isCheck ,_checkedKingSquere, _checkAttackerSqueres);
        //Checkの場合はキャスリング出来ないようにだけ書くこと
        FilterCastling();
    }
    /// <summary>
    /// キングとルークの間のマスを登録する
    /// </summary>
    void Initialize()
    {
        _isCheck = false;
        _checkedKingSquere = null;
        _checkAttackerSqueres = new List<Squere>();
        _shortDistanceIDs = _inGameManager.IsWhite? new [] { SquereID.b1, SquereID.c1 }: //w_s_c
                                                    new [] { SquereID.b8, SquereID.c8 };//b_s_c
        _longDistanceIDs = _inGameManager.IsWhite?  new [] { SquereID.e1, SquereID.f1}://w_l_c 
                                                    new []{ SquereID.e8, SquereID.f8};//b_l_c
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    void AddTurnCount()
    {
        _inGameManager._TurnCount++;
    }
    void CreateEnemyAtackRange()
    {
        string allyTag = _inGameManager.IsWhite ? "White" : "Black";
        string enemyTag = _inGameManager.IsWhite ? "Black" : "White";
        Squere[] enemyPieceSqueres = _squereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(enemyTag))).ToArray();
        _enemyAttackRange = new HashSet<SquereID>();
        //for すべての駒で
        for (int i = 0; i < enemyPieceSqueres.Length; i++)
        {
            string search = enemyPieceSqueres[i]._IsOnPieceObj.name.First().ToString();
            Piece attackerPiece = Instantiate(_inGameManager._PieceDict[search]);
            Vector3Int[] attackAreas = Enumerable.Repeat(enemyPieceSqueres[i]._SquereTilePos, attackerPiece._AttackAreas().Length).ToArray();
            if (search == "P" && !_inGameManager.IsWhite)
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
                        if (difendSquere._IsOnPieceObj && difendSquere._IsOnPieceObj.CompareTag(allyTag))
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
        bool isShortCastling = MinimalFilter(_shortDistanceIDs);
        bool isLongCastling = MinimalFilter(_shortDistanceIDs);
        //条件③ 両キャスリング時の移動経路が敵の攻撃範囲外ならキャスリングの使用を可能にする
        if (isShortCastling)
        {
            isShortCastling = _shortDistanceIDs.All(condition => !_enemyAttackRange.Contains(condition));
        }
        else if (isLongCastling)
        {
            isLongCastling = _longDistanceIDs.All(condition => !_enemyAttackRange.Contains(condition));
        }
        if (_inGameManager.IsWhite)
        {
            _inGameManager._isWhiteShortCastlingSwitch = isShortCastling;
            _inGameManager._isWhiteLongCastlingSwitch = isLongCastling;
        }
        else
        {
            _inGameManager._isBlackShortCastlingSwitch = isShortCastling;
            _inGameManager._isBlackLongCastlingSwitch = isLongCastling;
        }
    }
    /// <summary>
    /// 条件② 両キャスリング時の移動経路になにかしらの駒がある場合、攻撃範囲の検索を中断する
    /// 補足：b1, c1, / f1, g1, h1 / b8, c8, / f8, g8, h8 のどこかに駒がある場合は該当のキャスリングができない
    /// </summary>
    /// <returns></returns>
    bool MinimalFilter(SquereID[] distanceIDs)
    {
        bool isCastlingPossible;
        isCastlingPossible = distanceIDs.Select(id => (int)id).ToArray().All(id => _squereArrays[id / 8][id % 8]._IsOnPieceObj == null);
        return isCastlingPossible;
    }
}
