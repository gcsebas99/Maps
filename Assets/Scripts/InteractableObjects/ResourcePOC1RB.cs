using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePOC1RB : MonoBehaviour {

  public ConstantsAndEnums.ResourceType type = ConstantsAndEnums.ResourceType.R1;
  public bool pushedAway = false;

  private float distToGround;
  private Material material;
  private bool didTouchGround = false;
  private bool grounded = false;

  private void Awake() {
    //set resource color
    material = GetComponent<Renderer>().material;
    UpdateType();
  }

  private void Start() {
    distToGround = GetComponent<Collider>().bounds.extents.y;
  }

  private void Update() {
    if(didTouchGround) {
      TestGrounded();
    }
  }

  public void UpdateType() {
    switch(type) {
      case ConstantsAndEnums.ResourceType.R1:
        material.color = Color.red;
        break;
      case ConstantsAndEnums.ResourceType.R2:
        material.color = Color.blue;
        break;
      case ConstantsAndEnums.ResourceType.R3:
        material.color = Color.yellow;
        break;
      case ConstantsAndEnums.ResourceType.R4:
        material.color = Color.green;
        break;
      case ConstantsAndEnums.ResourceType.R5:
        material.color = Color.magenta;
        break;
    }
  }

  void OnCollisionEnter(Collision collision) {
    Vector3 normal = collision.contacts[0].normal;
    if(collision.gameObject.layer == ConstantsAndEnums.Constants.StaticTerrainLayer && normal.y > 0) { //Resource falls to ground
      didTouchGround = true;
      //Debug.Log("Resource falls to ground with normal:" + normal + " and position " + gameObject.transform.position);
      StartCoroutine(RemoveIfGrounded(2000));
    } else {
      //Debug.Log("||--I never touched the ground :(");
    }
  }

  private void TestGrounded() {
    grounded = IsGrounded();
  }

  private bool IsGrounded() {
    //Ray terrainRay = new Ray(transform.position + new Vector3(0, distToGround, 0), -Vector3.up);
    //Debug.DrawRay(terrainRay.origin, terrainRay.direction, Color.green);
    return Physics.Raycast(transform.position + new Vector3(0, distToGround, 0), -Vector3.up, distToGround + 0.5f, WorldController.StaticTerrainLayerMask);
  }

  private IEnumerator RemoveIfGrounded(int pauseMs) {
    yield return new WaitForSeconds(pauseMs / 1000f);
    //
    if(grounded) {
      //Debug.Log("Grounded & destroy");
      Destroy(gameObject);
    } else {
      //Debug.Log("||--Not touching ground anymore :)");
      didTouchGround = false;
    }
  }

}