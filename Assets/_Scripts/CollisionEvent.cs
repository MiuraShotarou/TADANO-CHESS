using System;
using UnityEngine;

public class CollisionEvent : ColorPallet
{
    //デリゲートの意味ないかも
    public static Action<GameObject> CollisionAction;
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("");
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Piece")
        {
            CollisionAction(collision.gameObject);
            Destroy(this.gameObject);
        }
    }
}
