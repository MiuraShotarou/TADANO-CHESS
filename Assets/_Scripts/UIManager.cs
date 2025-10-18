using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
/// <summary>
/// UIのバックエンドを担当
/// </summary>
public class UIManager : ColorPallet
{
    InGameManager _inGameManager;
    [SerializeField] GameObject _fadePanel;
    [SerializeField] GameObject _pieceIconWhite;
    [SerializeField] GameObject _pieceIconBlack;
    [SerializeField] Image i_activeTurnWhite;
    [SerializeField] Image i_activeTurnBlack;
    [SerializeField] TextMeshProUGUI t_decideGroup;
    [SerializeField] TextMeshProUGUI t_yourGroup;
    [SerializeField] Image i_countMask;
    [SerializeField] TextMeshProUGUI t_deceptionMoveCount;
    [SerializeField] TextMeshProUGUI t_truthMoveCount;
    [SerializeField] TextMeshProUGUI t_checkWhite;
    [SerializeField] TextMeshProUGUI t_checkBlack;
    [SerializeField] TextMeshProUGUI t_checkMateWhite;
    [SerializeField] TextMeshProUGUI t_checkMateBlack;
    [SerializeField] TextMeshProUGUI t_pinchWhite;
    [SerializeField] TextMeshProUGUI t_pinchBlack;
    [SerializeField] TextMeshProUGUI[] t_residuesCountW;
    [SerializeField] TextMeshProUGUI[] t_residuesCountB;
    [SerializeField] TextMeshProUGUI[] t_squereIdsW;
    [SerializeField] TextMeshProUGUI[] t_squereIdsB;
    [SerializeField] TextMeshProUGUI t_resultGroup;
    [SerializeField] TextMeshProUGUI t_resultMessage;
    Dictionary<string, TextMeshProUGUI> t_residuesDictW;
    Dictionary<string, TextMeshProUGUI> t_residuesDictB;
    Dictionary<string, List<TextMeshProUGUI>> t_squereIdsDictW;
    Dictionary<string, List<TextMeshProUGUI>> t_squereIdsDictB;
    public GameObject _DeathPieceObj {get;set;}
    public Squere _TargetSquere {get;set;}
    public GameObject _TargetPieceObj {get;set;}
    public Squere _CastlingRookSquere {get;set;}
    void Awake()
    {
        _inGameManager = GetComponent<InGameManager>();
        _DeathPieceObj = null;
        t_residuesDictW = t_residuesCountW.ToDictionary(t => t.name.Last().ToString(), t => t);
        t_residuesDictB = t_residuesCountB.ToDictionary(t => t.name.Last().ToString(), t => t);
        t_squereIdsDictW = t_squereIdsW.GroupBy(t => t.name[3].ToString()).ToDictionary(gc => gc.Key, gc => gc.ToList());
        t_squereIdsDictB = t_squereIdsB.GroupBy(t => t.name[3].ToString()).ToDictionary(gc => gc.Key, gc => gc.ToList());
        //すべてのDicのKeyは同一にしてあるが、コードが悪いのでそれがわかりずらい
    }
    /// <summary>
    /// ターンが切り替わった時、InGameManagerから一度だけ呼び出される。TurnBegin.csの後に呼び出される
    /// </summary>
    public void StartUpdateTurnUI()
    {
        UpdateTurnCountUI();
        UpdateActiveTurnUI();
        if (_DeathPieceObj)
        {
            DecreaseDeathPieceUI();
        }
        if (_CastlingRookSquere)
        {
            UpdateRPieceUI();
        }
        if (_TargetPieceObj)
        {
            UpdateAllySquereIDUI();
        }
    }
    /// <summary>
    /// t_turnCountの更新
    /// </summary>
    void UpdateTurnCountUI()
    {
        t_deceptionMoveCount.text = (_inGameManager.TurnCount - 1).ToString();
        t_truthMoveCount.text = _inGameManager.TurnCount.ToString();
        if (_inGameManager.IsWhite)
        {
            t_deceptionMoveCount.color = Color.black;
            t_deceptionMoveCount.fontStyle = FontStyles.Bold;
            t_truthMoveCount.color = Color.white;
            t_truthMoveCount.fontStyle = FontStyles.Normal;
        }
        else
        {
            t_deceptionMoveCount.color = Color.white;
            t_deceptionMoveCount.fontStyle = FontStyles.Normal;
            t_truthMoveCount.color = Color.black;
            t_truthMoveCount.fontStyle = FontStyles.Bold;
        }
        if (_inGameManager.TurnCount % 10 == 0)
        {
            int baseWidth = 10;
            int baseHeight = 30;
            i_countMask.rectTransform.sizeDelta = new Vector2(baseWidth * _inGameManager.TurnCount.ToString().Length, baseHeight);
        }
    }
    void UpdateActiveTurnUI()
    {
        if (_inGameManager.IsWhite)
        {
            i_activeTurnWhite.color = Color.white;
            i_activeTurnBlack.color = ChangeAlpha(Color.black, 50);
        }
        else
        {
            i_activeTurnWhite.color = ChangeAlpha(Color.white, 50);
            i_activeTurnBlack.color = Color.black;
        }
    }
    public void UpdateCheckUI()
    {
        if (_inGameManager.IsCheck)
        {
            if (!_inGameManager.IsWhite)
            {
                t_checkWhite.gameObject.SetActive(true);
                t_checkBlack.gameObject.SetActive(false);
                t_pinchWhite.gameObject.SetActive(false);
                t_pinchBlack.gameObject.SetActive(true);
            }
            else
            {
                t_checkWhite.gameObject.SetActive(false);
                t_checkBlack.gameObject.SetActive(true);
                t_pinchWhite.gameObject.SetActive(true);
                t_pinchBlack.gameObject.SetActive(false);
            }
        }
        else
        {
            t_checkWhite.gameObject.SetActive(false);
            t_checkBlack.gameObject.SetActive(false);
            t_pinchWhite.gameObject.SetActive(false);
            t_pinchBlack.gameObject.SetActive(false);
        }
    }
    public void UpdateCheckMateUI()
    {
        if (_inGameManager.IsCheckMate)
        {
            if (_inGameManager.IsWhite)
            {
                t_checkBlack.gameObject.SetActive(false);
                t_checkMateBlack.gameObject.SetActive(true);
            }
            else
            {
                t_checkWhite.gameObject.SetActive(false);
                t_checkMateWhite.gameObject.SetActive(true);
            }
        }
        else
        {
            
        }
    }
    /// <summary>
    /// 取られた駒が現れたとき、t_residuesを更新する
    /// </summary>
    void DecreaseDeathPieceUI()
    {
        TextMeshProUGUI t_decreaseResiduesCount = _inGameManager.IsWhite? t_residuesDictW[_DeathPieceObj.name.First().ToString()] : t_residuesDictB[_DeathPieceObj.name.First().ToString()];
        t_decreaseResiduesCount.text = (int.Parse(t_decreaseResiduesCount.text) - 1).ToString();
        //t_residues の更新にともなってt_squereIdを削除する
        TextMeshProUGUI[] t_decreaseSquereIds = _inGameManager.IsWhite? t_squereIdsDictW[_DeathPieceObj.name.First().ToString()].ToArray() : t_squereIdsDictB[_DeathPieceObj.name.First().ToString()].ToArray();
        TextMeshProUGUI t_decreaseSquereId = t_decreaseSquereIds[int.Parse(_DeathPieceObj.name.Last().ToString())];
        t_decreaseSquereId.gameObject.SetActive(false);
        Destroy(_DeathPieceObj);
        _DeathPieceObj = null;
    }

    void UpdateRPieceUI()
    {
        string[] search = _CastlingRookSquere._IsOnPieceObj.name.Split('_'); //Key[0] Index[4]
        List<TextMeshProUGUI> t_squereIds = _inGameManager.IsWhite? t_squereIdsDictB[search[0]] : t_squereIdsDictW[search[0]];
        TextMeshProUGUI t_squereId = t_squereIds[int.Parse(search[4])];
        t_squereId.text = _CastlingRookSquere._SquereID.ToString();
        _CastlingRookSquere = null;
    }
    /// <summary>
    /// t_squereIdの更新
    /// </summary>
    void UpdateAllySquereIDUI()
    {
        string[] search = _TargetPieceObj.name.Split('_'); //Key[0] Index[4]
        List<TextMeshProUGUI> t_squereIds = _inGameManager.IsWhite? t_squereIdsDictB[search[0]] : t_squereIdsDictW[search[0]];
        TextMeshProUGUI t_squereId = t_squereIds[int.Parse(search[4])];
        t_squereId.text = _TargetSquere._SquereID.ToString();
    }

    void StartGroupRollUI()
    {
        if (_inGameManager.IsPlayerTurn) //メインプレイヤーが白側の陣営になる
        {
            t_decideGroup.color = Color.white;
            t_decideGroup.text = "白";
            t_yourGroup.color = Color.white;
            t_yourGroup.text = "あなたは白\n先攻です";
            _inGameManager._AnimatorController.Play("GroupRollWhite");
        }
        else
        {
            t_decideGroup.color = Color.black;
            t_decideGroup.text = "黒";
            t_yourGroup.color = Color.black;
            t_yourGroup.text = "あなたは黒\n後攻です";
            _inGameManager._AnimatorController.Play("GroupRollBlack");
        }
    }

    void UpdatePromotionUI(string promotionName)
    {
        //自身が属している駒カテゴリのTMPをエディタ上で移動させる → t_residuesの子に設定すれば良い
        TextMeshProUGUI t_decreaseResiduesCount = _inGameManager.IsWhite? t_residuesDictW["P"] : t_residuesDictB["P"];
        t_decreaseResiduesCount.text = (int.Parse(t_decreaseResiduesCount.text) - 1).ToString();
        //t_residues の更新にともなってt_squereIdをプロモーション後のUIに移動させる
        TextMeshProUGUI[] t_shiftSquereIds = _inGameManager.IsWhite? t_squereIdsDictW["P"].ToArray() : t_squereIdsDictB["P"].ToArray();
        TextMeshProUGUI t_shiftSquereId = t_shiftSquereIds[int.Parse(_TargetPieceObj.name.Last().ToString())]; //name.last == PieceObjID
        //移動後にt_decreaseResiduesCountの親となるオブジェクトを取得
        TextMeshProUGUI t_addResiduesCount = _inGameManager.IsWhite ? t_residuesDictW[promotionName] : t_residuesDictB[promotionName];
        //親に割り当てる
        t_shiftSquereId.gameObject.transform.SetParent(t_addResiduesCount.gameObject.transform); //移動するのかどうかを見る必要あり
        //Pieceの数を増加(PromotionによってPone以外の駒の数が増えるため)
        t_addResiduesCount.text = (int.Parse(t_addResiduesCount.text) + 1).ToString();
        //t_shiftSquereID.textの更新
        t_shiftSquereId.text = _TargetSquere._SquereID.ToString(); //次のターン開始時に余計な上書きをされる恐れあり
        //内部的にDictionayValueの要素をひとつ追加し、Pの要素をひとつ現象させる
        if (_inGameManager.IsWhite)
        {
            t_squereIdsDictW[_TargetPieceObj.name.First().ToString()].Add(t_shiftSquereId);
        }
        else
        {
            t_squereIdsDictB[_TargetPieceObj.name.First().ToString()].Add(t_shiftSquereId);
        }
    }
    /// <summary>
    /// _turnDecide._selectedPieceObj を プロモーション後のPieceObjに置き換えるだけのメソッド
    /// </summary>
    /// <param name="promotionName"></param>
    public void DesidePromotion(string promotionName)
    {
        // _isPromotion = true;
        GameObject promotionObj = Instantiate(_inGameManager._PromotionDict[promotionName], _TargetPieceObj.transform.position, _TargetPieceObj.transform.rotation);
        promotionObj.transform.localScale = _TargetPieceObj.transform.localScale;
        promotionObj.GetComponent<SpriteRenderer>().flipX = !_inGameManager.IsWhite;
        promotionObj.GetComponent<SpriteRenderer>().color = _inGameManager.IsWhite? ChangeAlpha(Color.white, 150) : ChangeAlpha(Color.black, 150);
        promotionObj.tag = _TargetPieceObj.tag;
        promotionObj.name = promotionName + _TargetPieceObj.name.Substring(1).Replace(_TargetPieceObj.name.Last().ToString(), _inGameManager.IsWhite? t_residuesDictW[promotionName].text: t_residuesDictB[promotionName].text);
        EventTrigger.Entry entry = new EventTrigger.Entry{eventID = EventTriggerType.PointerClick};
        // 実行したい処理を登録
        entry.callback.AddListener(e => _inGameManager.PieceObjectPressed(promotionObj));
        promotionObj.GetComponent<EventTrigger>().triggers.Add(entry);
        promotionObj.SetActive(true);
        Destroy(_TargetPieceObj);
        _TargetPieceObj = promotionObj;
        //このあとの処理の挙動を見た方が良い
        _TargetSquere._IsOnPieceObj = promotionObj;
        UpdatePromotionUI(promotionName);
        _inGameManager.StartInactivePromotionRelay();
    }
    public void ActiveResultUI()
    {
        if (_inGameManager.IsWhite)
        {
            t_resultGroup.text = "Black";
            t_resultGroup.color = Color.black;
            t_resultMessage.color = Color.black;
        }
        else
        {
            t_resultGroup.text = "White";
            t_resultGroup.color = Color.white;
            t_resultMessage.color = Color.white;
        }
        _inGameManager._AnimatorController.Play("Result");
    }

    public void ReturnTitle()
    {
        _inGameManager._AnimatorController.Play("ReturnTitle");
    }
    public void TitleUI()
    {
        
    }
    /// <summary>
    /// 
    /// </summary>
    public void UpdateMultiPlayerGroupUI()
    {
        
    }

    void ActivePieceIconUI()
    {
        _pieceIconWhite.SetActive(true);
        _pieceIconBlack.SetActive(true);
    }

    void ActiveActiveTurnUI()
    {
        i_activeTurnWhite.gameObject.SetActive(true);
        i_activeTurnBlack.gameObject.SetActive(true);
    }
}
