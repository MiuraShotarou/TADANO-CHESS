using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
[CreateAssetMenu(fileName = "SquerePosition", menuName = "ScriptableObject/SquerePosition")]
public class Squere : ScriptableObject
{
    [SerializeField, FormerlySerializedAs("_squerePosition")] Vector2 _squerePiecePosition;
    [SerializeField] Vector3 _squereTileWorldPosition;
    [SerializeField] Vector3Int _squereTilePos;
    string _squereID;
    bool _isOnPiece;
    //駒にとって都合の良い座標
    public Vector2 _SquerePiecePosition => _squerePiecePosition;
    public Vector3 _SquereTileWorldPosition => _squereTileWorldPosition;
    //Tilemapにとって都合の良い座標
    public Vector3Int _SquereTilePos => _squereTilePos;
    public string _SquereID => _squereID;
    public bool _IsOnPiece {get => _isOnPiece; set => _isOnPiece = value;}
    void OnEnable()
    {
        _squereID = name;
        if ("1,2,7,8".Contains(_SquereID.Last()))
        {
            _isOnPiece = true;
        }
        else
        {
            _isOnPiece = false;
        }
    }
}