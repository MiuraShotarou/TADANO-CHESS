using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ArtificialIntelligence : MonoBehaviour
{
    InGameManager _inGameManager;
    OpenSelectableArea _openSelectableArea;
    AddPieceFunction _addPieceFunction;
    HashSet<GameObject> _canMovePieceObjectHash;
    SquereID[] _shortDistanceIDs;
    SquereID[] _longDistanceIDs;
    Squere _checkedKingSquere;
    List<Squere> _checkAttackerSqueres;
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _addPieceFunction = GetComponent<AddPieceFunction>();
        _openSelectableArea = GetComponent<OpenSelectableArea>();
    }
    
    public void StartArtificialIntelligence()
    {
        Initialize();
        HashSet<SquereID> enemyAttackRange = CreateAttackRange(false);
        _inGameManager.Check(JudgeCheck(false));
        // チェックがかけられていた場合、チェックメイトを判断する
        if (_inGameManager.IsCheck)
        {
            _inGameManager.CheckMate(JudgeCheckMate(false));
        }
        else
        {
            _inGameManager.CheckMate(false);
        }
        FilterCastling(enemyAttackRange);
    }
    /// <summary>
    /// キングとルークの間のマスを登録する
    /// </summary>
    void Initialize()
    {
        _checkedKingSquere = null;
        _checkAttackerSqueres = new List<Squere>();
        _canMovePieceObjectHash = new HashSet<GameObject>();
        _shortDistanceIDs = _inGameManager.IsWhite? new [] {SquereID.f1, SquereID.g1}://w_s_c
                                                    new [] {SquereID.f8, SquereID.g8};//b_s_c
        _longDistanceIDs = _inGameManager.IsWhite?  new [] {SquereID.c1, SquereID.d1}://w_l_c
                                                    new [] {SquereID.c8, SquereID.d8};//b_l_c
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
                if (groupTag.Contains("Black"))
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
                            if (_inGameManager.IsCastling.Any(b => b()))
                            {
                                allyCanMoveRange.Add(_inGameManager._SquereArrays[alphabet][number]._SquereID);
                            }
                        }
                    }
                    else
                    {
                        allyCanMoveRange.Add(_inGameManager._SquereArrays[alphabet][number]._SquereID);
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
        Squere[] groupPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(groupTag))).ToArray();
        HashSet<SquereID> enemyAttackRange = new HashSet<SquereID>();
        //for すべての駒で
        for (int i = 0; i < groupPieceSqueres.Length; i++)
        {
            string search = groupPieceSqueres[i]._IsOnPieceObj.name.First().ToString();
            Piece attackerPiece = Instantiate(_inGameManager._PieceDict[search]);
            Vector3Int[] attackAreas = Enumerable.Repeat(groupPieceSqueres[i]._SquereTilePos, attackerPiece._AttackAreas().Length).ToArray();
            if ("P".Contains(search) && groupTag.Contains("Black"))
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
                        //敵の駒が見つかった場合の条件
                        if (difendSquere._IsOnPieceObj.CompareTag(antiGroupTag))
                        {
                            enemyAttackRange.Add(difendSquere._SquereID);
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
        //条件① チェックだった場合
        if (_inGameManager.IsCheck){return;}
        //条件② 両キャスリング時の移動経路になにかしらの駒がある場合、攻撃範囲の検索を中断する
        bool isShortCastling = MinimalFilter(_shortDistanceIDs);
        bool isLongCastling = MinimalFilter(_longDistanceIDs);
        //条件③ 両キャスリング時の移動経路が敵の攻撃範囲外ならキャスリングの使用を可能にする
        if (isShortCastling)
        {
            isShortCastling = _shortDistanceIDs.All(condition => !enemyAttackRange.Contains(condition));
        }
        if (isLongCastling)
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
    /// <summary>
    /// チェックが起きているかを判断する
    /// </summary>
    /// <param name="isModeAlly"></param>
    /// <returns></returns>
    public bool JudgeCheck(bool isModeAlly)
    {
        isModeAlly = _inGameManager.IsWhite? isModeAlly : !isModeAlly;
        string groupTag = isModeAlly? "White" : "Black";
        string antiGroupTag = isModeAlly? "Black" : "White";
        Squere[] groupPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(groupTag))).ToArray();
        //for すべての駒で
        for (int i = 0; i < groupPieceSqueres.Length; i++)
        {
            string search = groupPieceSqueres[i]._IsOnPieceObj.name.First().ToString();
            Piece attackerPiece = Instantiate(_inGameManager._PieceDict[search]);
            Vector3Int[] attackAreas = Enumerable.Repeat(groupPieceSqueres[i]._SquereTilePos, attackerPiece._AttackAreas().Length).ToArray();
            if ("P".Contains(search) && groupTag.Contains("Black"))
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
                        //敵の駒が見つかった場合の条件
                        if (difendSquere._IsOnPieceObj.CompareTag(antiGroupTag) && "K".Contains(difendSquere._IsOnPieceObj.name.First().ToString()))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    /// <summary>
    /// group側がチェックメイトできているか否かを判断する
    /// </summary>
    /// <param name="isModeAlly"></param>
    /// <returns></returns>
    bool JudgeCheckMate(bool isModeAlly)
    {
        // antiGroup側がキングを守れるか否か
        isModeAlly = _inGameManager.IsWhite? isModeAlly : !isModeAlly;
        string groupTag = isModeAlly? "White" : "Black";
        string antiGroupTag = isModeAlly? "Black" : "White"; //チェクがかけられている側のグループを登録する
        //逃げる側の駒が置いてあるマスの配列
        Squere[] groupPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(antiGroupTag))).ToArray();
        // 逃げる側の駒が全員動くことを想定する
        for (int i = 0; i < groupPieceSqueres.Length; i++)
        {
            GameObject memorizePieceObj = groupPieceSqueres[i]._IsOnPieceObj;
            groupPieceSqueres[i]._IsOnPieceObj = null;
            string search = memorizePieceObj.name.First().ToString();
            Piece applicablePiece = Instantiate(_inGameManager._PieceDict[search]);
            Vector3Int[] moveAreas = Enumerable.Repeat(groupPieceSqueres[i]._SquereTilePos, applicablePiece._MoveAreas().Length).ToArray();
            Vector3Int[] attackAreas = Enumerable.Repeat(groupPieceSqueres[i]._SquereTilePos, applicablePiece._MoveAreas().Length).ToArray();
            // 駒の能力を調整する
            if ("P".Contains(search))
            {
                if (memorizePieceObj.transform.rotation.z == 0)
                {
                    applicablePiece = _addPieceFunction.AddMoveCount(applicablePiece);
                }
                if (antiGroupTag.Contains("Black"))
                {
                    //ポーンの攻撃方向を修正
                    applicablePiece = _addPieceFunction.UpdatePoneGroup(applicablePiece);
                }
            }
            for (int c = 0; c < applicablePiece._MoveCount(); c++)
            {
                //for 
                for (int d = 0; d < moveAreas.Length; d++)
                {
                    if (moveAreas[d].z == -1)
                    {
                        continue;
                    }
                    moveAreas[d] += applicablePiece._MoveAreas()[d]; //d == 方角
                    int alphabet = moveAreas[d].y;
                    int number = moveAreas[d].x;
                    if (!(-1 < alphabet && 8 > alphabet && -1 < number && 8 > number)) //盤外のマスであるならば
                    {
                        moveAreas[d].z = -1;
                        continue;
                    }
                    if (_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj)
                    {
                        moveAreas[d].z = -1;
                    }
                    else //駒が乗っていないなら
                    {
                        // 移動先のSquereに自分の駒を登録する
                        _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = memorizePieceObj;
                        // 仮に移動したとしてチェックが掛かっていなかった場合
                        if (!JudgeCheck(false))
                        {
                            // Debug.Log("NoCheckMate_Move");
                            _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = null;
                            groupPieceSqueres[i]._IsOnPieceObj = memorizePieceObj;
                            return false;
                        }
                        _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = null;
                    }
                }
            }
            // 攻撃のパターン
            for (int c = 0; c < applicablePiece._MoveCount(); c++)
            {
                for (int d = 0; d < attackAreas.Length; d++)
                {
                    if (attackAreas[d].z == -1)
                    {
                        continue;
                    }
                    attackAreas[d] += applicablePiece._AttackAreas()[d]; //d == 方角
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
                        //敵の駒が見つかった場合の条件
                        if (difendSquere._IsOnPieceObj.CompareTag(groupTag))
                        {
                            GameObject memorizeAntiPieceObj = _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj;
                            _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = memorizePieceObj;
                            // 仮に移動したとしてチェックが掛かっていなかった場合
                            if (!JudgeCheck(false))
                            {
                                // Debug.Log("NoCheckMate_Attack");
                                _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = memorizeAntiPieceObj;
                                groupPieceSqueres[i]._IsOnPieceObj = memorizePieceObj;
                                return false;
                            }
                            _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = memorizeAntiPieceObj;
                        }
                    }
                }
            }
            groupPieceSqueres[i]._IsOnPieceObj = memorizePieceObj;
        }
        return true; //回避できるパターンがひとつもなかった場合
    }

    public void MoveComputer()
    {
        _inGameManager.LockSafety();
        //動かすことが出来る駒を取得する
        GameObject[] canMovePieceObjArray = CreateCanMovePieceObject(true);
        // AIが移動先のSquare を選択し移動する処理
        // allyPieceSqueresから動けるコマを選別する。
        GameObject selectedPieceObj = canMovePieceObjArray[Random.Range(0, canMovePieceObjArray.Length)];//テスト用ランダム選出
        PointerEventData pointerData = new PointerEventData(EventSystem.current){position = Mouse.current.position.ReadValue()};
        selectedPieceObj.GetComponent<EventTrigger>()?.OnPointerClick(pointerData);
        DOVirtual.DelayedCall(1, DecideComputer);
    }
    void DecideComputer()
    {
        SpriteRenderer[] pieceCanMoveRange = _inGameManager._DeceptionTileFieldArrays.SelectMany(array => array.Where(field => field.gameObject.GetComponent<BoxCollider2D>().enabled)).ToArray();
        var decideSpriteRenderer = pieceCanMoveRange[Random.Range(0, pieceCanMoveRange.Length)];
        _openSelectableArea.TurnDesideRelay(decideSpriteRenderer);
    }
    /// <summary>
    /// 自身の駒が移動可能な範囲をすべて取得する
    /// </summary>
    GameObject[] CreateCanMovePieceObject(bool isModeAlly)
    {
        isModeAlly = _inGameManager.IsWhite? isModeAlly : !isModeAlly;
        string groupTag = isModeAlly? "White" : "Black";
        string antiGroupTag = isModeAlly? "Black" : "White";
        //自陣のコマが置いてあるマスの配列
        Squere[] groupPieceSqueres = _inGameManager._SquereArrays.SelectMany(flatSqueres => flatSqueres.Where(squere => squere._IsOnPieceObj && squere._IsOnPieceObj.CompareTag(groupTag))).ToArray();
        List<GameObject> canMovePieceObjArray = new List<GameObject>();
        //for すべての駒で
        for (int i = 0; i < groupPieceSqueres.Length; i++)
        {
            string search = groupPieceSqueres[i]._IsOnPieceObj.name.First().ToString();
            Piece applicablePiece = Instantiate(_inGameManager._PieceDict[search]);
            Vector3Int[] canMoveAreas = Enumerable.Repeat(groupPieceSqueres[i]._SquereTilePos, applicablePiece._MoveAreas().Length).ToArray();
            Vector3Int[] canAttackAreas = Enumerable.Repeat(groupPieceSqueres[i]._SquereTilePos, applicablePiece._AttackAreas().Length).ToArray();
            // 駒の能力を調整する
            if ("P".Contains(search))
            {
                if ("1_6".Contains(groupPieceSqueres[i]._SquerePiecePosition.x.ToString()))
                {
                    applicablePiece = _addPieceFunction.AddMoveCount(applicablePiece);
                }
                if (groupTag.Contains("Black"))
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
            //移動範囲
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
                            if (_inGameManager.IsCastling.Any(b => b()))
                            {
                                canMovePieceObjArray.Add(groupPieceSqueres[i]._IsOnPieceObj); //行動することが出来る駒を登録する。
                                break;
                            }
                        }
                    }
                    else
                    {
                        //キングが取られる選択肢は排除するように設定
                        _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = groupPieceSqueres[i]._IsOnPieceObj;
                        groupPieceSqueres[i]._IsOnPieceObj = null;
                        if (!JudgeCheck(false))
                        {
                            applicablePiece._MoveCount = () => 0;
                            canMovePieceObjArray.Add(_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj);
                        }
                        groupPieceSqueres[i]._IsOnPieceObj = _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj;
                        _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = null;
                        break;
                    }
                }
            }
            //攻撃範囲
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
                        if (difendSquere._IsOnPieceObj.CompareTag(antiGroupTag))
                        {
                            //キングが取られる選択肢は排除するように設定
                            GameObject memorizePieceObj = _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj;
                            _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = groupPieceSqueres[i]._IsOnPieceObj;
                            groupPieceSqueres[i]._IsOnPieceObj = null;
                            if (!JudgeCheck(false))
                            {
                                applicablePiece._MoveCount = () => 0;
                                canMovePieceObjArray.Add(_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj);
                            }
                            groupPieceSqueres[i]._IsOnPieceObj = _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj;
                            _inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj = memorizePieceObj;
                            break;
                        }
                    }
                }
            }
        }
        return canMovePieceObjArray.ToArray();
    }
}
