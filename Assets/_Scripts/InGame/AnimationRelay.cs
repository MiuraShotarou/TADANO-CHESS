using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    [SerializeField] TurnDecide _turnDecide;
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
        _turnDecide.StartAttackAnimation();
    }

    void StartRunAnimationRelay()
    {
        _turnDecide.StartRunAnimation();
    }

    void StartIdleAnimationRelay()
    {
        _turnDecide.StartIdleAnimation();
    }

    void StartTakeHitAnimationRelay()
    {
        _turnDecide.StartTakeHitAnimation();
    }

    void StartDeathAnimationRelay()
    {
        _turnDecide.StartDeathAnimation();
    }

    void StartStageOutAddForceRelay()
    {
        _turnDecide.StartStageOutAddForce();
    }

    /// <summary>
    /// 両キャスリングを実行するためのAnimationを再生する。K_Run.animからしか呼ばれない
    /// </summary>
    void StartCastlingRelay()
    {
        _turnDecide.StartCastlingAnimation();
    }
    void StartInactiveTargetPlayableRelay()
    {
        _turnDecide.StartInactiveTargetPlayable();
    }

    /// <summary>
    /// ルークの投石オブジェクトをアクティブにするメソッド。R_AttackEffect内のAnimationEventからしか呼ばれない
    /// </summary>
    void StartRAttackEffectRelay()
    {
        _turnDecide.StartRAttackEffect();
    }
    /// <summary>
    /// ビショップの魔弾オブジェクトをアクティブにするメソッド。B_AttackEffect内のAnimationEventからしか呼ばれない
    /// </summary>
    void StartBAttackEffectRelay()
    {
        _turnDecide.StartBAttackEffect();
    }
    /// <summary>
    /// クイーンの火炎放射オブジェクトをアクティブにするメソッド。Q_AttackEffect内のAnimationEventからしか呼ばれない
    /// </summary>
    void StartQAttackEffectRelay()
    {
        _turnDecide.StartQAttackEffect();
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
