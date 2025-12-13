using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public GameObject bg;
    public GameObject show;
    public Dictionary<string,GameObject> TileDic  = new Dictionary<string,GameObject>();
    //public TileData TileData;
    public bool isCollapsed;
    public Vector2 pos = new Vector2();
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        Transform[] trans = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < trans.Length; i++)
        {
            TileDic.Add(trans[i].gameObject.name, trans[i].gameObject);
        }
    }
}
