using System;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "Piece", menuName = "ScriptableObject/Piece")]
public class Piece : ScriptableObject
{
    [SerializeField] string _pieceName;
    [SerializeField] int _moveCount;
    [SerializeField] Vector3Int[] _moveAreas;
    [SerializeField] Vector3Int[] _attackAreas;
    public string _PieceName => _pieceName;
    public Func<int> _MoveCount;
    public Vector3Int[] _MoveAreas => _moveAreas;
    public Vector3Int[] _AttackAreas => _attackAreas;

    void OnEnable()
    {
        _pieceName = name;
        _MoveCount = () => _moveCount;
    }
}
