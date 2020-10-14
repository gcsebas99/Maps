using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceFaucetPOC1 : InteractableObject {
  public static GameObject[] prefabs;

  private Vector3 faucetCenter; //at top of faucet
  private bool faucetOn = false;
  private Transform statusInd;
  private Transform resource1Ind;
  private Transform resource2Ind;
  private Transform resource3Ind;
  private List<ConstantsAndEnums.ResourceTypePattern> pattern = new List<ConstantsAndEnums.ResourceTypePattern>();
  private bool spawnNeeded = false;
  private int nextResource = 0;

  public ConstantsAndEnums.SpawnPosition spawnPosition = ConstantsAndEnums.SpawnPosition.Xp;
  public ConstantsAndEnums.ResourceTypePattern resource1 = ConstantsAndEnums.ResourceTypePattern.None;
  public ConstantsAndEnums.ResourceTypePattern resource2 = ConstantsAndEnums.ResourceTypePattern.None;
  public ConstantsAndEnums.ResourceTypePattern resource3 = ConstantsAndEnums.ResourceTypePattern.None;
  public int spawnRate = 1500;

  void Start() {
    base.Start();
    prefabs = Resources.LoadAll<GameObject>("Prefabs");

    statusInd = transform.Find("Status");
    resource1Ind = transform.Find("Resource1");
    resource2Ind = transform.Find("Resource2");
    resource3Ind = transform.Find("Resource3");

    Vector3 sizeVec = GetComponent<Collider>().bounds.size;
    faucetCenter = transform.position + new Vector3((sizeVec.x / 2), sizeVec.y, (sizeVec.z / 2));

    SetStateIndicator();
    SetFaucetConf();

    //Debug.Log("Faucet position " + transform.position);
    //Debug.Log("Faucet size " + sizeVec);
  }

  // Update is called once per frame
  void Update() {
    if(interactions.faucetsOn != faucetOn) {
      spawnNeeded = interactions.faucetsOn;
      faucetOn = interactions.faucetsOn;
      nextResource = 0;
      SetStateIndicator();
    }
    if(faucetOn && spawnNeeded) {
      SpawnNextResource();
    }
  }

  private void SetFaucetConf() {
    SetResourceIndicator(resource1Ind, resource1);
    SetResourceIndicator(resource2Ind, resource2);
    SetResourceIndicator(resource3Ind, resource3);
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

  private void SetStateIndicator() {
    Material material = statusInd.GetComponent<Renderer>().material;
    if(faucetOn) {
      material.color = Color.green;
    } else {
      material.color = Color.black;
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

  private Vector3 GetSpawnPosition() {
    switch(spawnPosition) {
      case ConstantsAndEnums.SpawnPosition.Xp:
        return new Vector3(1f, 0, 0);
      case ConstantsAndEnums.SpawnPosition.Xn:
        return new Vector3(-1f, 0, 0);
      case ConstantsAndEnums.SpawnPosition.Zp:
        return new Vector3(0, 0, 1f);
      case ConstantsAndEnums.SpawnPosition.Zn:
        return new Vector3(0, 0, -1f);
    }
    return new Vector3(0, 0, 0);
  }

  private void SpawnNextResource() {
    spawnNeeded = false;
    //
    ConstantsAndEnums.ResourceType type = (ConstantsAndEnums.ResourceType)pattern[nextResource];
    nextResource += 1;
    if(nextResource == pattern.Count) {
      nextResource = 0;
    }
    //
    Vector3 position = faucetCenter + GetSpawnPosition();
    GameObject prefab = MapGeneratorUtilities.FindPrefab(prefabs, "ResourcePOC1RB");
    GameObject ngo = Instantiate(prefab, position, Quaternion.identity);
    ResourcePOC1RB resource = ngo.GetComponent<ResourcePOC1RB>();
    resource.type = type;
    resource.UpdateType();
    //
    StartCoroutine(RequireNewSpawn());
  }

  private IEnumerator RequireNewSpawn() {
    yield return new WaitForSeconds(spawnRate / 1000f);
    //
    spawnNeeded = true;
  }


}
