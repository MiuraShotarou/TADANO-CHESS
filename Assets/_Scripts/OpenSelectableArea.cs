using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
//特殊ルール群
//アンパッサン
//

/// <summary>
/// _canSelectedTileBaseを描画するためのクラス
/// </summary>
public class OpenSelectableArea : ColorPallet
{
    [SerializeField] Sprite _canSelectedSprite;//これはあって良い
    InGameManager _inGameManager;
    AddPieceFunction _addPieceFunction;
    TurnDeside _turnDeside;
    CollisionEvent _collisionEvent; //いらない
    GameObject _selectedPieceObj;
    SpriteRenderer[][] _deceptionTileFieldArrays;
    Piece _selectedPiece;
    Squere _selectedSquere;
    Vector3Int[] _attackAreas;
    Vector3Int[] _moveAreas;
    List<Vector3Int> _renderingAreas = new List<Vector3Int>(); //Propatiesにしておけ
    int _pieceMoveCount = 0;
    int _prefabCount = 0;
    GameObject _collider2DPrefab;
    int _PrefabCount {get {return _prefabCount;} set {_prefabCount = value; if (_prefabCount == 0){RenderingOneLine();};}}
    private bool _isShortCasting;
    private bool _isLongCasting; //試験的
    void Awake()
    {
        _inGameManager = GetComponent<InGameManager>();
        _addPieceFunction = GetComponent<AddPieceFunction>();
        _turnDeside = GetComponent<TurnDeside>();
        _deceptionTileFieldArrays = _inGameManager._DeceptionTileFieldArrays;
        _collider2DPrefab = _inGameManager._Collider2DPrefab;
        _collisionEvent = _collider2DPrefab.GetComponent<CollisionEvent>();
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
            //元のScriptableObjectに影響を与えたくないので新規生成で代入する
            _selectedPiece = Instantiate(_inGameManager._PieceDict[search[0]]);
        }
        if (_selectedPiece._PieceName == "P")
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
        else if (_selectedPiece._PieceName == "K") //チェックされていない、Kの通過するマスが攻撃範囲に入っていない
        {
            
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
    public void BeforeRendereringClear()
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
        CollisionEvent.CollisionAction = JudgmentGroup;
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
            //アルファベット（縦列）座標がy、数値（横列）座標がx
            int alphabet = _moveAreas[i].y;
            int number = _moveAreas[i].x;
            if (-1 < alphabet && 8 > alphabet && -1 < number && 8 > number //盤内のマスであればtrue
                &&
                !_inGameManager._SquereArrays[alphabet][number]._IsOnPiece
                &&
                //enpassantでないなら通す
                !(_selectedPiece._PieceName == "P" && _inGameManager._SquereArrays[alphabet][number]._IsActiveEnpassant))
            {
                _renderingAreas.Add(_moveAreas[i]);
            }
            else
            {
                _moveAreas[i] = new Vector3Int(0, 0, -1);
            }
        }
    }
    void JudgmentAttackArea()
    {
        _PrefabCount = _attackAreas.Length; //K が一度も動いていない場合、+ 1した時に必ずルークがいるであろう２つのマスを追加する
        for (int i = 0; i < _attackAreas.Length; i++)
        {
            if (_attackAreas[i].z == -1)
            {
                _PrefabCount--;
                continue;
            }
            _attackAreas[i] += _selectedPiece._AttackAreas()[i];
            int alphabet = _attackAreas[i].y;
            int number = _attackAreas[i].x;
            if (!(-1 < alphabet && 8 > alphabet && -1 < number && 8 > number))//盤外のマスであるならば
            {
                _PrefabCount--;
                _attackAreas[i] = new Vector3Int(0, 0, -1);
                continue;
            }
            if (_inGameManager._SquereArrays[alphabet][number]._IsOnPiece
                ||
                //enpassant可能であれば通過する
                (_selectedPiece._PieceName == "P" && _inGameManager._SquereArrays[alphabet][number]._IsActiveEnpassant))
            {
                Debug.Log(_inGameManager._SquereArrays[alphabet][number]._SquereID + _inGameManager._SquereArrays[alphabet][number]._SquerePiecePosition);
                Vector2 generatePos = _inGameManager._SquereArrays[alphabet][number]._SquerePiecePosition;
                Instantiate(_collider2DPrefab, new Vector3(generatePos.x, generatePos.y, 0), Quaternion.identity);
                //z = -1で次回の検索を回避する
                _attackAreas[i] = new Vector3Int(0, 0, -1);
            }
            else
            {
                _PrefabCount--;
            }
            if (_PrefabCount > 0){Debug.Log("");}
        }
    }
    /// <summary>
    /// 駒があることを検知して実体化されたColliderの衝突情報から呼ばれる
    /// </summary>
    /// <param name="collisionObj"></param>
    void JudgmentGroup(GameObject collisionObj)
    {
        SpriteRenderer spriteRenderer = collisionObj.GetComponent<SpriteRenderer>();
        if (!spriteRenderer)
        {
            spriteRenderer = collisionObj.transform.parent.gameObject.GetComponent<SpriteRenderer>();
        }
        //enpassantを取得したら → Pone以外の駒の場合はColliderを出現させないようにしているのでOK
        if (_selectedPieceObj.GetComponent<SpriteRenderer>().flipX != spriteRenderer.flipX)
        {
            string[] search = collisionObj.name.Split("_");
            Vector3Int enemyTilePos = _inGameManager._SquereArrays[int.Parse(search[1])][int.Parse(search[2])]._SquereTilePos;
            _renderingAreas.Add(enemyTilePos);
            DrawOutline(collisionObj);
        }
        _PrefabCount--;
    }
    void DrawOutline(GameObject obj)
    {
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            spriteRenderer.color = Color.white;
        }
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
            _inGameManager.StartSelectTileRelay();
            return;
        }
        if (_selectedPiece._PieceName == "P")
        {
            //Poneが二回行動可能な時、AttackAreaだけ二度目の描画を制限する
            Array.Fill(_attackAreas, new Vector3Int(0, 0, -1));
        }
        _inGameManager._AnimatorController.Play("AddOneLine", 0, 0);
    }
    public void TurnDesideRelay(SpriteRenderer currentSpriteRenderer)
    {
        string[] search = currentSpriteRenderer.gameObject.name.Split("_");
        //ここでもGameObjectの名前で検索している
        //描画する時は偽のポジション、攻撃対象のオブジェクトは真を取得しなければならない → 名前は偽のポジション名・敵のオブジェクトはParent設定で取得する
        Squere targetSquere = _inGameManager._SquereArrays[int.Parse(search[0])][int.Parse(search[1])];
        //地味に大事
        _turnDeside.enabled = true;
        _turnDeside.StartTurnDeside(currentSpriteRenderer, _selectedPieceObj, _selectedPiece, _selectedSquere, targetSquere);
    }
}
