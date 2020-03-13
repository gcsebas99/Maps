using UnityEngine;
using System.Collections;
using ConstantsAndEnums;

public class ObjectCollector : MonoBehaviour {
  private bool performingAction;
  public static GameObject[] prefabs;

  // Use this for initialization
  void Start() {
    performingAction = false;
    prefabs = Resources.LoadAll<GameObject>("Prefabs");
  }

  // Update is called once per frame
  void Update() {

  }

  public bool DropApple() {
    if(!performingAction) {
      performingAction = true;
      Debug.Log("Dropping an apple!! " + transform.name);

      Vector3 testPosition = transform.position;
      if(transform.localEulerAngles == Constants.StepUp) {
        testPosition += Vector3.forward;
      } else if(transform.localEulerAngles == Constants.StepDown) {
        testPosition += Vector3.back;
      } else if(transform.localEulerAngles == Constants.StepRight) {
        testPosition += Vector3.right;
      } else {
        testPosition += Vector3.left;
      }

      testPosition += new Vector3(0, 0.5f, 0);

      GameObject prefab = MapGeneratorUtilities.FindPrefab(prefabs, "Apple");
      GameObject apple = Instantiate(prefab, testPosition, Quaternion.identity);


      StartCoroutine(ClearAction(1000));
    }
    return true;
  }

  private IEnumerator ClearAction(int pauseMs) {
    yield return new WaitForSeconds(pauseMs / 1000f);
    //
    performingAction = false;
  }
}
