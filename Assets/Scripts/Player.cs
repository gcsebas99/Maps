using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerControllerOld))]

public class Player : MonoBehaviour {

  public float moveSpeed = 5;
  bool wasMovingHorizontal = false;
  PlayerControllerOld controller;

  private Vector3 fp;   //First touch position
  private Vector3 lp;   //Last touch position
  private float dragDistance;  //minimum distance for a swipe to be registered
  private float fpTime;
  private float lpTime;
  private string swipeDuration = "";
  private string lastSwipe = "";

  // Start is called before the first frame update
  void Start() {
    controller = GetComponent<PlayerControllerOld>();
    dragDistance = Screen.height * 15 / 100; //dragDistance is 15% height of the screen
  }

  // Update is called once per frame
  void Update() {
    float horizontal = Input.GetAxisRaw("Horizontal");
    bool isMovingHorizontal = Mathf.Abs(horizontal) > 0.5f;
    float vertical = Input.GetAxisRaw("Vertical");
    bool isMovingVertical = Mathf.Abs(vertical) > 0.5f;

    if(isMovingHorizontal && isMovingVertical) {
      if(wasMovingHorizontal) {
        Vector3 moveInput = new Vector3(horizontal, 0, 0);
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.move(moveVelocity);
      } else {
        Vector3 moveInput = new Vector3(0, 0, vertical);
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.move(moveVelocity);
      }
    } else if(isMovingHorizontal) {
      Vector3 moveInput = new Vector3(horizontal, 0, 0);
      Vector3 moveVelocity = moveInput.normalized * moveSpeed;
      controller.move(moveVelocity);
      wasMovingHorizontal = true;
    } else if(isMovingVertical) {
      Vector3 moveInput = new Vector3(0, 0, vertical);
      Vector3 moveVelocity = moveInput.normalized * moveSpeed;
      controller.move(moveVelocity);
      wasMovingHorizontal = false;
    } else {
      controller.move(Vector3.zero);
    }

    this.handleSwipeMove();


    //Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    //Vector3 moveVelocity = moveInput.normalized * moveSpeed;
    //controller.move(moveVelocity);
  }

  private void handleSwipeMove() {
    if(Input.touchCount == 1) {
      Touch touch = Input.GetTouch(0); // get the touch
      if(touch.phase == TouchPhase.Began) { //check for the first touch
        fp = touch.position;
        lp = touch.position;
        fpTime = Time.time;
      } else if(touch.phase == TouchPhase.Moved) { // update the last position based on where they moved
        lp = touch.position;
        this.checkSwipe(false);

      } else if(touch.phase == TouchPhase.Ended) { //check if the finger is removed from the screen
        lp = touch.position;  //last touch position. Ommitted if you use list
        lpTime = Time.time;

        swipeDuration = (lpTime - fpTime).ToString();

        this.checkSwipe(true);
      }
    }
  }

  private void OnGUI() {
    GUI.Label(new Rect(30, 40, 150, 20), ("Swipe dur: " + swipeDuration));
    GUI.Label(new Rect(30, 80, 150, 20), ("Last swipe: " + lastSwipe));
  }

  private bool isASwipe() {
    return Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance;
  }

  private bool isHorizontalSwipe() {
    return Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y);
  }

  private void checkSwipe(bool phaseEnded) {
    //Check if drag distance is greater than 15% of the screen height
    if(this.isASwipe()) {//It's a drag
      if(this.isHorizontalSwipe()) { //If the horizontal movement is greater than the vertical movement
        if((lp.x > fp.x)) { //Right swipe
          lastSwipe = "Right Swipe";
          Vector3 moveInput = new Vector3(1f, 0, 0);
          Vector3 moveVelocity = moveInput.normalized * moveSpeed;
          controller.move(moveVelocity);
        } else {   //Left swipe
          lastSwipe = "Left Swipe";
          Vector3 moveInput = new Vector3(-1f, 0, 0);
          Vector3 moveVelocity = moveInput.normalized * moveSpeed;
          controller.move(moveVelocity);
        }
      } else {   //the vertical movement is greater than the horizontal movement
        if(lp.y > fp.y) { //Up swipe
          lastSwipe = "Up Swipe";
          Vector3 moveInput = new Vector3(0, 0, 1f);
          Vector3 moveVelocity = moveInput.normalized * moveSpeed;
          controller.move(moveVelocity);
        } else {   //Down swipe
          lastSwipe = "Down Swipe";
          Vector3 moveInput = new Vector3(0, 0, -1f);
          Vector3 moveVelocity = moveInput.normalized * moveSpeed;
          controller.move(moveVelocity);
        }
      }
    } else {   //It's a tap as the drag distance is less than 20% of the screen height
      if(phaseEnded) {
        lastSwipe = "Tap";
      }
    }
  }
}
