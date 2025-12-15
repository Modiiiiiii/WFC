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
    public Dictionary<string,GameObject> TileDic  = new Dictionary<string,GameObject>();
    public HashSet<TileType> Candidates = new HashSet<TileType>();
    public bool isCollapsed;
    public Vector2 pos = new Vector2();
    public TileType currentTileType;
    
    public Dictionary <string,TileSo>  SoConfigDic => WfcGenerator.Instance.SoConfigDic;
    
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
            TileDic.Add(trans[i].gameObject.name, trans[i].gameObject);
        }
    }

    public void Choice(string typeName)
    {
        show.SetActive(true);
        isCollapsed = true;
        show.GetComponent<MeshRenderer>().material = TileDic[typeName].GetComponent<MeshRenderer>().material;
        EventDispatcher.TriggerEvent(Events.OnChoiceEvent,currentTileType,pos);
    }

    void OnChoiceEvent(TileType type,Vector2 pos)
    {
        
    }
}
