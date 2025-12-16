using System;
using System.Collections.Generic;
using UnityEngine;
using Modi;

public class WfcGenerator : SingletonMono<WfcGenerator>
{
    [Header("生成设置")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public Dictionary <string,TileSo> soConfigDic = new Dictionary<string, TileSo>();//So配置
    public TileMono[,] grid;
    
    [Header("显示设置")]
    public float tileSize = 4f;

    [Header("预制体")]
    public GameObject prefab;
    
    private Queue<TileMono> _collapseQueue = new Queue<TileMono>();
    
    
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
        soConfigDic.Clear();
        var all = Resources.LoadAll<TileSo>("So");
        for (int i = 0; i < all.Length; i++)
        {
            var so = all[i];
            var key = so.tileType.ToString();
            soConfigDic[key] = so;
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
                if (tile != null && soConfigDic.ContainsKey(typeName))
                {
                    CollapseTo(tile, typeName);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            RandomCollapse();
        }
    }

    void RandomCollapse()
    {
        //todo 随机选择一个格子进行坍缩
        var x = UnityEngine.Random.Range(0, gridWidth);
        var y = UnityEngine.Random.Range(0, gridHeight);
        var tile = grid[x, y];
        if (tile == null) return;
        if (tile.isCollapsed) return;
        tile.RandomCollapse();
    }

    //点击函数
    void CollapseTo(TileMono tile, string typeName)
    {
        if (tile == null || tile.tileParent == null) return;
        tile.Choice(typeName);
        _collapseQueue.Clear();
        _collapseQueue.Enqueue(tile);
        ProcessQueue();
        while (!IsComplete())
        {
            var next = FindMinEntropyTile();
            if (next == null) break;
            next.RandomCollapse();
            _collapseQueue.Enqueue(next);
            ProcessQueue();
        }
    }
    
    private void ProcessQueue()
    {
        while (_collapseQueue.Count > 0)
        {
            var current = _collapseQueue.Dequeue();
            if (current == null) continue;
            if (!current.isCollapsed) continue;
            var x = (int)current.pos.x;
            var y = (int)current.pos.y;
            var srcType = current.currentTileType.ToString();
            if (!soConfigDic.ContainsKey(srcType)) continue;
            var srcSo = soConfigDic[srcType];
            for (int dir = 0; dir < 4; dir++)
            {
                var nx = x + (dir == 1 ? 1 : dir == 3 ? -1 : 0);
                var ny = y + (dir == 0 ? 1 : dir == 2 ? -1 : 0);
                var neighbor = GetTile(nx, ny);
                if (neighbor == null) continue;
                var changed = ConstrainNeighbor(srcSo, dir, neighbor, current.currentRotation);
                if (!changed) continue;
                var count = neighbor.GetCandidateCount();
                if (count <= 0)
                {
                    if (neighbor.error != null) neighbor.error.SetActive(true);
                    continue;
                }
                if (!neighbor.isCollapsed && count == 1)
                {
                    var only = neighbor.GetCandidates()[0];
                    neighbor.Choice(only.type.ToString(), only.rotation);
                    _collapseQueue.Enqueue(neighbor);
                }
                else
                {
                    _collapseQueue.Enqueue(neighbor);
                }
            }
        }
    }
    
    private bool ConstrainNeighbor(TileSo srcSo, int dir, TileMono neighbor, int srcRotation)
    {
        if (neighbor.isCollapsed) return false;
        var need = srcSo.AllConnections[(dir - srcRotation + 4) % 4];
        var opp = Opposite(dir);
        var candidates = neighbor.GetCandidates();
        var changed = false;
        for (int i = 0; i < candidates.Count; i++)
        {
            var cand = candidates[i];
            var t = cand.type.ToString();
            if (!soConfigDic.ContainsKey(t)) continue;
            var nSo = soConfigDic[t];
            var seam = nSo.AllConnections[(opp - cand.rotation + 4) % 4];
            if (seam != need)
            {
                if (neighbor.RemoveCandidate(cand))
                {
                    changed = true;
                }
            }
        }
        return changed;
    }
    
    private int Opposite(int dir)
    {
        return (dir + 2) % 4;
    }
    
    private bool IsComplete()
    {
        if (grid == null) return true;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                var t = grid[x, y];
                if (t == null) continue;
                if (!t.isCollapsed && t.GetCandidateCount() > 0) return false;
            }
        }
        return true;
    }
    
    private TileMono FindMinEntropyTile()
    {
        TileMono result = null;
        var best = int.MaxValue;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                var t = grid[x, y];
                if (t == null) continue;
                if (t.isCollapsed) continue;
                var c = t.GetCandidateCount();
                if (c <= 0) continue;
                if (c < best)
                {
                    best = c;
                    result = t;
                }
            }
        }
        return result;
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
