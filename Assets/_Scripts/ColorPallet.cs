using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPallet : MonoBehaviour
{
    Color _canSelectedTileColor = new Color32(255, 255, 0, 50);
    Color _selectedTileColor = Color.yellow;
    protected Color _CanSelectedTileColor => _canSelectedTileColor;
    protected Color _SelectedTileColor => _selectedTileColor;
    protected Color ChangeAlpha(Color color, int alpha) => new (color.r, color.g, color.b, (float)alpha / 255);
}