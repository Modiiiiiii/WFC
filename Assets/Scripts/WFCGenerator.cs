using System;
using System.Collections;
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
    
    
    // 启动：加载SO配置并生成网格
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

    // 帧更新：处理点击与快捷键
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
        if (Input.GetKeyDown(KeyCode.D))
        {
            ResetMap();
        }
    }

    // 随机坍缩一个格子
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

    // 自动化协程
    private Coroutine _autoRunCoroutine;
    // 传播过程协程（用于细粒度控制）
    private Coroutine _propagationCoroutine;
    // Gizmos 绘制数据：传播路径线段 (起点, 终点)
    private List<System.Tuple<Vector3, Vector3>> _propagationGizmos = new List<System.Tuple<Vector3, Vector3>>();
    // 当前正在处理的格子（用于绘制高亮）
    private TileMono _currentProcessingTile;

    //点击函数
    // 点击坍缩并启动传播（仅传播一次，不自动补全）
    void CollapseTo(TileMono tile, string typeName)
    {
        if (tile == null || tile.tileParent == null) return;
        
        // 查找当前候选列表中，类型为 typeName 的第一个有效旋转
        // 这样可以保证我们选中的是一个合法的状态（如果之前已经受过约束）
        var candidates = tile.GetCandidates();
        var rotation = 0;
        var found = false;
        
        // 优先从现有候选中找
        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].type.ToString() == typeName)
            {
                rotation = candidates[i].rotation;
                found = true;
                break; // 找到第一个匹配的即可
            }
        }
        
        // 如果候选中没有（理论上不应发生，除非强制点击了非法类型），默认给0
        // 但为了健壮性，我们还是传给 Choice
        tile.Choice(typeName, rotation);
        
        // 停止之前的自动流程（如果还在跑）
        StopAllRunningCoroutines();
        
        // 清空之前的 Gizmos
        _propagationGizmos.Clear();
        _currentProcessingTile = null;

        _collapseQueue.Clear();
        _collapseQueue.Enqueue(tile);
        
        // 启动自动化流程
        _autoRunCoroutine = StartCoroutine(AutoRunSequence());
    }
    
    // 辅助：停止所有正在运行的逻辑协程
    private void StopAllRunningCoroutines()
    {
        if (_autoRunCoroutine != null) StopCoroutine(_autoRunCoroutine);
        if (_propagationCoroutine != null) StopCoroutine(_propagationCoroutine);
        _autoRunCoroutine = null;
        _propagationCoroutine = null;
    }

    // 自动化流程协程
    private IEnumerator AutoRunSequence()
    {
        // 首次进入，先处理点击产生的传播
        yield return StartCoroutine(ProcessQueueCoroutine());

        while (!IsComplete())
        {
            // 等待1秒（宏观步骤间隔）
            yield return new WaitForSeconds(1f);
            
            var next = FindMinEntropyTile();
            if (next == null) break;
            
            // 每次新的一步开始前，可以选择清空之前的传播线，或者累积
            // 这里为了清晰展示每一步的影响，我们清空
            _propagationGizmos.Clear();
            _currentProcessingTile = null;

            next.RandomCollapse();
            _collapseQueue.Enqueue(next);
            
            // 等待传播完成
            yield return StartCoroutine(ProcessQueueCoroutine());
        }
    }
    
    // 重置地图：清空队列并重新生成
    private void ResetMap()
    {
        StopAllRunningCoroutines();
        _propagationGizmos.Clear();
        _currentProcessingTile = null;
        _collapseQueue.Clear();
        Generate();
    }
    
    // 传播队列处理：约束邻居候选并触发后续坍缩
    // 改为协程以支持可视化停顿
    private IEnumerator ProcessQueueCoroutine()
    {
        while (_collapseQueue.Count > 0)
        {
            var current = _collapseQueue.Dequeue();
            if (current == null) continue;
            
            // 记录当前处理的格子用于可视化
            _currentProcessingTile = current;
            
            // 注意：不再要求 current.isCollapsed。
            // 即使是未坍缩的节点，只要它的候选集减少了，也需要通知邻居进行重新约束。

            var x = (int)current.pos.x;
            var y = (int)current.pos.y;

            for (int dir = 0; dir < 4; dir++)
            {
                // 微观停顿：每个方向停顿 0.1 秒
                yield return new WaitForSeconds(0.1f);
                
                var nx = x + (dir == 1 ? 1 : dir == 3 ? -1 : 0);
                var ny = y + (dir == 0 ? 1 : dir == 2 ? -1 : 0);
                
                var neighbor = GetTile(nx, ny);
                if (neighbor == null) continue;

                // 使用新的约束逻辑：基于 current 的所有剩余候选来约束 neighbor
                var changed = ConstrainNeighbor(current, dir, neighbor);
                
                if (!changed) continue;
                
                // 记录传播路径用于可视化
                _propagationGizmos.Add(new System.Tuple<Vector3, Vector3>(current.transform.position, neighbor.transform.position));

                var count = neighbor.GetCandidateCount();
                if (count <= 0)
                {
                    if (neighbor.error != null) neighbor.error.SetActive(true);
                    continue;
                }

                // 如果邻居只剩一个候选且尚未坍缩，则自动坍缩
                if (!neighbor.isCollapsed && count == 1)
                {
                    var only = neighbor.GetCandidates()[0];
                    // 自动坍缩会触发 Choice，进而更新显示和状态
                    // 注意：Choice 内部可能也会有些逻辑，但为了保持传播一致性，我们在这里入队
                    neighbor.Choice(only.type.ToString(), only.rotation);
                    _collapseQueue.Enqueue(neighbor);
                }
                else
                {
                    // 即使没有坍缩，只要候选减少了，就需要继续传播
                    // 为了防止循环入队，通常只有当候选集真正改变时才入队（前面已经 check changed）
                    // 此外，为了性能，可以检查是否已经在队列中（虽然 Queue 不方便检查，但 Set 可以）
                    // 这里简化处理，直接入队
                    _collapseQueue.Enqueue(neighbor);
                }
            }
        }
        
        // 队列处理完毕后，清空当前高亮
        _currentProcessingTile = null;
    }
    
    // 约束邻居候选：邻居的每个候选必须与当前瓦片的至少一个剩余候选兼容
    private bool ConstrainNeighbor(TileMono src, int dir, TileMono neighbor)
    {
        if (neighbor.isCollapsed) return false;

        var srcCandidates = src.GetCandidates();
        var neighborCandidates = neighbor.GetCandidates();
        var changed = false;
        
        // 预计算 src 在 dir 方向上所有可能的接缝集合，优化性能
        // 注意：这里存储 Seam 枚举，确保类型匹配
        var possibleSeams = new System.Collections.Generic.HashSet<Seam>();
        for (int i = 0; i < srcCandidates.Count; i++)
        {
            var sCand = srcCandidates[i];
            var sType = sCand.type.ToString();
            if (soConfigDic.ContainsKey(sType))
            {
                var sSo = soConfigDic[sType];
                // 计算 src 在 dir 方向的接缝
                // 旋转公式：(dir - rot + 4) % 4
                var sSeam = sSo.AllConnections[(dir - sCand.rotation + 4) % 4];
                possibleSeams.Add(sSeam);
            }
        }

        var opp = Opposite(dir);

        // 检查邻居的每个候选
        // 我们不能在遍历列表时删除元素，所以倒序遍历或者收集待删除项
        // TileMono.RemoveCandidate 内部是 List.Remove，倒序遍历安全
        for (int i = neighborCandidates.Count - 1; i >= 0; i--)
        {
            var nCand = neighborCandidates[i];
            var nType = nCand.type.ToString();
            
            // 如果邻居的类型在配置中不存在，直接移除（因为无法计算接缝）
            if (!soConfigDic.ContainsKey(nType))
            {
                if (neighbor.RemoveCandidate(nCand))
                {
                    changed = true;
                }
                continue;
            }

            var nSo = soConfigDic[nType];
            // 计算 neighbor 在 opp 方向的接缝
            var nSeam = nSo.AllConnections[(opp - nCand.rotation + 4) % 4];

            // 检查这个接缝是否在 src 的可能接缝集合中
            if (!possibleSeams.Contains(nSeam))
            {
                // 不兼容，移除该候选
                if (neighbor.RemoveCandidate(nCand))
                {
                    changed = true;
                }
            }
        }
        
        return changed;
    }
    
    // 方向反转：0/1/2/3 -> 对应的反方向
    private int Opposite(int dir)
    {
        return (dir + 2) % 4;
    }
    
    // 是否全部完成：所有未坍缩格子均无候选
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
    
    // 查找最小熵格子：候选最少的未坍缩格子
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

    // 清空现有网格对象
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
    
    // 取指定坐标的格子（越界返回null）
    public TileMono GetTile(int x, int y)
    {
        if (grid == null) return null;
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) return null;
        return grid[x, y];
    }


    private void OnDrawGizmos()
    {
        // 绘制当前正在处理的格子
        if (_currentProcessingTile != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_currentProcessingTile.transform.position+new Vector3(0,1,0), 0.5f);
        }
        
        // 绘制传播路径
        if (_propagationGizmos != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var line in _propagationGizmos)
            {
                if (line != null)
                {
                    Gizmos.DrawLine(line.Item1+new Vector3(0,1,0), line.Item2+new Vector3(0,1,0));
                    // 在终点画一个小球表示影响到达
                    Gizmos.DrawSphere(line.Item2, 0.3f);
                }
            }
        }
    }
}
