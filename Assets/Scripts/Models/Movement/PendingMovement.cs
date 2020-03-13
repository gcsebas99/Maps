using System;
using UnityEngine;
using ConstantsAndEnums;

public class PendingMovement {
  public Vector3 destination;
  public bool requiresPause;
  public Vector3 pausedPosition;
  public Direction pausedDirection;
  public int startMovePause;
  public int endMovePause;
  public bool generator;
  public bool pushPullTriggered;
}
