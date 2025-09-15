using UnityEngine;

public class QAttackEffect : AnimationRelay
{
    Animator _QAttackEffectAnimatorController;
    void Awake()
    {
        _QAttackEffectAnimatorController = gameObject.GetComponent<Animator>();
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        StartQAttackEffectAnimation();
    }

    void StartQAttackEffectAnimation()
    {
        _QAttackEffectAnimatorController.Play("Q_AttackEffect");
    }

    void ActiveOff()
    {
        gameObject.SetActive(false);
        //最適化でAnimaotrのEnabledを切っておくのもありなのかも
    }
}