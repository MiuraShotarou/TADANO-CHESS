using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InGameManager : MonoBehaviour
{
    [SerializeField] Piece[] _setPieces;
    [SerializeField] Squere[] _setSqueres;
    [SerializeField] SpriteRenderer[] _setDeceptionTileFields; //プロパティにしておけ
    Dictionary<string, Piece> _pieceDict = new Dictionary<string, Piece>();
    Squere[][] _squereArrays;
    SpriteRenderer[][] _deceptionTileFieldArrays;
    OpenSelectableArea _openSelectableArea;
    SelectTileController _selectTileController;
    TurnDeside _turnDeside;
    Animator _animatorController;
    GameObject _collider2DPrefab;
    public Dictionary<string, Piece> _PieceDict => _pieceDict;
    public Squere[][] _SquereArrays => _squereArrays;
    public SpriteRenderer[][] _DeceptionTileFieldArrays => _deceptionTileFieldArrays;
    public OpenSelectableArea _OpenSelectableArea => _openSelectableArea; //いらない
    public SelectTileController SelectTileController => _selectTileController; //いらない
    public Animator _AnimatorController => _animatorController;
    public GameObject _Collider2DPrefab => _collider2DPrefab;
    void Awake()
    {
        for (int i = 0; i < _setPieces.Length; i++)
        {
            //Add・Removeできるならプロパティの意味ない
            _PieceDict.Add(_setPieces[i]._PieceName, _setPieces[i]);
        }
        int arraySize = 8;
        _deceptionTileFieldArrays = new SpriteRenderer[arraySize][];
        _squereArrays = new Squere[arraySize][];
        for (int i = 0; i < arraySize; i++)
        {
            _deceptionTileFieldArrays[i] = new SpriteRenderer[arraySize];
            _squereArrays[i] = new Squere[arraySize];
            for (int j = 0; j < arraySize; j++)
            {
                //１次元目にアルファベット（縦列）座標を、２次元目に数値（横列）座標を割り当てる
                int index = i * 8 + j;
                _deceptionTileFieldArrays[i][j] = _setDeceptionTileFields[index];
                _SquereArrays[i][j] = _setSqueres[index];
            }
        }
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _selectTileController = GetComponent<SelectTileController>();
        _animatorController = GetComponent<Animator>();
        _collider2DPrefab = Resources.Load<GameObject>("BoxCollider2DPrefab");
    }
    public void PieceObjectPressed(GameObject pieceObj)
    {
        _openSelectableArea.StartOpenArea(pieceObj);
    }
    public void StartSelectTileRelay()
    {
        _selectTileController.enabled = true;
    }
}
// 初期化処理はデリゲート登録をして個個別別に作っていくべきなのか？