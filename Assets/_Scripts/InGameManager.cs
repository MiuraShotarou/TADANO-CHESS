using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InGameManager : MonoBehaviour
{
    bool _isWhite = true;
    //値が変更可能なboolにアクセスできる状態から 固定値にしかアクセスできない状態を作る
    
    //絶対にショートキャスリングができない状況になった時、ここのここの値をfalseにする
    static bool _isWhiteShortCastlingJudg = false;
    static bool _isWhiteLongCastling = false;
    //絶対にロングキャスリングができない状況になった時、ここのここの値をfalseにする
    static bool _isBlackShortCastling = false;
    static bool _isBlackLongCastling = false;
    public static bool IsWhiteShortCastlingJudg{ get  { return _isWhiteShortCastlingJudg; }set { _isWhiteShortCastlingJudg = value; }}
    public static bool IsWhiteLongCastlingJudg;
    //絶対にロングキャスリングができない状況になった時、ここのここの値をfalseにする
    public static bool IsBlackShortCastlingJudg;
    public static bool IsBlackLongCastlingJudg;
    
    [SerializeField] Piece[] _setPieces;
    [SerializeField] Squere[] _setSqueres;
    [SerializeField] GameObject[] _setPieceObjects;
    [SerializeField] SpriteRenderer[] _setDeceptionTileFields; //プロパティにしておけ
    [SerializeField] Piece _whitePiece;
    [SerializeField] Piece _blackPiece;
    Dictionary<string, Piece> _pieceDict = new Dictionary<string, Piece>();
    Squere[][] _squereArrays;
    SpriteRenderer[][] _deceptionTileFieldArrays;
    TurnBegin _turnBegin;
    OpenSelectableArea _openSelectableArea;
    SelectTileController _selectTileController;
    TurnDeside _turnDeside;
    Animator _animatorController;
    GameObject _collider2DPrefab;
    // public bool _IsWhite {get => _isWhite; set {{ _isWhite = value; if (_IsWhite) { _turnBegin.JudgmentCastling = ;}else{;}  StartTurnRelay();}}}}
    // // valueが変わった時、次のターンを開始するメソッドの投入・条件式は最悪いらない
    public Dictionary<string, Piece> _PieceDict => _pieceDict; //s
    public Squere[][] _SquereArrays => _squereArrays; //s
    public SpriteRenderer[][] _DeceptionTileFieldArrays => _deceptionTileFieldArrays; //fs
    public OpenSelectableArea _OpenSelectableArea => _openSelectableArea; //いらない
    public SelectTileController SelectTileController => _selectTileController; //いらない
    public Animator _AnimatorController => _animatorController;
    public GameObject _Collider2DPrefab => _collider2DPrefab; //s ro fs
    void Awake()
    {
        for (int i = 0; i < _setPieces.Length; i++)
        {
            //Add・Removeできるならプロパティの意味ない
            _PieceDict.Add(_setPieces[i]._PieceName, _setPieces[i]);
        }
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
        _animatorController = GetComponent<Animator>();
        _collider2DPrefab = Resources.Load<GameObject>("Objects/BoxCollider2DPrefab");
        _whitePiece.IsShortCastling = IsWhiteShortCastlingJudg;
        _whitePiece.IsLongCastling = IsWhiteLongCastlingJudg;
        _blackPiece.IsShortCastling = IsBlackShortCastlingJudg;
        _blackPiece.IsLongCastling = IsBlackLongCastlingJudg;
    }
    public void PieceObjectPressed(GameObject pieceObj)
    {
        _openSelectableArea.StartOpenArea(pieceObj);
    }
    public void StartSelectTileRelay()
    {
        _selectTileController.enabled = true;
    }

    void StartTurnRelay()
    {
        _turnBegin.StartTurn();
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