using UnityEngine;
using System.Collections;

public class CollectableObject : PhysicalObject {
  //animator
  public Animator floatingAnimator;

  // Use this for initialization
  void Start() {
    base.Start();

  }

  new void FixedUpdate() {
    base.FixedUpdate();

    if(controller.isGrounded) {
      floatingAnimator.enabled = true;
    } else {
      floatingAnimator.enabled = false;
    }
  }
}
