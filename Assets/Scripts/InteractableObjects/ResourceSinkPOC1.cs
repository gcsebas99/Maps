using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceSinkPOC1 : InteractableObject {

  private Transform statusInd;
  private Transform expectedResource1Ind;
  private Transform expectedResource2Ind;
  private Transform expectedResource3Ind;
  private int status = 0; //0=>start, 1=>in progress, 2=>failed, 3=>completed
  private List<ConstantsAndEnums.ResourceTypePattern> pattern = new List<ConstantsAndEnums.ResourceTypePattern>();
  private int completedRounds = 0;
  private int nextExpectedResource = 0;
  private AudioSource audioSource;

  public ConstantsAndEnums.ResourceTypePattern resource1 = ConstantsAndEnums.ResourceTypePattern.None;
  public ConstantsAndEnums.ResourceTypePattern resource2 = ConstantsAndEnums.ResourceTypePattern.None;
  public ConstantsAndEnums.ResourceTypePattern resource3 = ConstantsAndEnums.ResourceTypePattern.None;
  public int roundsToComplete = 3;

  void Start() {
    base.Start();

    statusInd = transform.Find("Status");
    expectedResource1Ind = transform.Find("ExpectedResource1");
    expectedResource2Ind = transform.Find("ExpectedResource2");
    expectedResource3Ind = transform.Find("ExpectedResource3");

    SetStateIndicator();
    SetSinkConf();

    audioSource = gameObject.AddComponent<AudioSource>();
  }

  private void SetStateIndicator() {
    Material material = statusInd.GetComponent<Renderer>().material;
    if(status == 0) {
      material.color = Color.black;
    } else if(status == 1) {
      material.color = Color.yellow;
    } else if(status == 2) {
      material.color = Color.red;
    } else {
      material.color = Color.green;
    }
  }

  private void SetSinkConf() {
    SetResourceIndicator(expectedResource1Ind, resource1);
    SetResourceIndicator(expectedResource2Ind, resource2);
    SetResourceIndicator(expectedResource3Ind, resource3);
    if(resource1 != ConstantsAndEnums.ResourceTypePattern.None) {
      pattern.Add(resource1);
    } else {
      return;
    }
    if(resource2 != ConstantsAndEnums.ResourceTypePattern.None) {
      pattern.Add(resource2);
    } else {
      return;
    }
    if(resource3 != ConstantsAndEnums.ResourceTypePattern.None) {
      pattern.Add(resource3);
    } else {
      return;
    }
  }

  private void SetResourceIndicator(Transform indicator, ConstantsAndEnums.ResourceTypePattern resource) {
    Material material = indicator.GetComponent<Renderer>().material;
    indicator.GetComponent<Renderer>().enabled = true;
    switch(resource) {
      case ConstantsAndEnums.ResourceTypePattern.R1:
        material.color = Color.red;
        break;
      case ConstantsAndEnums.ResourceTypePattern.R2:
        material.color = Color.blue;
        break;
      case ConstantsAndEnums.ResourceTypePattern.R3:
        material.color = Color.yellow;
        break;
      case ConstantsAndEnums.ResourceTypePattern.R4:
        material.color = Color.green;
        break;
      case ConstantsAndEnums.ResourceTypePattern.R5:
        material.color = Color.magenta;
        break;
      case ConstantsAndEnums.ResourceTypePattern.None:
        indicator.GetComponent<Renderer>().enabled = false;
        break;
    }
  }

  private void OnTriggerEnter(Collider other) {
    if(InteractableObjectsUtils.CollideWithResourceRB(other.gameObject)) {
      if(status == 2 || status == 3) { //when sink status is failed or completed, just remove resource
        Destroy(other.gameObject);
        return;
      }
      //test resource
      ResourcePOC1RB resource = other.gameObject.GetComponent<ResourcePOC1RB>();
      bool expected = IsExpectedResource(resource);
      if(expected) {
        //is good => status in progress => blink indicator => increase rounds if needed => status completed if needed
        status = 1;
        SetStateIndicator();
        //
        BlinkCurrentIndicator();
        //
        nextExpectedResource += 1;
        if(nextExpectedResource == pattern.Count) {
          nextExpectedResource = 0;
          completedRounds += 1;
          if(completedRounds >= roundsToComplete) {
            status = 3;
            SetStateIndicator();
            audioSource.clip = Resources.Load<AudioClip>("AudioSources/sink-completed");
            audioSource.Play();
            world.CompleteSink();
          }
        }
      } else {
        //is bad => status failed => clear failed after 2s and reset counters
        status = 2;
        SetStateIndicator();
        StartCoroutine(ClearFailedStatus());
        audioSource.clip = Resources.Load<AudioClip>("AudioSources/sink-failure");
        audioSource.Play();
      }
      //finally destroy resource
      Destroy(other.gameObject);
    }
  }

  private bool IsExpectedResource(ResourcePOC1RB resource) {
    return resource.type == (ConstantsAndEnums.ResourceType)pattern[nextExpectedResource];
  }

  private IEnumerator ClearFailedStatus() {
    yield return new WaitForSeconds(1500 / 1000f);
    //
    status = 0;
    SetStateIndicator();
    completedRounds = 0;
    nextExpectedResource = 0;
  }

  private void BlinkCurrentIndicator() {
    if(nextExpectedResource == 0) {
      StartCoroutine(BlinkIndicator(expectedResource1Ind));
    }
    if(nextExpectedResource == 1) {
      StartCoroutine(BlinkIndicator(expectedResource2Ind));
    }
    if(nextExpectedResource == 2) {
      StartCoroutine(BlinkIndicator(expectedResource3Ind));
    }
  }

  private IEnumerator BlinkIndicator(Transform indicator) {
    Material material = indicator.GetComponent<Renderer>().material;
    Color originalColor = material.color;
    yield return new WaitForSeconds(250 / 1000f);
    material.color = Color.black;
    yield return new WaitForSeconds(250 / 1000f);
    material.color = originalColor;
    yield return new WaitForSeconds(250 / 1000f);
    material.color = Color.black;
    yield return new WaitForSeconds(250 / 1000f);
    material.color = originalColor;
  }

}
