using System;
using System.IO;
using UnityEngine;

public static class MapGeneratorUtilities {

  public static MapCollection LoadMapCollection(string name) {
    string mapsPath = Application.dataPath + "/Maps/" + name;
    string collectionString = File.ReadAllText(mapsPath);
    return JsonUtility.FromJson<MapCollection>(collectionString);
  }

  public static Map LoadMap(string name) {
    string mapsPath = Application.dataPath + "/Maps/" + name;
    string mapString = File.ReadAllText(mapsPath);
    return JsonUtility.FromJson<Map>(mapString);
  }

  public static Vector3 GetMapTilePositionInWorld(MapTile tile, GameObject prefab, float layer, int mapOffsetX, int mapOffsetZ, bool rotated) {
    float x = mapOffsetX + tile.x;
    float z = mapOffsetZ + tile.z;
    float y = ((layer * 2) - 2) + prefab.transform.localPosition.y; //2=layer height
    if(rotated) {
      switch(tile.direction) {
        case 3:
          x = x + tile.spanX;
          break;
        case 2:
          x = x + tile.spanZ;
          z = z + tile.spanX;
          break;
        case 1:
          z = z + tile.spanZ;
          break;
        case 0:
        default:
          break;
      }
    }

    return new Vector3(x, y, z);
  }

  public static Vector3 GetMapItemPositionInWorld(MapItem item, GameObject prefab, float layer, int mapOffsetX, int mapOffsetZ) {
    float x = mapOffsetX + item.x + 0.5f + item.offset.x; //half tile
    float z = mapOffsetZ + item.z + 0.5f + item.offset.z; //half tile
    float y = (layer * 2) + prefab.transform.localPosition.y + item.offset.y; //2=layer height
    return new Vector3(x, y, z);
  }

  public static Vector3 GetMapTileScale(MapTile tile, GameObject prefab, float scaleY) {
    TerrainPrefabData prefabData = prefab.GetComponent<TerrainPrefabData>();
    bool invert = false;
    if(prefabData.direction != 0) {
      if(tile.direction == 1 || tile.direction == 3) {
        invert = true;
      }
    }
    if(invert) {
      return new Vector3((float)tile.spanZ, scaleY, (float)tile.spanX);
    } else {
      return new Vector3((float)tile.spanX, scaleY, (float)tile.spanZ);
    }
  }

  public static Vector3 GetMapItemScale(MapItem item, GameObject prefab) {
    return new Vector3(prefab.transform.localScale.x * item.scale,
                       prefab.transform.localScale.y * item.scale,
                       prefab.transform.localScale.z * item.scale);
  }

  public static void SetMapTileAllowedDirections(MapTile tile, GameObject prefab, GameObject mapTile) {
    TerrainPrefabData prefabData = prefab.GetComponent<TerrainPrefabData>();
    if(prefabData.direction != 0) {
      TerrainPrefabData data = mapTile.GetComponent("TerrainPrefabData") as TerrainPrefabData;
      if(tile.direction == 0 || tile.direction == 2) { //vertical
        data.arriveDirection = 1;
        data.departDirection = 1;
      } else {
        data.arriveDirection = 2;
        data.departDirection = 2;
      }
    }
  }

  public static Quaternion GetMapTileRotation(MapTile tile, GameObject prefab, GameObject mapTile) {
    TerrainPrefabData prefabData = prefab.GetComponent<TerrainPrefabData>();
    if(prefabData.direction != 0 && tile.direction != 0) {
      if(prefabData.direction == 2) { //two directions (vertical(Z) || horizontal(X))
        if(tile.direction == 0) {
          return Quaternion.identity;
        } else {
          return Quaternion.Euler(0, 90, 0);
        }
      } else { //four posible directions
        switch(tile.direction) {
          case 3:
            return Quaternion.Euler(0, 270, 0);
          case 2:
            return Quaternion.Euler(0, 180, 0);
          case 1:
            return Quaternion.Euler(0, 90, 0);
          case 0:
          default:
            return Quaternion.identity;
        }
      }
    }
    return Quaternion.identity;
  }

  public static Quaternion GetMapItemRotation(MapItem item, GameObject prefab) {
    ItemPrefabData prefabData = prefab.GetComponent<ItemPrefabData>();
    if(prefabData.direction != 0 && item.direction != 0) {
      if(prefabData.direction == 2) { //two directions (vertical(Z) || horizontal(X))
        if(item.direction == 0) {
          return Quaternion.identity;
        } else {
          return Quaternion.Euler(0, 90, 0);
        }
      } else { //four posible directions
        switch(item.direction) {
          case 3:
            return Quaternion.Euler(0, 270, 0);
          case 2:
            return Quaternion.Euler(0, 180, 0);
          case 1:
            return Quaternion.Euler(0, 90, 0);
          case 0:
          default:
            return Quaternion.identity;
        }
      }
    }
    return Quaternion.identity;
  }

  public static GameObject FindPrefab(GameObject[] prefabs, string key) {
    return Array.Find(prefabs, prefab => prefab.name == key);
  }
}
