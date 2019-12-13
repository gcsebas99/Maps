using System;

[Serializable]
public class MapItem {
  public string key;
  public int direction;
  public int x;
  public int z;
  public MapItemOffset offset;
  public float scale;
}
