using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerControllerOld : MonoBehaviour {
  Rigidbody myRigidBody;
  Vector3 velocity;

  // Start is called before the first frame update
  void Start() {
    myRigidBody = GetComponent<Rigidbody>();
  }

  // Update is called once per frame
  //void Update () {

  //}

  public void move(Vector3 _velocity) {
    velocity = _velocity;
  }

  public void FixedUpdate() {
    myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime);
  }
}
