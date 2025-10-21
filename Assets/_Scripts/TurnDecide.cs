using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

//Colliderのデストロイタイミング
/// <summary>
/// 駒が目標地点まで移動していくまでの処理を実装するクラス
/// </summary>
public class TurnDecide : ColorPallet
{
    InGameManager _inGameManager;
    OpenSelectableArea _openSelectableArea;
    UIManager _uiManager;
    GameObject _selectedPieceObj;
    Piece _selectedPiece;
    Squere _selectedSquere;
    Squere _targetSquere;
    Squere _enpassantSquere;
    Squere _castlingRookSquere;
    Animator _selectedPieceAnimatorController;
    Animator _targetPieceAnimatorController;
    RuntimeAnimatorController _selectedPieceRuntimeAnimator;
    AnimationCurve _endPositionCurve;
    GameObject _RAttackEffectObj;
    GameObject _BAttackEffectObj;
    GameObject _QAttackEffectObj;
    GameObject _targetObj;
    GameObject _enpassantObj;
    // Action _castlingAnimation;
    bool _isCastling;
    // GameObject _hitStopObj;
    PlayableGraph _selectedPlayableGraph;
    PlayableGraph _targetPlayableGraph;
    AnimationPlayableOutput _animationPlayableOutput;
    bool _isDirectionRight;
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _uiManager = GetComponent<UIManager>();
        _RAttackEffectObj = transform.GetChild(0).gameObject;
        _BAttackEffectObj = transform.GetChild(1).gameObject;
        _QAttackEffectObj = transform.GetChild(2).gameObject;
        _isCastling = false;
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
    public void StartTurnDeside(GameObject selectedPieceObj, Piece selectedPiece, Squere selectedSquere, Squere targetSquere)
    {
        //引数をキャッシュ化
        _selectedPieceObj = selectedPieceObj;
        _selectedPiece = selectedPiece;
        _selectedSquere = selectedSquere;
        _selectedPieceAnimatorController = _selectedPieceObj.GetComponent<Animator>();
        _selectedPieceRuntimeAnimator = _selectedPieceAnimatorController.runtimeAnimatorController;
        _targetSquere = targetSquere;
        if (targetSquere._IsOnPieceObj)
        {
            //enpassantObj を検知したならそのオブジェクトの親オブジェクトを取得する
            if (targetSquere._IsActiveEnpassant
                &&
                "P".Contains(_selectedPieceObj.name.First().ToString()))
            {
                _targetObj = targetSquere._IsOnPieceObj.transform.parent.gameObject;
            }
            else
            {
                _targetObj = targetSquere._IsOnPieceObj;
            }
            _targetPieceAnimatorController = _targetObj.GetComponent<Animator>();
        }
        //移動に伴って_SelectedPieceObjやSquererなどをアップデート → ラムダ候補
        char[] updateName = _selectedPieceObj.name.ToCharArray();
        updateName[2] = (char)('0' + _targetSquere._SquereTilePos.y);
        updateName[4] = (char)('0' + _targetSquere._SquereTilePos.x);
        _selectedPieceObj.name = new string(updateName);
        _selectedSquere._IsOnPieceObj = null;
        _isDirectionRight = _selectedSquere._SquereTilePos.x < _targetSquere._SquereTilePos.x;
        //初めて移動した駒であればrotation.zは 0 という勝手な仕様
        if (_selectedPieceObj.transform.rotation.z == 0)
        {
            //OpenSelectableAreaで利用する
            _selectedPieceObj.transform.rotation = Quaternion.Euler(0, 0, 360);
            //ルーク・キングが動いた瞬間に、一部のキャスリングが二度と使用できなくなる
            if ("R,K".Contains(updateName[0].ToString())) //R 若しくは K だった場合
            {
                SquereID id = _selectedSquere._SquereID;
                // キャスリング（targetObjが同じ陣営の駒だった場合）
                if (_targetObj
                &&
                    _selectedPieceObj.CompareTag(_targetObj.tag))
                {
                    _isCastling = true;
                }
                switch (id)
                {
                    //short && R
                    case SquereID.a1:
                    case SquereID.a8:
                        _inGameManager.IsCastling[1] = () => false;
                        break;
                    //long && R
                    case SquereID.h1:
                    case SquereID.h8:
                        _inGameManager.IsCastling[0] = () => false;
                        break;
                    //K
                    case SquereID.e1:
                    case SquereID.e8:
                        _inGameManager.IsCastling[0] = () => false;
                        _inGameManager.IsCastling[1] = () => false;
                        break;
                }
            }
        }
        StartRunAnimation();
        // StartCoroutine(StartRunAnimation());
        //移動 → 攻撃 → 移動 → Idle
        //移動 → 攻撃 → Idle の２パターンにこの後枝分かれをする
    }
    /// <summary>
    /// "Run"アニメーションを作成し、PlayableGraphで再生する。動作が独立している。
    /// </summary>
    public void StartRunAnimation()
    {
        // スプライトの反転
        _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
        // 目標位置とスケール
        Vector3 targetPosition = _targetSquere._SquerePiecePosition;
        float adjustScale = _selectedPieceObj.transform.localScale.y + 
                            (_targetSquere._SquereTilePos.y - _selectedSquere._SquereTilePos.y) * 0.143f;
        Vector3 targetScale = new Vector3(adjustScale, adjustScale, adjustScale);
        // アニメーション時間（元のAnimationClipの長さに合わせる）
        float duration = 1f;  // 必要に応じて調整
        // ✅ DOTweenで移動とスケール変更を同時実行
        // 位置のアニメーション
        _selectedPieceObj.transform.DOMove(targetPosition, duration)
            .SetEase(Ease.Linear);
        // スケールのアニメーション
        _selectedPieceObj.transform.DOScale(targetScale, duration)
            .SetEase(Ease.Linear);
        // アニメーション完了時のコールバック（必要に応じて）
        _selectedPieceAnimatorController.Play($"{_selectedPiece._PieceName}_Run");
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
            if (_selectedPlayableGraph.IsValid())
            {
                _selectedPlayableGraph.Stop();
                _selectedPlayableGraph.Destroy();
            }
            DOTween.KillAll();
            _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_selectedPieceObj.GetComponent<SpriteRenderer>().flipX; //攻撃するPieceの向いている方向を反転する
            _selectedPieceAnimatorController.Play("P_Attack");
            _targetSquere._IsActiveEnpassant = false;
            _enpassantSquere = null;
        }
        else if (_targetSquere._IsOnPieceObj)
        {
            if (_selectedPlayableGraph.IsValid())
            {
                _selectedPlayableGraph.Stop();
                _selectedPlayableGraph.Destroy();
            }
            DOTween.KillAll();
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
        if (_selectedPlayableGraph.IsValid())
        {
            _selectedPlayableGraph.Stop();
            _selectedPlayableGraph.Destroy();
        }
        _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_inGameManager.IsWhite;
        string search = new string($"{_selectedPiece._PieceName}_Idle");
        _selectedPieceAnimatorController.Play(search);
        //ターンを終えた後の処理
        EndTurn();
    }
    /// <summary>
    /// 敵のTakeHitAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartTakeHitAnimation()
    {
        if (_selectedPlayableGraph.IsValid())//
        {
            _selectedPlayableGraph.Stop();
            _selectedPlayableGraph.Destroy();
        }
        string search = new string($"{_targetObj.name.First()}_TakeHit");
        _targetPieceAnimatorController.Play(search);
    }
    /// <summary>
    /// 敵のDeathAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartDeathAnimation()
    {
        _targetObj.GetComponent<SpriteRenderer>().flipX = !_selectedPieceObj.GetComponent<SpriteRenderer>().flipX;
        string search = new string($"{_targetObj.name.First()}_Death"); //_targetobjがnull
        _targetPieceAnimatorController.Play(search);
    }

    public void StartStageOutAddForce()
    {
        _targetObj.GetComponent<Collider2D>().enabled = false;
        Rigidbody2D rigidbody2D = _targetObj.GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        rigidbody2D.velocity = Vector2.zero;
        Vector2 duration = _selectedPieceObj.GetComponent<SpriteRenderer>().flipX ? new Vector2(-100, 100): new Vector2(100, 100);
        _targetPieceAnimatorController.enabled = false;
        rigidbody2D.velocity = duration;
        //Castlingのルークがターゲットであれば
        if (_targetObj.tag.Contains(_selectedPieceObj.tag))
        {
            return;
        }
        GameObject copyObj = new GameObject(_targetObj.name);
        _uiManager._DeathPieceObj = copyObj;
        int destroyTimer = 2;
        Destroy(_targetObj, destroyTimer);
    }
    
    public void StartCastlingAnimation()
    {
        if (_isCastling)
        {
#if UNITY_EDITOR
            Time.timeScale = 2;
#endif
            if (_selectedPlayableGraph.IsValid())
            {
                _selectedPlayableGraph.Stop();
                _selectedPlayableGraph.Destroy();
            }
            SquereID id = _targetSquere._SquereID;
            StartKCastlingAnimation(id);
            StartRCastlingAnimation(id);
        }
    }

    void StartKCastlingAnimation(SquereID id)
    {
        DOTween.KillAll();
        // 初期値の設定
        float targetScale = 0;
        Vector3 targetPos = Vector3.zero;
        string search = "";
        switch (id)
        {
            case SquereID.h1:
                targetPos = new Vector3(-5.15f, -1.2f, 0);
                targetScale = 3.358f;
                search = "K_ShortCastling_W";
                break;
            case SquereID.h8:
                targetPos = new Vector3(5.05f, -1.2f, 0);
                targetScale = 3.358f;
                search = "K_ShortCastling_B";
                break;
            case SquereID.a1:
                targetPos = new Vector3(-3.9f, 3.07f, 0);
                targetScale = 2.786f;
                search = "K_LongCastling_W";
                break;
            case SquereID.a8:
                targetPos = new Vector3(3.76f, 3.07f, 0);
                targetScale = 2.786f;
                search = "K_LongCastling_B";
                break;
        }
        float duration = 1f; //AnimationClipの長さに応じて変更する
        _selectedPieceObj.transform.DOMove(targetPos, duration)
            .SetEase(Ease.Linear);
        _selectedPieceObj.transform.DOScale(targetScale, duration)
            .SetEase(Ease.Linear);
        _selectedPieceAnimatorController.Play(search);
    }
    void StartRCastlingAnimation(SquereID id)
    {
        // パラメータ設定
        float relayPosX = 0;
        float targetPosX = 0;
        float targetPosY = 0;
        float targetScale = 0;
        string animationName = "";

        switch (id)
        {
            case SquereID.h1:
                relayPosX = -15.6f;
                targetPosX = -4.75f;
                targetPosY = 0.1f;
                targetScale = 3.215f;
                animationName = "R_ShortCastling_W";
                break;
            case SquereID.h8:
                relayPosX = 15.5f;
                targetPosX = 4.65f;
                targetPosY = 0.1f;
                targetScale = 3.215f;
                animationName = "R_ShortCastling_B";
                break;
            case SquereID.a1:
                relayPosX = -13.5f;
                targetPosX = -4.1f;
                targetPosY = 2.1f;
                targetScale = 2.929f;
                animationName = "R_LongCastling_W";
                break;
            case SquereID.a8:
                relayPosX = 13.42f;
                targetPosX = 4.06f;
                targetPosY = 2.1f;
                targetScale = 2.929f;
                animationName = "R_LongCastling_B";
                break;
        }

        // タイミング設定
        float startTime = 0f;
        float reralyTime = 0.5f;
        float intervalTime = 0.5f;
        float endTime = 1f;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_targetObj.transform.DOMoveX(relayPosX, reralyTime).SetEase(Ease.Linear));
        sequence.AppendInterval(intervalTime);
        sequence.Append(_targetObj.transform.DOMoveX(targetPosX, endTime).SetEase(Ease.Linear));
        sequence.Join(_targetObj.transform.DOMoveY(targetPosY, 0).SetEase(Ease.Linear));
        sequence.Join(_targetObj.transform.DOScale(targetScale, 0).SetEase(Ease.Linear));
        // 再生
        sequence.Play();
        _targetPieceAnimatorController.Play(animationName);
    }
    public void StartInactiveTargetPlayable()
    {
        if (_targetPlayableGraph.IsValid())
        {
            _targetPlayableGraph.Stop();
            _targetPlayableGraph.Destroy();
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
        _RAttackEffectObj.GetComponent<SpriteRenderer>().color = _inGameManager.IsWhite? Color.white : Color.black;
        _RAttackEffectObj.SetActive(true);
        //SetActiveがtrueになると岩が回転するAnimationが再生される
    }

    public void StartBAttackEffect()
    {
        _BAttackEffectObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
        _BAttackEffectObj.GetComponent<SpriteRenderer>().color = _inGameManager.IsWhite? ChangeAlpha(Color.white, 0) : ChangeAlpha(Color.black, 0);
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
        _QAttackEffectObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
        _QAttackEffectObj.GetComponent<SpriteRenderer>().color = _inGameManager.IsWhite? ChangeAlpha(Color.white, 0) : ChangeAlpha(Color.black, 0);
        Vector3 basePos = _targetObj.transform.position;
        if (_QAttackEffectObj.GetComponent<SpriteRenderer>().flipX)
        {
            basePos.x += 0.5f;
        }
        else
        {
            basePos.x -= 0.5f;
        }
        basePos.y += 0.25f;
        _QAttackEffectObj.transform.position = basePos;
        _QAttackEffectObj.transform.localScale = _targetObj.transform.localScale;
        _QAttackEffectObj.SetActive(true);
    }
    void EndTurn()
    {
        if (_enpassantSquere) //nullチェックしたくないからこの条件で判断している
        {
            _enpassantSquere._IsActiveEnpassant = false;
            Destroy(_enpassantSquere._IsOnPieceObj);
            _enpassantSquere = null;
        }
        if (_isCastling)  //falseの上書き忘れない
        {
            _targetSquere._IsOnPieceObj = null;
            SquereID kingPosID = SquereID.a1;
            SquereID rookPosID = SquereID.a1;
            SquereID id = _targetSquere._SquereID;
            switch (id)
            {
                case SquereID.h1:
                    kingPosID = SquereID.g1;
                    rookPosID = SquereID.f1;
                    break;
                case SquereID.h8:
                    kingPosID = SquereID.g8;
                    rookPosID = SquereID.f8;
                    break;
                case SquereID.a1:
                    kingPosID = SquereID.c1;
                    rookPosID = SquereID.d1;
                    break;
                case SquereID.a8:
                    kingPosID = SquereID.c8;
                    rookPosID = SquereID.d8;
                    break;
            }
            _targetSquere = _inGameManager._SquereArrays[(int)kingPosID / 8][(int)kingPosID % 8];
            _castlingRookSquere = _inGameManager._SquereArrays[(int)rookPosID / 8][(int)rookPosID % 8];
            _targetSquere._IsOnPieceObj = _selectedPieceObj;
            _castlingRookSquere._IsOnPieceObj = _targetObj;
            char[] updateKingName = _selectedPieceObj.name.ToCharArray();
            updateKingName[2] = (char)('0' + (int)kingPosID / 8);
            updateKingName[4] = (char)('0' + (int)kingPosID % 8);
            _selectedPieceObj.name = new string(updateKingName);
            char[] updateRookName = _targetObj.name.ToCharArray();
            updateRookName[2] = (char)('0' + (int)rookPosID / 8);
            updateRookName[4] = (char)('0' + (int)rookPosID % 8);
            _targetObj.name = new string(updateRookName);
            _targetObj.GetComponent<SpriteRenderer>().color = ChangeAlpha(_targetObj.GetComponent<SpriteRenderer>().color, 150);
            _isCastling = false;
        }
        else
        {
            _targetSquere._IsOnPieceObj = _selectedPieceObj; //_targetSquereに_selectedPieceObjが到着した
        }
        _uiManager._TargetSquere = _targetSquere;
        _uiManager._TargetPieceObj = _targetSquere._IsOnPieceObj; //ちょっと設計がよくない → そもそもGameObjectを入れたくない
        if (_castlingRookSquere)
        {
            _uiManager._CastlingRookSquere = _castlingRookSquere;
            _castlingRookSquere = null;
        }
        _openSelectableArea.BeforeRendereringClear();
        //Poneが移動した後にアンパッサン・プロモーションの発生を判断する
        if ("P".Contains(_selectedPiece._PieceName))
        {
            //被アンパッサン（状況作成側）の処理
            if (Math.Abs(_selectedSquere._SquereTilePos.x - _targetSquere._SquereTilePos.x) == 2)
            {
                CreateEnpassant();
                //enpassantObj == true
            }
            //プロモーションの処理
            else if ("1, 8".Contains(_targetSquere._SquereID.ToString()[1]))
            {
                _inGameManager.StartActivePromotionRelay();
                return;
                //プロモーション先の駒をUIで選択したら_inGameManager._IsWhiteを切り替える
            }
        }
        _inGameManager.TurnChange(); //攻守交代
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
        }
        else
        {
            //WhitePieceのenpassant座標Xは必然的に[5]である
            enpassantNumber = 5;
        }
        _enpassantSquere = _inGameManager._SquereArrays[alphabet][enpassantNumber];
        _enpassantSquere._IsActiveEnpassant = true;
        //enpassantObjの生成
        GameObject enpassantObj = new GameObject();// EmptyObj
        //ennpassantObjの名前をポジションと同一にする
        enpassantObj.name = new string($"E_{alphabet}_{enpassantNumber}_{search[3]}_{search[4]}");
        enpassantObj.transform.SetParent(_selectedPieceObj.transform);
        _enpassantSquere._IsOnPieceObj = enpassantObj;//
    }
}
