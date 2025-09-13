using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
[CreateAssetMenu(fileName = "SquerePosition", menuName = "ScriptableObject/SquerePosition")]
public class Squere : ScriptableObject
{
    [SerializeField, FormerlySerializedAs("_squerePosition")] Vector2 _squerePiecePosition;
    [SerializeField] Vector3Int _squereTilePos;
    [SerializeField] Vector2 _miniBordPos ;
    // GameObject _isOnPieceObj;
    SquereID _squereID;
    //駒にとって都合の良い座標
    public Vector2 _SquerePiecePosition => _squerePiecePosition;
    public Vector3 _MiniBordPos => _miniBordPos;
    //Tilemapにとって都合の良い座標
    public Vector3Int _SquereTilePos => _squereTilePos;
    public SquereID _SquereID => _squereID;
    public bool _IsActiveEnpassant { get; set; }
    // public GameObject _IsOnPieceObj { get => _isOnPieceObj; set { _isOnPieceObj = value; UpdateMiniBorad(this);}}
    public GameObject _IsOnPieceObj { get ; set;}
    //_IsOnPieceが書き換わるたびに強制で呼ばれるデリゲート
    // public Action<Squere> UpdateMiniBorad; //この関数の中で駒の種類を判別する
    void OnEnable()
    {
        int alphabet = "abcdefgh".IndexOf(name.First());
        int number = "12345678".IndexOf(name.Last());
        int index = (alphabet * 8) + number;
        _squereID = (SquereID)index;
        //miniBoradに通知する
        // UpdateMiniBorad = MiniBoard.StartUpdateMiniBorad;
        _IsActiveEnpassant = false;
    }
}