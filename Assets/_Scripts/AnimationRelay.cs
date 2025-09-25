using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    [SerializeField] TurnDeside _turnDeside;
    [SerializeField] UIManager _uiManager;
    // Animator _thisAnimator;
    // void Start()
    // {
    //     _thisAnimator = GetComponent<Animator>();
    //     HitStopSwich.PauseHitStop += PauseAnimator;
    //     HitStopSwich.ResumeHitStop += ResumeAnimator;
    // }
    void StartAttackAnimationRelay()
    {
        _turnDeside.StartAttackAnimation();
    }

    void StartRunAnimationRelay()
    {
        _turnDeside.StartRunAnimation();
    }

    void StartIdleAnimationRelay()
    {
        _turnDeside.StartIdleAnimation();
    }

    void StartTakeHitAnimationRelay()
    {
        _turnDeside.StartTakeHitAnimation();
    }

    void StartDeathAnimationRelay()
    {
        _turnDeside.StartDeathAnimation();
    }

    void StartStageOutAddForceRelay()
    {
        _turnDeside.StartStageOutAddForce();
    }

    /// <summary>
    /// 両キャスリングを実行するためのAnimationを再生する。K_Run.animからしか呼ばれない
    /// </summary>
    void StartCastlingRelay()
    {
        _turnDeside.StartCastlingAnimation();
    }

    void StartAdjustFlipXRelay()
    {
        _turnDeside.StartAdjustFlipX();
    }

    void StartInactiveTargetPlayableRelay()
    {
        _turnDeside.StartInactiveTargetPlayable();
    }

    /// <summary>
    /// ルークの投石オブジェクトをアクティブにするメソッド。R_AttackEffect内のAnimationEventからしか呼ばれない
    /// </summary>
    void StartRAttackEffectRelay()
    {
        _turnDeside.StartRAttackEffect();
    }
    /// <summary>
    /// ビショップの魔弾オブジェクトをアクティブにするメソッド。B_AttackEffect内のAnimationEventからしか呼ばれない
    /// </summary>
    void StartBAttackEffectRelay()
    {
        _turnDeside.StartBAttackEffect();
    }
    /// <summary>
    /// クイーンの火炎放射オブジェクトをアクティブにするメソッド。Q_AttackEffect内のAnimationEventからしか呼ばれない
    /// </summary>
    void StartQAttackEffectRelay()
    {
        _turnDeside.StartQAttackEffect();
    }
    /// <summary>
    /// 駒にアタッチされているためStageOutAddForceAnimationの再生後に呼ばれる
    /// </summary>
    void OnDestroy()
    {
        _uiManager._DeathPieceObj = this.gameObject;
    }
    // void PauseAnimator()
    // {
    //     _thisAnimator.speed = 0;
    // }
    // void ResumeAnimator()
    // {
    //     _thisAnimator.speed = 1;
    // }
    //
    // void OnDestroy()
    // {
    //     HitStopSwich.PauseHitStop -= PauseAnimator;
    //     HitStopSwich.ResumeHitStop -= ResumeAnimator;
    // }
}
