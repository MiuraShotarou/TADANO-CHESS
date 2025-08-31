using UnityEngine;
[CreateAssetMenu(fileName = "SquerePosition", menuName = "ScriptableObject/SquerePosition")]
public class Squere : ScriptableObject
{
    [SerializeField] Vector2 _squerePosition;
    [SerializeField] Vector3Int _squereTilePos;
    public string _squereID;
    bool _isOnPiece;
    public Vector2 _SquerePosition => _squerePosition;
    public Vector3Int _SquereTilePos => _squereTilePos;
    public string _SquereID => _squereID;
    public bool _IsOnPiece {get => _isOnPiece; set => _isOnPiece = value;}
    void OnEnable()
    {
        _squereID = name;
        _isOnPiece = false;
    }
}