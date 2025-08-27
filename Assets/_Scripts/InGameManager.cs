using UnityEngine;

public class InGameManager : MonoBehaviour
{
    //遠近法を用いて一番上列のマスから一番下列のマスへ行くに連れてScaleが＋1.0 になる必要がある → １マスにつき +0.125
    //移動量に応じて、描画の仕方だけを決めるScriptが必要かも
    //Tilemap上での移動量は分かりやすいから、
    //1マスにつき X 1.68 Y (変動する)
    //AnimationClip"Move" に 移動先のTransformを設定したKeyfreamを挿入するScriptを書けば簡単に移動時のAnimationを実装できる → 現在の座標が"始点" → ""
    //Positionが複雑なので、チェス公式の座標で管理しよう → V == a~h, H == 1~8, 左下 == a1, 右上 == h8 
}
