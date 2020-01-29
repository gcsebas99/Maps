using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InteractionController))]

public class WorldController : MonoBehaviour {
  public static float Gravity { get; private set; } = -9.8f;

  //layer detection
  public static readonly int TerrainLayerMask = 1 << 8; //hit terrain only
  public static readonly int StaticLayerMask = 1 << 9; //hit static objects only
  public static readonly int InteractableLayerMask = 1 << 10; //hit interactive objects only


  public static WorldController Instance { get; private set; }

  private void Awake() {
    if(Instance != null && Instance != this) {
      Destroy(this.gameObject);
    } else {
      Instance = this;
    }
  }
}
