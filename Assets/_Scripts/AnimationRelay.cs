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

    void StartIdleAnimationRelay()
    {
        _turnDeside.StartIdleAnimation();
    }

    void OnCollisionEnter2D(Collision2D collision) //SelectedPieceObjが動きはじめたタイミングでDynamicに変更する
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Piece"))
        {
            Debug.Log("hoge");
        }
    }
}
