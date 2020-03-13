using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InteractionController))]

public class WorldController : MonoBehaviour {
  //gravity
  public static float Gravity { get; private set; } = -9.8f;

  //layer detection
  public static readonly int StaticTerrainLayerMask = 1 << 8;
  public static readonly int StaticObjectLayerMask = 1 << 9;
  public static readonly int IOPhysicalLayerMask = 1 << 10;
  public static readonly int IONoPhysicalLayerMask = 1 << 11;

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
