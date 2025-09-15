using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//変換処理・コレクション処理を探せ
public class InGameManager : MonoBehaviour
{
    bool _isWhite = true;
    public bool _IsCheckedWhiteKing { get; set; }
    public bool _IsCheckedBlackKing { get; set; }
    //値が変更可能なboolにアクセスできる状態から 固定値にしかアクセスできない状態を作る
    public bool IsWhiteShortCastlingSwitch;
    public bool IsWhiteLongCastlingSwitch;
    public bool IsBlackShortCastlingSwitch;
    public bool IsBlackLongCastlingSwitch;
    public bool[] IsCastlingSwitch;
    public Func<bool> IsWhiteShortCastling;
    public Func<bool> IsWhiteLongCastling;
    public Func<bool> IsBlackShortCastling;
    public Func<bool> IsBlackLongCastling;
    public Func<bool>[] IsCastling;
    [SerializeField] Piece[] _setPieces;
    [SerializeField] Squere[] _setSqueres;
    [SerializeField] GameObject[] _setPieceObjects;
    [SerializeField] SpriteRenderer[] _setDeceptionTileFields; //プロパティにしておけ
    [SerializeField] Piece _whitePieceK;
    [SerializeField] Piece _blackPieceK;
    Dictionary<string, Piece> _pieceDict = new Dictionary<string, Piece>();
    Squere[][] _squereArrays;
    SpriteRenderer[][] _deceptionTileFieldArrays;
    TurnBegin _turnBegin;
    OpenSelectableArea _openSelectableArea;
    SelectTileController _selectTileController;
    TurnDeside _turnDeside;
    Animator _animatorController;
    GameObject _collider2DPrefab;
    public bool _IsWhite {get => _isWhite; set { _isWhite = value; StartTurnRelay();}}
    // // valueが変わった時、次のターンを開始するメソッドの投入・条件式は最悪いらない
    public Dictionary<string, Piece> _PieceDict => _pieceDict; //s
    public Squere[][] _SquereArrays => _squereArrays; //s
    public SpriteRenderer[][] _DeceptionTileFieldArrays => _deceptionTileFieldArrays; //fs
    public Piece _WhitePieceK {get => _whitePieceK; set => _whitePieceK = value;}
    public Piece _BlackPieceK {get => _blackPieceK; set => _blackPieceK = value;}
    public OpenSelectableArea _OpenSelectableArea => _openSelectableArea; //いらない
    public SelectTileController SelectTileController => _selectTileController; //いらない
    public Animator _AnimatorController => _animatorController;
    public GameObject _Collider2DPrefab => _collider2DPrefab; //s ro fs
    void Awake()
    {
        _pieceDict = _setPieces.ToDictionary(piece => piece._PieceName, piece => piece);
        int arraySize = 8;
        _deceptionTileFieldArrays = new SpriteRenderer[arraySize][];
        _squereArrays = new Squere[arraySize][];
        int count = 0;
        for (int i = 0; i < arraySize; i++)
        {
            _deceptionTileFieldArrays[i] = new SpriteRenderer[arraySize];
            _squereArrays[i] = new Squere[arraySize];
            for (int j = 0; j < arraySize; j++)
            {
                //１次元目にアルファベット（縦列）座標を、２次元目に数値（横列）座標を割り当てる
                int index = i * 8 + j;
                _deceptionTileFieldArrays[i][j] = _setDeceptionTileFields[index];
                _SquereArrays[i][j] = _setSqueres[index];
                if ("0,1,6,7".Contains(j.ToString()))
                {
                    _SquereArrays[i][j]._IsOnPieceObj = _setPieceObjects[count];
                    count++;
                }
                else
                {
                    _SquereArrays[i][j]._IsOnPieceObj = null;
                }
                // _SquereArrays[i][j].UpdateMiniBorad = MiniBoard.UpdateMiniBoard;
            }
        }
        // for (int i = 0; i < arraySize; i++)
        // {
        //     for (int j = 0; j < arraySize; j++)
        //     {
        //         //１次元目にアルファベット（縦列）座標を、２次元目に数値（横列）座標を割り当てる
        //         if (_SquereArrays[i][j]._IsOnPieceObj)
        //         {
        //             // Debug.Log(_SquereArrays[i][j]._IsOnPieceObj.name + _SquereArrays[i][j]._SquereID);
        //             // Debug.Log(_SquereArrays[i][j]._SquereID);
        //         }
        //     }
        // }
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _selectTileController = GetComponent<SelectTileController>();
        _turnBegin = GetComponent<TurnBegin>();
        _animatorController = GetComponent<Animator>();
        _collider2DPrefab = Resources.Load<GameObject>("Objects/BoxCollider2DPrefab");
        IsWhiteShortCastlingSwitch = false;
        IsWhiteLongCastlingSwitch = false;
        IsBlackShortCastlingSwitch = false;
        IsBlackLongCastlingSwitch = false;
        IsWhiteShortCastling = () => IsWhiteShortCastlingSwitch;
        IsWhiteLongCastling = () => IsWhiteShortCastlingSwitch;
        IsBlackShortCastling = () => IsBlackShortCastlingSwitch;
        IsBlackLongCastling = () => IsBlackShortCastlingSwitch;
    }
    /// <summary>
    /// CreateEnemyAtackRange() にてキングが攻撃範囲内にいた時、チェックのフラグを立てる
    /// </summary>
    /// <param name="allySquere"></param>
    /// <param name="enemySquere"></param>
    public void Check(bool isCheck, Squere allySquere, List<Squere> enemySquere)
    {
        if (_IsWhite)
        {
            _IsCheckedWhiteKing = isCheck;
        }
        else
        {
            _IsCheckedBlackKing = isCheck;
        }
        if (isCheck){Debug.Log("Check");}
    }
    /// <summary>
    /// TurnBegin.cs にて moveRange全検索からのenemyRange全検索で判定できるかもしれないが、あまりにも難しすぎるので今回は断念する
    /// </summary>
    void CheckMate()
    {
        
    }
    public void PieceObjectPressed(GameObject pieceObj)
    {
        _openSelectableArea.StartOpenArea(pieceObj);
    }
    public void StartSelectTileRelay()
    {
        _selectTileController.enabled = true;
    }
    /// <summary>
    /// public Bool _IsWihte から呼び出される
    /// </summary>
    void StartTurnRelay()
    {
        Initialize();
        _turnBegin.StartTurn();
    }

    public void StartActivePromotionRelay()
    {
        _AnimatorController.Play("ActivePromotionPanel");
    }

    void Initialize()
    {
        IsCastling = _IsWhite? new[] { IsWhiteShortCastling, IsWhiteLongCastling }:
                                new []{ IsBlackShortCastling, IsBlackLongCastling};
        IsCastlingSwitch = _IsWhite? new []{IsWhiteShortCastlingSwitch, IsWhiteLongCastlingSwitch}:
                                        new []{IsBlackShortCastlingSwitch, IsBlackLongCastlingSwitch};
    }
}
public enum SquereID //8 * 8 の配列OnPieceだったら該当のbitを1にする 
{
    //ulong |= 1UL << (int)BordSquere.hoge :追加
    //board &= ~(1UL << (int)BoardSquare.e4); 削除 → ここから勉強
    a1,
    a2,
    a3,
    a4,
    a5,
    a6,
    a7,
    a8,
    b1,
    b2,
    b3,
    b4,
    b5,
    b6,
    b7,
    b8,
    c1,
    c2,
    c3,
    c4,
    c5,
    c6,
    c7,
    c8,
    d1,
    d2,
    d3,
    d4,
    d5,
    d6,
    d7,
    d8,
    e1,
    e2,
    e3,
    e4,
    e5,
    e6,
    e7,
    e8,
    f1,
    f2,
    f3,
    f4,
    f5,
    f6,
    f7,
    f8,
    g1,
    g2,
    g3,
    g4,
    g5,
    g6,
    g7,
    g8,
    h1,
    h2,
    h3,
    h4,
    h5,
    h6,
    h7,
    h8
}