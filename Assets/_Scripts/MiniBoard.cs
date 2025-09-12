using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBoard : MonoBehaviour //InGamemanagerを継承しても良いかもしれない
{
    InGameManager _inGameManager;

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
    public static ulong _MiniBoard { get => _miniBoard; set {_miniBoard = value; UpdateMiniBorad(value);}}
    static ulong _MiniBoardWA { get => _miniBoardWA; set => _miniBoardWA = value; }
    static ulong _MiniBoardWP { get => _miniBoardWP; set => _miniBoardWP = value; }
    static ulong _MiniBoardWR { get => _miniBoardWR; set => _miniBoardWR = value; }
    static ulong _MiniBoardWN { get => _miniBoardWN; set => _miniBoardWN = value; }
    static ulong _MiniBoardWB { get => _miniBoardWB; set => _miniBoardWB = value; }
    static ulong _MiniBoardWQ { get => _miniBoardWQ; set => _miniBoardWQ = value; }
    static ulong _MiniBoardWK { get => _miniBoardWK; set => _miniBoardWK = value; }
    static ulong _MiniBoardBA { get => _miniBoardBA; set => _miniBoardBA = value; }
    static ulong _MiniBoardBP { get => _miniBoardBP; set => _miniBoardBP = value; }
    static ulong _MiniBoardBR { get => _miniBoardBR; set => _miniBoardBR = value; }
    static ulong _MiniBoardBN { get => _miniBoardBN; set => _miniBoardBN = value; }
    static ulong _MiniBoardBB { get => _miniBoardBB; set => _miniBoardBB = value; }
    static ulong _MiniBoardBQ { get => _miniBoardBQ; set => _miniBoardBQ = value; }
    static ulong _MiniBoardBK { get => _miniBoardBK; set => _miniBoardBK = value; }
    
    //ShortChastingの条件
    //a0 == R, b0 == 0, c0 == 0, d0 == K 
    //LongChastingの条件
    void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _squereArrays = _inGameManager._SquereArrays; //miniBord上でのPos 攻撃範囲が入っている（？）
    }

    public static int count = 0;
    /// <summary>
    /// W / B 両方のMiniBpradを更新する必要がある → 自陣の駒に変化があった場合のみ更新すれば良い
    /// TurnDesideの最後 OR _squereArrays.IsOnPieceに変化があったとき → 状態管理が難しいので前者を採用する
    /// 駒が倒された場合、OnDestroyから通知
    /// 駒が移動した場合、IsOnPieceObjectから通知する → 
    /// 
    /// </summary>
    public static void UpdateMiniBorad(Squere square)
    {
        GameObject isOnPieceobj = square._IsOnPieceObj;
        //Squereの情報からどのミニボートに変更を加えるのかをやたらと判断する
        //PieceObjの名前からすべて判断可能
        //(種類) (number) (alphabet) (陣営)
        Queue<ulong> updateBorad = new Queue<ulong>();
        if (!isOnPieceobj)
        {
            //削除の時は呼ばなくて良い？
            // _MiniBoard = Ben(square._SquereID, _MiniBoard);
        }
        else
        {
            count++;
            _MiniBoard = Bin(square._SquereID, _MiniBoard);
        }
    }

    static void UpdateMiniBorad(ulong value) //ulong 123ULなど
    {
        // Debug.Log(value);
    }
    
    /// <summary>
    /// 指定したminiBorad内でsquereと該当しているbit座標の値を1に変換する
    /// </summary>
    /// <param name="squreID"></param>
    /// <param name="miniBorad"></param>
    static ulong Bin(SquereID squreID, ulong miniBorad)
    {
        Debug.Log((int)squreID);
        return miniBorad |= 1UL << (int)squreID; //
    }
    /// <summary>
    /// 指定したminiBorad内でsquereと該当しているbit座標の値を0変換する
    /// </summary>
    /// <param name="squre"></param>
    /// <param name="miniBorad"></param>
    static ulong Ben(SquereID squereID, ulong miniBorad)
    {
        return miniBorad &= 1Ul << (int)squereID;
    }
}
