using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    [SerializeField] TurnDeside _turnDeside;
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

    void StartAdjustFlipXRelay()
    {
        _turnDeside.StartAdjustFlipX();
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
