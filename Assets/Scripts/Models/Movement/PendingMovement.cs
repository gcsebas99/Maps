using System;
using UnityEngine;

public class PendingMovement {
  public Vector3 destination;
  public bool requiresPause;
  public Vector3 pausedPosition;
  public MovableObject.Direction pausedDirection;
  public int startMovePause;
  public int endMovePause;
  public bool generator;
  public bool pushPullTriggered;
}
