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
    public Func<Vector3Int[]> _MoveAreas;
    public Func<Vector3Int[]> _AttackAreas;
    void OnEnable()
    {
        _pieceName = name.First().ToString();
        _MoveCount = () => _moveCount;
        _MoveAreas = () => _moveAreas;
        _AttackAreas = () => _attackAreas;
    }
}
