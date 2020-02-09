using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]

public class MapEnd : InteractableObject {
  //collider
  private BoxCollider collider;
  //colliding
  private bool isColliding;
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
  }

  // Update is called once per frame
  //void Update() {

  //}

  private void OnTriggerEnter(Collider other) {
    if(!isColliding) {
      if(InteractableObjectsUtils.CollideWithPlayer(other)) {
        Debug.Log("||--Entering PLAYER " + other.transform.name);
        isColliding = true;
        world.MovePlayerToLevel(moveToLevel);
      }
    }

  }

  //private void OnTriggerStay(Collider other) {

  //}

  private void OnTriggerExit(Collider other) {
    if(InteractableObjectsUtils.CollideWithPlayer(other)) {
      Debug.Log("||--Exit " + other.transform.name);
      isColliding = false;
    }
  }
}
