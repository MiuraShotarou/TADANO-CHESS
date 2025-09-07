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
}
