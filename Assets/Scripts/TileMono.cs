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
    corner,
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
        TileType.corner,
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
        isCollapsed = true;
        show.GetComponent<MeshRenderer>().material = _tileDic[typeName].GetComponent<MeshRenderer>().material;
        EventDispatcher.TriggerEvent(Events.OnChoiceEvent,currentTileType,pos);
    }

    void OnChoiceEvent(TileType type,Vector2 pos)
    {
        //to do 接受这个事件时，如果是邻家的格子，检查是否需要隐藏一些TileDic
    }
}
