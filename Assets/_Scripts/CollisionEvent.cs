using System;
using UnityEngine;

public class CollisionEvent : ColorPallet
{
    public static Action<GameObject> CollisionAction;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Piece")
        {
            CollisionAction(collision.gameObject);
        }
    }
}
