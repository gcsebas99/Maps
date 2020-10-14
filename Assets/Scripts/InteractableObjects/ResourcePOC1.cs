using UnityEngine;

public class ResourcePOC1 : PhysicalObject {

  public ConstantsAndEnums.ResourceType type = ConstantsAndEnums.ResourceType.R1;

  private Material material;

  private bool touchingGround = false;
  private bool touchingTerrain = false;

  private void Awake() {
    //set resource color
    material = GetComponent<Renderer>().material;
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


  void Update() {
    if(!touchingGround && controller.isGrounded) {
      TestGround();
    }
  }

  void OnControllerColliderHit(ControllerColliderHit hit) {
    if(hit.gameObject.layer == ConstantsAndEnums.Constants.StaticTerrainLayer && hit.moveDirection.y < 0) { //Resource falls to ground
      Debug.Log("Resource falls to ground!!");
    }
    //Debug.Log("Resource hits " + hit.gameObject.layer);
    //Debug.Log("Resource hits dir " + hit.moveDirection.y);
  }

  private void TestGround() {
    touchingGround = true;
  }


}