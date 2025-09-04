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
    [SerializeField] Camera _rayCamera;
    Tilemap _tilemap;
    // TileBase _canSelectedTileBase;
    // TileBase _SelectedTileBase;
    SpriteRenderer _currentSpriteRenderer; 
    SpriteRenderer _beforeSpriteRenderer; 
    // Vector3Int _beforeTilePos = default;
    OpenSelectableArea _openSelectableArea;
    private int _maskID;
    void Awake()
    {
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _tilemap = _openSelectableArea._Tilemap;
        _maskID = LayerMask.GetMask("DeceptionTileField");
        enabled = false;
    }
    /// <summary>
    /// _canSelectedTileBaseの上にカーソルがあるときのみ_selectedTileBaseが描画される
    /// </summary>
    void Update()
    {
        //毎フレームマウスのポジションを記録し、カーソルの下にあるタイルベースも更新する。
        Vector3 mousePos = Mouse.current.position.ReadValue();
        int mousePosZ = 0;
        if (mousePos.y >= 160)
        {
            mousePosZ = 9;
        }
        else
        {
            mousePosZ = 6;
        }
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mousePosZ));//7 ~ 9
        Debug.Log(mousePos.y);
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
                    _openSelectableArea.TurnDesideRelay();
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
