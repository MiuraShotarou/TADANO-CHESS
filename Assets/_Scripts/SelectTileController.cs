using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
/// <summary>
/// raycastで取得したオブジェクトからSpriteRendererを取得して、alpha値がColor32()の50であれば255の値を、255の値であれば50の値を代入する
/// </summary>
public class SelectTileController : ColorPallet
{
    SpriteRenderer _currentSpriteRenderer;
    SpriteRenderer _beforeSpriteRenderer;
    InGameManager _inGameManager;
    OpenSelectableArea _openSelectableArea;
    UIManager _uiManager;
    Camera _camera;
    int _maskID;
    RaycastHit2D _memoraizedHit;
    void Awake()
    {
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _inGameManager = GetComponent<InGameManager>();
        _uiManager = GetComponent<UIManager>();
        _camera = Camera.main;
        _maskID = LayerMask.GetMask("DeceptionTileField");
    }
    /// <summary>
    /// _canSelectedTileBaseの上にカーソルがあるときのみ_selectedTileBaseが描画される
    /// </summary>
    void Update()
    {
        //毎フレームマウスのポジションを記録し、カーソルの下にあるタイルベースも更新する。
        Vector3 mousePos = Mouse.current.position.ReadValue();
        int mouseX;
        int mousePosZ;
        if (mousePos.y >= 200)
        {
            mousePosZ = 9;
        }
        else
        {
            mousePosZ = 6;
        }
        if (mousePos.x >= 200)
        {
            
        }
        Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mousePosZ));//7 ~ 9
        RaycastHit2D hit2D = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 15, _maskID);
        if (!hit2D)
        {
            if (_beforeSpriteRenderer)
            {
                _beforeSpriteRenderer.color = _CanSelectedTileColor;
                _beforeSpriteRenderer = null;
            }
        }
        else
        {
            if (hit2D.transform.gameObject.name.Length == 3)
            {
                _currentSpriteRenderer = hit2D.transform.gameObject.GetComponent<SpriteRenderer>();
                Color32 checkColor  = _currentSpriteRenderer.color;
                if (checkColor.a == 50)//
                {
                    if (!_beforeSpriteRenderer) _beforeSpriteRenderer = _currentSpriteRenderer;
                    _beforeSpriteRenderer.color = _CanSelectedTileColor;
                    _currentSpriteRenderer.color = _SelectedTileColor;
                    _beforeSpriteRenderer = _currentSpriteRenderer;
                }
                //SelectedTileBaseの上でマウスをクリックすると一度だけ呼び出される
                else if (checkColor.a == 255 && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _openSelectableArea.TurnDesideRelay(_currentSpriteRenderer);//
                    enabled = false;
                }
            }
        }
        //CanSelectedTileBaseにカーソルを合わせた時、原理的に一度だけ呼び出される。
    }
    void OnDisable()
    {
        _beforeSpriteRenderer = null;
    }
}
