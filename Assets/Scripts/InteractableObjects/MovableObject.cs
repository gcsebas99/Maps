using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]

public class MovableObject : InteractableObject/*, MovableObjectTarget*/ {
  //layer masks
  //TODO: Move this to Global world controller
  private static readonly int terrainLayerMask = 1 << 8; //hit terrain only
  private static readonly int staticLayerMask = 1 << 9; //hit static objects only
  private static readonly int interactableLayerMask = 1 << 10; //hit interactable objects only
  public float gravityY = -9.8f;
  public float objectMass = 1.0f;
  //public bool showGravityDebug = false;

  //constants
  private static float nextPositionRayLengthAddition = 1.0f; //how much (below ground) should ray test for next position
  private static float standPositionRayLengthAddition = 0.25f; //how much (below ground) should ray test for current position
  public enum GroundOptions { Terrain, Interactable, Fall };
  public enum GroundPositions { Standing, Target };

  //direction constants
  public enum Direction { Up, Down, Right, Left };
  private Vector3 up = Vector3.zero;
  private Vector3 right = new Vector3(0, 90, 0);
  private Vector3 down = new Vector3(0, 180, 0);
  private Vector3 left = new Vector3(0, 270, 0);

  //direction & position
  private Vector3 currentDirection;
  private Vector3 testPosition;

  //movement restrictions variables
  public bool canClimb = true;
  public bool canDescend = true;
  public bool canFall = true;
  public bool allowTransfer = true;
  public bool allowStick = true;
  public bool shouldRotate = false;
  public bool walkableOnly = false;

  //object dimension
  private float objectHeight;
  private float objectXWidth;
  private float objectZWidth;

  //fall threshold value
  public float fallThreshold = 0.5f; //distance (from ground) to detect ground or fall
  public float frontCollisionHeight = 0.9f; //height (from ground) for front collision ray

  //applied force
  public float appliedForce = 0.0f; //force to be applied when hitting moveble objects

  //speed & taolerance
  public float speed = 5f; //moving speed
  public float moveTolerance = 0.07f;

  //controller
  private CharacterController controller;

  //movements
  private Queue<Vector3> pendingMovements;
  private Vector3 currentDestination;

  //attempt testing
  private bool resolvingAttempt;
  private Transform standingGround;
  private GroundOptions standingGroundType;
  private float standingGroundHitDistance;
  private Transform targetGround;
  private GroundOptions targetGroundType;
  private float targetGroundHitDistance;

  //---MonoBehaviour Cycle---
  void Start() {
    pendingMovements = new Queue<Vector3>();
    currentDirection = up;
    currentDestination = transform.position;
    resolvingAttempt = false;
    //
    Collider collider = transform.gameObject.GetComponent<Collider>();
    if(!collider) {
      collider = transform.GetChild(0).GetComponent<Collider>();
    }
    objectHeight = collider.bounds.size.y;
    objectXWidth = collider.bounds.size.x;
    objectZWidth = collider.bounds.size.z;
    //
    controller = GetComponent<CharacterController>();
  }

  // Update is called once per frame
  void FixedUpdate() {
    ResolvePendingMovements();
  }


  //---Move Object---
  //attempt to move object in specified direction
  public bool MoveAttempt(Direction direction, float receivedForce = 0.0f) {
    resolvingAttempt = true;
    testPosition = transform.position;
    switch(direction) {
      case Direction.Up: currentDirection = up; testPosition += Vector3.forward; break;
      case Direction.Down: currentDirection = down; testPosition += Vector3.back; break;
      case Direction.Right: currentDirection = right; testPosition += Vector3.right; break;
      case Direction.Left: currentDirection = left; testPosition += Vector3.left; break;
    }

    //adjust to .5 position
    testPosition.x = (float)Math.Round(testPosition.x * 2) / 2;
    testPosition.z = (float)Math.Round(testPosition.z * 2) / 2;

    //rotation
    if(shouldRotate) {
      transform.localEulerAngles = currentDirection;
    }

    //surface verification
    SetGroundByPosition(GroundPositions.Standing, transform.position, standPositionRayLengthAddition);
    SetGroundByPosition(GroundPositions.Target, testPosition, nextPositionRayLengthAddition);

    //terrain verification
    //1. fall
    if(targetGroundType == GroundOptions.Fall && !canFall) {
      Debug.Log("No fall");
      resolvingAttempt = false;
      return false;
    }
    //2. walkable only
    if(targetGroundType == GroundOptions.Terrain) {
      TerrainPrefabData targetTerrainData = targetGround.gameObject.GetComponent<TerrainPrefabData>();
      if(!targetTerrainData.walkable && walkableOnly) {
        Debug.Log("Walkableonly");
        resolvingAttempt = false;
        return false;
      }
    }
    //3. front collision
    if(IsFrontGroundHit(direction)) {
      Debug.Log("Front collision");
      resolvingAttempt = false;
      return false;
    }
    //4. ramps
    if(targetGroundType == GroundOptions.Terrain) {
      TerrainPrefabData targetTerrainData = targetGround.gameObject.GetComponent<TerrainPrefabData>();
      if(targetTerrainData.isRamp && targetGroundHitDistance < standingGroundHitDistance && !canClimb) {
        Debug.Log("No climb");
        resolvingAttempt = false;
        return false;
      }
      if(targetTerrainData.isRamp && targetGroundHitDistance > standingGroundHitDistance && !canDescend) {
        Debug.Log("No descend");
        resolvingAttempt = false;
        return false;
      }
    }
    //5.IO stack
    if(targetGroundType == GroundOptions.Interactable) {
      InteractableObject targetInteractableData = targetGround.gameObject.GetComponent<InteractableObject>();
      if(!targetInteractableData.allowStack) {
        Debug.Log("No IO stack");
        resolvingAttempt = false;
        return false;
      }
    }
    //6. terrain allows transition
    if(standingGroundType != GroundOptions.Fall && targetGroundType != GroundOptions.Fall && !GameObject.ReferenceEquals(standingGround.gameObject, targetGround.gameObject)) { //if not same terrain object
      int departDirection = 0;
      int arriveDirection = 0;
      if(standingGroundType == GroundOptions.Terrain) {
        TerrainPrefabData standingTerrainData = standingGround.gameObject.GetComponent<TerrainPrefabData>();
        departDirection = standingTerrainData.departDirection;
      }
      if(targetGroundType == GroundOptions.Terrain) {
        TerrainPrefabData targetTerrainData = targetGround.gameObject.GetComponent<TerrainPrefabData>();
        arriveDirection = targetTerrainData.arriveDirection;
      }
      if(!TerrainAllowsTransition(departDirection, arriveDirection, currentDirection)) {
        Debug.Log("No transition allowed");
        resolvingAttempt = false;
        return false;
      }
    }

    //static objects verification
    if(IsStaticObjectBlockingMovement(transform.position, direction, shouldRotate)) {
      Debug.Log("Static Object blocking");
      resolvingAttempt = false;
      return false;
    }




    //interactive objects/ground top-surface verification (step up verification)



    //interactive objects force transfer (push) (pull??)
    if(AttemptMovingInteractiveObjectFail(direction, receivedForce)) {
      Debug.Log("IO pushing fail");
      resolvingAttempt = false;
      return false;
    }




    //enqueue movement
    pendingMovements.Enqueue(testPosition);

    resolvingAttempt = false;
    return true;
  }

  //check if movement has finished
  public bool AcceptNewMovement() {
    return Vector3.Distance(currentDestination, transform.position) <= moveTolerance && !resolvingAttempt;
  }

  private void ResolvePendingMovements() {
    if(pendingMovements.Count > 0 && Vector3.Distance(currentDestination, transform.position) <= moveTolerance) {
      currentDestination = pendingMovements.Dequeue();
    }
    //
    //transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentDestination.x, transform.position.y, currentDestination.z), speed * Time.deltaTime);
    currentDestination.y = transform.position.y;

    //MoveTowardsTarget(currentDestination);

    MoveToPoint();
  }

  void MoveTowardsTarget(Vector3 target) {
    var offset = target - transform.position;
    //Get the difference.
    if(offset.magnitude > moveTolerance) {
      //If we're further away than .1 unit, move towards the target.
      //The minimum allowable tolerance varies with the speed of the object and the framerate. 
      // 2 * tolerance must be >= moveSpeed / framerate or the object will jump right over the stop.
      offset = offset.normalized * speed;
    }

    //add gravity
    if(!controller.isGrounded) {
      //offset.y -= gravity * Time.fixedDeltaTime;
      offset += Physics.gravity;
    }

    controller.Move(offset * Time.fixedDeltaTime);
  }

  void MoveToPoint() {
    if((Vector3.Distance(currentDestination, transform.position) <= moveTolerance) && controller.isGrounded) {
      return;
    }
    //if(currentDestination == transform.position && controller.isGrounded) {
    //  return;
    //}

    Vector3 moveDiff = currentDestination - transform.position;
    Vector3 moveDir = moveDiff.normalized * speed * Time.fixedDeltaTime;

    //Vector3 grav = Physics.gravity * objectMass * Time.fixedDeltaTime;
    Vector3 grav = new Vector3(0, gravityY * objectMass * Time.fixedDeltaTime, 0);

    if(moveDir.sqrMagnitude < moveDiff.sqrMagnitude) {
      //add gravity
      if(!controller.isGrounded) {
        moveDir += grav;
        //if(showGravityDebug) {
        //  Debug.Log("gravity1: " + grav + " moveDir: " + moveDir);
        //}
      }
      Debug.Log("Moving to: " + currentDestination);
      controller.Move(moveDir);
    } else {

      moveDiff = moveDiff * speed * Time.fixedDeltaTime;

      if(!controller.isGrounded) {
        moveDiff += grav;
        //if(showGravityDebug) {
        //  Debug.Log("gravity2: " + grav + " moveDiff: " + moveDiff);
        //}
      }
      Debug.Log("Moving to: " + currentDestination);
      controller.Move(moveDiff);
    }
  }










  //set ground by specific position
  private void SetGroundByPosition(GroundPositions groundType, Vector3 testPosition, float terrainRayLengthAddition) {
    Ray terrainRay = new Ray(testPosition, new Vector3(0, -1, 0)); //pointing down
    Ray interactiveObjRay = new Ray(testPosition, new Vector3(0, -1, 0)); //pointing down
    RaycastHit terrainHit;
    RaycastHit interactiveObjHit;
    float rayLength = (objectHeight / 2) + terrainRayLengthAddition;
    float fallDistance = (objectHeight / 2) + fallThreshold;
    //
    Debug.DrawRay(terrainRay.origin, terrainRay.direction, Color.red);
    Debug.DrawRay(terrainRay.origin, terrainRay.direction, Color.blue);
    //
    bool terrainFirst = false;
    bool interactiveFirst = false;
    bool hitsTerrain = Physics.Raycast(terrainRay, out terrainHit, rayLength, terrainLayerMask);
    bool hitsInteractive = Physics.Raycast(interactiveObjRay, out interactiveObjHit, rayLength, interactableLayerMask);

    if(hitsTerrain || hitsInteractive) {
      if(hitsTerrain && hitsInteractive) {
        //define by distance
        if(terrainHit.distance < interactiveObjHit.distance) {
          terrainFirst = true;
        } else {
          interactiveFirst = true;
        }
      } else if(hitsTerrain) {
        terrainFirst = true;
      } else {
        interactiveFirst = true;
      }
    }
    if(terrainFirst) {
      TerrainPrefabData terrainData = terrainHit.transform.gameObject.GetComponent<TerrainPrefabData>();
      //Debug.Log("Hit terrain at " + terrainHit.distance + " and fallDistance: " + fallDistance + " for: " + groundType);
      if(terrainHit.distance < fallDistance || (terrainData.walkable && terrainData.isRamp)) {
        if(groundType == GroundPositions.Standing) {
          standingGround = terrainHit.transform;
          standingGroundType = GroundOptions.Terrain;
          standingGroundHitDistance = terrainHit.distance;
        } else {
          targetGround = terrainHit.transform;
          targetGroundType = GroundOptions.Terrain;
          targetGroundHitDistance = terrainHit.distance;
        }
        return;
      }
    } else if(interactiveFirst) {
      //Debug.Log("Hit interactive at: " + interactiveObjHit.distance + " and fall: " + fallDistance + " for: " + groundType);
      if(interactiveObjHit.distance < fallDistance) {
        if(groundType == GroundPositions.Standing) {
          standingGround = interactiveObjHit.transform;
          standingGroundType = GroundOptions.Interactable;
          standingGroundHitDistance = interactiveObjHit.distance;
        } else {
          targetGround = interactiveObjHit.transform;
          targetGroundType = GroundOptions.Interactable;
          targetGroundHitDistance = interactiveObjHit.distance;
        }
        return;
      }
    }

    //else is a fall
    if(groundType == GroundPositions.Standing) {
      standingGround = null;
      standingGroundType = GroundOptions.Fall;
      standingGroundHitDistance = 0.0f;
    } else {
      targetGround = null;
      targetGroundType = GroundOptions.Fall;
      targetGroundHitDistance = 0.0f;
    }
  }

  private bool IsStaticObjectBlockingMovement(Vector3 currentPosition, Direction direction, bool objectRotates) {
    Vector3 origin = currentPosition;
    Vector3 dimension = Vector3.zero;
    switch(direction) {
      case Direction.Up: origin += new Vector3(0f, 0f, 0.75f); break;
      case Direction.Down: origin += new Vector3(0f, 0f, -0.75f); break;
      case Direction.Right: origin += new Vector3(0.75f, 0f, 0f); break;
      case Direction.Left: origin += new Vector3(-0.75f, 0f, 0f); break;
    }
    //dimension
    switch(direction) {
      case Direction.Up:
      case Direction.Down:
        dimension = new Vector3(objectXWidth, objectHeight, 1.0f + (objectZWidth / 2));
        break;
      case Direction.Right:
      case Direction.Left:
        if(objectRotates) {
          dimension = new Vector3(1.0f + (objectZWidth / 2), objectHeight, objectXWidth);
        } else {
          dimension = new Vector3(1.0f + (objectXWidth / 2), objectHeight, objectZWidth);
        }
        break;
    }
    Collider[] hitStaticObjectColliders = Physics.OverlapBox(origin, (dimension / 2), Quaternion.identity, staticLayerMask);
    return hitStaticObjectColliders.Length > 0;
  }

  private bool IsFrontGroundHit(Direction direction) {
    Vector3 origin = transform.position - (new Vector3(0, (objectHeight / 2), 0)) + (new Vector3(0, frontCollisionHeight, 0));
    Vector3 rayDirection = Vector3.forward;
    switch(direction) {
      case Direction.Up: rayDirection = Vector3.forward; break;
      case Direction.Down: rayDirection = Vector3.back; break;
      case Direction.Right: rayDirection = Vector3.right; break;
      case Direction.Left: rayDirection = Vector3.left; break;
    }
    Ray terrainRay = new Ray(origin, rayDirection);
    RaycastHit terrainHit;
    float rayLength = 0.7f;
    //Debug.DrawRay(terrainRay.origin, terrainRay.direction, Color.green);
    if(Physics.Raycast(terrainRay, out terrainHit, rayLength, terrainLayerMask)) {
      return true;
    } else {
      return false;
    }
  }

  private bool AttemptMovingInteractiveObjectFail(Direction direction, float receivedForce) {
    Vector3 origin = transform.position - (new Vector3(0, (objectHeight / 2), 0)) + (new Vector3(0, 0.4f, 0));
    Vector3 rayDirection = Vector3.forward;
    switch(direction) {
      case Direction.Up: rayDirection = Vector3.forward; break;
      case Direction.Down: rayDirection = Vector3.back; break;
      case Direction.Right: rayDirection = Vector3.right; break;
      case Direction.Left: rayDirection = Vector3.left; break;
    }
    Ray ioRay = new Ray(origin, rayDirection);
    RaycastHit ioHit;
    float rayLength = 1.0f;
    Debug.DrawRay(ioRay.origin, ioRay.direction, Color.green);
    if(Physics.Raycast(ioRay, out ioHit, rayLength, interactableLayerMask)) {
      Debug.Log("IO found!!");
      MovableObject movableObject = ioHit.transform.gameObject.GetComponent<MovableObject>(); //TODO: this may be IO but not movable
      bool moved = movableObject.MoveAttempt(direction, receivedForce + appliedForce);
      Debug.Log("IO moved:" + moved);
      return !moved;
    } else {
      return false;
    }
  }

  private bool TerrainAllowsTransition(int departDirection, int arriveDirection, Vector3 direction) {
    if(departDirection == 1 && (direction == right || direction == left)) { //vertical
      return false;
    } else if(departDirection == 2 && (direction == up || direction == down)) { //horizontal
      return false;
    } else if(arriveDirection == 1 && (direction == right || direction == left)) { //vertical
      return false;
    } else if(arriveDirection == 2 && (direction == up || direction == down)) { //horizontal
      return false;
    }
    return true;
  }




  //private float positionRayLength = 1.5f;

  //public void PhysicPush(GameObject generator, Vector3 nextPosition) {
  //  Debug.Log("PhysicPush CALLED");
  //  Vector3 testPosition = transform.position + nextPosition;
  //  //detect if push can be done
  //  (RaycastHit standingHit, RaycastHit nextPositionHit) = GetTerrainTransitionObjects(testPosition);
  //  //if(nextPositionHit.)
  //  //Debug.Log("standingHit " + standingHit.transform.gameObject.name);

  //  //terrain accepts push
  //  //valid terrain change
  //  //no blocker objects
  //  //or
  //  //there's a hole (no terrain)
  //  //no blocker objects

  //  ExecuteEvents.Execute<InteractableActionGenerator>(generator, null, (x, y) => x.PhysicPushResult(true));
  //}

  //private (RaycastHit, RaycastHit) GetTerrainTransitionObjects(Vector3 testPosition) {
  //  int layerMask = 1 << 8; //hit terrain only
  //  Ray standingRay = new Ray(transform.position + new Vector3(0, 0.1f, 0), new Vector3(0, -1, 0));
  //  Ray nextPositionRay = new Ray(testPosition + new Vector3(0, 0.1f, 0), new Vector3(0, -1, 0));
  //  RaycastHit standingHit;
  //  RaycastHit nextPositionHit = new RaycastHit();
  //  Debug.DrawRay(standingRay.origin, standingRay.direction, Color.red);
  //  Debug.DrawRay(nextPositionRay.origin, nextPositionRay.direction, Color.green);

  //  //Physics.Raycast(nextPositionRay, out nextPositionHit, positionRayLength, layerMask);



  //  if(Physics.Raycast(standingRay, out standingHit, positionRayLength, layerMask)) {
  //    Debug.Log("here " + standingHit);
  //    if(Physics.Raycast(nextPositionRay, out nextPositionHit, positionRayLength, layerMask)) {

  //      //Debug.Log("Did Hit " + nextPositionHit.distance);
  //      //TerrainPrefabData standingData = standingHit.transform.gameObject.GetComponent<TerrainPrefabData>();
  //      //TerrainPrefabData nextPositionData = nextPositionHit.transform.gameObject.GetComponent<TerrainPrefabData>();
  //      //next is walkable
  //      //if(nextPositionData.walkable) {
  //      //  //(current accepts out direction and next accepts in direction) or current and next are same object
  //      //  if(TerrainAllowsTransition(standingData, nextPositionData, direction) || GameObject.ReferenceEquals(standingHit.transform.gameObject, nextPositionHit.transform.gameObject)) {
  //      //    return true;
  //      //  }
  //      //}
  //    }
  //    Debug.Log("there " + nextPositionHit.transform == null);
  //  }
  //  return (standingHit, nextPositionHit);
  //}

}

//public interface MovableObjectTarget : IEventSystemHandler {
//  void PhysicPush(GameObject generator, Vector3 nextPosition);
//}