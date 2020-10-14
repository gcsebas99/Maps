using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InteractionController))]

public class WorldController : MonoBehaviour {
  //gravity
  public static float Gravity { get; private set; } = -9.8f;

  //layer detection
  public static readonly int StaticTerrainLayerMask = 1 << ConstantsAndEnums.Constants.StaticTerrainLayer;
  public static readonly int StaticObjectLayerMask = 1 << 9;
  public static readonly int IOPhysicalLayerMask = 1 << 10;
  public static readonly int IONoPhysicalLayerMask = 1 << 11;

  //instance
  public static WorldController Instance { get; private set; }

  //map changer animation
  private MapChanger mapChanger;
  private string nextLevelToLoad;


  private int totalSinks = 0;
  private int sinksCompleted = 0;
  private AudioSource audioSource;

  //level complete
  public bool levelCompleted = false;

  private void Awake() {
    if(Instance != null && Instance != this) {
      Destroy(this.gameObject);
    } else {
      Instance = this;
    }
    mapChanger = GameObject.Find("MapChanger").GetComponent<MapChanger>();
    totalSinks = GameObject.FindGameObjectsWithTag("SinkPOC1").Length;
    //
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.clip = Resources.Load<AudioClip>("AudioSources/level-completed");
  }

  //load scenes
  public void MovePlayerToLevel(string levelName) {
    nextLevelToLoad = levelName;
    mapChanger.FadeOutLevel();
  }

  public void ReloadLevel() {
    nextLevelToLoad = SceneManager.GetActiveScene().name;
    mapChanger.FadeOutLevel();
  }

  public void LoadNextLevel() {
    SceneManager.LoadScene(nextLevelToLoad);
  }

  public void CompleteSink() {
    sinksCompleted += 1;
    CheckLevelCompleted();
  }

  private void CheckLevelCompleted() {
    if(sinksCompleted >= totalSinks) {
      levelCompleted = true;
      audioSource.Play();
    }
  }
}
