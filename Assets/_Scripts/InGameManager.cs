using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    [SerializeField] Piece[] _setPieces;
    [SerializeField] Squere[] _setSqueres;
    OpenSelectableArea _openSelectableArea;
    public Animator _animatorController; //プロパティにしておけ
    //Class "MiniBordController" "MoveAnimation" "BordController" 
    //遠近法を用いて一番上列のマスから一番下列のマスへ行くに連れてScaleが＋1.0 になる必要がある → １マスにつき +0.143
    //Animation KeyFrameの挿入 (Vector2, scale) 役割：スプライトの移動
    //攻撃判定はどうするのか？　→ Collider出現方法で一旦やってみたいか？
    //シンプルに、座標系で判断すればよいのでは？　そうすればTilePositionと駒のポジションが一致するわけなので、内部でその情報を保持していればColliderで検出する必要はないように思う
    //ていうかそれがないとMiniBordが作れんじゃん。MiniBord == start(-164.5, -165), distanceXY(47), if (Y == 164){Y = 163.5} ※すべてLocalPos
    //各駒のポジションはGameObjectの名前に付与していれば良い
    //駒が一つ消えると複数のUIが一気に更新される
    // public Squere[][] _Squeres => _squeres;
    Dictionary<string, Piece> _pieceDict = new Dictionary<string, Piece>();
    Squere[][] _squereArrays;
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
        _animatorController = GetComponent<Animator>();
    }
    void PieceObjectPressed(GameObject pieceObj)
    {
        _openSelectableArea.StartOpenArea(pieceObj);
    }
    void Start()
    {
        
    }
}
