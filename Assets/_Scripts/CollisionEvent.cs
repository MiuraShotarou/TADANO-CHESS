using System;
using UnityEngine;

public class CollisionEvent : ColorPallet
{
    public Action<GameObject> CollisionAction;
    void OnCollisionEnter2D(Collision2D collision)
    {
            Debug.Log("0");
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Piece")
        {
            Debug.Log("1");
            CollisionAction(collision.gameObject);
        }
    }
}
