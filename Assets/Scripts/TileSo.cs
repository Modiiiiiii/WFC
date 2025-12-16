using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "TileData", menuName = "WFC/TileData")]
public class TileSo : ScriptableObject
{
    public TileType tileType;
    //public Sprite sprite;
    public Seam[] AllConnections
    {
        get
        {
            return new[]
            {
                up,
                right,
                down,
                left
            };
        }
    } //上右下左 顺时针 
    public Seam up;
    public Seam right;
    public Seam down;
    public Seam left;
}