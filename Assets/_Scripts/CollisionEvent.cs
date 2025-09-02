using UnityEngine;

public class CollisionEvent : OpenSelectableArea
{
    void OnCollisionEnter(Collision collision)
    {
        JudgmentGroup(collision.gameObject);
    }
}
