using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modi.Event;

public enum TileType
{
    bridge,
    connection,
    dskew,
    skew,
    substrate,
    t,
    track,
    transition,
    turn,
    viad,
    vias,
    wire,
}

/// <summary>
/// 接缝
/// </summary>
public enum Seam
{
    greenLine,
    greyLine,
    allGrenn,
}

public class TileMono : MonoBehaviour
{
    public Transform tileParent;
    public GameObject show;
    public GameObject error;
    private Dictionary<string,GameObject> _tileDic  = new Dictionary<string,GameObject>();
    public struct Candidate { public TileType type; public int rotation; }
    private List<Candidate> _candidates = new List<Candidate>();
    
    public bool isCollapsed; //是否已经坍塌完毕
    public Vector2 pos = new Vector2();
    public TileType currentTileType;
    public int currentRotation;
    
    public Dictionary <string,TileSo>  SoConfigDic => WfcGenerator.Instance.soConfigDic;
    
    // 初始化：缓存子对象、填充候选并注册事件
    private void Awake()
    {
        Init();
        EventDispatcher.AddEventListener<TileType,Vector2>(Events.OnChoiceEvent,OnChoiceEvent);
    }

    private void OnDestroy()
    {
        EventDispatcher.RemoveEventListener<TileType,Vector2>(Events.OnChoiceEvent,OnChoiceEvent);
    }

    // 初始化字典与候选（类型×4个旋转）
    private void Init()
    {
        Transform[] trans = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < trans.Length; i++)
        {
            _tileDic.Add(trans[i].gameObject.name, trans[i].gameObject);
        }
        _candidates.Clear();
        var types = new[]
        {
            TileType.bridge,
            TileType.connection,
            TileType.dskew,
            TileType.skew,
            TileType.substrate,
            TileType.t,
            TileType.track,
            TileType.transition,
            TileType.turn,
            TileType.viad,
            TileType.vias,
            TileType.wire,
        };
        for (int ti = 0; ti < types.Length; ti++)
        {
            for (int r = 0; r < 4; r++)
            {
                _candidates.Add(new Candidate { type = types[ti], rotation = r });
            }
        }
    }

    // 坍缩到指定类型与旋转
    public void Choice(string typeName,int rotate = 0)
    {
        show.SetActive(true);
        if (Enum.TryParse<TileType>(typeName, out var chosen))
        {
            currentTileType = chosen;
        }
        currentRotation = rotate;
        isCollapsed = true;
        show.GetComponent<MeshRenderer>().material = _tileDic[typeName].GetComponent<MeshRenderer>().material;
        show.transform.localEulerAngles = new Vector3(0f, 0, -90f * rotate);
        tileParent.gameObject.SetActive(false);
        _candidates.Clear();
        _candidates.Add(new Candidate { type = currentTileType, rotation = currentRotation });
        EventDispatcher.TriggerEvent(Events.OnChoiceEvent,currentTileType,pos);
    }

    // 随机从候选中选择并坍缩
    public void RandomCollapse()
    {
        if (isCollapsed) return;
        if (_candidates == null || _candidates.Count == 0)
        {
            if (error != null) error.SetActive(true);
            return;
        }
        var idx = UnityEngine.Random.Range(0, _candidates.Count);
        var chosen = _candidates[idx];
        Choice(chosen.type.ToString(), chosen.rotation);
    }

    // 获取候选列表副本
    public List<Candidate> GetCandidates()
    {
        return new List<Candidate>(_candidates);
    }
    
    // 移除指定候选（类型+旋转）
    public bool RemoveCandidate(Candidate candidate)
    {
        var before = _candidates.Count;
        for (int i = _candidates.Count - 1; i >= 0; i--)
        {
            var c = _candidates[i];
            if (c.type == candidate.type && c.rotation == candidate.rotation)
            {
                _candidates.RemoveAt(i);
            }
        }
        return _candidates.Count < before;
    }
    
    // 候选数量
    public int GetCandidateCount()
    {
        return _candidates.Count;
    }
    
    // 选择事件回调（保留供扩展）
    void OnChoiceEvent(TileType type,Vector2 pos)
    {
        if (isCollapsed) return;
        if (!SoConfigDic.ContainsKey(type.ToString())) return;
        var dir = GetDirectionFromNeighbor(pos, this.pos);
    }
    
    private void RemoveCandidateSafe(TileType type)
    {
        // if (_candidates.Contains(type))
        // {
        //     if (_tileDic.ContainsKey(type.ToString()))
        //     {
        //         _tileDic[type.ToString()].SetActive(false);
        //     }
        //     _candidates.Remove(type);
        // }
    }
    
    // 由邻居位置计算方向（上右下左 -> 0/1/2/3）
    private int GetDirectionFromNeighbor(Vector2 neighborPos, Vector2 selfPos)
    {
        var dx = (int)(selfPos.x - neighborPos.x);
        var dy = (int)(selfPos.y - neighborPos.y);
        if (dx == 0 && dy == 1) return 0;  // self is above neighbor -> neighbor's up
        if (dx == 0 && dy == -1) return 1; // self is below neighbor -> neighbor's down
        if (dx == -1 && dy == 0) return 2; // self is left  of neighbor -> neighbor's left
        if (dx == 1 && dy == 0) return 3;  // self is right of neighbor -> neighbor's right
        return -1;
    }
}
