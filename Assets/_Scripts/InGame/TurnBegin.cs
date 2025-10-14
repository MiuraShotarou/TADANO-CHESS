using UnityEngine;

public class TurnBegin : MonoBehaviour
{
    InGameManager _inGameManager;
    void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
    }
    /// <summary>
    /// ターンが切り替わった時、InGameManagerから一度だけ呼び出される
    /// </summary>
    public void StartTurn()
    {
        AddTurnCount();
        ChangePlayerTurn();
    }
    void AddTurnCount()
    {
        _inGameManager._TurnCount++;
    }
    void ChangePlayerTurn()
    {
        _inGameManager.IsPlayerTurn = !_inGameManager.IsPlayerTurn;
    }
}
