using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MiniBoard))]
public class MiniBoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MiniBoard miniBoard = target as MiniBoard;
        if (GUILayout.Button("CheckSquereBit"))
        {
            ulong bits = MiniBoard._MiniBoard;
            string binaryStr = "";
            for (int i = 1; i < 64 + 1; i++)
            {
                binaryStr += ((bits >> i - 1) & 1UL) == 1 ? "1" : "0";
                if (i % 8 == 0) binaryStr += " "; // 読みやすさのため8ビットごとに区切る
            }
            Debug.Log(binaryStr);
            // Debug.Log(MiniBoard.count);
            // Debug.Log(Convert.ToString((int)SquereID.a4, 2));
        }
        
    }
    

}
