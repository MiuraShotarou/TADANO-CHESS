using System;
using UnityEngine;

public class CollisionEvent : ColorPallet
{
    public Action<GameObject> CollisionAction;
    void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Piece")
        {
            CollisionAction(collision.gameObject);
        }
    }
}
