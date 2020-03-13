using System;
using System.Collections;
using System.Collections.Generic;
using ConstantsAndEnums;
using UnityEngine;

public class MovableObject : PhysicalObject {

  public float objectMass = 1.0f;

  public float terrainOffsetAllowedDistance = 0.15f;
  public float interactiveOffsetDetection = 0.4f;

  private bool movementPaused = false;
  private bool pushPullTriggered;

  //constants
  private static float nextPositionRayLengthAddition = 1.0f; //how much (below ground) should ray test for next position
  private static float standPositionRayLengthAddition = 0.7f; //how much (below ground) should ray test for current position

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
  public float movableRayOffsetYIncrement = 0f; //how much y-axis should be moved to set collision detection rays

  //applied force
  public float appliedForce = 0.0f; //force to be applied when hitting moveble objects

  //speed & taolerance
  public float speed = 5f; //moving speed
  public float driveSpeed = 2.5f;
  public float moveTolerance = 0.07f;

  //movements
  private Queue<PendingMovement> pendingMovements;
  private Vector3 currentDestination;
  private PendingMovement currentMovement;

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
    base.Start();

    pendingMovements = new Queue<PendingMovement>();
    currentDirection = Constants.StepUp;
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

  }

  // Update is called once per frame
  new void FixedUpdate() {
    ResolvePendingMovements();
    base.FixedUpdate();
  }


  //---Move Object---
  //attempt to move object in specified direction
  public bool MoveAttempt(Direction direction, float receivedForce = 0.0f, bool moveGenerator = false) {
    resolvingAttempt = true;
    pushPullTriggered = false;
    testPosition = transform.position;
    switch(direction) {
      case Direction.Up: currentDirection = Constants.StepUp; testPosition += Vector3.forward; break;
      case Direction.Down: currentDirection = Constants.StepDown; testPosition += Vector3.back; break;
      case Direction.Right: currentDirection = Constants.StepRight; testPosition += Vector3.right; break;
      case Direction.Left: currentDirection = Constants.StepLeft; testPosition += Vector3.left; break;
    }

    //adjust to .5 position
    testPosition.x = (float)Math.Round(testPosition.x * 2) / 2;
    testPosition.z = (float)Math.Round(testPosition.z * 2) / 2;

    //test force if it's a push/pull
    if(!moveGenerator) {
      if(receivedForce < objectMass) {
        //Debug.Log("No enough force for " + transform.name);
        resolvingAttempt = false;
        return false;
      }
    }

    //rotation
    if(shouldRotate) {
      transform.localEulerAngles = currentDirection;
    }

    //surface verification
    SetGroundByPosition(GroundPositions.Standing, transform.position, standPositionRayLengthAddition);
    SetGroundByPosition(GroundPositions.Target, testPosition, nextPositionRayLengthAddition);

    //Debug.Log("--ST:" + standingGroundType + "    --TG:" + targetGroundType + "     --for: " + transform.name);

    //world verification
    if(!interactions.IsMovementAllowed(transform.position, direction)) {
      //Debug.Log("Position+Direction is blocked! for " + transform.name);
      resolvingAttempt = false;
      return false;
    }
    //terrain verification
    //1. fall
    if(targetGroundType == GroundOptions.Fall && !canFall) {
      //Debug.Log("No fall for " + transform.name);
      resolvingAttempt = false;
      return false;
    }
    //2. walkable only
    if(targetGroundType == GroundOptions.Terrain) {
      TerrainPrefabData targetTerrainData = targetGround.gameObject.GetComponent<TerrainPrefabData>();
      if(!targetTerrainData.walkable && walkableOnly) {
        //Debug.Log("Walkableonly for " + transform.name);
        resolvingAttempt = false;
        return false;
      }
    }
    //3. front collision
    if(IsFrontGroundHit(direction)) {
      //Debug.Log("Front collision for " + transform.name);
      resolvingAttempt = false;
      return false;
    }
    //4. ramps
    if(targetGroundType == GroundOptions.Terrain) {
      TerrainPrefabData targetTerrainData = targetGround.gameObject.GetComponent<TerrainPrefabData>();
      if(targetTerrainData.isRamp && targetGroundHitDistance < standingGroundHitDistance && !canClimb) {
        //Debug.Log("No climb for " + transform.name);
        resolvingAttempt = false;
        return false;
      }
      if(targetTerrainData.isRamp && targetGroundHitDistance > standingGroundHitDistance && !canDescend) {
        //Debug.Log("No descend for " + transform.name);
        resolvingAttempt = false;
        return false;
      }
    }
    //5.IO stack
    if(targetGroundType == GroundOptions.Interactable) {
      GameObject ioObject = InteractableObjectsUtils.GetInteractableObject(targetGround.gameObject);
      MovableObject targetInteractableData = ioObject.GetComponent<MovableObject>();
      if(!targetInteractableData.allowGroundable) {
        //Debug.Log("No IO stack for " + transform.name);
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
        //Debug.Log("No transition allowed for " + transform.name);
        resolvingAttempt = false;
        return false;
      }
    }

    //static objects verification
    if(IsStaticObjectBlockingMovement(transform.position + new Vector3(0f, movableRayOffsetYIncrement, 0f), direction, shouldRotate)) {
      //Debug.Log("Static Object blocking for " + transform.name);
      resolvingAttempt = false;
      return false;
    }

    //interactive objects force transfer (push) (pull??)
    if(AttemptMovingInteractiveObjectFail(direction, receivedForce, moveGenerator)) {
      //Debug.Log("IO pushing fail for " + transform.name);
      resolvingAttempt = false;
      return false;
    }

    //enqueue movement
    PendingMovement movement = new PendingMovement {
      destination = testPosition,
      generator = moveGenerator,
      pushPullTriggered = pushPullTriggered
    };
    if(targetGroundType == GroundOptions.Fall) {
      movement.requiresPause = true;
      movement.pausedPosition = transform.position;
      movement.pausedDirection = direction;
    }
    if(movement.generator && movement.pushPullTriggered) {
      movement.startMovePause = 120;
    }

    pendingMovements.Enqueue(movement);

    resolvingAttempt = false;
    return true;
  }

  //check if movement has finished
  public bool AcceptNewMovement() {
    //old condition to accept movements (stops continuos movements a little)
    //return !resolvingAttempt && !movementPaused && Vector3.Distance(currentDestination, transform.position) <= moveTolerance;
    return !resolvingAttempt && !movementPaused && Vector3.Distance(currentDestination, transform.position) <= 0.1f && pendingMovements.Count == 0;
  }

  private void ResolvePendingMovements() {
    if(pendingMovements.Count > 0 && Vector3.Distance(currentDestination, transform.position) <= moveTolerance) {
      currentMovement = pendingMovements.Dequeue();
      currentDestination = currentMovement.destination;
      if(currentMovement.requiresPause) {
        interactions.AddMovementPauseOverPosition(currentMovement.pausedPosition, currentMovement.pausedDirection, 700);
      }
      if(currentMovement.startMovePause > 0) {
        movementPaused = true;
        StartCoroutine(ClearMovementPause(currentMovement.startMovePause));
      }
    }
    currentDestination.y = transform.position.y;
    if(!movementPaused) {
      MoveToPoint();
    }
  }

  private void MoveToPoint() {
    if(Vector3.Distance(currentDestination, transform.position) < (moveTolerance + 0.01f)) {
      transform.position = currentDestination;
    }

    if((Vector3.Distance(currentDestination, transform.position) <= moveTolerance) && controller.isGrounded) {
      if(currentMovement != null && currentMovement.endMovePause > 0) {
        movementPaused = true;

        StartCoroutine(ClearMovementPause(currentMovement.endMovePause));
        currentMovement.endMovePause = 0;
      }
      return;
    }

    float currentSpeed = (currentMovement != null && currentMovement.generator && !currentMovement.pushPullTriggered) ? speed : driveSpeed;

    Vector3 moveDiff = currentDestination - transform.position;
    Vector3 moveDir = moveDiff.normalized * currentSpeed * Time.fixedDeltaTime;

    if(moveDir.sqrMagnitude < moveDiff.sqrMagnitude) {
      //controller.Move(moveDir);
      objectMovements += moveDir;
    } else {
      moveDiff = moveDiff * currentSpeed * Time.fixedDeltaTime;
      //controller.Move(moveDiff);
      objectMovements += moveDiff;
    }
  }

  private IEnumerator ClearMovementPause(int pauseMs) {
    yield return new WaitForSeconds(pauseMs / 1000f);
    //
    movementPaused = false;
  }

  //set ground by specific position
  private void SetGroundByPosition(GroundPositions groundType, Vector3 testPosition, float terrainRayLengthAddition) {
    testPosition += new Vector3(0f, movableRayOffsetYIncrement, 0f);
    Ray terrainRay = new Ray(testPosition, new Vector3(0, -1, 0)); //pointing down
    Ray interactiveObjRay = new Ray(testPosition, new Vector3(0, -1, 0)); //pointing down
    RaycastHit terrainHit;
    RaycastHit interactiveObjHit;
    float rayLength = movableRayOffsetYIncrement + terrainRayLengthAddition;
    float fallDistance = movableRayOffsetYIncrement + fallThreshold;
    //
    //Debug.DrawRay(terrainRay.origin, terrainRay.direction, Color.red);
    //DrawRay(terrainRay.origin, terrainRay.direction, Color.blue);
    //
    bool terrainFirst = false;
    bool interactiveFirst = false;
    bool hitsTerrain = Physics.Raycast(terrainRay, out terrainHit, rayLength, WorldController.StaticTerrainLayerMask);
    bool hitsInteractive = Physics.Raycast(interactiveObjRay, out interactiveObjHit, rayLength, WorldController.IOPhysicalLayerMask);
    hitsInteractive = hitsInteractive && interactiveObjHit.transform != transform; //not hit on itself

    bool validHitsInteractive = false;
    if(hitsInteractive) {
      float stepDistance = interactiveObjHit.distance - movableRayOffsetYIncrement;
      validHitsInteractive = stepDistance >= (terrainOffsetAllowedDistance * -1) && stepDistance <= terrainOffsetAllowedDistance;
    }
    if(hitsTerrain || validHitsInteractive) {
      if(hitsTerrain && validHitsInteractive) {
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
    Collider[] hitStaticObjectColliders = Physics.OverlapBox(origin, (dimension / 2), Quaternion.identity, WorldController.StaticObjectLayerMask);
    return hitStaticObjectColliders.Length > 0;
  }

  private bool IsFrontGroundHit(Direction direction) {
    Vector3 origin = transform.position + new Vector3(0f, movableRayOffsetYIncrement, 0f) - (new Vector3(0, (objectHeight / 2), 0)) + (new Vector3(0, frontCollisionHeight, 0));
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
    if(Physics.Raycast(terrainRay, out terrainHit, rayLength, WorldController.StaticTerrainLayerMask)) {
      return true;
    } else {
      return false;
    }
  }

  private bool AttemptMovingInteractiveObjectFail(Direction direction, float receivedForce, bool moveGenerator) {
    Vector3 origin = transform.position + new Vector3(0f, interactiveOffsetDetection, 0f);
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
    //Debug.DrawRay(ioRay.origin, ioRay.direction, Color.green);
    if(Physics.Raycast(ioRay, out ioHit, rayLength, WorldController.IOPhysicalLayerMask)) {
      GameObject ioObject = InteractableObjectsUtils.GetInteractableObject(ioHit.transform.gameObject);
      MovableObject movableObject = ioObject.GetComponent<MovableObject>(); //TODO: this may be IO but not movable
      float outForce = appliedForce;
      if(!moveGenerator && allowTransfer) {
        outForce += (receivedForce - objectMass);
      }
      bool moved = movableObject.MoveAttempt(direction, outForce);
      if(moved) {
        pushPullTriggered = true;
        interactions.AddMovementPauseOverPosition(transform.position, direction, 1000);
      }
      return !moved;
    } else {
      return false;
    }
  }

  private bool TerrainAllowsTransition(int departDirection, int arriveDirection, Vector3 direction) {
    if(departDirection == 1 && (direction == Constants.StepRight || direction == Constants.StepLeft)) { //vertical
      return false;
    } else if(departDirection == 2 && (direction == Constants.StepUp || direction == Constants.StepDown)) { //horizontal
      return false;
    } else if(arriveDirection == 1 && (direction == Constants.StepRight || direction == Constants.StepLeft)) { //vertical
      return false;
    } else if(arriveDirection == 2 && (direction == Constants.StepUp || direction == Constants.StepDown)) { //horizontal
      return false;
    }
    return true;
  }

}
