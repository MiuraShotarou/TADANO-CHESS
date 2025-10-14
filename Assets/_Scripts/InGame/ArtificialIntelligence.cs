using System.Linq; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;
public class ArtificialIntelligence : MonoBehaviour
{
    InGameManager _inGameManager;
    OpenSelectableArea _openSelectableArea;
    AddPieceFunction _addPieceFunction;
    ArtificialIntelligence _artificialIntelligence;
    HashSet<SquereID> _allyCanMoveRange;
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
        _artificialIntelligence = GetComponent<ArtificialIntelligence>();
        _openSelectableArea = GetComponent<OpenSelectableArea>();
    }
    
    public void StartArtificialIntelligence()
    {
        Initialize();
        _enemyAttackRange = CreateEnemyAtackRange();
        _inGameManager.Check(_isCheck ,_checkedKingSquere, _checkAttackerSqueres);
        //Checkの場合はキャスリング出来ないようにだけ書くこと
        FilterCastling();
        _allyCanMoveRange = CreateAllyCanMoveRange();
        
    }
    /// <summary>
    /// キングとルークの間のマスを登録する
    /// </summary>
    void Initialize()
    {
        _isCheck = false;
        _checkedKingSquere = null;
        _checkAttackerSqueres = new List<Squere>();
        _shortDistanceIDs = _inGameManager.IsWhite? new [] { SquereID.g1, SquereID.f1 }: //w_s_c
                                                    new [] { SquereID.g8, SquereID.f8 };//b_s_c
        _longDistanceIDs = _inGameManager.IsWhite?  new [] { SquereID.d1, SquereID.c1}://w_l_c
                                                    new []{ SquereID.d8, SquereID.c8};//b_l_c
    }
    /// <summary>
    /// 自身の駒が移動可能な範囲をすべて取得する // 攻撃範囲は除外する
    /// </summary>
    HashSet<SquereID> CreateAllyCanMoveRange()
    {
        string allyTag = _inGameManager.IsWhite ? "White" : "Black";
        //自陣のコマが置いてあるマスの配列
        Squere[] allyPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(allyTag))).ToArray();
        HashSet<SquereID> allyCanMoveRange = new HashSet<SquereID>();
        //for すべての駒で
        for (int i = 0; i < allyPieceSqueres.Length; i++)
        {
            string search = allyPieceSqueres[i]._IsOnPieceObj.name.First().ToString();
            Piece moverPiece = Instantiate(_inGameManager._PieceDict[search]);
            Vector3Int[] canMoveAreas = Enumerable.Repeat(allyPieceSqueres[i]._SquereTilePos, moverPiece._MoveAreas().Length).ToArray();
            // 駒の能力を調整する
            if ("P".Contains(search))
            {
                if ("1_6".Contains(allyPieceSqueres[i]._SquerePiecePosition.x.ToString()))
                {
                    moverPiece = _addPieceFunction.AddMoveCount(moverPiece);
                }
                if (_inGameManager.IsWhite)
                {
                    //ポーンの攻撃方向を修正
                    moverPiece = _addPieceFunction.UpdatePoneGroup(moverPiece);
                }
            }
            if ("K".Contains(search))
            {
                // ここで判定するのは怪しすぎる
                if (_inGameManager.IsCastling[0]())
                {
                    moverPiece = _addPieceFunction.AddShortCastlingArea(moverPiece);
                }
                if (_inGameManager.IsCastling[1]())
                {
                    moverPiece = _addPieceFunction.AddLongCastlingArea(moverPiece);
                }
            }
            //駒が移動できる範囲を検索する回数。ここから先の処理はPieceが固定されている
            for (int c = 0; c < moverPiece._MoveCount(); c++)
            {
                //for 駒が移動できる全方位のSquereを検索。何かの駒にぶつかったら検索しなくて良い
                for (int d = 0; d < canMoveAreas.Length; d++)
                {
                    if (canMoveAreas[d].z == -1)
                    {
                        continue;
                    }
                    canMoveAreas[d] += moverPiece._MoveAreas()[d]; //d == 方角
                    int alphabet = canMoveAreas[d].y;
                    int number = canMoveAreas[d].x;
                    //盤外である　は二度と検索しなくて良い条件 
                    if (!(-1 < alphabet && 8 > alphabet && -1 < number && 8 > number)) //盤外のマスであるならば
                    {
                        canMoveAreas[d].z = -1;
                        continue;
                    }
                    // 駒がある の場合も二度と検索しなくて良い条件
                    if (_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj)
                    {
                        canMoveAreas[d].z = -1;
                        if ("K".Contains(search))
                        {
                            if (_inGameManager.IsCastling.Any())
                            {
                                allyCanMoveRange.Add(_inGameManager._SquereArrays[alphabet][number]._SquereID);
                            }
                        }
                    }
                    else
                    {
                        allyCanMoveRange.Add(_inGameManager._SquereArrays[alphabet][number]._SquereID);
                    }
                }
            }
        }
        return allyCanMoveRange;
    }
    /// <summary>
    /// 攻撃可能なエリアをすべて取得する
    /// </summary>
    HashSet<SquereID> CreateEnemyAtackRange()
    {
        string allyTag = _inGameManager.IsWhite ? "White" : "Black";
        string enemyTag = _inGameManager.IsWhite ? "Black" : "White";
        Squere[] enemyPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(enemyTag))).ToArray();
        HashSet<SquereID> enemyAttackRange = new HashSet<SquereID>();
        //for すべての駒で
        for (int i = 0; i < enemyPieceSqueres.Length; i++)
        {
            string search = enemyPieceSqueres[i]._IsOnPieceObj.name.First().ToString();
            Piece attackerPiece = Instantiate(_inGameManager._PieceDict[search]);
            Vector3Int[] attackAreas = Enumerable.Repeat(enemyPieceSqueres[i]._SquereTilePos, attackerPiece._AttackAreas().Length).ToArray();
            if ("P".Contains(search) && !_inGameManager.IsWhite)
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
                        if (difendSquere._IsOnPieceObj.CompareTag(allyTag))
                        {
                            enemyAttackRange.Add(difendSquere._SquereID);
                            //しかもチェックだった
                            if ("K".Contains(difendSquere._IsOnPieceObj.name.First().ToString()))
                            {
                                _isCheck = true;
                                //用途不明
                                _checkedKingSquere = difendSquere;
                                _checkAttackerSqueres.Add(enemyPieceSqueres[i]);
                            }
                        }
                    }
                    else
                    {
                        enemyAttackRange.Add(difendSquere._SquereID);
                    }
                }
            }
        }
        return enemyAttackRange;
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
        bool isLongCastling = MinimalFilter(_longDistanceIDs);
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
    /// </summary>
    /// <returns></returns>
    bool MinimalFilter(SquereID[] distanceIDs)
    {
        bool isCanCastling = distanceIDs.Select(id => (int)id).ToArray().All(id => _inGameManager._SquereArrays[id / 8][id % 8]._IsOnPieceObj == null);
        return isCanCastling;
    }

    public void MoveComputer()
    {
        HashSet<SquereID> moveRange = _allyCanMoveRange.Union(_enemyAttackRange).ToHashSet();
        Squere[] allyPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(_inGameManager.IsWhite ? "White" : "Black"))).ToArray();
        //AIが移動先のSquare を選択し移動する処理
        GameObject selectedPieceObj = allyPieceSqueres[Random.Range(0, allyPieceSqueres.Length)]._IsOnPieceObj;
        PointerEventData pointerData = new PointerEventData(EventSystem.current){position = Mouse.current.position.ReadValue()};
        selectedPieceObj.GetComponent<EventTrigger>()?.OnPointerClick(pointerData);
        Debug.Log(selectedPieceObj.name);
        // PointerEventData ポインター入力に関する情報を保持しているクラス
        // EventTrigger 
        DOVirtual.DelayedCall(3, DecideComputer);
    }
    void DecideComputer()
    {
        SpriteRenderer[] pieceCanMoveRange = _inGameManager._DeceptionTileFieldArrays.SelectMany(array => array.Where(field => field.gameObject.GetComponent<BoxCollider2D>().enabled)).ToArray();
        var hoge = pieceCanMoveRange[Random.Range(0, pieceCanMoveRange.Length)];
        Debug.Log(hoge.gameObject.name);
        _openSelectableArea.TurnDesideRelay(hoge);
        // _openSelectableArea.TurnDesideRelay(pieceCanMoveRange[Random.Range(0, pieceCanMoveRange.Length)]);
    }
}
