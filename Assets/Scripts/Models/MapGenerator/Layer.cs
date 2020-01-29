using System;
using System.Collections.Generic;

[Serializable]
public class Layer {
  public float yOrder;
  public string refColor;
  public List<MapTile> tiles;
  public List<MapTile> optimization;
  public List<MapTile> construction;
  public List<MapItem> items;
}
