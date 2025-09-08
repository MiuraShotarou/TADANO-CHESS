using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    [SerializeField] TurnDeside _turnDeside;
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
}
