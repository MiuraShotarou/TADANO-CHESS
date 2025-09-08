using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Playables;
using UnityEngine.Animations;

/// <summary>
/// 駒が目標地点まで移動していく処理を実装するクラス
/// </summary>
public class TurnDeside : ColorPallet
{
    InGameManager _inGameManager;
    OpenSelectableArea _openSelectableArea; //いらないかも
    CollisionEvent _collisionEvent; //いらない
    SpriteRenderer _selectedSpriteRenderer; //移動後の透明化用
    GameObject _selectedPieceObj;
    Piece _selectedPiece;
    Squere _selectedSquere;
    Squere _targetSquere;
    Animator _selectedPieceAnimatorController;
    Animator _targetPieceAnimatorController; //念入り
    RuntimeAnimatorController _selectedPieceRuntimeAnimator;
    AnimationCurve _endPositionCurve;
    GameObject _collider2DPrefab;
    GameObject _targetObj;
    PlayableGraph _playableGraph;
    AnimationPlayableOutput _animationPlayableOutput;
    bool _temporaryIsEncount;
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _collider2DPrefab = _inGameManager._Collider2DPrefab;
        _collisionEvent = _collider2DPrefab.GetComponent<CollisionEvent>();
    }
    /// <summary>
    /// _SelectedTileSprite上でマウスクリックされた時に一度だけ呼び出される
    /// </summary>
    /// <param name="currentSpriteRenderer"></param>
    /// <param name="selectedPieceObj"></param>
    /// <param name="selectedPiece"></param>
    /// <param name="selectedSquere"></param>
    /// <param name="targetSquere"></param>
    public void StartTurnDeside(SpriteRenderer currentSpriteRenderer, GameObject selectedPieceObj, Piece selectedPiece, Squere selectedSquere, Squere targetSquere)
    {
        //引数をキャッシュ化
        _selectedSpriteRenderer = currentSpriteRenderer;
        _selectedPieceObj = selectedPieceObj;
        _selectedPiece = selectedPiece;
        _selectedSquere = selectedSquere;
        _targetSquere = targetSquere;
        _selectedPieceAnimatorController = _selectedPieceObj.GetComponent<Animator>();
        _selectedPieceRuntimeAnimator = _selectedPieceAnimatorController.runtimeAnimatorController;
        //移動に伴って_SelectedPieceObjやSquererなどをアップデート
        char[] updateName = _selectedPieceObj.name.ToCharArray();
        updateName[2] = (char)('0' + _targetSquere._SquereTilePos.y);
        updateName[4] = (char)('0' + _targetSquere._SquereTilePos.x);
        _selectedPieceObj.name = new string(updateName);
        if (_selectedPieceObj.transform.rotation.z == 0)
        {
            _selectedPieceObj.transform.rotation = Quaternion.Euler(0, 0, 360);
        }
        _selectedSquere._IsOnPiece = false;
        if (_targetSquere._IsOnPiece)
        {
            //移動先に敵駒がある場合の処理
            //ここのタイミングで呼ぶのは違う
            _temporaryIsEncount = true;
            CollisionEvent.CollisionAction = RegisterTarget;
            Instantiate(_collider2DPrefab, _targetSquere._SquerePiecePosition, Quaternion.identity);
            // DeathAnimationRelay(); //ここじゃなくて良いかも
        }
        else
        {
            _temporaryIsEncount = false; //初期化を処理の後ろにするのはあり
        }
        _targetSquere._IsOnPiece = true;
        if (_selectedPiece._IsAttackFirst)
        {
            StartAttackAnimation();
        }
        else
        {
            StartRunAnimation();
        }
        //攻撃 → 移動 → Idle
        //移動 → 攻撃 → Idle の２パターンに分けなければならない
    }
    /// <summary>
    /// CollisionEvent.csからの衝突情報で移動先にあるGameObjectを取得する
    /// </summary>
    void RegisterTarget(GameObject collisionObj)
    {
        _targetObj = collisionObj;
        _targetPieceAnimatorController = collisionObj.GetComponent<Animator>();
    }
    /// <summary>
    /// "Run"アニメーションを作成し、PlayableGraphで再生する。動作が独立している。
    /// </summary>
    public void StartRunAnimation()
    {
        AnimationCurve animationCurveX = AnimationCurve.Linear(0f, _selectedPieceObj.transform.position.x, 1f, _targetSquere._SquerePiecePosition.x);
        AnimationCurve animationCurveY = AnimationCurve.Linear(0f, _selectedPieceObj.transform.position.y, 1f, _targetSquere._SquerePiecePosition.y);
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
        //AnimatorOverrideControllerを使用して差し替え
        // AnimatorOverrideController overrideController = new AnimatorOverrideController(_selectedPieceAnimatorController.runtimeAnimatorController);
        // overrideController["Run"] = animationClip; // 複製したclipに差し替え
        // _selectedPieceAnimatorController.runtimeAnimatorController = overrideController;
        //再生
        _playableGraph.Play();  //途中再生と、一から再生の２パターンある
    }
    /// <summary>
    /// MoveAnimationの再生後にAnimationEventで１回呼ばれる。動作が独立している。
    /// </summary>
    public void StartAttackAnimation() //移動 → 攻撃 の駒はその後も少し移動する必要がある
    {
        if (_temporaryIsEncount)
        {
            _playableGraph.Stop();
            _playableGraph.Destroy();
            string search = new string($"{_selectedPiece._PieceName}_Attack");
            _selectedPieceAnimatorController.Play(search);
            _temporaryIsEncount = false;
        }
    }
    /// <summary>
    /// SelectedPieceのIdleAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartIdleAnimation()
    {
        _playableGraph.Stop();
        _playableGraph.Destroy();
        string search = new string($"{_selectedPiece._PieceName}_Idle");
        _selectedPieceAnimatorController.Play(search);
    }
    /// <summary>
    /// 敵のTakeHitAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartTakeHitAnimation()
    {
        //TakeHitアニメーションを作成すること
        string search = new string($"{_targetObj.name.First()}_TakeHit");
        _targetPieceAnimatorController.Play(search);
    }
    /// <summary>
    /// 敵のDeathAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartDeathAnimation()
    {
        string search = new string($"{_targetObj.name.First()}_Death");
        _targetPieceAnimatorController.Play(search);
    }

    public void StartStageOutAddForce()
    {
        _targetObj.GetComponent<BoxCollider2D>().enabled = false;
        Rigidbody2D rigidbody2D = _targetObj.GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        Vector2 duration = new Vector2(0, 0);
        if (_targetObj.GetComponent<SpriteRenderer>().flipX)
        {
            duration = new Vector2(100, 100);
        }
        else
        {
            duration = new Vector2(-100, 100);
        }
        _targetPieceAnimatorController.enabled = false;
        rigidbody2D.velocity = duration;
        int destroyTimer = 3;
        Destroy(_targetObj, destroyTimer);
    }
}
