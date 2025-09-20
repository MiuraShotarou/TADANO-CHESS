using System;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Playables;
using UnityEngine.Animations;
//Colliderのデストロイタイミング
/// <summary>
/// 駒が目標地点まで移動していくまでの処理を実装するクラス
/// </summary>
public class TurnDeside : ColorPallet
{
    InGameManager _inGameManager;
    OpenSelectableArea _openSelectableArea;
    UIManager _uiManager;
    GameObject _selectedPieceObj;
    Piece _selectedPiece;
    Squere _selectedSquere;
    Squere _targetSquere;
    Squere _enpassantSquere;
    Animator _selectedPieceAnimatorController;
    Animator _targetPieceAnimatorController;
    RuntimeAnimatorController _selectedPieceRuntimeAnimator;
    AnimationCurve _endPositionCurve;
    GameObject _RAttackEffectObj;
    GameObject _BAttackEffectObj;
    GameObject _QAttckEffectObj;
    GameObject _targetObj;
    GameObject _enpassantObj;
    SpriteRenderer _selectedTileSpriteRenderer;
    Action _castlingAnimation;
    // GameObject _hitStopObj;
    PlayableGraph _playableGraph;
    AnimationPlayableOutput _animationPlayableOutput;
    bool _isDirectionRight;
    float _direction;
    float _Direction { get {return _direction ;} set { _direction = value; if (_direction < 0){ _isDirectionRight = false; } else {_isDirectionRight = true;}}}//リファクタ
    
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _uiManager = GetComponent<UIManager>();
        _RAttackEffectObj = transform.GetChild(0).gameObject;
        _BAttackEffectObj = transform.GetChild(1).gameObject;
        _QAttckEffectObj = transform.GetChild(2).gameObject;
        // _hitStopObj = transform.GetChild(2?).gameObject;
    }
    /// <summary>
    /// _SelectedTileSprite上でマウスクリックされた時に一度だけ呼び出される
    /// </summary>
    /// <param name="currentSpriteRenderer"></param>
    /// <param name="selectedPieceObj"></param>
    /// <param name="selectedPiece"></param>
    /// <param name="selectedSquere"></param>
    /// <param name="targetSquere"></param>
    public void StartTurnDeside(SpriteRenderer currentSpriteRenderer, GameObject selectedPieceObj, Piece selectedPiece, Squere selectedSquere, GameObject targetObj, Squere targetSquere)
    {
        //引数をキャッシュ化
        _selectedTileSpriteRenderer = currentSpriteRenderer;
        _selectedPieceObj = selectedPieceObj;
        _selectedPiece = selectedPiece;
        _selectedSquere = selectedSquere;
        _selectedPieceAnimatorController = _selectedPieceObj.GetComponent<Animator>();
        _selectedPieceRuntimeAnimator = _selectedPieceAnimatorController.runtimeAnimatorController;
        _targetSquere = targetSquere;
        _targetObj = targetObj;
        _targetPieceAnimatorController = targetObj.GetComponent<Animator>();
        //移動に伴って_SelectedPieceObjやSquererなどをアップデート → ラムダ候補
        char[] updateName = _selectedPieceObj.name.ToCharArray();
        updateName[2] = (char)('0' + _targetSquere._SquereTilePos.y);
        updateName[4] = (char)('0' + _targetSquere._SquereTilePos.x);
        _selectedPieceObj.name = new string(updateName);
        _selectedSquere._IsOnPieceObj = null;
        _Direction = _targetSquere._SquereTilePos.x - _selectedSquere._SquereTilePos.x;
        //初めて移動した駒であればrotation.zは 0 という勝手な仕様
        if (_selectedPieceObj.transform.rotation.z == 0)
        {
            //OpenSelectableAreaで利用する
            _selectedPieceObj.transform.rotation = Quaternion.Euler(0, 0, 360);
            //ルーク・キングが動いた瞬間に、一部のキャスリングが二度と使用できなくなる
            if (updateName[0] == 'R' || updateName[0] == 'K') //R 若しくは K だった場合
            {
                SquereID id = _selectedSquere._SquereID;
                // キャスリング（targetObjが同じ陣営の駒だった場合）
                if (_selectedPieceObj.CompareTag(_targetSquere._IsOnPieceObj.tag))
                {
                    //キャスリングが可能かどうかを判定した後、該当のAnimation(メソッド)を_castlingAnimationに登録している → ここに到達した時点でどちらかのキャスリングは可能なのだから、片方のboolだけを見て判断しているということだ
                    _castlingAnimation = _inGameManager.IsCastling[0]()? () => StartShortCastlingAnimation() : () => StartLongCastlingAnimation();
                }
                switch (id)
                {
                    //short && R
                    case SquereID.a1:
                    case SquereID.a8:
                        _inGameManager.IsCastling[0] = () => false;
                        break;
                    //long && R
                    case SquereID.h1:
                    case SquereID.h8:
                        _inGameManager.IsCastling[1] = () => false;
                        break;
                    //K
                    case SquereID.d1:
                    case SquereID.d8:
                        _inGameManager.IsCastling[0] = () => false;
                        _inGameManager.IsCastling[1] = () => false;
                        break;
                }
            }
        }
        StartRunAnimation();
        //移動 → 攻撃 → 移動 → Idle
        //移動 → 攻撃 → Idle の２パターンにこの後枝分かれをする
    }
    /// <summary>
    /// "Run"アニメーションを作成し、PlayableGraphで再生する。動作が独立している。
    /// </summary>
    public void StartRunAnimation()
    {
        //knightの時は攻撃の移動に合わせて始点と終点を指定したい
        AnimationCurve animationCurveX = AnimationCurve.Linear(0f, _selectedPieceObj.transform.position.x, 1f, _targetSquere._SquerePiecePosition.x);
        AnimationCurve animationCurveY = AnimationCurve.Linear(0f, _selectedPieceObj.transform.position.y, 1f, _targetSquere._SquerePiecePosition.y);
        float adjustScale = _selectedPieceObj.transform.localScale.y + ((_targetSquere._SquereTilePos.y - _selectedSquere._SquereTilePos.y) * 0.143f);
        AnimationCurve animationCurveSX = AnimationCurve.Linear(0f, _selectedPieceObj.transform.localScale.x, 1f, adjustScale);
        AnimationCurve animationCurveSY = AnimationCurve.Linear(0f, _selectedPieceObj.transform.localScale.y, 1f, adjustScale);
        AnimationCurve animationCurveSZ = AnimationCurve.Linear(0f, _selectedPieceObj.transform.localScale.z, 1f, adjustScale);
        //"Run"という名前のついたanimationClipからコピーを新規作成
        AnimationClip animationClip = _selectedPieceRuntimeAnimator.animationClips.FirstOrDefault(clip => clip.name.Contains("Run"));
        //新しく作成・編集したAnimationCurveをAnimationClipに代入する
        animationClip.SetCurve("", typeof(Transform), "localPosition.x", animationCurveX);
        animationClip.SetCurve("", typeof(Transform), "localPosition.y", animationCurveY);
        //PlayableGraphを作成
        _playableGraph = PlayableGraph.Create();
        //AnimationClipPlayableを作成
        AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(_playableGraph, animationClip);
        //AnimationPlayableOutputを作成してAnimatorと連結
        _animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "AnimOutput", _selectedPieceAnimatorController);
        _animationPlayableOutput.SetSourcePlayable(animationClipPlayable);
        //再生
        _playableGraph.Play();
    }
    /// <summary>
    /// MoveAnimationの再生後にAnimationEventで１回呼ばれる。動作が独立している。
    /// </summary>
    public void StartAttackAnimation() //移動 → 攻撃 の駒はその後も少し移動する必要がある
    {
        //enpassantであれば
        if (_selectedPiece._PieceName == "P"
            &&
            _targetSquere._IsActiveEnpassant)
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Stop();
                _playableGraph.Destroy();
            }
            _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_selectedPieceObj.GetComponent<SpriteRenderer>().flipX; //攻撃するPieceの向いている方向を反転する
            _selectedPieceAnimatorController.Play("P_Attack");
            _targetSquere._IsActiveEnpassant = false;
        }
        else if (_targetSquere._IsOnPieceObj)
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Stop();
                _playableGraph.Destroy();
            }
            string search = $"{_selectedPiece._PieceName}_Attack";
            _selectedPieceAnimatorController.Play(search);
            _targetSquere._IsOnPieceObj = null;
        }
    }
    /// <summary>
    /// SelectedPieceのIdleAnimationを再生する。動作が独立している。TurnDesid.csが処理をしなくなる直前に必ず１回呼び出される。
    /// </summary>
    public void StartIdleAnimation()
    {
        if (_playableGraph.IsValid())
        {
            _playableGraph.Stop();
            _playableGraph.Destroy();
        }
        _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_inGameManager.IsWhite;
        string search = new string($"{_selectedPiece._PieceName}_Idle");
        _selectedPieceAnimatorController.Play(search);
        //ターンを終えた後の処理
        EndTurn();
        // _miniBoard.UpdateMiniBorad();
        //TurnDeside側から通知できること
        //_selectedPieceがどこに動いたのか
        //_誰が倒されたのか（重要）
        //Promotionが起きたかどうか（重要）
        //MiniBoradに通知することで両者の攻撃範囲が自動で取得できる、という構造を創る
    }
    /// <summary>
    /// 敵のTakeHitAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartTakeHitAnimation()
    {
        string search = new string($"{_targetObj.name.First()}_TakeHit");
        _targetPieceAnimatorController.Play(search);
    }
    /// <summary>
    /// 敵のDeathAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartDeathAnimation()
    {
        // _hitStopObj.SetActive(true);
        _targetObj.GetComponent<SpriteRenderer>().flipX = !_selectedPieceObj.GetComponent<SpriteRenderer>().flipX;
        string search = new string($"{_targetObj.name.First()}_Death"); //_targetobjがnull
        _targetPieceAnimatorController.Play(search);
    }

    public void StartStageOutAddForce()
    {
        _targetObj.GetComponent<Collider2D>().enabled = false;
        Rigidbody2D rigidbody2D = _targetObj.GetComponent<Rigidbody2D>();
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        Vector2 duration = _selectedPieceObj.GetComponent<SpriteRenderer>().flipX ? new Vector2(-100, 100): new Vector2(100, 100);
        _targetPieceAnimatorController.enabled = false;
        rigidbody2D.velocity = duration;
        if (_targetObj.tag.Contains(_selectedPieceObj.tag)){ return; }
        int destroyTimer = 2;
        Destroy(_targetObj, destroyTimer);
    }

    public void StartAdjustRigidbody()
    {
        _targetObj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
    }

    public void StartCastlingAnimation()
    {
        _castlingAnimation?.Invoke();
    }
    void StartShortCastlingAnimation()
    {
        if (_inGameManager.IsWhite)
        {
            _selectedPieceAnimatorController.Play("K_ShortCastling_W");
            _targetPieceAnimatorController.Play("R_ShortCastling_W");
        }
        else
        {
            _selectedPieceAnimatorController.Play("K_ShortCastling_B");
            _targetPieceAnimatorController.Play("R_ShortCastling_B");
        }
        _castlingAnimation = null;
    }
    void StartLongCastlingAnimation()
    {
        //long King f1 Rook e1
        if (_inGameManager.IsWhite)
        {
            _selectedPieceAnimatorController.Play("K_LongCastling_W");
            _targetPieceAnimatorController.Play("R_LongCastling_W");
        }
        else
        {
            _selectedPieceAnimatorController.Play("K_LongCastling_B");
            _targetPieceAnimatorController.Play("R_LongCastling_B");
        }
        _castlingAnimation = null;//
    }
    /// <summary>
    /// 適宜、flipXを反転させるAnimation。ほとんどのAnimationClipで再生時にEventとして呼ばれる
    /// /// </summary>
    public void StartAdjustFlipX()
    {
        //向かっていく方向によって決まる → Directionを取得すれば良い → ここの set をして、AnimationEventに割り当てる
        _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
    }

    public void StartAdjustPosition()
    {
        if (_selectedPiece._PieceName == "N")
        {
            Vector3 updatePos = _selectedPieceObj.transform.position;
            updatePos.x += _isDirectionRight? 1.5f : -1.5f;
            _selectedPieceObj.transform.position = updatePos;
        }
    }
    /// <summary>
    /// RotateRockを指定したポジションにセットし、SetActiveをtrueにする
    /// </summary>
    public void StartRAttackEffect()
    {
        //生成場所 → -3.5 + -3.26 → 4.35 + 5.53, X == 0.24 Y == 1.2
        Vector3 basePos = _selectedPieceObj.transform.position;
        Vector3 adjustPos = new Vector3(0.24f, 1.8f, 0);
        if (!_isDirectionRight)
        {
            adjustPos = new Vector3(-0.24f, 1.8f, 0);
            _RAttackEffectObj.GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            _RAttackEffectObj.GetComponent<SpriteRenderer>().flipX = false;
        }
        adjustPos += basePos;
        _RAttackEffectObj.transform.position = adjustPos;
        _RAttackEffectObj.GetComponent<R_AttackEffect>()._targetPos = _targetObj.transform.position;
        _RAttackEffectObj.SetActive(true);
        //SetActiveがtrueになると岩が回転するAnimationが再生される
    }

    public void StartBAttackEffect()
    {
        _BAttackEffectObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
        Vector3 basePos = _targetObj.transform.position;
        if (_BAttackEffectObj.GetComponent<SpriteRenderer>().flipX)
        {
            basePos.x += 0.5f;
        }
        else
        {
            basePos.x -= 0.5f;
        }
        basePos.y += 0.25f;
        _BAttackEffectObj.transform.position = basePos;
        _BAttackEffectObj.transform.localScale = _targetObj.transform.localScale;
        _BAttackEffectObj.SetActive(true);
    }

    public void StartQAttackEffect()
    {
        _QAttckEffectObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
        Vector3 basePos = _targetObj.transform.position;
        if (_QAttckEffectObj.GetComponent<SpriteRenderer>().flipX)
        {
            basePos.x += 0.5f;
        }
        else
        {
            basePos.x -= 0.5f;
        }
        basePos.y += 0.25f;
        _QAttckEffectObj.transform.position = basePos;
        _QAttckEffectObj.transform.localScale = _targetObj.transform.localScale;
        _QAttckEffectObj.SetActive(true);
    }
    void EndTurn()
    {
        _targetSquere._IsOnPieceObj = _selectedPieceObj; //_targetSquereに_selectedPieceObjが到着した
        _uiManager._TargetSquere = _targetSquere;
        _uiManager._TargetPieceObj = _targetSquere._IsOnPieceObj; //ちょっと設計がよくない
        if (_enpassantSquere) {_enpassantSquere._IsActiveEnpassant = false;}
        Destroy(_enpassantObj);
        _openSelectableArea.BeforeRendereringClear();
        //Poneが移動した後にアンパッサン・プロモーションの発生を判断する
        if (_selectedPiece._PieceName == "P")
        {
            //被アンパッサン（状況作成側）の処理
            if (Math.Abs(_selectedSquere._SquereTilePos.x - _targetSquere._SquereTilePos.x) == 2)
            {
                CreateEnpassant();
                //_enpassantObj == true
            }
            else if (_targetSquere._SquereID.ToString().Contains("1, 8"))
            {
                _inGameManager.StartActivePromotionRelay();
                return;
                //プロモーション先の駒をUIで選択したら_inGameManager._IsWhiteを切り替える
            }
        }
        _inGameManager.TrunChange(); //攻守交代
        //次のターンへ
    }
    /// <summary>
    /// 特殊ルール"アンパッサン"のシチュエーションを作成する処理
    /// </summary>
    /// <param name="_IsWhite"></param>
    public void CreateEnpassant()
    {
        string[] search = _selectedPieceObj.name.Split("_"); //P_alphabet_number
        int alphabet = int.Parse(search[1]);
        int enpassantNumber;
        if (_inGameManager.IsWhite)
        {
            //WhitePieceのenpassant座標Xは必然的に[2]である
            enpassantNumber = 2;
            _enpassantSquere = _inGameManager._SquereArrays[alphabet][enpassantNumber];
            _enpassantSquere._IsActiveEnpassant = true; //ここの処理はあっても良さそう
            //enpassantObjの生成
            _enpassantObj = Instantiate(_selectedPieceObj);
            //ennpassantObjの名前をポジションと同一にする
            _enpassantObj.name = new string($"{search[0]}_{alphabet}_{enpassantNumber}");
            _enpassantSquere._IsOnPieceObj = _enpassantObj;//
        }
        else
        {
            //WhitePieceのenpassant座標Xは必然的に[5]である
            enpassantNumber = 5;
            _enpassantSquere = _inGameManager._SquereArrays[alphabet][enpassantNumber];
            _enpassantSquere._IsActiveEnpassant = true;
            _enpassantObj = Instantiate(_selectedPieceObj);
            _enpassantObj.name = new string($"{search[0]}_{alphabet}_{enpassantNumber}");
            _enpassantSquere._IsOnPieceObj = _enpassantObj;//
        }
    }
}
