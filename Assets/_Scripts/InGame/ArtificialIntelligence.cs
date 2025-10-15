using System;
using System.Linq; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;
using Random = UnityEngine.Random;

public class ArtificialIntelligence : MonoBehaviour
{
    InGameManager _inGameManager;
    OpenSelectableArea _openSelectableArea;
    AddPieceFunction _addPieceFunction;
    ArtificialIntelligence _artificialIntelligence;
    // HashSet<SquereID> _allyCanMoveRange;
    // HashSet<SquereID> _allyCanAttackRange;
    // HashSet<SquereID> _enemyAttackRange;
    HashSet<GameObject> _canMovePieceObjectHash;
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
        HashSet<SquereID> enemyAttackRange = CreateAttackRange(false);
        _inGameManager.Check(_isCheck ,_checkedKingSquere, _checkAttackerSqueres);
        //Checkの場合はキャスリング出来ないようにだけ書くこと
        FilterCastling(enemyAttackRange);
    }
    /// <summary>
    /// キングとルークの間のマスを登録する
    /// </summary>
    void Initialize()
    {
        _isCheck = false;
        _checkedKingSquere = null;
        _checkAttackerSqueres = new List<Squere>();
        _canMovePieceObjectHash = new HashSet<GameObject>();
        _shortDistanceIDs = _inGameManager.IsWhite? new [] { SquereID.g1, SquereID.f1 }: //w_s_c
                                                    new [] { SquereID.g8, SquereID.f8 };//b_s_c
        _longDistanceIDs = _inGameManager.IsWhite?  new [] { SquereID.d1, SquereID.c1}://w_l_c
                                                    new []{ SquereID.d8, SquereID.c8};//b_l_c
    }
    /// <summary>
    /// 自身の駒が移動可能な範囲をすべて取得する // 攻撃範囲は除外する
    /// </summary>
    HashSet<SquereID> CreateCanMoveRange(bool isModeAlly)
    {
        isModeAlly = _inGameManager.IsWhite? isModeAlly : !isModeAlly;
        string groupTag = isModeAlly? "White" : "Black";
        //自陣のコマが置いてあるマスの配列
        Squere[] allyPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(groupTag))).ToArray();
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
                // ここで判定するのは怪しすぎる → コードを見る限りだと怪しそうだ
                if (_inGameManager.IsCastling[0]())
                {
                    moverPiece = _addPieceFunction.AddShortCastlingArea(moverPiece);
                }
                if (_inGameManager.IsCastling[1]())
                {
                    moverPiece = _addPieceFunction.AddLongCastlingArea(moverPiece);
                }
            }
            // Debug.Log($"T0 {allyPieceSqueres[i]._IsOnPieceObj.name}"); //現在行動しようとしている味方の駒の名前
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
                                Debug.Log($"0 {_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj}");
                                _canMovePieceObjectHash.Add(_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj); //行動することが出来る駒を登録する。
                            }
                        }
                    }
                    else
                    {
                        allyCanMoveRange.Add(_inGameManager._SquereArrays[alphabet][number]._SquereID);
                        Debug.Log($"1 {allyPieceSqueres[i]._IsOnPieceObj.name}"); //現在行動しようとしている駒の名前
                        _canMovePieceObjectHash.Add(_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj);
                    }
                }
            }
        }
        return allyCanMoveRange;
    }
    /// <summary>
    /// 敵が攻撃可能なエリアをすべて取得する
    /// </summary>
    HashSet<SquereID> CreateAttackRange(bool isModeAlly)
    {
        isModeAlly = _inGameManager.IsWhite? isModeAlly : !isModeAlly;
        string groupTag = isModeAlly? "White" : "Black";
        string antiGroupTag = isModeAlly? "Black" : "White";
        Squere[] enemyPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(antiGroupTag))).ToArray();
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
                        if (difendSquere._IsOnPieceObj.CompareTag(groupTag))
                        {
                            enemyAttackRange.Add(difendSquere._SquereID);
                            //しかもチェックだった
                            if ("K".Contains(difendSquere._IsOnPieceObj.name.First().ToString()))
                            {
                                _isCheck = true;
                            }
                        }
                    }
                }
            }
        }
        return enemyAttackRange;
    }
    /// <summary>
    /// キャスリングができるか、出来ないかを判断するメソッド
    /// </summary>
    void FilterCastling(HashSet<SquereID> enemyAttackRange)
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
            isShortCastling = _shortDistanceIDs.All(condition => !enemyAttackRange.Contains(condition));
        }
        else if (isLongCastling)
        {
            isLongCastling = _longDistanceIDs.All(condition => !enemyAttackRange.Contains(condition));
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
        HashSet<SquereID> allyCanMoveRange = CreateCanMoveRange(true); //味方が動ける範囲を取得
        HashSet<SquereID> allyCanAttackRange = CreateAttackRange(true);//味方が攻撃出来る範囲を取得
        //動ける範囲のみ
        HashSet<SquereID> concatMoveRange = allyCanMoveRange.Union(allyCanAttackRange).ToHashSet(); //味方が行動可能な範囲を動ける範囲と攻撃できる範囲を併せてひとつの変数にまとめる
        HashSet<SquereID> onPieceRange = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres
            .Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(_inGameManager.IsWhite ? "White" : "Black")))
            .Select(squere => squere._SquereID).ToHashSet();//味方の駒が乗っているSquareのコレクションを作成する
        //動かすことが出来る駒を取得する
        GameObject[] canMovePieceObjArray = CreateCanMovePieceObject(true);

        //AIが移動先のSquare を選択し移動する処理
        // allyPieceSqueresから動けるコマを選別する。
        // GameObject[] canMovePieceObjArray = _canMovePieceObjectHash.ToArray();
        _canMovePieceObjectHash.Clear();
        GameObject selectedPieceObj = canMovePieceObjArray[Random.Range(0, canMovePieceObjArray.Length)];//テスト用ランダム選出
        // GameObject selectedPieceObj = allyPieceSqueres[Random.Range(0, allyPieceSqueres.Length)]._IsOnPieceObj; //テスト用ランダム選出
        PointerEventData pointerData = new PointerEventData(EventSystem.current){position = Mouse.current.position.ReadValue()};
        Debug.Log(canMovePieceObjArray.Length);
        Array.ForEach(canMovePieceObjArray, obj => Debug.Log(obj.name));
        selectedPieceObj.GetComponent<EventTrigger>()?.OnPointerClick(pointerData);
        // PointerEventData ポインター入力に関する情報を保持しているクラス
        // EventTrigger
        DOVirtual.DelayedCall(3, DecideComputer);
    }
    void DecideComputer()
    {
        SpriteRenderer[] pieceCanMoveRange = _inGameManager._DeceptionTileFieldArrays.SelectMany(array => array.Where(field => field.gameObject.GetComponent<BoxCollider2D>().enabled)).ToArray();
        var hoge = pieceCanMoveRange[Random.Range(0, pieceCanMoveRange.Length)];
        _openSelectableArea.TurnDesideRelay(hoge);
        // _openSelectableArea.TurnDesideRelay(pieceCanMoveRange[Random.Range(0, pieceCanMoveRange.Length)]);
    }
    /// <summary>
    /// 自身の駒が移動可能な範囲をすべて取得する // 攻撃範囲は除外する
    /// </summary>
    GameObject[] CreateCanMovePieceObject(bool isModeAlly)
    {
        isModeAlly = _inGameManager.IsWhite? isModeAlly : !isModeAlly;
        string groupTag = isModeAlly? "White" : "Black";
        //自陣のコマが置いてあるマスの配列
        Squere[] allyPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(groupTag))).ToArray();
        List<GameObject> canMovePieceObjArray = new List<GameObject>();
        //for すべての駒で
        for (int i = 0; i < allyPieceSqueres.Length; i++)
        {
            string search = allyPieceSqueres[i]._IsOnPieceObj.name.First().ToString();
            Piece applicablePiece = Instantiate(_inGameManager._PieceDict[search]);
            Vector3Int[] canMoveAreas = Enumerable.Repeat(allyPieceSqueres[i]._SquereTilePos, applicablePiece._MoveAreas().Length).ToArray();
            Vector3Int[] canAttackAreas = Enumerable.Repeat(allyPieceSqueres[i]._SquereTilePos, applicablePiece._AttackAreas().Length).ToArray();
            // 駒の能力を調整する
            if ("P".Contains(search))
            {
                if ("1_6".Contains(allyPieceSqueres[i]._SquerePiecePosition.x.ToString()))
                {
                    applicablePiece = _addPieceFunction.AddMoveCount(applicablePiece);
                }
                if (_inGameManager.IsWhite)
                {
                    //ポーンの攻撃方向を修正
                    applicablePiece = _addPieceFunction.UpdatePoneGroup(applicablePiece);
                }
            }
            if ("K".Contains(search))
            {
                // ここで判定するのは怪しすぎる → コードを見る限りだと大丈夫そうだ
                if (_inGameManager.IsCastling[0]())
                {
                    applicablePiece = _addPieceFunction.AddShortCastlingArea(applicablePiece);
                }
                if (_inGameManager.IsCastling[1]())
                {
                    applicablePiece = _addPieceFunction.AddLongCastlingArea(applicablePiece);
                }
            }
            //駒が移動できる範囲を検索する回数。ここから先の処理はPieceが固定されている
            for (int c = 0; c < applicablePiece._MoveCount(); c++)
            {
                //for 駒が移動できる全方位のSquereを検索。移動できるマスがあったら該当の駒を登録し、次の駒で検索を再開する
                for (int d = 0; d < canMoveAreas.Length; d++)
                {
                    if (canMoveAreas[d].z == -1)
                    {
                        continue;
                    }
                    canMoveAreas[d] += applicablePiece._MoveAreas()[d]; //d == 方角
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
                                
                                _canMovePieceObjectHash.Add(_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj); //行動することが出来る駒を登録する。
                            }
                        }
                    }
                    else
                    {
                        applicablePiece._MoveCount = () => 0;
                        canMovePieceObjArray.Add(allyPieceSqueres[i]._IsOnPieceObj);
                    }
                }
            }
            //駒が攻撃できる範囲を検索する回数。ここから先の処理はPieceが固定されている
            for (int c = 0; c < applicablePiece._MoveCount(); c++)
            {
                //for 駒が攻撃できる全方位のSquereを検索。何かの駒にぶつかったら検索しなくて良い
                for (int d = 0; d < canAttackAreas.Length; d++)
                {
                    if (canAttackAreas[d].z == -1)
                    {
                        continue;
                    }
                    canAttackAreas[d] += applicablePiece._AttackAreas()[d]; //d == 方角
                    int alphabet = canAttackAreas[d].y;
                    int number = canAttackAreas[d].x;
                    //盤外である　は二度と検索しなくて良い条件 
                    if (!(-1 < alphabet && 8 > alphabet && -1 < number && 8 > number)) //盤外のマスであるならば
                    {
                        canAttackAreas[d].z = -1;
                        continue;
                    }

                    Squere difendSquere = _inGameManager._SquereArrays[alphabet][number];
                    // 駒がある の場合も二度と検索しなくて良い条件 + アンパッサンは通常のポーンの攻撃範囲と変わらないので条件にかける必要がない
                    if (difendSquere._IsOnPieceObj)
                    {
                        canAttackAreas[d].z = -1;
                        //敵の駒が見つかった場合の条件
                        if (difendSquere._IsOnPieceObj.CompareTag(groupTag))
                        {
                            applicablePiece._MoveCount = () => 0;
                            canMovePieceObjArray.Add(allyPieceSqueres[i]._IsOnPieceObj);
                        }
                    }
                }
            }
        }
        return canMovePieceObjArray.ToArray();
    }
}
