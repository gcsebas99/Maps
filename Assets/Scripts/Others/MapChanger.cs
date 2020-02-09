using UnityEngine;

public class MapChanger : MonoBehaviour {
  //world controller
  private WorldController worldController;
  //animator
  public Animator animator;

  private void Start() {
    worldController = GameObject.Find("Controllers").GetComponent<WorldController>();
  }

  public void FadeOutLevel() {
    animator.SetTrigger("FadeOut");
  }

  public void OnFadeComplete() {
    worldController.LoadNextLevel();
  }
}
