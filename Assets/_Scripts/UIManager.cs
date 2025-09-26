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
    public GameObject _CastlingRookPieceObj {get;set;}
    bool _isPromotion;
    void Awake()
    {
        _inGameManager = GetComponent<InGameManager>();
        _turnDeside = GetComponent<TurnDeside>();
        _DeathPieceObj = null;
        _isPromotion = false;
        // _isPromotion = true;//デバッグ
        t_residuesDictW = t_residuesCountW.ToDictionary(t => t.name.Last().ToString(), t => t);
        t_residuesDictB = t_residuesCountB.ToDictionary(t => t.name.Last().ToString(), t => t);
        //(KeyGroupCollection(Key, Value[])) ← Valueが配列なのでToArrayが必要
        t_squereIdsDictW = t_squereIdsW.GroupBy(t => t.name[3].ToString()).ToDictionary(gc => gc.Key, gc => gc.ToList());
        t_squereIdsDictB = t_squereIdsB.GroupBy(t => t.name[3].ToString()).ToDictionary(gc => gc.Key, gc => gc.ToList());
        //すべてのDicのKeyは同一にしてあるが、コードが悪いのでそれがわかりずらい
    }
    public void ActiveFadePanel()
    {
        _fadePanel.SetActive(true);
    }
    public void InactiveFadePanel()
    {
        _fadePanel.SetActive(false);
    }
    /// <summary>
    /// MultiPlayButtonを押した時に一度だけ呼び出される
    /// </summary>
    public void StartMulti()
    {
        _inGameManager._AnimatorController.Play("StartMulti");
    }
    /// <summary>
    /// ターンが切り替わった時、InGameManagerから一度だけ呼び出される。TurnBegin.csの後に呼び出される
    /// </summary>
    public void StartTurnUI()
    {
        //t_turnCountの更新
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
        //t_residues の更新
        if (_DeathPieceObj)
        {
            //DeatPieceObjなのでIsWhiteが逆になる
            TextMeshProUGUI t_decreaseResiduesCount = _inGameManager.IsWhite? t_residuesDictB[_DeathPieceObj.name.First().ToString()] : t_residuesDictW[_DeathPieceObj.name.First().ToString()];
            t_decreaseResiduesCount.text = (int.Parse(t_decreaseResiduesCount.text) - 1).ToString();
            //t_residues の更新にともなってt_squereIdを削除する
            TextMeshProUGUI[] t_decreaseSquereIds = _inGameManager.IsWhite? t_squereIdsDictB[_DeathPieceObj.name.First().ToString()].ToArray() : t_squereIdsDictW[_DeathPieceObj.name.First().ToString()].ToArray();
            TextMeshProUGUI t_decreaseSquereId = t_decreaseSquereIds.FirstOrDefault(t => t.name.Contains(_DeathPieceObj.name.Last().ToString()));
            t_decreaseSquereId.gameObject.SetActive(false);
            _DeathPieceObj = null;
        }
        if (_isPromotion)
        {
            //自身が属している駒カテゴリのTMPをエディタ上で移動させる → t_residuesの子に設定すれば良い
            TextMeshProUGUI t_decreaseResiduesCount = _inGameManager.IsWhite? t_residuesDictW["P"] : t_residuesDictB["P"];
            t_decreaseResiduesCount.text = (int.Parse(t_decreaseResiduesCount.text) - 1).ToString();
            //t_residues の更新にともなってt_squereIdをプロモーション後のUIに移動させる
            TextMeshProUGUI[] t_shiftSquereIds = _inGameManager.IsWhite? t_squereIdsDictW["P"].ToArray() : t_squereIdsDictB["P"].ToArray();
            TextMeshProUGUI t_shiftSquereId = t_shiftSquereIds.FirstOrDefault(t => t.name.Contains(_TargetPieceObj.name.Last().ToString())); //name.last == PieceObjID
            t_shiftSquereId.gameObject.transform.SetParent(t_decreaseResiduesCount.gameObject.transform); //移動するのかどうかを見る必要あり
            t_shiftSquereId.text = _TargetSquere._SquereID.ToString();
            // t_shiftSquereId.gameObject.SetActive(false);
            if (_inGameManager.IsWhite)
            {
                t_squereIdsDictW[_TargetPieceObj.name.First().ToString()].Add(t_shiftSquereId);
            }
            else
            {
                t_squereIdsDictB[_TargetPieceObj.name.First().ToString()].Add(t_shiftSquereId);
            }
            _isPromotion = false;
        }
        else if(_inGameManager._TurnCount != 1)
        {
            //t_squereIdの更新
            string[] search = _TargetPieceObj.name.Split('_'); //Key[0] Index[4]
            List<TextMeshProUGUI> t_squereIds = _inGameManager.IsWhite? t_squereIdsDictB[search[0]] : t_squereIdsDictW[search[0]];
            TextMeshProUGUI t_squereId = t_squereIds[int.Parse(search[4])];
            t_squereId.text = _TargetSquere._SquereID.ToString();
            if (_CastlingRookSquere != null)
            {
                search = _CastlingRookSquere._IsOnPieceObj.name.Split('_'); //Key[0] Index[4]
                t_squereIds = _inGameManager.IsWhite? t_squereIdsDictB[search[0]] : t_squereIdsDictW[search[0]];
                t_squereId = t_squereIds[int.Parse(search[4])];
                t_squereId.text = _CastlingRookSquere._SquereID.ToString();
                _CastlingRookSquere = null;
            }
        }
    }
    /// <summary>
    /// _turnDeside._selectedPieceObj を プロモーション後のPieceObjに置き換えるだけのメソッド
    /// </summary>
    /// <param name="promotionName"></param>
    public void DesidePromotion(string promotionName)
    {
        _isPromotion = true;
        GameObject promotionObj = Instantiate(Resources.Load<GameObject>($"Objects/{promotionName}"), _TargetPieceObj.transform.position, _TargetPieceObj.transform.rotation);
        promotionObj.transform.localScale = _TargetPieceObj.transform.localScale;
        Debug.Log(promotionObj.transform.localScale);
        promotionObj.GetComponent<SpriteRenderer>().flipX = !_inGameManager.IsWhite;
        promotionObj.name = promotionName + _TargetPieceObj.name.Substring(1);
        EventTrigger.Entry entry = new EventTrigger.Entry{eventID = EventTriggerType.PointerClick};
        // 実行したい処理を登録
        entry.callback.AddListener(e => _inGameManager.PieceObjectPressed(promotionObj));
        promotionObj.GetComponent<EventTrigger>().triggers.Add(entry);
        Destroy(_TargetPieceObj);
        _TargetPieceObj = promotionObj;
        //このあとの処理の挙動を見た方が良い
        // _TargetPieceObj.SetActive(false);
        _TargetSquere._IsOnPieceObj = promotionObj;
        _inGameManager.StartInactivePromotionRelay();
    }

    void AdjustUI()
    {
        _pieceIconWhite.SetActive(true);
        _pieceIconBlack.SetActive(true);
    }
}
