using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]

public class MapEnd : InteractableObject {
  //collider
  private BoxCollider collider;
  //colliding
  private bool isColliding;
  //
  public bool enableWhenLevelIsCompleted = false;
  private bool levelCompleted = false;

  //level to move
  public string moveToLevel;

  // Use this for initialization
  void Start() {
    base.Start();
    collider = GetComponent<BoxCollider>();
    collider.isTrigger = true;
    gameObject.isStatic = true;

    // tracking colliding works only for trigger areas that allows one specific object at a time
    isColliding = false;

    if(enableWhenLevelIsCompleted) {
      SetEnable(false);
    }
  }

  void Update() {
    if(enableWhenLevelIsCompleted && world.levelCompleted != levelCompleted && world.levelCompleted) {
      levelCompleted = world.levelCompleted;
      SetEnable(true);
    }
  }

  private void OnTriggerEnter(Collider other) {
    if(!isColliding) {
      GameObject ioObject = InteractableObjectsUtils.GetInteractableObject(other.gameObject);
      if(InteractableObjectsUtils.CollideWithPlayer(ioObject)) {
        isColliding = true;
        world.MovePlayerToLevel(moveToLevel);
      }
    }

  }

  private void OnTriggerExit(Collider other) {
    GameObject ioObject = InteractableObjectsUtils.GetInteractableObject(other.gameObject);
    if(InteractableObjectsUtils.CollideWithPlayer(ioObject)) {
      isColliding = false;
    }
  }

  private void SetEnable(bool status) {
    GetComponent<Renderer>().enabled = status;
    GetComponent<BoxCollider>().enabled = status;
  }
}
