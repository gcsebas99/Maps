using System;
using UnityEngine;

public class InteractableObjectsUtils {
  //tells a collider is PlayerController or not
  public static bool CollideWithPlayer(Collider collider) {
    return collider.gameObject.GetComponent<PlayerController>() != null;
  }
}
