using UnityEngine;

public class PlayerControllerPOC1 : PhysicalObject {

  public float speed = 2.5f;

  private float resetStartTime = 0f;
  private float holdTime = 1.5f;
  private bool resetOnce = false;


  // Start is called before the first frame update
  new void Start() {
    base.Start();
  }

  // Update is called once per frame
  void Update() {
    float z = Input.GetAxisRaw("Vertical");
    float x = Input.GetAxis("Horizontal");
    Vector3 direction = new Vector3(x, 0, z).normalized;
    Vector3 velocity = direction * speed * Time.fixedDeltaTime;
    objectMovements += velocity;
    if(velocity.magnitude > 0) {
      float yAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
      transform.localEulerAngles = new Vector3(0, yAngle, 0);
    }

    ListenForInputs();
  }

  new void FixedUpdate() {
    base.FixedUpdate();
  }

  private void ListenForInputs() {
    //FAUCETS ON/OFF
    if(!interactions.togglingFaucets && Input.GetKey(KeyCode.P)) {
      interactions.ToggleFaucets();
    }

    //RESET LEVEL
    if(Input.GetKeyDown(KeyCode.Tab)) {
      resetStartTime = Time.time;
    }
    if(Input.GetKeyUp(KeyCode.Tab)) {
      resetStartTime = 0f;
    }
    if(Input.GetKey(KeyCode.Tab)) {
      if(Time.time >= (resetStartTime + holdTime) && !resetOnce) {
        resetOnce = true;
        world.ReloadLevel();
      }
    }
  }
}
