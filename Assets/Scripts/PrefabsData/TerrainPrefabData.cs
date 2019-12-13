using UnityEngine;

public class TerrainPrefabData : MonoBehaviour {
  public bool walkable = true;
  public bool isRamp = false; //allows objects to go climb/descend
  public int direction = 0;
  public int arriveDirection = 0; //0=any, 1=vertical, 2=horizontal
  public int departDirection = 0; //0=any, 1=vertical, 2=horizontal
}
