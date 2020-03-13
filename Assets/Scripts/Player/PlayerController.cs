using UnityEngine;
using System.Collections;
using ConstantsAndEnums;

[RequireComponent(typeof(MovableObject))]
[RequireComponent(typeof(ObjectCollector))]
public class PlayerController : MonoBehaviour {

  private MovableObject movableObject;
  private ObjectCollector objectCollector;

  // Use this for initialization
  void Start() {
    movableObject = GetComponent<MovableObject>();
    objectCollector = GetComponent<ObjectCollector>();
  }

  // Update is called once per frame
  void Update() {
    ListenForInputs();
  }

  private void ListenForInputs() {
    bool allowNewMovementInput = movableObject.AcceptNewMovement();

    if(allowNewMovementInput) {
      if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
        movableObject.MoveAttempt(Direction.Up, 0f, true);
      }
      if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
        movableObject.MoveAttempt(Direction.Down, 0f, true);
      }
      if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
        movableObject.MoveAttempt(Direction.Right, 0f, true);
      }
      if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
        movableObject.MoveAttempt(Direction.Left, 0f, true);
      }
    }

    if(Input.GetKey(KeyCode.Q)) {
      objectCollector.DropApple();
    }

    if(Input.GetKey(KeyCode.Space)) {
      transform.position = new Vector3(transform.position.x, 4.0f, transform.position.z);
    }
  }
}
