using UnityEngine;
public class AddPieceFunction : MonoBehaviour
{
    public Piece UpdatePoneGroup(Piece piece)
    {
        Piece updatePiece = Instantiate(piece);
        updatePiece._MoveAreas = () => new Vector3Int[] { new Vector3Int(-1, 0, 0)};
        updatePiece._AttackAreas = () => new Vector3Int[] { new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0)};
        return updatePiece;
    }
    public Piece AddMoveCount(Piece piece)
    {
        Piece updatePiece = Instantiate(piece);
        updatePiece._MoveCount = () => 2;
        return updatePiece;
    }
}
