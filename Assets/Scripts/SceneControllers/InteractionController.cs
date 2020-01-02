using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour {
  public static InteractionController Instance { get; private set; }

  private List<PausedMovement> pausedMovements = new List<PausedMovement>();

  private void Awake() {
    if(Instance != null && Instance != this) {
      Destroy(this.gameObject);
    } else {
      Instance = this;
    }
  }

  public void AddMovementPauseOverPosition(Vector3 position, MovableObject.Direction direction, int pauseMs) {
    PausedMovement movement = new PausedMovement();
    movement.pausedPosition = position;
    movement.pausedDirection = direction;
    pausedMovements.Add(movement);
    StartCoroutine(ClearPausedMovement(position, direction, pauseMs));
  }

  public bool IsMovementAllowed(Vector3 position, MovableObject.Direction direction) {
    PausedMovement movement = pausedMovements.Find(x => x.pausedDirection == direction && (Vector3.Distance(x.pausedPosition, position) < 0.5f));
    if(movement != null) {
      //movement is blocked
      return false;
    }
    return true;
  }

  private IEnumerator ClearPausedMovement(Vector3 position, MovableObject.Direction direction, int pauseMs) {
    yield return new WaitForSeconds(pauseMs / 1000f);
    //
    PausedMovement movement = pausedMovements.Find(x => x.pausedPosition == position && x.pausedDirection == direction);
    if(movement != null) {
      pausedMovements.Remove(movement);
    }
  }
}
