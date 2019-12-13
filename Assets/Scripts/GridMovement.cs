using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class GridMovement : MonoBehaviour/*, InteractableActionGenerator*/ {

  private Vector3 up = Vector3.zero;
  private Vector3 right = new Vector3(0, 90, 0);
  private Vector3 down = new Vector3(0, 180, 0);
  private Vector3 left = new Vector3(0, 270, 0);
  private Vector3 currentDirection = Vector3.zero;

  private Vector3 nextPosition, destination;

  private float speed = 5f;
  private float positionRayLength = 1.5f;
  private bool canMove;

  void Start() {
    currentDirection = up;
    nextPosition = Vector3.forward;
    destination = transform.position;
  }

  // Update is called once per frame
  void Update() {
    Move();
  }

  private void Move() {
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(destination.x, transform.position.y, destination.z), speed * Time.deltaTime);
    destination.y = transform.position.y;
    bool allowNewInput = Vector3.Distance(transform.position, destination) < 0.01f;

    if(allowNewInput) {
      if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
        nextPosition = Vector3.forward;
        currentDirection = up;
        canMove = true;
      }
      if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
        nextPosition = Vector3.back;
        currentDirection = down;
        canMove = true;
      }
      if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
        nextPosition = Vector3.right;
        currentDirection = right;
        canMove = true;
      }
      if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
        nextPosition = Vector3.left;
        currentDirection = left;
        canMove = true;
      }
    }

    if(Vector3.Distance(destination, transform.position) <= 0.00001f) {
      transform.localEulerAngles = currentDirection;
      if(canMove) {
        Vector3 testNewPosition = transform.position + nextPosition;
        if(ValidPositionByTileData(testNewPosition, currentDirection) &&
           ValidPositionWithNavigatableSpace(transform.position, testNewPosition, currentDirection, nextPosition)) {
          destination = transform.position + nextPosition;
          canMove = false;
        } else {
          canMove = false;
        }
      }
    }
  }

  private bool ValidPositionByTileData(Vector3 testPosition, Vector3 direction) {
    //set rays
    int layerMask = 1 << 8; //hit terrain only
    Ray standingRay = new Ray(transform.position, new Vector3(0, -1, 0));
    Ray nextPositionRay = new Ray(testPosition, new Vector3(0, -1, 0));
    RaycastHit standingHit;
    RaycastHit nextPositionHit;
    //Debug.DrawRay(standingRay.origin, standingRay.direction, Color.red);
    //Debug.DrawRay(nextPositionRay.origin, nextPositionRay.direction, Color.green);
    //detect collision
    if(Physics.Raycast(standingRay, out standingHit, positionRayLength, layerMask)) {
      if(Physics.Raycast(nextPositionRay, out nextPositionHit, positionRayLength, layerMask)) {
        //Debug.Log("Did Hit " + nextPositionHit.distance);
        TerrainPrefabData standingData = standingHit.transform.gameObject.GetComponent<TerrainPrefabData>();
        TerrainPrefabData nextPositionData = nextPositionHit.transform.gameObject.GetComponent<TerrainPrefabData>();
        //next is walkable
        if(nextPositionData.walkable) {
          //(current accepts out direction and next accepts in direction) or current and next are same object
          if(TerrainAllowsTransition(standingData, nextPositionData, direction) || GameObject.ReferenceEquals(standingHit.transform.gameObject, nextPositionHit.transform.gameObject)) {
            return true;
          }
        }
      }
    }
    return false;
  }

  private bool TerrainAllowsTransition(TerrainPrefabData standingData, TerrainPrefabData nextPositionData, Vector3 direction) {
    if(standingData.departDirection == 1 && (direction == right || direction == left)) { //vertical
      return false;
    } else if(standingData.departDirection == 2 && (direction == up || direction == down)) { //horizontal
      return false;
    } else if(nextPositionData.arriveDirection == 1 && (direction == right || direction == left)) { //vertical
      return false;
    } else if(nextPositionData.arriveDirection == 2 && (direction == up || direction == down)) { //horizontal
      return false;
    }
    return true;
  }

  private bool ValidPositionWithNavigatableSpace(Vector3 currentPosition, Vector3 testPosition, Vector3 direction, Vector3 nextPosition) {
    //check for objects that may block navigation

    int layerMask = 1 << 9; //hit staticObjects only
    Collider[] hitColliders = Physics.OverlapBox(testPosition, new Vector3(0.4f, 0.55f, 0.4f), Quaternion.identity, layerMask);
    if(hitColliders.Length > 0) {
      int i = 0;
      //Check when there is a new collider coming into contact with the box
      while(i < hitColliders.Length) {
        //Output all of the collider names
        //Debug.Log("Hit : " + hitColliders[i].name);
        //Increase the number of Colliders in the array
        i++;
      }
      return false;
    }

    //between cells
    Vector3 middle = (currentPosition + testPosition) / 2;
    Vector3 middleShape = (direction == up || direction == down) ? new Vector3(0.4f, 0.55f, 0.15f) : new Vector3(0.15f, 0.55f, 0.4f);
    Collider[] hitMiddleColliders = Physics.OverlapBox(middle, middleShape, Quaternion.identity, layerMask);
    if(hitMiddleColliders.Length > 0) {
      int i = 0;
      //Check when there is a new collider coming into contact with the box
      while(i < hitMiddleColliders.Length) {
        //Output all of the collider names
        //Debug.Log("Middle Hit : " + hitMiddleColliders[i].name);
        //Increase the number of Colliders in the array
        i++;
      }
      return false;
    }

    //check for interactable objects to push/pull
    int interactableLayerMask = 1 << 10;
    Collider[] interactableColliders = Physics.OverlapBox(testPosition, new Vector3(0.4f, 0.55f, 0.4f), Quaternion.identity, interactableLayerMask);
    if(interactableColliders.Length > 0) {
      if(interactableColliders.Length > 1) {
        //more than 1
        return false;
      }
      Collider target = interactableColliders[0];
      if((target.gameObject.GetComponent("MovableObject") as MovableObject) != null) {


        //ExecuteEvents.Execute<MovableObjectTarget>(target.gameObject, null, (x, y) => x.PhysicPush(gameObject, nextPosition));
        //Debug.Log("Movable RESULT: " + resultWhat);

        //no move if push required
        return false;
      } else {
        //not movable
        return false;
      }
      //int i = 0;
      ////Check when there is a new collider coming into contact with the box
      //while(i < interactableColliders.Length) {
      //  //Output all of the collider names
      //  Debug.Log("HitInteract : " + interactableColliders[i].name);
      //  //Increase the number of Colliders in the array
      //  i++;
      //}
      //return false;
    }

    return true;
  }

  //interface
  //public void PhysicPushResult(bool result) {
  //  Debug.Log("PhysicPushResult CALLED : " + result);
  //}
}

//public interface InteractableActionGenerator : IEventSystemHandler {
//  void PhysicPushResult(bool result);
//}
