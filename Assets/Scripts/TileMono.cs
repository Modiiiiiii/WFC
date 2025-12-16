using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modi.Event;
public enum TileType
{
    bridge,
    component,
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
    private Dictionary<string,GameObject> _tileDic  = new Dictionary<string,GameObject>();
    private List<TileType> _candidates = new List<TileType>()
    {
        TileType.bridge,
        TileType.component,
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
    
    public bool isCollapsed; //是否已经坍塌完毕
    public Vector2 pos = new Vector2();
    public TileType currentTileType;
    
    public Dictionary <string,TileSo>  SoConfigDic => WfcGenerator.Instance.SoConfigDic;

    private void RemoveCandidate(TileType type)
    {
        _tileDic[type.ToString()].SetActive(false);
        _candidates.Remove(type);
    }
    
    private void Awake()
    {
        Init();
        EventDispatcher.AddEventListener<TileType,Vector2>(Events.OnChoiceEvent,OnChoiceEvent);
    }

    private void OnDestroy()
    {
        EventDispatcher.RemoveEventListener<TileType,Vector2>(Events.OnChoiceEvent,OnChoiceEvent);
    }

    private void Init()
    {
        Transform[] trans = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < trans.Length; i++)
        {
            _tileDic.Add(trans[i].gameObject.name, trans[i].gameObject);
        }
    }

    public void Choice(string typeName)
    {
        show.SetActive(true);
        if (Enum.TryParse<TileType>(typeName, out var chosen))
        {
            currentTileType = chosen;
        }
        isCollapsed = true;
        show.GetComponent<MeshRenderer>().material = _tileDic[typeName].GetComponent<MeshRenderer>().material;
        foreach (var cand in new List<TileType>(_candidates))
        {
            if (cand.ToString() != typeName)
            {
                if (_tileDic.ContainsKey(cand.ToString()))
                {
                    _tileDic[cand.ToString()].SetActive(false);
                }
            }
        }
        _candidates.Clear();
        _candidates.Add(currentTileType);
        EventDispatcher.TriggerEvent(Events.OnChoiceEvent,currentTileType,pos);
    }

    void OnChoiceEvent(TileType type,Vector2 pos)
    {
        if (isCollapsed) return;
        if (!SoConfigDic.ContainsKey(type.ToString())) return;
        var dir = GetDirectionFromNeighbor(pos, this.pos);
    }
    
    private void RemoveCandidateSafe(TileType type)
    {
        if (_candidates.Contains(type))
        {
            if (_tileDic.ContainsKey(type.ToString()))
            {
                _tileDic[type.ToString()].SetActive(false);
            }
            _candidates.Remove(type);
        }
    }
    
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
