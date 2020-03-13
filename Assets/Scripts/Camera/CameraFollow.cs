using UnityEngine;

public class CameraFollow : MonoBehaviour {

  public Transform target;
  public float smoothSpeed = 0.03f;
  public Vector3 offset;
  public bool enabled;
  public float distanceBeforeFollow = 3.0f;

  private Vector3 pivotPosition;
  private bool following;

  private void Start() {
    pivotPosition = target.position;
    transform.position = pivotPosition + offset;
    transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
    following = false;
  }

  private void FixedUpdate() {
    if(enabled) {
      float distance = Vector3.Distance(pivotPosition, target.position);
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
