using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// UIのバックエンドを担当
/// </summary>
public class UIManager : MonoBehaviour
{
    InGameManager _inGameManager;
    [SerializeField] TextMeshProUGUI _TmoveCount;

    void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
    }
    void ActivePromotion()
    {
        
    }
}
