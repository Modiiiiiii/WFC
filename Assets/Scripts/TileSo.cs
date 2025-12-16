using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "TileData", menuName = "WFC/TileData")]
public class TileSo : ScriptableObject
{
    public TileType tileType;
    //public Sprite sprite;
    public Seam[] allConnections;//上右下左 顺时针 
}