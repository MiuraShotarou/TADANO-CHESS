using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// _canSelectedTileBaseを描画するためのクラス
/// </summary>
public class OpenSelectableArea : MonoBehaviour
{
    [SerializeField] Tilemap _tilemap;
    [SerializeField] TileBase  _canSelectedTileBase;
    [SerializeField] TileBase  _selectedTileBase;
    [SerializeField] GameObject _collider2DPrefab;
    InGameManager _inGameManager;
    GameObject _selectedPieceObj;
    Piece _selectedPiece;
    Squere _selectedSquere;
    Vector3Int[] _attackAreas;
    Vector3Int[] _moveAreas;
    List<Vector3Int> _renderingAreas;
    void Awake()
    {
        _inGameManager = GetComponent<InGameManager>();
    }
    /// <summary>
    /// 駒を選択してから一回だけ呼ばれる
    /// </summary>
    /// <param name="pieceObj"></param>
    public void StartOpenArea(GameObject pieceObj)
    {
        _selectedPieceObj = pieceObj;
        string[] search = _selectedPieceObj.name.Split("_");
        _selectedPiece = _inGameManager._PieceDict[search[0]];
        _selectedSquere = _inGameManager._SquereArrays[int.Parse(search[1])][int.Parse(search[2])];
        Initialize();
        _inGameManager._animatorController.Play("AddOneLine");
        //pieceObjの移動可能領域を検索 → 移動可能な範囲だけを検索し、そこのbool型がfalseだったら描画する。
        //pieceObjの攻撃可能領域を検索 → 攻撃可能な範囲だけを検索し、そこのbool型がfalseだったら描画する。
        //移動可能領域に_selectedTileを描画する
        //Animatoinの再生（描画が出来れば良い）
    }
    /// <summary>
    /// fieldにあるコレクションをすべて初期化する
    /// </summary>
    void Initialize()
    {
        _moveAreas = new Vector3Int[_selectedPiece._MoveAreas.Length];
        _attackAreas = new Vector3Int[_selectedPiece._AttackAreas.Length];
        Array.Fill(_moveAreas, _selectedSquere._SquereTilePos);
        Array.Fill(_attackAreas, _selectedSquere._SquereTilePos);
        _renderingAreas = new List<Vector3Int>();
    }
    /// <summary>
    /// Animationの再生一回につき一度だけ呼ばれる。複数回呼ばれる可能性あり。
    /// </summary>
    public void AddOpenArea()
    {
        JudgmentMoveArea();
        JudgmentAttackArea();
        RenderingOneLine();
    }
    void JudgmentMoveArea()
    {
        for (int i = 0; i < _selectedPiece._MoveAreas.Length; i++)
        {
            if (_moveAreas[i].z == -1) { continue; }
            _moveAreas[i] += _selectedPiece._MoveAreas[i];
            int alphabet = _moveAreas[i].x;
            int number = _moveAreas[i].y;
            if (!_inGameManager._SquereArrays[alphabet][number]._IsOnPiece)
            {
                _renderingAreas.Add(_moveAreas[i]);
            }
            else
            {
                Vector2 generatePos = _inGameManager._SquereArrays[alphabet][number]._SquerePosition;
                GameObject collider2DObj = Instantiate(_collider2DPrefab, new Vector3(generatePos.x,  generatePos.y, 0), Quaternion.identity);
                Destroy(collider2DObj, i);
                _moveAreas[i].z = -1;
            }
        }
        //_selectedPieceがいる場所を中心に、移動可能領域の座標(_squereID)を求める
        //そこと一致するsquereを配列の中から全検索し、_isOnPieceがfalseだったら移動可能領域に含める
    }
    void JudgmentAttackArea()
    {
        for (int i = 0; i < _selectedPiece._MoveAreas.Length; i++)
        {
            if (_moveAreas[i].z == -1) { continue; }
            _moveAreas[i] += _selectedPiece._MoveAreas[i];
            int alphabet = _moveAreas[i].x;
            int number = _moveAreas[i].y;
            if (!_inGameManager._SquereArrays[alphabet][number]._IsOnPiece)
            {
                _renderingAreas.Add(_moveAreas[i]);
            }
            else
            {
                _moveAreas[i].z = -1;
            }
        }
    }
    public void JudgmentGroup(GameObject collisionObj)
    {
        SpriteRenderer spriteRenderer = collisionObj.GetComponent<SpriteRenderer>();
        if (!_selectedPieceObj.GetComponent<SpriteRenderer>().flipX == spriteRenderer.flipX)
        {
            string[] search = collisionObj.name.Split("_");
            Vector3Int enemyTilePos  = _inGameManager._SquereArrays[int.Parse(search[1])][int.Parse(search[2])]._SquereTilePos;
            _renderingAreas.Add(enemyTilePos);
        }
    }
    void RenderingOneLine()
    {
        for (int i = 0; i < _renderingAreas.Count; i++)
        {
            _tilemap.SetTile(_renderingAreas[i], _canSelectedTileBase);
        }
        _renderingAreas.Clear();
    }
}
