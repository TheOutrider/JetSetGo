using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MapDatabase",
    menuName = "Map Game/Map Database")]
public class MapDatabase : ScriptableObject
{
    public List<MapData> Maps = new List<MapData>();
}

[System.Serializable]
public class MapData
{

    public string Name;
    
}