using System;
using System.Collections.Generic;
using UnityEngine;
using Modi;

public class WfcGenerator : SingletonMono<WfcGenerator>
{
    [Header("生成设置")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public  Dictionary <string,TileSo> SoConfigDic = new Dictionary<string, TileSo>();//So配置
    public TileMono[,] grid;
    
    [Header("显示设置")]
    public float tileSize = 4f;

    [Header("预制体")]
    public GameObject prefab;
    
    
    
    void Start()
    {
        InitSoConfig();
        if (prefab != null)
        {
            Generate();
        }
    }

    [ContextMenu("Init SO Config")]
    public void InitSoConfig()
    {
        SoConfigDic.Clear();
        var all = Resources.LoadAll<TileSo>("So");
        for (int i = 0; i < all.Length; i++)
        {
            var so = all[i];
            var key = so.tileType.ToString();
            SoConfigDic[key] = so;
        }
    }

    [ContextMenu("Generate Grid")]
    public void Generate()
    {
        ClearGrid();
        grid = new TileMono[gridWidth, gridHeight];
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (prefab == null) return;
                var go = Instantiate(prefab, transform);
                go.name = $"Tile_{x}_{y}";
                go.transform.localPosition = new Vector3(x * tileSize, 0f, y * tileSize);
                var tile = go.GetComponent<TileMono>();
                tile.pos =  new Vector2(x, y);
                grid[x, y] = tile;
                tile.Candidates.Clear();
                foreach (var kv in SoConfigDic)
                {
                    tile.Candidates.Add(kv.Value.tileType);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var cam = Camera.main;
            if (cam == null) return;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f))
            {
                var clicked = hit.collider.gameObject;
                var typeName = clicked.name;
                var tile = clicked.GetComponentInParent<TileMono>();
                Debug.Log($"Click{tile.name}_{typeName}");
                if (tile != null && SoConfigDic.ContainsKey(typeName))
                {
                    CollapseTo(tile, typeName);
                }
            }
        }
    }

    void CollapseTo(TileMono tile, string typeName)
    {
        if (tile == null || tile.tileParent == null) return;
        tile.Choice(typeName);
    }

    void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
        grid = null;
    }
    
    public TileMono GetTile(int x, int y)
    {
        if (grid == null) return null;
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) return null;
        return grid[x, y];
    }
    
}
