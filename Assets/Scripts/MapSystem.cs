#if (UNITY_EDITOR)

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


//[ExecuteIn---EditMode]
public class MapSystem : MonoBehaviour {

  private MapSystem mInstance;
  public MapSystem Instance { get { return mInstance; } }

  public GameObject ground1;
  public GameObject ground2;
  public GameObject lava1;
  public GameObject water1;
  public GameObject ramp1;
  public GameObject ramp2;
  public GameObject ramp3;

  private string path;
  private string jsonString;

  private void Awake() {
    //print("Editor causes this Awake");
    mInstance = this;
  }

  private void OnDestroy() {
    mInstance = null;
  }

  // Start is called before the first frame update
  void Start() {
    //print("Create fuarking map here!");
    //loadMap();
  }

  // Update is called once per frame
  void Update() {
    //print("Editor causes this Update");
  }

  public void showOption(string option) {
    //print("Selected: " + option);
    loadMap(option);
  }

  public void loadMap(string name) {
    path = Application.dataPath + "/Maps/" + name;
    jsonString = File.ReadAllText(path);
    Map map = JsonUtility.FromJson<Map>(jsonString);

    GameObject go = new GameObject {
      name = map.name
    };// Instantiate(new GameObject(), transform.position, Quaternion.identity);

    //foreach(TileLayer layer in map.layers) {
    //  //print("Layer: " + layer.yOrder);
    //  createLayer(layer, go);
    //}
  }

  private void createLayer(Layer layer, GameObject parentMap) {
    GameObject current;
    string name;
    string tileType;
    foreach(MapTile tile in layer.construction) {
      //name = tile.aspect.ToLower();
      //tileType = tile.type.ToLower();
      //current = null;
      //switch(name) {
      //  case "ground1":
      //    if(tileType == "plain1" || tileType == "plain2") {
      //      current = ground1;
      //    } else if(tileType == "ramp1") {
      //      current = ramp1;
      //    } else if(tileType == "ramp2") {
      //      current = ramp2;
      //    } else if(tileType == "ramp3") {
      //      current = ramp3;
      //    }
      //    break;
      //  case "ground2":
      //    current = ground2;
      //    break;
      //  case "lava1":
      //    current = lava1;
      //    break;
      //  case "water1":
      //    current = water1;
      //    break;
      //}
      //if(current != null) {
      //  print("Should draw " + tile.x + "," + tile.z + " type:" + tile.type + " aspect:" + tile.aspect + " spanX:" + tile.spanX + " spanZ:" + tile.spanZ);
      //  GameObject go = Instantiate(current, tilePositionInWorld(tile, layer.yOrder), Quaternion.identity);
      //  go.transform.localScale = new Vector3((float)tile.spanX, go.transform.localScale.y, (float)tile.spanZ);
      //  go.transform.parent = parentMap.transform;


      //}
    }
  }

  private Vector3 tilePositionInWorld(MapTile tile, int layer) {
    float x = tile.x;
    float z = (tile.spanZ > 1) ? ((float)(tile.z + Math.Ceiling((float)tile.spanZ / 2))) : tile.z;

    return new Vector3(x, (float)(layer * 2), z);
  }
}

[CustomEditor(typeof(MapSystem))]
public class GeneratorEditor : Editor {
  private string path;
  private int index = 0;
  private List<string> options = new List<string>();
  private FileInfo[] info;

  public override void OnInspectorGUI() {
    path = Application.dataPath + "/Maps/";
    DirectoryInfo dir = new DirectoryInfo(path);
    info = dir.GetFiles("*.json");
    foreach(FileInfo f in info) {
      options.Add(f.Name);
    }

    index = EditorGUILayout.Popup(index, options.ToArray());

    if(GUILayout.Button("CREATE MAP")) {
      //((MapSystem)target).loadMap();
      ((MapSystem)target).showOption(options[index]);
    }
  }
}

#endif