using System;
using UnityEngine;

public class TransportationBand : InteractableObject {
  public float scrollXspeed = 0f;
  public float scrollYspeed = -0.5f;
  public float forceMult = 0.02f;

  private float bandCenter;
  private bool horizontal;

  void Start() {
    base.Start();
    float halfWidth = transform.localScale.x / 2;
    switch(transform.eulerAngles.y) {
      case 270f:
        bandCenter = transform.position.z + halfWidth;
        horizontal = true;
        break;
      case 180f:
        bandCenter = transform.position.x - halfWidth;
        horizontal = false;
        break;
      case 90f:
        bandCenter = transform.position.z - halfWidth;
        horizontal = true;
        break;
      case 0f:
      default:
        bandCenter = transform.position.x + halfWidth;
        horizontal = false;
        break;
    }
  }

  void FixedUpdate() {
    float offsetX = Time.fixedTime * scrollXspeed;
    float offsetY = Time.fixedTime * scrollYspeed;
    GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offsetX, offsetY);
  }

  //private void OnTriggerEnter(Collider other) {
  //  if(InteractableObjectsUtils.CollideWithResourceRB(other.gameObject)) {
  //    //Debug.Log("||--ResEnter- beltX: " + transform.localScale.x);
  //    //Debug.Log(transform.name + "||--ResEnter- beltPos: " + transform.position);
  //    //Debug.Log(transform.name + "||--ResEnter- resPos: " + other.transform.position);
  //    //Debug.Log(transform.name + "||--ResEnter- beltCenter: " + bandCenter + " -horizontal: " + horizontal);
  //  }
  //}

  private void OnTriggerStay(Collider other) {
    if(InteractableObjectsUtils.CollideWithResourceRB(other.gameObject)) {
      Vector3 centerAdjust = Vector3.zero;
      ResourcePOC1RB resource = other.gameObject.GetComponent<ResourcePOC1RB>();
      if(!resource.pushedAway) {
        if(horizontal) {
          float diffToCenter = Mathf.Abs(other.attachedRigidbody.position.z - bandCenter);
          if(diffToCenter > 0.05f) {
            float dir = bandCenter - other.attachedRigidbody.position.z;
            centerAdjust = (new Vector3(0f, 0f, dir).normalized) * forceMult;
          }
        } else {
          float diffToCenter = Mathf.Abs(other.attachedRigidbody.position.x - bandCenter);
          if(diffToCenter > 0.05f) {
            float dir = bandCenter - other.attachedRigidbody.position.x;
            centerAdjust = (new Vector3(dir, 0f, 0f).normalized) * forceMult;
          }
        }
      }
      other.attachedRigidbody.MovePosition(other.attachedRigidbody.position + (transform.forward * forceMult) + centerAdjust);
      
    }
  }

  //private void OnTriggerExit(Collider other) {
  //  if(InteractableObjectsUtils.CollideWithResourceRB(other.gameObject)) {
  //    other.attachedRigidbody.constraints = RigidbodyConstraints.None;
  //  }
  //}
}
