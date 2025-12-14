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
        //SetTileVisual(tile, typeName);
        tile.Choice(typeName);
        tile.Candidates.Clear();
        if (Enum.TryParse<TileType>(typeName, out var tt))
        {
            tile.Candidates.Add(tt);
            PropagateFrom(tile, tt);
        }
    }

    void SetTileVisual(TileMono tile, string typeName)
    {
        /*for (int i = 0; i < tile.tileParent.childCount; i++)
        {
            var child = tile.tileParent.GetChild(i);
            bool active = child.gameObject.name == typeName;
            child.gameObject.SetActive(active);
        }
        tile.isCollapsed = true;
        if (tile.show != null) tile.show.SetActive(false);*/
        tile.Choice(typeName);
    }

    void PropagateFrom(TileMono tile, TileType collapsed)
    {
        var q = new Queue<TileMono>();
        q.Enqueue(tile);
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            TileType curType = collapsed;
            if (cur.Candidates != null && cur.Candidates.Count == 1)
            {
                foreach (var t in cur.Candidates) { curType = t; break; }
            }
            string key = curType.ToString();
            if (!SoConfigDic.ContainsKey(key)) continue;
            var so = SoConfigDic[key];

            TryRestrictNeighbor(cur, 0, so.upConnections, q);
            TryRestrictNeighbor(cur, 1, so.downConnections, q);
            TryRestrictNeighbor(cur, 2, so.rightConnections, q);
            TryRestrictNeighbor(cur, 3, so.leftConnections, q);
        }
    }

    void TryRestrictNeighbor(TileMono cur, int dir, TileType[] allowed, Queue<TileMono> q)
    {
        int x = (int)cur.pos.x;
        int y = (int)cur.pos.y;
        int nx = x, ny = y;
        if (dir == 0) ny = y + 1;
        else if (dir == 1) ny = y - 1;
        else if (dir == 2) nx = x + 1;
        else if (dir == 3) nx = x - 1;
        var neighbor = GetTile(nx, ny);
        if (neighbor == null || allowed == null || allowed.Length == 0) return;
        var before = new HashSet<TileType>(neighbor.Candidates);
        var after = new HashSet<TileType>();
        for (int i = 0; i < allowed.Length; i++)
        {
            var t = allowed[i];
            if (before.Contains(t)) after.Add(t);
        }
        if (after.Count == 0) return;
        if (after.SetEquals(before)) return;
        neighbor.Candidates = after;
        if (!neighbor.isCollapsed && after.Count == 1)
        {
            foreach (var t in after)
            {
                SetTileVisual(neighbor, t.ToString());
                q.Enqueue(neighbor);
                break;
            }
        }
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
