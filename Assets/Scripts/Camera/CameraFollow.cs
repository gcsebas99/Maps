using UnityEngine;

public class CameraFollow : MonoBehaviour {

  public Transform target;
  public float smoothSpeed = 0.03f;
  public Vector3 offset;
  public bool enabled;
  public float distanceBeforeFollow = 3.0f;

  private Vector3 pivotPosition;
  private bool following;

  //private Vector3 lastTargetPosition;

  private void Start() {
    pivotPosition = target.position;
    transform.position = pivotPosition + offset;
    following = false;
  }

  private void FixedUpdate() {
    if(enabled) {
      float distance = Vector3.Distance(pivotPosition, target.position);
      //Debug.Log("pivot: " + pivotPosition + "distance: " + distance);
      if(distance > distanceBeforeFollow) {
        following = true;
      }
      if(distance < 0.03f) {
        following = false;
      }
      if(following) {

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

        pivotPosition = smoothedPosition - offset;
        //transform.LookAt(target);
      }
    }
  }

}
