using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]

public class WeightTrigger : InteractableObject {
  //collider
  private BoxCollider collider;
  //colliding
  private bool isColliding;
  private PhysicalObject collidingObject;
  //off/on state
  private Transform offState;
  private Transform onState;
  //activation
  public float activationWeight;
  //upate
  public int updateLapseMs;
  private bool testRequired;
  //trigger status
  private bool isTriggerOn;

  // Use this for initialization
  void Start() {
    base.Start();
    collider = GetComponent<BoxCollider>();
    collider.isTrigger = true;

    //
    offState = transform.Find("OffState");
    onState = transform.Find("OnState");

    // tracking colliding works only for trigger areas that allows one specific object at a time
    isColliding = false;
    collidingObject = null;
    testRequired = false;
    isTriggerOn = false;
  }

  private void OnTriggerEnter(Collider other) {
    if(!isColliding) {
      GameObject ioObject = InteractableObjectsUtils.GetInteractableObject(other.gameObject);
      if(InteractableObjectsUtils.IsPhysicalObject(ioObject)) {
        collidingObject = ioObject.GetComponent<PhysicalObject>();
        isColliding = true;
        testRequired = false;
        TestWeight();
        StartCoroutine(RequestTest());
      }
    }

  }

  private void OnTriggerStay(Collider other) {
    if(isColliding && testRequired) {
      testRequired = false;
      TestWeight();
      StartCoroutine(RequestTest());
    }
  }

  private IEnumerator RequestTest() {
    yield return new WaitForSeconds(updateLapseMs / 1000f);
    testRequired = true;
  }

  private void OnTriggerExit(Collider other) {
    GameObject ioObject = InteractableObjectsUtils.GetInteractableObject(other.gameObject);
    if(InteractableObjectsUtils.IsPhysicalObject(ioObject)) {
      isColliding = false;
      collidingObject = null;
      TestWeight();
    }
  }

  private void TestWeight() {
    if(isColliding) {
      float wei = collidingObject.ChainedWeight();
      Debug.Log("||--test weight: " + wei);
      if(wei >= activationWeight) {
        SetStateOn();
      } else {
        SetStateOff();
      }
    } else { //no colliding, off state
      SetStateOff();
    }
  }

  private void SetStateOff() {
    if(offState != null && onState != null) {
      onState.gameObject.SetActive(false);
      offState.gameObject.SetActive(true);
      isTriggerOn = false;
    }
  }

  private void SetStateOn() {
    if(offState != null && onState != null) {
      offState.gameObject.SetActive(false);
      onState.gameObject.SetActive(true);
      isTriggerOn = true;
    }
  }

  public bool IsOn() {
    return isTriggerOn;
  }
}
