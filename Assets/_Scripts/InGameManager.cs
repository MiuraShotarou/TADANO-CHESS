using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    [SerializeField] Piece[] _setPieces;
    [SerializeField] Squere[] _setSqueres;
    OpenSelectableArea _openSelectableArea;
    SelectTileController _selectTileController;
    Animator _animatorController;
    Dictionary<string, Piece> _pieceDict = new Dictionary<string, Piece>();
    Squere[][] _squereArrays;
    public Animator _AnimatorController => _animatorController; //プロパティにしておけ
    public Dictionary<string, Piece> _PieceDict => _pieceDict;
    public Squere[][] _SquereArrays => _squereArrays;
    void Awake()
    {
        for (int i = 0; i < _setPieces.Length; i++)
        {
            //Add・Removeできるならプロパティの意味ない
            _PieceDict.Add(_setPieces[i]._PieceName, _setPieces[i]);
        }
        int arraySize = 8;
        _squereArrays = new Squere[arraySize][];
        for (int i = 0; i < arraySize; i++)
        {
            _squereArrays[i] = new Squere[arraySize];
            for (int j = 0; j < arraySize; j++)
            {
                _SquereArrays[i][j] = _setSqueres[i * 8 + j];
            }
        }
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _selectTileController = GetComponent<SelectTileController>();
        _animatorController = GetComponent<Animator>();
    }
    public void PieceObjectPressed(GameObject pieceObj)
    {
        _openSelectableArea.StartOpenArea(pieceObj);
    }
    public void StartSelectTileRelay()
    {
        _selectTileController.StartSelectTile();
    }
}
