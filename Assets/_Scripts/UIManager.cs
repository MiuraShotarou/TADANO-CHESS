using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
/// <summary>
/// UIのバックエンドを担当
/// </summary>
public class UIManager : MonoBehaviour
{
    InGameManager _inGameManager;
    TurnDeside _turnDeside;
    [SerializeField] TextMeshProUGUI t_moveCount;
    [SerializeField] TextMeshProUGUI t_afterMoveCount;

    void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
    }
    public void DesidePromotion(string pieceName)
    {
        GameObject promotionObj = _turnDeside._promotionObj;
        promotionObj.name = promotionObj.name.Replace("P", pieceName);
        promotionObj.GetComponent<Animator>().runtimeAnimatorController = _inGameManager._PieceRuntimeAnimatorControllers[pieceName];
    }
}
