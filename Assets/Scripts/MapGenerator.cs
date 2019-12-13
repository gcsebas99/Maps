using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MapGenerator : MonoBehaviour {
  public static GameObject[] prefabs;

  public void GenerateMapCollection(string collectionName) {
    //load prefabs
    prefabs = Resources.LoadAll<GameObject>("Prefabs");
    //load collection
    MapCollection mapCollection = MapGeneratorUtilities.LoadMapCollection(collectionName);
    if(mapCollection.maps.Length > 0) {
      //generate MPC object to contain all maps
      GameObject mpc = new GameObject { name = collectionName };
      mpc.isStatic = true;
      //generate items object
      GameObject itemsObj = new GameObject { name = "Items" };
      itemsObj.isStatic = true;
      itemsObj.transform.parent = mpc.transform;
      //generate each map
      foreach(string mapFileName in mapCollection.maps) {
        Map map = MapGeneratorUtilities.LoadMap(mapFileName);
        GenerateMap(map, mpc, itemsObj);
      }
    }

  }

  private void GenerateMap(Map map, GameObject parent, GameObject itemsObj) {
    //generate each layer
    foreach(Layer layer in map.layers) {
      GenerateLayer(layer, map, parent, itemsObj);
    }
  }

  private void GenerateLayer(Layer layer, Map map, GameObject parent, GameObject itemsObj) {
    GameObject currentPrefab;
    //generate each piece
    foreach(MapTile tile in layer.construction) {
      currentPrefab = MapGeneratorUtilities.FindPrefab(prefabs, tile.key);
      if(currentPrefab != null) {
        if(tile.direction != 0) {
          GenerateMapTileWithRotation(tile, currentPrefab, layer, map, parent);
        } else {
          GenerateMapTileWithoutRotation(tile, currentPrefab, layer, map, parent);
        }
      }
    }
    //generate items in layer
    foreach(MapItem item in layer.items) {
      currentPrefab = MapGeneratorUtilities.FindPrefab(prefabs, item.key);
      if(currentPrefab != null) {
        GenerateMapItem(item, currentPrefab, layer, map, itemsObj);
      }
    }
  }

  private void GenerateMapTileWithoutRotation(MapTile tile, GameObject prefab, Layer layer, Map map, GameObject parent) {
    GameObject mapTile = Instantiate(prefab, MapGeneratorUtilities.GetMapTilePositionInWorld(tile, prefab, layer.yOrder, map.startX, map.startZ, false), Quaternion.identity);
    mapTile.layer = 8; //terrain
    mapTile.isStatic = true;
    //allowed directions
    MapGeneratorUtilities.SetMapTileAllowedDirections(tile, prefab, mapTile);
    //scale
    mapTile.transform.localScale = MapGeneratorUtilities.GetMapTileScale(tile, prefab, mapTile.transform.localScale.y);
    //add to mpc
    mapTile.transform.parent = parent.transform;
  }

  private void GenerateMapTileWithRotation(MapTile tile, GameObject prefab, Layer layer, Map map, GameObject parent) {
    GameObject mapTile = Instantiate(prefab, Vector3.zero, Quaternion.identity);
    mapTile.layer = 8; //terrain
    mapTile.isStatic = true;
    //scale first
    mapTile.transform.localScale = MapGeneratorUtilities.GetMapTileScale(tile, prefab, mapTile.transform.localScale.y);
    //rotate
    mapTile.transform.localRotation = MapGeneratorUtilities.GetMapTileRotation(tile, prefab, mapTile);
    //allowed directions
    MapGeneratorUtilities.SetMapTileAllowedDirections(tile, prefab, mapTile);
    //reposition
    mapTile.transform.localPosition = MapGeneratorUtilities.GetMapTilePositionInWorld(tile, prefab, layer.yOrder, map.startX, map.startZ, true);
    //add to mpc
    mapTile.transform.parent = parent.transform;
  }

  private void GenerateMapItem(MapItem item, GameObject prefab, Layer layer, Map map, GameObject itemsObj) {
    GameObject mapItem = Instantiate(prefab, Vector3.zero, Quaternion.identity);
    mapItem.layer = 9; //StaticObject
    mapItem.isStatic = true;
    mapItem.transform.localScale = MapGeneratorUtilities.GetMapItemScale(item, prefab);
    mapItem.transform.localRotation = MapGeneratorUtilities.GetMapItemRotation(item, prefab);
    mapItem.transform.localPosition = MapGeneratorUtilities.GetMapItemPositionInWorld(item, prefab, layer.yOrder, map.startX, map.startZ);
    mapItem.transform.parent = itemsObj.transform;
  }
}

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {
  private string mapsPath;
  private int selectedMapCollection = 0;
  private List<string> availableMapCollections = new List<string>();
  private FileInfo[] mapCollectionsInfo;

  public override void OnInspectorGUI() {
    mapsPath = Application.dataPath + "/Maps/";
    DirectoryInfo dir = new DirectoryInfo(mapsPath);
    mapCollectionsInfo = dir.GetFiles("*.mpc");
    foreach(FileInfo mapCollection in mapCollectionsInfo) {
      availableMapCollections.Add(mapCollection.Name);
    }

    //collection selector
    selectedMapCollection = EditorGUILayout.Popup(selectedMapCollection, availableMapCollections.ToArray());

    //generate button
    if(GUILayout.Button("Generate Map")) {
      ((MapGenerator)target).GenerateMapCollection(availableMapCollections[selectedMapCollection]);
    }
  }
}
