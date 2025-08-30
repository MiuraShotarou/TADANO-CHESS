using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ScriptableObject", menuName = "SquerePosition")]
public class SquerePosition : ScriptableObject
{
    [SerializeField] Vector3 _squerePosition;
    string _squerePositionName;
    void OnEnable()
    {
        _squerePositionName = name;
    }
}