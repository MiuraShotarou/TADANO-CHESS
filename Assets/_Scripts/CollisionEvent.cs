using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEvent : MonoBehaviour
{
    [SerializeField] OpenSelectableArea _openSelectableArea;
    void OnCollisionEnter(Collision collision)
    {
        _openSelectableArea.JudgmentGroup(collision.gameObject);
    }
}
