using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]

public class PhysicalObject : InteractableObject {
  //allow other objects to stack on top
  public bool allowGroundable = true;
  //weight
  public float objectWeight = 1.0f;
  //movements to apply on each fixedUpdate call
  protected Vector3 objectMovements = Vector3.zero;

  //controller
  protected CharacterController controller;

  // Use this for initialization
  public virtual void Start() {
    base.Start();
    controller = GetComponent<CharacterController>();
  }

  protected void FixedUpdate() {
    ApplyGravity();
    //if required, you can calculate physical interaction/reactions
    //i.e. chained weight

    //finally apply all pending movements
    ResolveMovement();
  }

  private void ApplyGravity() {
    if(!controller.isGrounded) {
      Vector3 grav = new Vector3(0, WorldController.Gravity * objectWeight * Time.fixedDeltaTime, 0);
      //controller.Move(grav);
      objectMovements += grav;
    }
  }

  private void ResolveMovement() {
    if(objectMovements != Vector3.zero) {
      controller.Move(objectMovements);
      objectMovements = Vector3.zero;
    }
  }

  //Feature: Chained Weight (allows any object to notyfy what is its weight plus any weight from other objects stacked on top)
  public float ChainedWeight() {
    if(allowGroundable) {
      RaycastHit stackedObjectHit;
      Ray stackedObjectRay = new Ray(transform.position, new Vector3(0, 1, 0)); //pointing up
      Collider collider = transform.gameObject.GetComponent<Collider>();
      if(!collider) {
        collider = transform.GetChild(0).GetComponent<Collider>();
      }
      float rayLength = collider.bounds.size.y + 0.5f;
      bool hitsInteractive = Physics.Raycast(stackedObjectRay, out stackedObjectHit, rayLength, WorldController.IOPhysicalLayerMask);
      //if hits, test IO is physical and request chained weight
      if(hitsInteractive) {
        GameObject ioObject = InteractableObjectsUtils.GetInteractableObject(stackedObjectHit.transform.gameObject);
        if(InteractableObjectsUtils.IsPhysicalObject(ioObject)) {
          PhysicalObject nextObject = ioObject.GetComponent<PhysicalObject>();
          float stackedWeight = nextObject.ChainedWeight();
          return objectWeight + stackedWeight;
        }
      }
      //no hit, return own weight
      return objectWeight;
    } else {
      //no groundable, return own weight
      return objectWeight;
    }
  }

}
