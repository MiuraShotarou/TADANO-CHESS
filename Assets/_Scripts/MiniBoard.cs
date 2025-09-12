using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MiniBoard : MonoBehaviour //InGamemanagerを継承しても良いかもしれない
{
    InGameManager _inGameManager;
    static SquereID _squereID;
    static string _pieceID;
    private Squere[][] _squereArrays; //s or InstanceClass
    //同じ方式で考える
    //miniBordの強いところは敵AIで利用可能な新しいアーキテクチャを持てること
    //どこになんの駒があるかをどうやって判断するのか → GameObjectにInstanceClassを持たせどこのマスに誰がいるのかを監視する → TurnDesideからの通知で毎回判断することはできる
    //
    //IsOnPieceで確実にBit == 1は判断できる → SquereのプロパティにEventを追加する
    //AttackAreasで攻撃範囲を正確に判断できる → 
    static ulong _miniBoard = 0UL; //MiniBorad全体 駒があるかどうか
    static ulong _miniBoardWA = 0UL; //White陣営 駒があるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardWP = 0UL; //White陣営 Pがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardWR = 0UL; //White陣営 Rがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardWN = 0UL; //White陣営 Nがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardWB = 0UL; //White陣営 Bがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardWQ = 0UL; //White陣営 Qがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardWK = 0UL; //White陣営 Kがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardBA = 0UL; //Black陣営 駒があるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardBP = 0UL; //Black陣営 Pがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardBR = 0UL; //Black陣営 Rがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardBN = 0UL; //Black陣営 Nがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardBB = 0UL; //Black陣営 Bがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardBQ = 0UL; //Black陣営 Qがあるかどうか / 攻撃できるマスの判断
    static ulong _miniBoardBK = 0UL; //Black陣営 Kがあるかどうか / 攻撃できるマスの判断
    public static ulong _MiniBoard { get => _miniBoard; set { if(_miniBoard != value){ _miniBoard = value; UpdateMiniBoard(value);}}}
    static ulong _MiniBoardWA { get => _miniBoardWA; set { if(_miniBoardWA != value){ _miniBoardWA = value; UpdateMiniBoardW(value);}} }
    static ulong _MiniBoardWP { get => _miniBoardWP; set { if(_miniBoardWP != value){ _miniBoardWP = value; UpdateMiniBoardW(value);}} }
    static ulong _MiniBoardWR { get => _miniBoardWR; set { if(_miniBoardWR != value){ _miniBoardWR = value; UpdateMiniBoardW(value);}} }
    static ulong _MiniBoardWN { get => _miniBoardWN; set { if(_miniBoardWN != value){ _miniBoardWN = value; UpdateMiniBoardW(value);}} }
    static ulong _MiniBoardWB { get => _miniBoardWB; set { if(_miniBoardWB != value){ _miniBoardWB = value; UpdateMiniBoardW(value);}} }
    static ulong _MiniBoardWQ { get => _miniBoardWQ; set { if(_miniBoardWQ != value){ _miniBoardWQ = value; UpdateMiniBoardW(value);}} }
    static ulong _MiniBoardWK { get => _miniBoardWK; set { if(_miniBoardWK != value){ _miniBoardWK = value; UpdateMiniBoardW(value);}} }
    static ulong _MiniBoardBA { get => _miniBoardBA; set { if(_miniBoardBA != value){ _miniBoardBA = value; UpdateMiniBoardB(value);}} }
    static ulong _MiniBoardBP { get => _miniBoardBP; set { if(_miniBoardBP != value){ _miniBoardBP = value; UpdateMiniBoardB(value);}} }
    static ulong _MiniBoardBR { get => _miniBoardBR; set { if(_miniBoardBR != value){ _miniBoardBR = value; UpdateMiniBoardB(value);}} }
    static ulong _MiniBoardBN { get => _miniBoardBN; set { if(_miniBoardBN != value){ _miniBoardBN = value; UpdateMiniBoardB(value);}} }
    static ulong _MiniBoardBB { get => _miniBoardBB; set { if(_miniBoardBB != value){ _miniBoardBB = value; UpdateMiniBoardB(value);}} }
    static ulong _MiniBoardBQ { get => _miniBoardBQ; set { if(_miniBoardBQ != value){ _miniBoardBQ = value; UpdateMiniBoardB(value);}} }
    static ulong _MiniBoardBK { get => _miniBoardBK; set { if(_miniBoardBK != value){ _miniBoardBK = value; UpdateMiniBoardB(value);}} }
    
    //ShortChastingの条件
    //a0 == R, b0 == 0, c0 == 0, d0 == K 
    //LongChastingの条件
    void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _squereArrays = _inGameManager._SquereArrays; //miniBord上でのPos 攻撃範囲が入っている（？）
    }

    public static void StartUpdateMiniBorad(Squere square)
    {
        if (square._IsOnPieceObj)
        {
            _squereID = square._SquereID;
            _pieceID = square._IsOnPieceObj.name.First().ToString();
            _MiniBoard = Bin(_squereID, _MiniBoard);
        }
    }
    public static int count = 0;
    /// <summary>
    /// W / B 両方のMiniBpradを更新する必要がある → 自陣の駒に変化があった場合のみ更新すれば良い
    /// TurnDesideの最後 OR _squereArrays.IsOnPieceに変化があったとき → 状態管理が難しいので前者を採用する
    /// 駒が倒された場合、OnDestroyから通知
    /// 駒が移動した場合、IsOnPieceObjectから通知する → 
    /// 
    /// </summary>
    public static void UpdateMiniBoard(Squere square)
    {
        GameObject isOnPieceobj = square._IsOnPieceObj;
        _squereID = square._SquereID;
        //Squereの情報からどのミニボートに変更を加えるのかをやたらと判断する
        //PieceObjの名前からすべて判断可能
        //(種類) (number) (alphabet) (陣営)
        if (isOnPieceobj)
        {
            _pieceID = isOnPieceobj.name.First().ToString();
            _MiniBoard = Bin(_squereID, _MiniBoard);
        }
        else
        {
            // 削除の時は呼ばなくて良い？
            _MiniBoard = Bon(_squereID, _MiniBoard);
        }
    }

    static void UpdateMiniBoard(ulong value) //ulong 123ULなど
    {
        if (InGameManager._IsWhite)
        {
            //isOnPieceobj == true
            if (value != 0)
            {
                //Whiteが移動した後
                _MiniBoardWA = Bin(_squereID, _MiniBoardWA);
            }
            //isOnPieceobj == false 削除の処理
            else
            {
                //Whiteが移動した直後 →
                //Blackがやられている → 必ず両方起動 → 欠陥アリ
                _MiniBoardWA = Bon(_squereID, _MiniBoardWA);
                _miniBoardBA = Bon(_squereID, _MiniBoardBA);
            }
        }
        else
        {
            if (value != 0)
            {                
                //Blackが移動した後
                _MiniBoardBA = Bin(_squereID, _MiniBoardWA);
            }
            else
            {
                //Whiteがやられている or Blackが移動した直後
            }
        }
        
        Debug.Log(value);
    }

    static void UpdateMiniBoardW(ulong value)
    {
        //value == 0のときのことを考えて、駒があるところのboradだけ書き換える処理を描く必要がある
        if (value != 0)
        {
            switch (_pieceID) //search[0], [3]を呼べば余分な処理を防げる
            {
                case "P":
                    _MiniBoardWP = Bin(_squereID, _MiniBoardWP);
                    break;
                case "R":
                    _MiniBoardWR = Bin(_squereID, _MiniBoardWR);
                    break;
                case "N":
                    _MiniBoardWN = Bin(_squereID, _MiniBoardWN);
                    break;
                case "B":
                    _MiniBoardWB = Bin(_squereID, _MiniBoardWB);
                    break;
                case "Q":
                    _miniBoardWQ = Bin(_squereID, _MiniBoardWQ);
                    break;
                case "K":
                    _MiniBoardWK = Bin(_squereID, _MiniBoardWK);
                    break;
            }
        }
        else
        {
            switch (_pieceID)
            {
                case "P":
                    _MiniBoardWP = Bon(_squereID, _MiniBoardWP);
                    break;
                case "R":
                    _MiniBoardWR = Bon(_squereID, _MiniBoardWR);
                    break;
                case "N":
                    _MiniBoardWN = Bon(_squereID, _MiniBoardWN);
                    break;
                case "B":
                    _MiniBoardWB = Bon(_squereID, _MiniBoardWB);
                    break;
                case "Q":
                    _miniBoardWQ = Bon(_squereID, _MiniBoardWQ);
                    break;
                case "K":
                    _MiniBoardWK = Bon(_squereID, _MiniBoardWK);
                    break;
            }
        }
    }
    static void UpdateMiniBoardB(ulong value)
    {
        
    }
    /// <summary>
    /// 指定したminiBorad内でsquereと該当しているbit座標の値を1に変換する
    /// </summary>
    /// <param name="squreID"></param>
    /// <param name="miniBorad"></param>
    static ulong Bin(SquereID squreID, ulong miniBorad)
    {
        return miniBorad |= 1UL << (int)squreID; //
    }
    /// <summary>
    /// 指定したminiBorad内でsquereと該当しているbit座標の値を0変換する
    /// </summary>
    /// <param name="squre"></param>
    /// <param name="miniBorad"></param>
    static ulong Bon(SquereID squereID, ulong miniBorad)
    {
        return miniBorad &= 1Ul << (int)squereID;
    }
}
