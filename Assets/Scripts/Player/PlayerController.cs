using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MovableObject))]
public class PlayerController : MonoBehaviour {

  private MovableObject movableObject;
  //private Vector3 nextPosition;

  // Use this for initialization
  void Start() {
    movableObject = GetComponent<MovableObject>();
  }

  // Update is called once per frame
  void Update() {
    ListenForInputs();
  }

  private void ListenForInputs() {
    bool allowNewInput = movableObject.AcceptNewMovement();

    if(allowNewInput) {
      if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
        movableObject.MoveAttempt(MovableObject.Direction.Up);
      }
      if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
        movableObject.MoveAttempt(MovableObject.Direction.Down);
      }
      if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
        movableObject.MoveAttempt(MovableObject.Direction.Right);
      }
      if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
        movableObject.MoveAttempt(MovableObject.Direction.Left);
      }
    }
  }
}
