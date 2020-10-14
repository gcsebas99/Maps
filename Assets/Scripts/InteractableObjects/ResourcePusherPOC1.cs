using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourcePusherPOC1 : InteractableObject {

  public ConstantsAndEnums.ResourceType type = ConstantsAndEnums.ResourceType.R1;
  public float forceMult = 0.025f;

  private Transform resourceIndicator;
  private Transform direction;

  void Start() {
    base.Start();

    resourceIndicator = transform.Find("ResourceIndicator");
    direction = transform.Find("Direction");


    SetIndicator();
  }

  private void SetIndicator() {
    Material indicatorMaterial = resourceIndicator.GetComponent<Renderer>().material;
    Material directionMaterial = direction.GetComponent<Renderer>().material;
    switch(type) {
      case ConstantsAndEnums.ResourceType.R1:
        indicatorMaterial.color = Color.red;
        directionMaterial.color = Color.red;
        break;
      case ConstantsAndEnums.ResourceType.R2:
        indicatorMaterial.color = Color.blue;
        directionMaterial.color = Color.blue;
        break;
      case ConstantsAndEnums.ResourceType.R3:
        indicatorMaterial.color = Color.yellow;
        directionMaterial.color = Color.yellow;
        break;
      case ConstantsAndEnums.ResourceType.R4:
        indicatorMaterial.color = Color.green;
        directionMaterial.color = Color.green;
        break;
      case ConstantsAndEnums.ResourceType.R5:
        indicatorMaterial.color = Color.magenta;
        directionMaterial.color = Color.magenta;
        break;
    }
  }

  private void OnTriggerEnter(Collider other) {
    if(InteractableObjectsUtils.CollideWithResourceRB(other.gameObject)) {
      ResourcePOC1RB resource = other.gameObject.GetComponent<ResourcePOC1RB>();
      if(resource.type == type) {
        resource.pushedAway = true;
      }
    }
  }

  private void OnTriggerStay(Collider other) {
    if(InteractableObjectsUtils.CollideWithResourceRB(other.gameObject)) {
      ResourcePOC1RB resource = other.gameObject.GetComponent<ResourcePOC1RB>();
      if(resource.type == type && resource.pushedAway) {
        other.attachedRigidbody.MovePosition(other.attachedRigidbody.position + (transform.forward * forceMult));
      }
    }
  }

  private void OnTriggerExit(Collider other) {
    if(InteractableObjectsUtils.CollideWithResourceRB(other.gameObject)) {
      ResourcePOC1RB resource = other.gameObject.GetComponent<ResourcePOC1RB>();
      if(resource.type == type) {
        resource.pushedAway = false;
      }
    }
  }
}
