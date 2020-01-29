using System;
using System.Collections.Generic;

[Serializable]
public class Map {
  public string name;
  public int version;
  public int sizeX;
  public int sizeZ;
  public int startX;
  public int startZ;
  public List<Layer> layers;
}
