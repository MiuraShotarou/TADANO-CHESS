using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
[CreateAssetMenu(fileName = "SquerePosition", menuName = "ScriptableObject/SquerePosition")]
public class Squere : ScriptableObject
{
    [SerializeField, FormerlySerializedAs("_squerePosition")] Vector2 _squerePiecePosition;
    [SerializeField] Vector3Int _squereTilePos;
    [SerializeField] Vector2 _miniBordPos ;
    string _squereID;
    //駒にとって都合の良い座標
    public Vector2 _SquerePiecePosition => _squerePiecePosition;
    public Vector3 _MiniBordPos => _miniBordPos;
    //Tilemapにとって都合の良い座標
    public Vector3Int _SquereTilePos => _squereTilePos;
    public string _SquereID => _squereID;
    public bool _IsOnPiece {get; set;}
    public bool _IsActiveEnpassant { get; set; }
    public GameObject _IsOnPieceObj { get; set; }
    void OnEnable()
    {
        _squereID = name;
        if ("1,2,7,8".Contains(_SquereID.Last()))
        {
            _IsOnPiece = true;
        }
        else
        {
            _IsOnPiece = false;
        }
        _IsActiveEnpassant = false;
    }
}