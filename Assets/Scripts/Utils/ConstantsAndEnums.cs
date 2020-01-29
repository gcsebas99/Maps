
using UnityEngine;

namespace ConstantsAndEnums {
  //allowed directions
  public enum Direction { Up, Down, Right, Left };

  //ground
  public enum GroundOptions { Terrain, Interactable, Fall };
  public enum GroundPositions { Standing, Target };

  //constants
  class Constants {
    //step directions
    public static readonly Vector3 StepUp = Vector3.zero;
    public static readonly Vector3 StepRight = new Vector3(0, 90, 0);
    public static readonly Vector3 StepDown = new Vector3(0, 180, 0);
    public static readonly Vector3 StepLeft = new Vector3(0, 270, 0);
  }

}
