using System;
using UnityEngine;

public class InteractableObjectsUtils {
  //tells a collider is an IO or IO should be referenced as a parent/child
  public static GameObject GetInteractableObject(GameObject gameObject) {
    if(gameObject.GetComponent<InteractableObjectDelegation>() != null) {
      if(gameObject.GetComponent<InteractableObjectDelegation>().reference == ConstantsAndEnums.DelegationReference.Parent) {
        return gameObject.transform.parent.gameObject;
      }
    }
    return gameObject;
  }

  //tells a collider is PlayerController or not
  public static bool CollideWithPlayer(GameObject gameObject) {
    return gameObject.GetComponent<PlayerController>() != null;
  }

  //tells a collider is a PhysicalObject or not
  public static bool IsPhysicalObject(GameObject gameObject) {
    return gameObject.GetComponent<PhysicalObject>() != null;
  }

  public static Collider GetMainCollider(Transform transform) {
    GameObject ioObject = InteractableObjectsUtils.GetInteractableObject(transform.gameObject);
    Collider collider = ioObject.GetComponent<Collider>();
    if(!collider || collider is CharacterController) {
      collider = ioObject.transform.GetChild(0).GetComponent<Collider>();
      return collider;
    } else {
      return collider;
    }
  }
}
