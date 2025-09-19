using UnityEngine;

public class BAttackEffect : AnimationRelay
{
    Animator _BAttackEffectAnimatorController;
    void Awake()
    {
        _BAttackEffectAnimatorController = gameObject.GetComponent<Animator>();
    }
    void OnEnable()
    {
        StartBAttackEffectAnimation();
    }

    void StartBAttackEffectAnimation()
    {
        _BAttackEffectAnimatorController.Play("B_AttackEffect");
    }

    void ActiveOff()
    {
        gameObject.SetActive(false);
        //最適化でAnimaotrのEnabledを切っておくのもありなのかも
    }
}
