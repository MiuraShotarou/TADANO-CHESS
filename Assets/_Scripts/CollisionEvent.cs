using System;
using System.Linq;
using UnityEngine;
//Pone の動き（斜め前攻撃、　※最初のみ二回行動、アンパッサン（2歩進んだ直後のポーンの後ろのマスに自身のポーンの攻撃範囲が充てられている場合、そこに移動し相手のポーンを取ってしまうことができる））→ Colliderを残す形でアンパッサンを実装する → 他の特殊ルール的にも「初回の動きであるか否か」のbool型は必要かも
//キャスリング（条件：キングが一度も動いていない、ルークが一度も動いていない、キングとルークの間に駒がない、チェックされていない、キングの両隣が攻撃範囲に入っていない　効果：キングはルークがいる方向に２マス進み、ルークはキングを飛び越えてひとつ目のマスに移動する）キングはマスで条件を判断する
//右側にキャスリングすることを「ショートキャスリング」、左側にキャスリングすることを「ロングキャスリング」と呼ぶ → この２つの動きは固定のようだ
//プロモーション（条件：ポーンが突き当りのマスに到着した時、キング・ポーン以外の好きな駒に変化することができる）
//ステイルメイト（効果；チェックがかかっていない状態で何かの駒を動かすと敗北が決まってしまうという時、試合結果はドローになる）
//ドロー（条件：キング対キングになること、ステイルメイト、3回同一局面、50回ポーンが動かずどの駒も取られていない、合意によるドロー）
public class CollisionEvent : ColorPallet
{
    public static Action<GameObject> CollisionAction;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Piece")
        {
            CollisionAction(collision.gameObject);
            if (this.gameObject.name.First() != 'U')
            {
                // int destroyTimer = 3;
                Destroy(this.gameObject);
                // Destroy(this.gameObject, destroyTimer); タイマーいらないかも
            }
        }
    }
}
