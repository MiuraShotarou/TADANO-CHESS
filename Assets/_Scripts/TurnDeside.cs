using System;
using System.Collections.Generic;
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
        _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
        //knightの時は攻撃の移動に合わせて始点と終点を指定したい
        AnimationCurve animationCurveX = AnimationCurve.Linear(0f, _selectedPieceObj.transform.position.x, 1f, _targetSquere._SquerePiecePosition.x);
        AnimationCurve animationCurveY = AnimationCurve.Linear(0f, _selectedPieceObj.transform.position.y, 1f, _targetSquere._SquerePiecePosition.y);
        float adjustScale = _selectedPieceObj.transform.localScale.y + (_selectedSquere._SquereTilePos.y - _targetSquere._SquereTilePos.y) * 0.143f;
        AnimationCurve animationCurveSX = AnimationCurve.Linear(0f, _selectedPieceObj.transform.localScale.x, 1f, adjustScale);
        AnimationCurve animationCurveSY = AnimationCurve.Linear(0f, _selectedPieceObj.transform.localScale.y, 1f, adjustScale);
        AnimationCurve animationCurveSZ = AnimationCurve.Linear(0f, _selectedPieceObj.transform.localScale.z, 1f, adjustScale);
        //"Run"という名前のついたanimationClipからコピーを新規作成
        AnimationClip animationClip = _selectedPieceRuntimeAnimator.animationClips.FirstOrDefault(clip => clip.name.Contains("Run"));
        //新しく作成・編集したAnimationCurveをAnimationClipに代入する
        animationClip.SetCurve("", typeof(Transform), "localPosition.x", animationCurveX);
        animationClip.SetCurve("", typeof(Transform), "localPosition.y", animationCurveY);
        animationClip.SetCurve("", typeof(Transform), "localScale.x", animationCurveSX);
        animationClip.SetCurve("", typeof(Transform), "localScale.y", animationCurveSY);
        animationClip.SetCurve("", typeof(Transform), "localScale.z", animationCurveSZ);
        //PlayableGraphを作成
        _selectedPlayableGraph = PlayableGraph.Create();
        //AnimationClipPlayableを作成
        AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(_selectedPlayableGraph, animationClip);
        //AnimationPlayableOutputを作成してAnimatorと連結
        _animationPlayableOutput = AnimationPlayableOutput.Create(_selectedPlayableGraph, "AnimOutput", _selectedPieceAnimatorController);
        _animationPlayableOutput.SetSourcePlayable(animationClipPlayable);
        //再生
        _selectedPlayableGraph.Play();
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
            Time.timeScale = 1;
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
        float phaseOneCurveXStart = 0;
        float phaseOneCurveXEnd = 0;
        float phaseOneCurveYStart = 1.17f;
        float phaseOneCurveYEnd = 11.17f;
        float phaseOneCurveRXStart = 0f;
        float phaseOneCurveRXEnd = -0.066f;
        float phaseTwoCurveYStart = 0;
        float phaseTwoCurveYEnd = 0;
        float phaseOneScalePoint = 0;
        string search = "";
        switch (id)
        {
            case SquereID.a1:
                phaseOneCurveXStart = -4.42f;
                phaseOneCurveXEnd = -5.15f;
                phaseOneScalePoint = 3.358f;
                phaseTwoCurveYStart = 8.8f;
                phaseTwoCurveYEnd = -1.2f;
                search = "K_ShortCastling_W";
                break;
            case SquereID.a8:
                phaseOneCurveXStart = 4.33f;
                phaseOneCurveXEnd = 5.05f;
                phaseOneScalePoint = 3.358f;
                phaseTwoCurveYStart = 8.8f;
                phaseTwoCurveYEnd = -1.2f;
                search = "K_ShortCastling_B";
                break;
            case SquereID.h1:
                phaseOneCurveXStart = -4.42f;
                phaseOneCurveXEnd = -3.9f;
                phaseOneScalePoint = 2.786f;
                phaseTwoCurveYStart = 13.07f;
                phaseTwoCurveYEnd = 3.07f;
                search = "K_LongCastling_W";
                break;
            case SquereID.h8:
                phaseOneCurveXStart = 4.33f;
                phaseOneCurveXEnd = 3.76f;
                phaseOneScalePoint = 2.929f;
                phaseTwoCurveYStart = 13.07f;
                phaseTwoCurveYEnd = 3.07f;
                search = "K_LongCastling_B";
                break;
        }
        float phaseOneStartTime = 2f;
        float phaseOneEndTime = 2.25f;
        float phaseTwoStartTime = 6.83f;
        float phaseTwoEndTime = 7f;
        AnimationCurve curveY = AnimationCurve.Linear(phaseOneStartTime, phaseOneCurveYStart, phaseOneEndTime, phaseOneCurveYEnd);
        AnimationCurve curveRX = AnimationCurve.Constant(phaseOneStartTime, phaseOneStartTime, phaseOneCurveRXStart);
        AnimationCurve curveX = AnimationCurve.Linear(0f, phaseOneCurveXStart, phaseOneStartTime, phaseOneCurveXStart);
        AnimationCurve curveSX = AnimationCurve.Constant(phaseTwoStartTime, phaseTwoStartTime, phaseOneScalePoint);
        AnimationCurve curveSY = AnimationCurve.Constant(phaseTwoStartTime, phaseTwoStartTime, phaseOneScalePoint);
        AnimationCurve curveSZ = AnimationCurve.Constant(phaseTwoStartTime, phaseTwoStartTime, phaseOneScalePoint);
        curveY.AddKey(phaseTwoStartTime, phaseTwoCurveYStart);
        curveY.AddKey(phaseTwoEndTime, phaseTwoCurveYEnd);
        curveX.AddKey(phaseTwoStartTime, phaseOneCurveXEnd);
        //"Run"という名前のついたanimationClipからコピーを新規作成
        AnimationClip animationClip = _selectedPieceAnimatorController.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name.Contains(search));
        //新しく作成・編集したAnimationCurveをAnimationClipに代入する
        animationClip.SetCurve("", typeof(Transform), "localPosition.x", curveX);
        animationClip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
        animationClip.SetCurve("", typeof(Transform), "localScale.x", curveSX);
        animationClip.SetCurve("", typeof(Transform), "localScale.y", curveSY);
        animationClip.SetCurve("", typeof(Transform), "localScale.z", curveSZ);
        animationClip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.x", curveRX);
        //PlayableGraphを作成
        _selectedPlayableGraph = PlayableGraph.Create();
        //AnimationClipPlayableを作成
        AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(_selectedPlayableGraph, animationClip);
        //AnimationPlayableOutputを作成してAnimatorと連結
        _animationPlayableOutput = AnimationPlayableOutput.Create(_selectedPlayableGraph, "AnimOutput", _selectedPieceAnimatorController);
        _animationPlayableOutput.SetSourcePlayable(animationClipPlayable);
        //再生
        _selectedPlayableGraph.Play();
    }
    void StartRCastlingAnimation(SquereID id)
    {
        float phaseOneCurveXStart = 0;
        float phaseOneCurveXEnd = 0;
        float phaseOneAllCurveY = 0;
        float phaseOneAllScale = 0;
        float phaseTwoCurveXStart = 0;
        float phaseTwoCurveXEnd = 0;
        float phaseTwoAllCurveY = 0;
        float phaseTwoAllScale = 0;
        string search = "";
        switch (id)
        {
            case SquereID.a1:
                phaseOneCurveXStart = -5.6f;
                phaseOneCurveXEnd = -15.6f;
                phaseOneAllCurveY = -2.7f;
                phaseOneAllScale = 3.501f;
                phaseTwoCurveXStart = -14.75f;
                phaseTwoCurveXEnd = -4.75f;
                phaseTwoAllCurveY = 0.1f;
                phaseTwoAllScale = 3.215f;
                search = "R_ShortCastling_W";
                break;
            case SquereID.a8:
                phaseOneCurveXStart = 5.5f;
                phaseOneCurveXEnd = 15.5f;
                phaseOneAllCurveY = -2.7f;
                phaseOneAllScale = 3.501f;
                phaseTwoCurveXStart = 14.65f;
                phaseTwoCurveXEnd = 4.65f;
                phaseTwoAllCurveY = 0.1f;
                phaseTwoAllScale = 3.215f;
                search = "R_ShortCastling_B";
                break;
            case SquereID.h1:
                phaseOneCurveXStart = -3.5f;
                phaseOneCurveXEnd = -13.5f;
                phaseOneAllCurveY = 4.35f;
                phaseOneAllScale = 2.5f;
                phaseTwoCurveXStart = -14.1f;
                phaseTwoCurveXEnd = -4.1f;
                phaseTwoAllCurveY = 2.1f;
                phaseTwoAllScale = 2.929f;
                search = "R_LongCastling_W";
                break;
            case SquereID.h8:
                phaseOneCurveXStart = 3.42f;
                phaseOneCurveXEnd = 13.42f;
                phaseOneAllCurveY = 4.35f;
                phaseOneAllScale = 2.5f;
                phaseTwoCurveXStart = 14.06f;
                phaseTwoCurveXEnd = 4.06f;
                phaseTwoAllCurveY = 2.1f;
                phaseTwoAllScale = 2.929f;
                search = "R_LongCastling_B";
                break;
        }
        float phaseOneStartTime = 4.6f;
        float phaseOneEndTime = 6.92f;
        float phaseTwoStartTime = 10f;
        float phaseTwoEndTime = 12.5f;
        AnimationCurve curveX = AnimationCurve.Linear(phaseOneStartTime, phaseOneCurveXStart, phaseOneEndTime, phaseOneCurveXEnd);
        AnimationCurve curveY = AnimationCurve.Linear(phaseOneStartTime, phaseOneAllCurveY, phaseOneEndTime, phaseOneAllCurveY);
        AnimationCurve curveSX = AnimationCurve.Linear(phaseOneStartTime, phaseOneAllScale, phaseOneEndTime, phaseOneAllScale);
        AnimationCurve curveSY = AnimationCurve.Linear(phaseOneStartTime, phaseOneAllScale, phaseOneEndTime, phaseOneAllScale);
        AnimationCurve curveSZ = AnimationCurve.Linear(phaseOneStartTime, phaseOneAllScale, phaseOneEndTime, phaseOneAllScale);
        curveX.AddKey(phaseTwoStartTime, phaseTwoCurveXStart);
        curveX.AddKey(phaseTwoEndTime, phaseTwoCurveXEnd);
        curveY.AddKey(phaseTwoStartTime, phaseTwoAllCurveY);
        curveSX.AddKey(phaseTwoStartTime, phaseTwoAllScale);
        curveSY.AddKey(phaseTwoStartTime, phaseTwoAllScale);
        curveSZ.AddKey(phaseTwoStartTime, phaseTwoAllScale);
        //"Run"という名前のついたanimationClipからコピーを新規作成
        AnimationClip animationClip = _targetPieceAnimatorController.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name.Contains(search));
        //新しく作成・編集したAnimationCurveをAnimationClipに代入する
        animationClip.SetCurve("", typeof(Transform), "localPosition.x", curveX);
        animationClip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
        animationClip.SetCurve("", typeof(Transform), "localScale.x", curveSX);
        animationClip.SetCurve("", typeof(Transform), "localScale.y", curveSY);
        animationClip.SetCurve("", typeof(Transform), "localScale.z", curveSZ);
        //PlayableGraphを作成
        _targetPlayableGraph = PlayableGraph.Create();
        //AnimationClipPlayableを作成
        AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(_targetPlayableGraph, animationClip);
        //AnimationPlayableOutputを作成してAnimatorと連結
        _animationPlayableOutput = AnimationPlayableOutput.Create(_targetPlayableGraph, "AnimOutput", _targetPieceAnimatorController);
        _animationPlayableOutput.SetSourcePlayable(animationClipPlayable);
        //再生
        _targetPlayableGraph.Play();
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
                case SquereID.a1:
                    kingPosID = SquereID.b1;
                    rookPosID = SquereID.c1;
                    break;
                case SquereID.a8:
                    kingPosID = SquereID.b8;
                    rookPosID = SquereID.c8;
                    break;
                case SquereID.h1:
                    kingPosID = SquereID.f1;
                    rookPosID = SquereID.e1;
                    break;
                case SquereID.h8:
                    kingPosID = SquereID.f8;
                    rookPosID = SquereID.e8;
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
                Debug.Log("");
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
        enpassantObj.name = new string($"P_{alphabet}_{enpassantNumber}_{search[3]}_{search[4]}");
        enpassantObj.transform.SetParent(_selectedPieceObj.transform);
        _enpassantSquere._IsOnPieceObj = enpassantObj;//
    }
}
