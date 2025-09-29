using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UIのバックエンドを担当
/// </summary>
public class UIManager : ColorPallet
{
    InGameManager _inGameManager;
    TurnDeside _turnDeside;
    [SerializeField] GameObject _fadePanel;
    [SerializeField] GameObject _pieceIconWhite;
    [SerializeField] GameObject _pieceIconBlack;
    [SerializeField] GameObject _countMask;
    [SerializeField] TextMeshProUGUI t_deceptionMoveCount;
    [SerializeField] TextMeshProUGUI t_truthMoveCount;
    [SerializeField] TextMeshProUGUI[] t_residuesCountW;
    [SerializeField] TextMeshProUGUI[] t_residuesCountB;
    [SerializeField] TextMeshProUGUI[] t_squereIdsW;
    [SerializeField] TextMeshProUGUI[] t_squereIdsB;
    Dictionary<string, TextMeshProUGUI> t_residuesDictW;
    Dictionary<string, TextMeshProUGUI> t_residuesDictB;
    Dictionary<string, List<TextMeshProUGUI>> t_squereIdsDictW;
    Dictionary<string, List<TextMeshProUGUI>> t_squereIdsDictB;
    public GameObject _DeathPieceObj {get;set;}
    public Squere _TargetSquere {get;set;}
    public GameObject _TargetPieceObj {get;set;}
    public Squere _CastlingRookSquere {get;set;}
    // public GameObject _CastlingRPieceObj {get;set;}
    // bool _isPromotion;
    void Awake()
    {
        _inGameManager = GetComponent<InGameManager>();
        _turnDeside = GetComponent<TurnDeside>();
        _DeathPieceObj = null;
        // _isPromotion = false;
        // _isPromotion = true;//デバッグ
        t_residuesDictW = t_residuesCountW.ToDictionary(t => t.name.Last().ToString(), t => t);
        t_residuesDictB = t_residuesCountB.ToDictionary(t => t.name.Last().ToString(), t => t);
        //(KeyGroupCollection(Key, Value[])) ← Valueが配列なのでToArrayが必要
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
        t_deceptionMoveCount.text = (_inGameManager._TurnCount - 1).ToString();
        t_truthMoveCount.text = _inGameManager._TurnCount.ToString();
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
            // t_squereIdsDictW["P"].Remove(t_shiftSquereId); Indexが変わってしまう
            t_squereIdsDictW[_TargetPieceObj.name.First().ToString()].Add(t_shiftSquereId);
        }
        else
        {
            // t_squereIdsDictB["P"].Remove(t_shiftSquereId);
            t_squereIdsDictB[_TargetPieceObj.name.First().ToString()].Add(t_shiftSquereId);
        }
        // _isPromotion = false;
    }
    /// <summary>
    /// _turnDeside._selectedPieceObj を プロモーション後のPieceObjに置き換えるだけのメソッド
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
    /// <summary>
    /// 
    /// </summary>
    public void UpdateMultiPlayerGroupUI()
    {
        
    }
    
    public void ActiveFadePanel()
    {
        _fadePanel.SetActive(true);
    }
    public void InactiveFadePanel()
    {
        _fadePanel.SetActive(false);
    }
    void AdjustUI()
    {
        _pieceIconWhite.SetActive(true);
        _pieceIconBlack.SetActive(true);
    }
}
