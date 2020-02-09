using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InteractionController))]

public class WorldController : MonoBehaviour {
  //gravity
  public static float Gravity { get; private set; } = -9.8f;

  //layer detection
  public static readonly int TerrainLayerMask = 1 << 8; //hit terrain only
  public static readonly int StaticLayerMask = 1 << 9; //hit static objects only
  public static readonly int InteractableLayerMask = 1 << 10; //hit interactive objects only

  //instance
  public static WorldController Instance { get; private set; }

  //map changer animation
  private MapChanger mapChanger;
  private string nextLevelToLoad;

  private void Awake() {
    if(Instance != null && Instance != this) {
      Destroy(this.gameObject);
    } else {
      Instance = this;
    }
    mapChanger = GameObject.Find("MapChanger").GetComponent<MapChanger>();
  }

  //load scenes
  public void MovePlayerToLevel(string levelName) {
    nextLevelToLoad = levelName;
    mapChanger.FadeOutLevel();
  }

  public void LoadNextLevel() {
    SceneManager.LoadScene(nextLevelToLoad);
  }
}
