using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Linq;
public class R_AttackEffect : AnimationRelay
{
    GameObject _RAttackEffectObj;
    Animator _RAttackEffectAnimatorController;
    RuntimeAnimatorController _RAttackEffectRuntimeAnimatorController;
    PlayableGraph _playableGraph;
    public Vector3 _targetPos;
    /// <summary>
    /// Awakeの代わり。非アクティブなまま始まるのでOnDisableから呼ばれる
    /// </summary>
    void Awake()
    {
        _RAttackEffectObj = this.gameObject;
        _RAttackEffectAnimatorController = _RAttackEffectObj.GetComponent<Animator>();
        _RAttackEffectRuntimeAnimatorController = _RAttackEffectAnimatorController.runtimeAnimatorController;
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        StartRAttackEffectAnimation();
    }
    /// <summary>
    /// ルークの攻撃モーション「投石」の石が始点から終点に向かって回転していくだけのAnimationを再生する。動作が独立している。
    /// </summary>
    void StartRAttackEffectAnimation()
    {
        AnimationCurve animationCurvePosX = AnimationCurve.Linear(0f, transform.position.x, 1f, _targetPos.x);
        AnimationCurve animationCurvePosY = AnimationCurve.EaseInOut(0f, transform.position.y, 1f, _targetPos.y);
        float intermediatePointY = transform.position.y + (Mathf.Abs(_targetPos.y - transform.position.y) / 2f);
        animationCurvePosY.AddKey(0.8f, intermediatePointY);
        AnimationCurve animationCurveRotZ = AnimationCurve.Linear(0f, transform.rotation.z, 1f, 1080);
        if (_targetPos.x - transform.position.x < 0)
        {
            animationCurveRotZ = AnimationCurve.Linear(0f, transform.rotation.z, 1f, -1080);
        }
        //"Run"という名前のついたanimationClipからコピーを新規作成
        AnimationClip animationClip = _RAttackEffectRuntimeAnimatorController.animationClips.First();
        //新しく作成・編集したAnimationCurveをAnimationClipに代入する
        animationClip.SetCurve("", typeof(Transform), "localPosition.x", animationCurvePosX);
        animationClip.SetCurve("", typeof(Transform), "localPosition.y", animationCurvePosY);
        animationClip.SetCurve("", typeof(Transform), "localEulerAngles.z", animationCurveRotZ);
        //PlayableGraphを作成
        _playableGraph = PlayableGraph.Create();
        //AnimationClipPlayableを作成
        AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(_playableGraph, animationClip);
        //AnimationPlayableOutputを作成してAnimatorと連結
        AnimationPlayableOutput animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "AnimOutput", _RAttackEffectAnimatorController);
        animationPlayableOutput.SetSourcePlayable(animationClipPlayable);
        //再生
        _playableGraph.Play();
    }
    /// <summary>
    /// このオブジェクトを非アクティブにし再度待機状態にする
    /// </summary>
    void ActiveOff()
    {
        _playableGraph.Stop();
        _playableGraph.Destroy();
        _RAttackEffectObj.SetActive(false);
        _RAttackEffectObj.transform.position = default;
        _RAttackEffectObj.transform.rotation = default;
    }
}
