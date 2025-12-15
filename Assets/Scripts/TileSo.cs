using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "TileData", menuName = "WFC/TileData")]
public class TileSo : ScriptableObject
{
    public TileType tileType;
    //public GameObject prefab;
    public Sprite sprite;
    //public Color tileColor = Color.white;
    
    // 连接规则：上、右、下、左
    public TileType[] upConnections;
    public TileType[] downConnections;
    public TileType[] leftConnections;
    public TileType[] rightConnections;
 
    
    // 检查是否可以连接
    public bool CanConnectTo(TileType otherTile, int direction)
    {
        TileType[] connections = GetConnectionsForDirection(direction);
        
        if (connections == null || connections.Length == 0)
            return false;
            
        foreach (TileType allowedType in connections)
        {
            if (allowedType == otherTile)
                return true;
        }
        
        return false;
    }
    
    private TileType[] GetConnectionsForDirection(int direction)
    {
        switch (direction)
        {
            case 0: return upConnections;    // 上
            case 1: return downConnections; // 右
            case 2: return rightConnections;  // 下
            case 3: return leftConnections;  // 左
            default: return null;
        }
    }
}