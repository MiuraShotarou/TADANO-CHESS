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
    [SerializeField] TextMeshProUGUI t_deceptionMoveCount;
    [SerializeField] TextMeshProUGUI t_truthMoveCount;
    [SerializeField] TextMeshProUGUI[] t_residuesW;
    [SerializeField] TextMeshProUGUI[] t_residuesB;
    [SerializeField] TextMeshProUGUI[] t_squeresW;
    [SerializeField] TextMeshProUGUI[] t_squeresB;
    Dictionary<string, TextMeshProUGUI> t_residuesDictW;
    Dictionary<string, TextMeshProUGUI> t_residuesDictB;
    public GameObject _deathPieceObj;
    public SquereID _selectedSquereID;
    public GameObject _selectedPieceObj;
    void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _turnDeside = GetComponent<TurnDeside>();
        _deathPieceObj = null;
        t_residuesDictW = t_residuesW.ToDictionary(t => t.name.Last().ToString(), t => t);
        t_residuesDictB = t_residuesB.ToDictionary(t => t.name.Last().ToString(), t => t);
    }
    /// <summary>
    /// TurnStartAnimation再生時に一度だけ呼び出される
    /// </summary>
    void UpdateTurnCountUI()
    {
        t_deceptionMoveCount.text = _inGameManager._IsWhite? (_inGameManager._WhiteTurnCount -1).ToString() : (_inGameManager._BlackTurnCount -1).ToString();
        t_truthMoveCount.text = _inGameManager._IsWhite? _inGameManager._WhiteTurnCount.ToString() : _inGameManager._BlackTurnCount.ToString();
        int SquereIndex = int.Parse(_selectedPieceObj.name.Substring(8));
        TextMeshProUGUI t_squereId = _inGameManager._IsWhite? t_squeresW[SquereIndex] : t_squeresB[SquereIndex];
        t_squereId.text = _selectedSquereID.ToString();
        if (_deathPieceObj != null)
        {
            int ripSquereIndex = int.Parse(_deathPieceObj.name.Substring(8));
            TextMeshProUGUI t_ripSquere = _inGameManager._IsWhite? t_squeresW[ripSquereIndex] : t_squeresB[ripSquereIndex];
            t_ripSquere.gameObject.SetActive(false);
            string search = _deathPieceObj.name.First().ToString();
            TextMeshProUGUI t_influenceRediues = _deathPieceObj.CompareTag("White")? t_residuesDictW[search] : t_residuesDictB[search];
            t_influenceRediues.text = (int.Parse(t_influenceRediues.text) - 1).ToString();
            _deathPieceObj = null;
        }
    }
    public void DesidePromotion(string promotionName)
    {
        GameObject promotionObj = Instantiate(Resources.Load<GameObject>($"Objects/{promotionName}"), _turnDeside._promotionObj.transform.position, _turnDeside._promotionObj.transform.rotation);
        promotionObj.transform.localScale = _turnDeside._promotionObj.transform.localScale;
        promotionObj.GetComponent<SpriteRenderer>().flipX = !_inGameManager._IsWhite;
        promotionObj.name = promotionName + _turnDeside._promotionObj.name.Substring(1);
        Destroy(_turnDeside._promotionObj);
        _inGameManager.StartInactivePromotionRelay();
    }
}
