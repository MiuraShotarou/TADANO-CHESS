using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
/// <summary>
/// <禁じ手/>
/// 自殺手
/// チェックを解除しない動き
/// <ドロー条件/>
/// チェックがかかってない（キングが直接的に攻撃されていない）状態でキング以外の駒がなく該当者のターンを迎えてしまった時（ステイルメイト）
/// ３回同一局面
/// 50回Poneが動かず、その間どの駒も取られていなかった場合。
/// 合意によるもの
/// <プレイスキル/>
/// 
/// </summary>
//変換処理・コレクション処理を探せ
public class InGameManager : MonoBehaviour
{
    public GameMode GameMode;
    public int ComputerLevel;
    bool _isWhite;
    //値が変更可能なboolにアクセスできる状態から 固定値にしかアクセスできない状態を作る
    public bool _isWhiteShortCastlingSwitch;
    public bool _isWhiteLongCastlingSwitch;
    public bool _isBlackShortCastlingSwitch;
    public bool _isBlackLongCastlingSwitch;
    //以下のFunc変数は読み取り専用のプロパティとして利用する
    Func<bool> _isWhiteShortCastling;
    Func<bool> _isWhiteLongCastling;
    Func<bool> _isBlackShortCastling;
    Func<bool> _isBlackLongCastling;
    //読み取り専用のプロパティをひとまとめにしている変数
    public Func<bool>[] IsCastling;
    [SerializeField] Piece[] _setPieces;
    [SerializeField] Squere[] _setSqueres;
    [SerializeField] GameObject[] _setPieceObjects;
    [SerializeField] GameObject[] _setPromotionObjects;
    [SerializeField] SpriteRenderer[] _setDeceptionTileFields;
    [SerializeField] RuntimeAnimatorController[] _setPeiceRuntimeAnims;
    [SerializeField] AudioSource _bgmAudioSource;
    [SerializeField] AudioSource _seAudioSource;
    [SerializeField] AudioClip[] _bgmAudioClips;
    [SerializeField] AudioClip[] _seAudioClips;
    [SerializeField] TimelineAsset _titleTimeline;
    [SerializeField] TimelineAsset _rebootTitleTimeline;
    Dictionary<string, Piece> _pieceDict;
    Dictionary<string, GameObject> _promotionDict;
    Squere[][] _squereArrays;
    SpriteRenderer[][] _deceptionTileFieldArrays;
    UIManager _uiManager;
    OpenSelectableArea _openSelectableArea;
    SelectTileController _selectTileController;
    TurnDecide _turnDecide;
    ArtificialIntelligence _artificialIntelligence;
    Animator _animatorController;
    PlayableDirector _playableDirector;
    bool _IsWhite {get => _isWhite; set { _isWhite = value; StartTurnRelay();}}
    public bool IsWhite{ get => _isWhite;}
    public bool IsCheck { get; private set; }
    public bool IsCheckMate { get; private set; }
    public bool IsPlayerTurn;
    public int TurnCount;
    static bool _isSecondPlay = false;
    // // valueが変わった時、次のターンを開始するメソッドの投入・条件式は最悪いらない
    public Dictionary<string, Piece> _PieceDict => _pieceDict; //s
    public Dictionary<string, GameObject> _PromotionDict => _promotionDict;
    public Squere[][] _SquereArrays => _squereArrays; //s
    public SpriteRenderer[][] _DeceptionTileFieldArrays => _deceptionTileFieldArrays; //fs
    public AudioSource _BGMAudioSource => _bgmAudioSource;
    public AudioSource _SEAudioSource => _seAudioSource;
    public Dictionary<string, AudioClip[]> _BGMAudioClipDict { get; set; }
    public Dictionary<string, AudioClip[]> _SEAudioClipDict { get; set; }
    public Animator _AnimatorController => _animatorController;
    void Awake()
    {
        _pieceDict = _setPieces.ToDictionary(piece => piece._PieceName, piece => piece);
        //配列が番号順になっている保証はないので注意
        _BGMAudioClipDict = _bgmAudioClips.GroupBy(bc => bc.name.First().ToString()).ToDictionary(gc => gc.Key, g => g.ToArray());
        _SEAudioClipDict = _seAudioClips.GroupBy(sc => sc.name.First().ToString()).ToDictionary(gc => gc.Key, g => g.ToArray());
        _promotionDict = _setPromotionObjects.ToDictionary(obj => obj.name.First().ToString(), obj => obj);
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
        _animatorController = GetComponent<Animator>();
        _playableDirector = GetComponent<PlayableDirector>();
        _artificialIntelligence = GetComponent<ArtificialIntelligence>();
        _isWhiteShortCastlingSwitch = false;
        _isWhiteLongCastlingSwitch = false;
        _isBlackShortCastlingSwitch = false;
        _isBlackLongCastlingSwitch = false;
        _isWhiteShortCastling = () => _isWhiteShortCastlingSwitch;
        _isWhiteLongCastling = () => _isWhiteLongCastlingSwitch;
        _isBlackShortCastling = () => _isBlackShortCastlingSwitch;
        _isBlackLongCastling = () => _isBlackLongCastlingSwitch;
    }

    void Start()
    {
        _isWhite = true;
        GameMode = GameMode.None;
        _BGMAudioSource.clip = _BGMAudioClipDict["I"].FirstOrDefault(c => c.name.Contains("1")); //鐘の音
        _BGMAudioSource.Play();
        if (!_isSecondPlay)
        {
            _playableDirector.playableAsset = _titleTimeline;
        }
        else
        {
            _playableDirector.playableAsset = _rebootTitleTimeline;
        }
        _playableDirector.Play();
        Time.timeScale = 10;
    }
    /// <summary>
    /// _titleTimeLineから一度だけ呼び出される
    /// </summary>
    void StartTitle()
    {
        _AnimatorController.Play("Title");
    }

    /// <summary>
    /// StartComputer.animの再生後にEventから一度だけ呼び出される
    /// </summary>
    void StartSelectComputerStrength()
    {
        _AnimatorController.Play("StartSelectComputerStrength");
    }
    /// <summary>
    /// MultiPlayButtonを押した時に一度だけ呼び出される。シーン遷移の演出用アニメーションを再生しているだけ
    /// </summary>
    public void StartMulti()
    {
        GameMode = GameMode.Multi;
        _AnimatorController.Play("StartMulti");
    }
    public void StartComputer()
    {
        GameMode = GameMode.Computer;
        ComputerLevel = 1;
        _AnimatorController.Play("StartComputer");
    }
    /// <summary>
    /// StartComputer.animの再生後にEventから一度だけ呼び出される
    /// </summary>
    void StartDecideMultiPlayerGroup()
    {
        _uiManager.UpdateMultiPlayerGroupUI();
        _animatorController.Play("StartDecideMultiPlayerGroup");
    }
    /// <summary>
    /// 攻撃側のグループを変更させるメソッド。IsWhiteが変更されると"StartTurn".animが再生される
    /// </summary>
    public void TurnChange()
    {
        _IsWhite = !_IsWhite;
    }
    /// <summary>
    /// public Bool _IsWihte から呼び出される
    /// </summary>
    void StartTurnRelay()
    {
        Initialize();
        AddTurnCount();
        _artificialIntelligence.StartArtificialIntelligence();
        _uiManager.StartUpdateTurnUI();
        if (TurnCount == 1)
        {
            Time.timeScale = 1;
            _playableDirector.enabled = false;
            IsPlayerTurn = Random.Range(0, 2) == 0; //プレイヤー１、２が白側・黒側どちらに所属されるのかを決定する
            _AnimatorController.Play("GameStart");
        }
        else
        {
            ChangePlayerTurn();
            _AnimatorController.Play("StartTurn");
        }
    }
    void AddTurnCount()
    {
        TurnCount++;
    }
    /// <summary>
    /// プレイヤーターンの切り替え
    /// </summary>
    void ChangePlayerTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;
    }

    void UnLockSafety()
    {
        _uiManager.InactiveFadePanel();
    }

    void LockSafety()
    {
        _uiManager.ActiveFadePanel();
    }
    /// <summary>
    /// CreateEnemyAtackRange() にてキングが攻撃範囲内にいた時、チェックのフラグを立てる
    /// </summary>
    /// <param name="isCheck"></param>
    public void Check(bool isCheck)
    {
        IsCheck = isCheck;
        if (IsCheck){Debug.Log("Check");}
        _uiManager.UpdateCheckUI();
    }
    /// <summary>
    /// TurnBegin.cs にて moveRange全検索からのenemyRange全検索で判定できるかもしれないが、あまりにも難しすぎるので今回は断念する
    /// </summary>
    public void CheckMate(bool isCheckMate)
    {
        IsCheckMate = isCheckMate;
        if (IsCheckMate)
        {
            _uiManager.UpdateCheckMateUI();
            _uiManager.ActiveResultUI();
        }
        if (IsCheckMate){Debug.Log("CheckMate");}
    }
    public void PieceObjectPressed(GameObject pieceObj)
    {
        _openSelectableArea.StartOpenArea(pieceObj);
    }
    public void StartSelectTileRelay()
    {
        UnLockSafety();
        _selectTileController.enabled = true;
    }

    public void StartActivePromotionRelay()
    {
        _AnimatorController.Play("ActivePromotion");
    }

    public void StartInactivePromotionRelay()
    {
        _AnimatorController.Play("InactivePromotion");
    }
    public void MoveComputerRelay()
    {
        if (GameMode == GameMode.Computer && !IsPlayerTurn)
        {
            _artificialIntelligence.MoveComputer();
        }
    }
    void Initialize()
    {
        IsCastling = _IsWhite? new[] { _isWhiteShortCastling, _isWhiteLongCastling }:
                                new []{ _isBlackShortCastling, _isBlackLongCastling};
    }

    void ReturnTitleInit()
    {
        // _isSecondPlay = true; ※
        SceneManager.LoadScene("InGameScene");
    }
}

public enum GameMode
{
    None,
    Online,
    Computer,
    Multi,
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