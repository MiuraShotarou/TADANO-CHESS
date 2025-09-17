using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
//変換処理・コレクション処理を探せ
public class InGameManager : MonoBehaviour
{
    bool _isWhite = true;
    bool _IsCheckedWhiteKing { get; set; }
    bool _IsCheckedBlackKing { get; set; }
    //値が変更可能なboolにアクセスできる状態から 固定値にしかアクセスできない状態を作る
    bool _isWhiteShortCastlingSwitch;
    bool _isWhiteLongCastlingSwitch; 
    bool _isBlackShortCastlingSwitch;
    bool _isBlackLongCastlingSwitch;
    public bool[] IsCastlingSwitch;
    Func<bool> _isWhiteShortCastling; 
    Func<bool> _isWhiteLongCastling;
    Func<bool> _isBlackShortCastling;
    Func<bool> _isBlackLongCastling;
    public Func<bool>[] IsCastling;
    [SerializeField] Piece[] _setPieces;
    [SerializeField] Squere[] _setSqueres;
    [SerializeField] GameObject[] _setPieceObjects;
    [SerializeField] SpriteRenderer[] _setDeceptionTileFields;
    [SerializeField] RuntimeAnimatorController[] _setPeiceRuntimeAnims;
    [SerializeField] AudioSource _bgmAudioSource;
    [SerializeField] AudioSource _seAudioSource;
    [SerializeField] AudioClip[] _bgmAudioClips;
    [SerializeField] AudioClip[] _seAudioClips;
    Dictionary<string, Piece> _pieceDict;
    Squere[][] _squereArrays;
    SpriteRenderer[][] _deceptionTileFieldArrays;
    TurnBegin _turnBegin;
    UIManager _uiManager;
    OpenSelectableArea _openSelectableArea;
    SelectTileController _selectTileController;
    TurnDeside _turnDeside;
    Animator _animatorController;
    GameObject _collider2DPrefab;
    bool _IsWhite {get => _isWhite; set { _isWhite = value; StartTurnRelay();}}
    public bool IsWhite{ get => _isWhite;}
    public int _WhiteTurnCount { get; set; }
    public int _BlackTurnCount { get; set;}
    // // valueが変わった時、次のターンを開始するメソッドの投入・条件式は最悪いらない
    public Dictionary<string, Piece> _PieceDict => _pieceDict; //s
    public Squere[][] _SquereArrays => _squereArrays; //s
    public SpriteRenderer[][] _DeceptionTileFieldArrays => _deceptionTileFieldArrays; //fs
    public AudioSource _BGMAudioSource => _bgmAudioSource;
    public AudioSource _SEAudioSource => _seAudioSource;
    public Dictionary<string, AudioClip[]> _BGMAudioClipDict { get; set; }
    public Dictionary<string, AudioClip[]> _SEAudioClipDict { get; set; }
    public Animator _AnimatorController => _animatorController;
    public GameObject _Collider2DPrefab => _collider2DPrefab; //s ro fs
    void Awake()
    {
        _pieceDict = _setPieces.ToDictionary(piece => piece._PieceName, piece => piece);
        //配列が番号順になっている保証はないので注意
        _BGMAudioClipDict = _bgmAudioClips.GroupBy(bc => bc.name.First().ToString()).ToDictionary(gc => gc.Key, g => g.ToArray());
        _SEAudioClipDict = _seAudioClips.GroupBy(sc => sc.name.First().ToString()).ToDictionary(gc => gc.Key, g => g.ToArray());
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
        //             Debug.Log(_SquereArrays[i][j]._IsOnPieceObj.name + _SquereArrays[i][j]._SquereID);
        //             // Debug.Log(_SquereArrays[i][j]._SquereID);
        //         }
        //     }
        // }
        _uiManager = GetComponent<UIManager>();
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _selectTileController = GetComponent<SelectTileController>();
        _turnBegin = GetComponent<TurnBegin>();
        _animatorController = GetComponent<Animator>();
        _collider2DPrefab = Resources.Load<GameObject>("Objects/BoxCollider2DPrefab");
        _isWhiteShortCastlingSwitch = false;
        _isWhiteLongCastlingSwitch = false;
        _isBlackShortCastlingSwitch = false;
        _isBlackLongCastlingSwitch = false;
        _isWhiteShortCastling = () => _isWhiteShortCastlingSwitch;
        _isWhiteLongCastling = () => _isWhiteShortCastlingSwitch;
        _isBlackShortCastling = () => _isBlackShortCastlingSwitch;
        _isBlackLongCastling = () => _isBlackShortCastlingSwitch;
    }

    void Start()
    {
        if (_uiManager.FadePanel.activeSelf)
        {
            _AnimatorController.Play("StartInGame");
        }
        else
        {
            
        }
    }

    void UnLockSafety()
    {
        _uiManager.InactiveFadePanel();
    }

    void LockSafety()
    {
        
    }
    /// <summary>
    /// CreateEnemyAtackRange() にてキングが攻撃範囲内にいた時、チェックのフラグを立てる
    /// </summary>
    /// <param name="allySquere"></param>
    /// <param name="enemySquere"></param>
    public void Check(bool isCheck, Squere allySquere, List<Squere> enemySquere)
    {
        if (IsWhite)
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
    /// 攻撃側のグループを変更させるメソッド。IsWhiteが変更されると"StartTurn".animが再生される
    /// </summary>
    public void TrunChange()
    {
        _IsWhite = !_IsWhite;
    }
    /// <summary>
    /// public Bool _IsWihte から呼び出される
    /// </summary>
    void StartTurnRelay()
    {
        Initialize();
        _turnBegin.StartTurn();
        _uiManager.StartTurnUI();
        _animatorController.Play("StartTurn");
    }
    public void StartActivePromotionRelay()
    {
        _AnimatorController.Play("ActivePromotion");
    }

    public void StartInactivePromotionRelay()
    {
        _AnimatorController.Play("InactivePromotion");
    }
    void Initialize()
    {
        IsCastling = _IsWhite? new[] { _isWhiteShortCastling, _isWhiteLongCastling }:
                                new []{ _isBlackShortCastling, _isBlackLongCastling};
        IsCastlingSwitch = _IsWhite? new []{_isWhiteShortCastlingSwitch, _isWhiteLongCastlingSwitch}:
                                        new []{_isBlackShortCastlingSwitch, _isBlackLongCastlingSwitch};
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