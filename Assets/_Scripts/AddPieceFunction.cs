using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// OpenSelectableAreaで新規生成したPieceクラスに特有の機能を追加する
/// </summary>
public class AddPieceFunction : UIManager
{
    public Piece UpdatePoneGroup(Piece piece)
    {
        piece._MoveAreas = () => new Vector3Int[] { new Vector3Int(-1, 0, 0)};
        piece._AttackAreas = () => new Vector3Int[] { new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0)};
        return piece;
    }
    public Piece AddMoveCount(Piece piece) //引数で受け取っているのに上書きされる謎
    {
        piece._MoveCount = () => 2;
        return piece;
    }

    public Piece AddShortCastlingArea(Piece piece)
    {
        piece._AttackAreas = () => piece._AttackAreas().Concat(new Vector3Int[] { new Vector3Int(0, -3, 0)}).ToArray();
        return piece;
    }
    public Piece AddLongCastlingArea(Piece piece)
    {
        piece._AttackAreas = () => piece._AttackAreas().Concat(new Vector3Int[] { new Vector3Int(0, 4, 0)}).ToArray();
        return piece;
    }
    // public GameObject Promotion()
    // {
    //     string promotionPieceName = ActivePromotionPanel();
    //     //文字列を頼りにResources.Loadで変更後のGameObjectをreturnしていく仕様に
    //     //正直、後回しでも良いような気がしてきた
    //     
    // }
}

