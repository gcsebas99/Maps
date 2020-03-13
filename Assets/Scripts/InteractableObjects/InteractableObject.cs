using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour {
  //enable interaction
  public bool interactEnabled = true;

  protected WorldController world;
  protected InteractionController interactions;


  // Use this for initialization
  public virtual void Start() {
    interactions = GameObject.Find("Controllers").GetComponent<InteractionController>();
    world = GameObject.Find("Controllers").GetComponent<WorldController>();
  }

  //// Update is called once per frame
  //void Update() {
  //}
}
