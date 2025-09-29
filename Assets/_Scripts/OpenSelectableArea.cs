using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// _canSelectedTileBaseを描画するためのクラス
/// </summary>
public class OpenSelectableArea : ColorPallet
{
    [SerializeField] Sprite _canSelectedSprite;//これはあって良い
    InGameManager _inGameManager;
    AddPieceFunction _addPieceFunction;
    TurnDeside _turnDeside;
    UIManager _uiManager;
    GameObject _selectedPieceObj;
    SpriteRenderer[][] _deceptionTileFieldArrays;
    Piece _selectedPiece;
    Squere _selectedSquere;
    Squere _targetSquere;
    Vector3Int[] _attackAreas;
    Vector3Int[] _moveAreas;
    List<Vector3Int> _renderingAreas = new List<Vector3Int>();
    List<Vector2Int> _memorizeRenderingAreas = new List<Vector2Int>();
    Queue<Vector3Int> _beforeTurnMoveAreaQueue = new Queue<Vector3Int>(); //要素数 < 5, 前のターンに描画がリセットされなかったSquereの描画をDequeueでリセットする → 被りがあった場合はそこだけ描画をリセットしない
    List<GameObject> _memorizeRenderingPieceObjects = new List<GameObject>();
    int _pieceMoveCount = 0;
    int _prefabCount = 0;
    GameObject _collider2DPrefab;
    int _PrefabCount {get {return _prefabCount;} set {_prefabCount = value; if (_prefabCount == 0){RenderingOneLine();}}}
    bool _isShortCasting;
    bool _isLongCasting; //試験的
    void Awake()
    {
        _inGameManager = GetComponent<InGameManager>();
        _addPieceFunction = GetComponent<AddPieceFunction>();
        _turnDeside = GetComponent<TurnDeside>();
        _uiManager = GetComponent<UIManager>();
        _deceptionTileFieldArrays = _inGameManager._DeceptionTileFieldArrays;
    }
    /// <summary>
    /// 駒を選択してから一回だけ呼ばれる
    /// </summary>
    /// <param name="pieceObj"></param>
    public void StartOpenArea(GameObject pieceObj)
    {
        if (pieceObj == _selectedPieceObj
            || 
            (_inGameManager.IsWhite && pieceObj.CompareTag("Black")) || (!_inGameManager.IsWhite && pieceObj.CompareTag("White"))){ return; } //RayCastで拒否したい
        //ユーザーの入力を遮る
        _uiManager.ActiveFadePanel();
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
            if (!_inGameManager.IsWhite) //!isWhiteでも良い
            { 
                _selectedPiece = _addPieceFunction.UpdatePoneGroup(_selectedPiece);
            }
            if (_selectedPieceObj.transform.rotation.z == 0)
            {
                _selectedPiece = _addPieceFunction.AddMoveCount(_selectedPiece);
            }
        }
        else if (_selectedPiece._PieceName == "K")
        { 
            //キャスリングができることを示すための条件式
            //ショートキャスリングできるかどうかは既に判定済みである
            if (_inGameManager.IsCastling[0]())
            {
                 _selectedPiece = _addPieceFunction.AddShortCastlingArea(_selectedPiece);
            }
            //ロングキャスリングできるかどうかは既に判定済みである
            if (_inGameManager.IsCastling[1]())
            {
                 _selectedPiece = _addPieceFunction.AddLongCastlingArea(_selectedPiece);
            }
        }
        _selectedSquere = _inGameManager._SquereArrays[int.Parse(search[1])][int.Parse(search[2])];
        _pieceMoveCount = _selectedPiece._MoveCount();
        Initialize();
        DrawOutline(_selectedPieceObj);
        _inGameManager._AnimatorController.Play("AddOneLine", 0, 0);
    }
    public void BeforeRendereringClear()
    {
        // obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/_Outline");
        Array.ForEach(_memorizeRenderingPieceObjects.ToArray(), obj => obj.GetComponent<SpriteRenderer>().color = ChangeAlpha(obj.GetComponent<SpriteRenderer>().color, 150));
        for (int i = 0; i < _memorizeRenderingAreas.Count; i++)
        {
            _deceptionTileFieldArrays[_memorizeRenderingAreas[i].y][_memorizeRenderingAreas[i].x].color = Color.clear;
            _deceptionTileFieldArrays[_memorizeRenderingAreas[i].y][_memorizeRenderingAreas[i].x].gameObject.GetComponent<Collider2D>().enabled = false;
        }
        _memorizeRenderingPieceObjects.Clear();
        if (_beforeTurnMoveAreaQueue.Count > 2)
        {
            Vector3Int selectedSquereTilePos = _beforeTurnMoveAreaQueue.Dequeue();
            _deceptionTileFieldArrays[selectedSquereTilePos.y][selectedSquereTilePos.x].color = Color.clear;
            _deceptionTileFieldArrays[selectedSquereTilePos.y][selectedSquereTilePos.x].gameObject.GetComponent<Collider2D>().enabled = false;
            Vector3Int targetSquereTilePos = _beforeTurnMoveAreaQueue.Dequeue();
            if (targetSquereTilePos == _targetSquere._SquereTilePos)
            {
                return;
            }
            else
            {
                _deceptionTileFieldArrays[targetSquereTilePos.y][targetSquereTilePos.x].color = Color.clear;
                _deceptionTileFieldArrays[targetSquereTilePos.y][targetSquereTilePos.x].gameObject.GetComponent<Collider2D>().enabled = false;
            }
        }
    }
    /// <summary>
    /// fieldにあるコレクションをすべて初期化する
    /// </summary>
    void Initialize()
    {
        _moveAreas = Enumerable.Repeat(_selectedSquere._SquereTilePos, _selectedPiece._MoveAreas().Length).ToArray();
        _attackAreas = Enumerable.Repeat(_selectedSquere._SquereTilePos, _selectedPiece._AttackAreas().Length).ToArray();
        _renderingAreas.Clear();
        _memorizeRenderingAreas.Clear();
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
                !_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj)
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
                _attackAreas[i] = new Vector3Int(0, 0, -1);
            }
            else if (_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj)
            {
                if (!IsCanAttackTargetObject(_inGameManager._SquereArrays[alphabet][number]._IsOnPieceObj))
                {
                    _PrefabCount--;
                    _attackAreas[i] = new Vector3Int(0, 0, -1);
                    continue;
                }
                _renderingAreas.Add(_attackAreas[i]);
                _attackAreas[i] = new Vector3Int(0, 0, -1);
                //z = -1で次回の検索を回避する
            }
            _PrefabCount--;
        }
    }
    // /// <summary>
    // /// targetPieceObjのいるSquareに移動するべきかを判断する
    // /// </summary>
    // /// <param name="collisionObj"></param>
    bool IsCanAttackTargetObject(GameObject targetObj)
    {
        //敵だった場合
        if (!_selectedPieceObj.CompareTag(targetObj.tag))
        {
            //_enpassantObjだった場合
            if ("P".Contains(_selectedPieceObj.name.First().ToString())
                &&
                targetObj.transform.parent) //E
            {
                DrawOutline(targetObj.transform.parent.gameObject);
                return true;
            }
            DrawOutline(targetObj);
            return true;
        }
        //味方でかつキングが動く場合でかつTargetobjがRだった場合
        else if(_selectedPiece._PieceName == "K" && targetObj.name.First().ToString() == "R")
        {
            // IsCastlingで分かっていること → キャスリング可能な状態が整っていた場合
            if (_inGameManager.IsCastling.Any(del => del()) && _selectedPieceObj.transform.rotation.z == 0)
            {
                DrawOutline(targetObj);
                return true; //キャスリング可能であればtrue
            }
        }
        return false;
    }
    void DrawOutline(GameObject obj)
    {
        //_enpassantObjならreturnする
        if (!obj.GetComponent<SpriteRenderer>()) return;
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            Color color = spriteRenderer.color;
            color.a = 1;
            spriteRenderer.color = color;
        }
        _memorizeRenderingPieceObjects.Add(obj);
        // obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/_Outline");
    }
    /// <summary>
    /// _prefabCountが0になったとき、１回だけ呼ばれる
    /// </summary>
    void RenderingOneLine()
    {
        _memorizeRenderingAreas = _memorizeRenderingAreas.Union(VectorIntConverter(_renderingAreas.ToArray())).ToList();
        if (_renderingAreas.Count == 0)
        {
            _inGameManager.StartSelectTileRelay();
            return;
        }
        // Debug.Log(_renderingAreas[0]);
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
        _renderingAreas.Clear();
        _inGameManager._AnimatorController.Play("AddOneLine", 0, 0);
    }
    public void TurnDesideRelay(SpriteRenderer currentSpriteRenderer)
    {
        string[] search = currentSpriteRenderer.gameObject.name.Split('_');
        //ここでもGameObjectの名前で検索している
        //描画する時は偽のポジション、攻撃対象のオブジェクトは真を取得しなければならない → 名前は偽のポジション名・敵のオブジェクトはParent設定で取得する
        _targetSquere = _inGameManager._SquereArrays[int.Parse(search[0])][int.Parse(search[1])];
        if (_targetSquere._IsOnPieceObj)
        {
            if (_targetSquere._IsOnPieceObj.transform.parent)
            {
                _memorizeRenderingPieceObjects.Remove(_targetSquere._IsOnPieceObj.transform.parent.gameObject);
            }
            _memorizeRenderingPieceObjects.Remove(_targetSquere._IsOnPieceObj);
        }
        //ActiveになったSquereをListから削除
        Debug.Log(_selectedSquere._SquereTilePos);
        Array.ForEach(_memorizeRenderingAreas.ToArray(), a => Debug.Log(a));
        _memorizeRenderingAreas.Remove(VectorIntConverter(_selectedSquere._SquereTilePos));
        _memorizeRenderingAreas.Remove(VectorIntConverter(_targetSquere._SquereTilePos));
        //次のターン終了時に描画のリセットを行う予定のSquereをQueueに追加する
        _beforeTurnMoveAreaQueue.Enqueue(_selectedSquere._SquereTilePos);
        _beforeTurnMoveAreaQueue.Enqueue(_targetSquere._SquereTilePos);
        //地味に大事
        _turnDeside.enabled = true;
        _turnDeside.StartTurnDeside(_selectedPieceObj, _selectedPiece, _selectedSquere, _targetSquere);
    }
    Vector2Int VectorIntConverter(Vector3Int vector2Int) => new (vector2Int.x, vector2Int.y);  //zを除去する必要は、実はないのかもしれない → Squereの修正をすれば直るかも → tilePosY座標を反転させる必要がある → Tilemapを利用しているわけではないのでScriptableObjectから書き換えるだけで良いのかも
    Vector2Int[] VectorIntConverter(Vector3Int[] vector2IntArray)
    {
        List<Vector2Int> vector2IntList = new List<Vector2Int>();
        Array.ForEach(vector2IntArray, vector2Int => vector2IntList.Add(new Vector2Int(vector2Int.x, vector2Int.y)));
        return vector2IntList.ToArray();
    }
}
