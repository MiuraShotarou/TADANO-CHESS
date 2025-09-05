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
public class OpenSelectableArea : ColorPallet
{
    [SerializeField] Tilemap _tilemap;
    [SerializeField] Sprite _canSelectedSprite;//これはあって良い
    [SerializeField] GameObject _collider2DPrefab;
    InGameManager _inGameManager;
    AddPieceFunction _addPieceFunction;
    GameObject _selectedPieceObj;
    SpriteRenderer[][] _deceptionTileFieldArrays;
    Piece _selectedPiece;
    Squere _selectedSquere;
    Vector3Int[] _attackAreas;
    Vector3Int[] _moveAreas;
    List<Vector3Int> _renderingAreas = new List<Vector3Int>(); //Propatiesにしておけ
    int _pieceMoveCount = 0;
    int _prefabCount = 0;
    int _PrefabCount {get {return _prefabCount;} set {_prefabCount = value; if (_prefabCount == 0){RenderingOneLine();};}}
    public Tilemap _Tilemap => _tilemap;
    void Awake()
    {
        _inGameManager = GetComponent<InGameManager>();
        _addPieceFunction = GetComponent<AddPieceFunction>();
        _deceptionTileFieldArrays = _inGameManager._DeceptionTileFieldArrays;
    }
    /// <summary>
    /// 駒を選択してから一回だけ呼ばれる
    /// </summary>
    /// <param name="pieceObj"></param>
    public void StartOpenArea(GameObject pieceObj)
    {
        if (pieceObj == _selectedPieceObj){ return; }
        if (_selectedPieceObj)
        {
            BeforeRendereringClear();
        }
        _selectedPieceObj = pieceObj;
        string[] search = _selectedPieceObj.name.Split("_");
        if (!_selectedPiece
            ||
            search[0] != _selectedPiece.name)
        {
            _selectedPiece = _inGameManager._PieceDict[search[0]];
        }
        if (_selectedPiece.name == "P")
        {
            if (_selectedPieceObj.GetComponent<SpriteRenderer>().flipX)
            { 
                _selectedPiece = _addPieceFunction.UpdatePoneGroup(_selectedPiece);
            }
            if (_selectedPieceObj.transform.rotation.z == 0)
            {
                _selectedPiece = _addPieceFunction.AddMoveCount(_selectedPiece);
            }
        }
        _selectedSquere = _inGameManager._SquereArrays[int.Parse(search[1])][int.Parse(search[2])];
        _pieceMoveCount = _selectedPiece._MoveCount();
        Initialize();
        DrawOutline(_selectedPieceObj);
        _inGameManager._AnimatorController.Play("AddOneLine", 0, 0);
        //pieceObjの移動可能領域を検索 → 移動可能な範囲だけを検索し、そこのbool型がfalseだったら描画する。
        //pieceObjの攻撃可能領域を検索 → 攻撃可能な範囲だけを検索し、そこのbool型がfalseだったら描画する。
        //移動可能領域に_selectedTileを描画する
        //Animatoinの再生（描画が出来れば良い）
    }
    void BeforeRendereringClear()
    {
        SpriteRenderer spriteRenderer = _selectedPieceObj.GetComponent<SpriteRenderer>();
        spriteRenderer.color = _UnSelectedPieceColor;
        // obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/_Outline");
        for (int i = 0; i < _renderingAreas.Count; i++)
        {
            _deceptionTileFieldArrays[_renderingAreas[i].y][_renderingAreas[i].x].color = Color.clear;
            _deceptionTileFieldArrays[_renderingAreas[i].y][_renderingAreas[i].x].gameObject.GetComponent<Collider2D>().enabled = false;
        }
    }
    /// <summary>
    /// fieldにあるコレクションをすべて初期化する
    /// </summary>
    void Initialize()
    {
        _moveAreas = new Vector3Int[_selectedPiece._MoveAreas().Length];
        _attackAreas = new Vector3Int[_selectedPiece._AttackAreas().Length];
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
    }
    void JudgmentMoveArea()
    {
        for (int i = 0; i < _moveAreas.Length; i++)
        {
            if (_moveAreas[i].z == -1)
            {
                continue;
            }
            _moveAreas[i] += _selectedPiece._MoveAreas()[i]; //<=0, >=7
            int alphabet = _moveAreas[i].x;
            int number = _moveAreas[i].y;
            if (-1 < alphabet && 8 > alphabet && -1 < number && 8 > number //盤内のマスであればtrue
                &&
                !_inGameManager._SquereArrays[alphabet][number]._IsOnPiece)
            {
                _renderingAreas.Add(_moveAreas[i]);
            }
            else
            {
                _moveAreas[i] = new Vector3Int(0, 0, -1);
            }
        }
        //_selectedPieceがいる場所を中心に、移動可能領域の座標(_squereID)を求める
        //そこと一致するsquereを配列の中から全検索し、_isOnPieceがfalseだったら移動可能領域に含める
    }
    void JudgmentAttackArea()
    {
        _PrefabCount = _attackAreas.Length;
        for (int i = 0; i < _attackAreas.Length; i++)
        {
            if (_attackAreas[i].z == -1)
            {
                _prefabCount--;
                continue;
            }
            _attackAreas[i] += _selectedPiece._AttackAreas()[i];
            int alphabet = _attackAreas[i].x;
            int number = _attackAreas[i].y;
            if (!(-1 < alphabet && 8 > alphabet && -1 < number && 8 > number))//盤外のマスであるならば
            {
                _PrefabCount--;
                _attackAreas[i] = new Vector3Int(0, 0, -1);
                continue;
            }
            if (_inGameManager._SquereArrays[alphabet][number]._IsOnPiece)
            {
                Debug.Log("kore");
                Vector2 generatePos = _inGameManager._SquereArrays[alphabet][number]._SquerePiecePosition;
                GameObject collider2DObj = Instantiate(_collider2DPrefab, new Vector3(generatePos.x, generatePos.y, 0), Quaternion.identity);
                Destroy(collider2DObj, i);
                _attackAreas[i] =  new Vector3Int(0, 0, -1);
            }
            else
            {
                _PrefabCount--;
            }
        }
    }
    /// <summary>
    /// 駒があることを検知して実体化されたColliderの衝突情報から呼ばれる
    /// </summary>
    /// <param name="collisionObj"></param>
    public void JudgmentGroup(GameObject collisionObj)
    {
        SpriteRenderer spriteRenderer = collisionObj.GetComponent<SpriteRenderer>();
        if (_selectedPieceObj.GetComponent<SpriteRenderer>().flipX != spriteRenderer.flipX)
        {
            string[] search = collisionObj.name.Split("_");
            Vector3Int enemyTilePos  = _inGameManager._SquereArrays[int.Parse(search[1])][int.Parse(search[2])]._SquereTilePos;
            _renderingAreas.Add(enemyTilePos);
            DrawOutline(collisionObj);
        }
        _PrefabCount--;
    }
    void DrawOutline(GameObject obj)
    {
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
        // obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/_Outline");
    }
    /// <summary>
    /// _prefabCountが0になったとき、１回だけ呼ばれる
    /// </summary>
    void RenderingOneLine()
    {
        if (_renderingAreas.Count == 0)
        {
            _inGameManager.StartSelectTileRelay();
            return;
        }
        for (int i = 0; i < _renderingAreas.Count; i++)
        {
            _deceptionTileFieldArrays[_renderingAreas[i].y][_renderingAreas[i].x].color = _CanSelectedTileColor;
            _deceptionTileFieldArrays[_renderingAreas[i].y][_renderingAreas[i].x].gameObject.GetComponent<Collider2D>().enabled = true;
        }
        _pieceMoveCount--;
        if (_pieceMoveCount == 0)
        {
            return;
        }
        _inGameManager._AnimatorController.Play("AddOneLine", 0, 0);
    }
    private void Update()
    {
        // Debug.Log(_prefabCount);
    }
    public void TurnDesideRelay()
    {
        
    }
}
