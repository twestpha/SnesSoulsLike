using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

#if UNITY_EDITOR
[ExecuteInEditMode]
public class MapBuilder : MonoBehaviour {

    public float tileUnitScale = 2.0f;

    public TextAsset mapFile;
    public MapPaletteData palette;

    public void GenerateMap(){
        // Destroy all existing children
        while(transform.childCount > 0){
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Mark this as dirty so the save asterisk appears
        EditorUtility.SetDirty(gameObject);

        string[] lines = mapFile.text.Trim().Split('\n');

        // Parse the CSV file
        int verticalCount = lines.Length;
        int horizontalCount = lines[0].Split(',').Length;

        if(verticalCount <= 0 || horizontalCount <= 0){
            Debug.LogError("MapBuilder.GenerateMap: The parsed vertical tile count is " + verticalCount + " and the horiztonal tile count is " + horizontalCount + " which is not allowed.");
            return;
        }

        for(int i = 0; i < verticalCount; ++i){
            string[] indices = lines[i].Trim().Split(',');

            for(int k = 0; k < horizontalCount; ++k){
                int tileIndex = 0;

                try {
                    tileIndex = Int32.Parse(indices[k]);
                } catch {
                    Debug.LogWarning("MapBuilder.GenerateMap: Tile at position (" + i + ", " + k + ") was not a parseable integer, was " + indices[k] + ", skipping instantiation.");
                    continue;
                }

                PalletteEntry entry = palette.entries[tileIndex];

                GameObject tilePrefab = null;
                if(entry.entryType == EntryType.Empty){
                    continue;
                } else if(entry.entryType == EntryType.Floor){
                    tilePrefab = palette.floorPrefab;
                } else if(entry.entryType == EntryType.Wall){
                    tilePrefab = palette.wallPrefab;
                } else if(entry.entryType == EntryType.Ceiling){
                    tilePrefab = palette.ceilingPrefab;
                }

                // Spawn as a prefab, so changes to it still reflect in editor
                GameObject newTile = (GameObject)(PrefabUtility.InstantiatePrefab(tilePrefab));

                // Apply random material from materials
                if(entry.materials != null){
                    MeshRenderer[] newTileMeshRenderers = newTile.GetComponentsInChildren<MeshRenderer>();

                    foreach(MeshRenderer newTileMeshRenderer in newTileMeshRenderers){
                        newTileMeshRenderer.material = entry.materials[UnityEngine.Random.Range(0, entry.materials.Length)];
                    }
                }

                int nameId = (i * horizontalCount) + k;
                newTile.name = gameObject.name + "_" + nameId;

                newTile.transform.parent = transform;
                newTile.transform.localPosition = new Vector3((float)(k) * tileUnitScale, 0.0f, -(float)(i) * tileUnitScale);
            }
        }
    }
}
#endif // UNITY_EDITOR
