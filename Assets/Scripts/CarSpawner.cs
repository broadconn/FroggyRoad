using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour {
    [SerializeField]
    Material[] mats;

    Transform carTypesParent;
    Transform lastSpawnedCar;

    //car settings
    int carType;
    float speed;
    int dir; //-1 or 1
    float distBetweenSpawns;
    int matIdx;

    //misc 
    float spawnX, despawnX;

    private void Awake() {
        carTypesParent = GameObject.Find("CarTypes").transform;
    }

    // Use this for initialization
    void Start() {
        carType = Random.Range(0, carTypesParent.childCount);
        speed = Random.Range(1f, 3f);
        dir = Random.Range(0f, 1f) > 0.5f ? 1 : -1;
        matIdx = Random.Range(0, 4);
        distBetweenSpawns = Random.Range(4, 10);

        float roadLength = 18f;
        spawnX = -dir * roadLength; 
        despawnX = dir * spawnX;

        SpawnInitialCars();
    }

    void SpawnInitialCars() {
        float curX = spawnX;
        bool finishedSpawning = false;
        while (!finishedSpawning) {
            curX += dir * distBetweenSpawns;
            SpawnCar(curX);
            if (dir < 0 && curX < despawnX || dir > 0 && curX > despawnX) finishedSpawning = true;
        }
    }

    // Update is called once per frame
    void Update() {
        if (lastSpawnedCar == null || Mathf.Abs(spawnX - lastSpawnedCar.position.x) > distBetweenSpawns) {
            SpawnCar(spawnX);
        }
    }

    void SpawnCar(float xPos) {
        //clone chosen car
        Transform clone = Instantiate(carTypesParent.GetChild(carType));
        clone.transform.SetParent(null, false);

        //place at spawn point
        clone.transform.position = new Vector3(xPos, clone.transform.position.y, transform.position.z);

        //orient to face direction
        clone.transform.localScale = new Vector3(clone.transform.localScale.x * -dir, clone.transform.localScale.y, clone.transform.localScale.z);

        //set car settings
        clone.GetComponent<Car>().SetSettings(dir, speed, gameObject);

        //set materials
        Renderer[] ChildrenRenderer = clone.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in ChildrenRenderer) {
            if (r.sharedMaterial.name == "Red")
                r.sharedMaterial = mats[matIdx];
        }

        distBetweenSpawns = Random.Range(4, 10);
        lastSpawnedCar = clone;
    }
}
