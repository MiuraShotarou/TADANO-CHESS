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
    RuntimeAnimatorController _selectedPieceRuntimeAnimator;
    RuntimeAnimatorController _targetPieceRuntimeAnimator; //念入り
    AnimationCurve _endPositionCurve;
    GameObject _collider2DPrefab;
    PlayableGraph _playableGraph;
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
            DeathAnimationRelay();
        }
        else
        {
            _temporaryIsEncount = false;
            _targetSquere._IsOnPiece = true;
        }
        StartMoveAnimation();
    }
    void StartMoveAnimation()
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
        AnimationPlayableOutput animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "AnimOutput", _selectedPieceAnimatorController);
        //AnimationClipClipPlayableをAnimationPlayableOutputと連結する
        animationPlayableOutput.SetSourcePlayable(animationClipPlayable);
        //AnimatorOverrideControllerを使用して差し替え
        AnimatorOverrideController overrideController = new AnimatorOverrideController(_selectedPieceAnimatorController.runtimeAnimatorController);
        overrideController["Run"] = animationClip; // 複製したclipに差し替え
        _selectedPieceAnimatorController.runtimeAnimatorController = overrideController;
        //再生
        _playableGraph.Play();
    }
    /// <summary>
    /// MoveAnimationの再生後にAnimationEventで１回呼ばれる
    /// </summary>
    public void StartAttackAnimation()
    {
        if (_temporaryIsEncount)
        {
            _playableGraph.Stop();
            _playableGraph.Destroy();
            string search = new string($"{_selectedPiece._PieceName}_Attack");
            _selectedPieceAnimatorController.Play(search);
        }
    }
    public void StartIdleAnimation()
    {
        _playableGraph.Stop();
        _playableGraph.Destroy();
        string search = new string($"{_selectedPiece._PieceName}_Idle");
        _selectedPieceAnimatorController.Play(search);
    }
    /// <summary>
    /// 敵の情報をColliderの衝突で取得するためだけのメソッド（削除予定）
    /// </summary>
    void DeathAnimationRelay()
    {
        CollisionEvent.CollisionAction = StartDeathAnimation;
        GameObject collider2DObj = Instantiate(_collider2DPrefab, _targetSquere._SquerePiecePosition, Quaternion.identity);
        int limitTime = 2;
        Destroy(collider2DObj, limitTime);
    }
    /// <summary>
    /// 駒があることを検知して実体化されたColliderの衝突情報から呼ばれる
    /// </summary>
    void StartDeathAnimation(GameObject collisionObj)
    {
        _targetPieceRuntimeAnimator = collisionObj.GetComponent<Animator>().runtimeAnimatorController;
        CustomDeathAnimation();
        PlayDeathAnimation();
    }
    /// <summary>
    /// DeathAnimationの編集・設定
    /// </summary>
    void CustomDeathAnimation()
    {
    }
    /// <summary>
    /// DeathAnimationの再生
    /// </summary>
    void PlayDeathAnimation()
    {
        //DethAnimationの再生

    }
}
