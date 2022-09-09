using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public enum EntryType {
    Empty,
    Floor,
    Wall,
    Ceiling
}

[Serializable]
public class PalletteEntry {
    public string name;
    public EntryType entryType;
    public Material[] materials;
}

[CreateAssetMenu(fileName = "MapPaletteData", menuName = "Map/Map Palette Data", order = 1)]
public class MapPaletteData : ScriptableObject {

    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject ceilingPrefab;

    [Space(10)]
    public PalletteEntry[] entries;
}
