using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldGen : MonoBehaviour {
    public static int worldWidth = 9;

    static int difficulty = 1;
    public static int Difficulty {
        get { return difficulty; }
    }

    List<GameObject> world = new List<GameObject>();
    Transform player;
    WorldLayerType curLayer = WorldLayerType.Grass;
    int curSpawnZ = 2;
    float playerOffset = 0;

    // Use this for initialization
    void Start() {
        //spawn initial world
        SpawnLayers(15);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerOffset = curSpawnZ - player.position.z;
    }

    // Update is called once per frame
    void Update() {
        //remove oldest 
        if(player.position.z + playerOffset > curSpawnZ) {
            SpawnNextSection();
            DespawnOldest();
        }
    }

    void SpawnLayers(int num2Spawn) { 
        while (curSpawnZ < num2Spawn) {
            SpawnNextSection();
        }
    }

    //controls how big each section can be
    void SpawnNextSection() {
        curLayer = GetNextLayer();
        int numLayers = 0;
        switch (curLayer) {
            case WorldLayerType.Grass:
                numLayers = Random.Range(1, 3);
                break;
            case WorldLayerType.Logs:
                numLayers = Random.Range(1, 5);
                break;
            case WorldLayerType.Road:
                numLayers = Random.Range(1, 7);
                break;
        }
        for(int i = 0; i < numLayers; i++) {
            GameObject l = SpawnLayer(curLayer);

            //remove the dashed line from the first road layer
            if(i == 0 && curLayer == WorldLayerType.Road) {
                l.transform.Find("Dash").gameObject.SetActive(false);
            }
        }
    }

    void DespawnOldest() {
        while (world[0].transform.position.z < player.position.z - 5) {
            GameObject old = world[0];
            world.RemoveAt(0);
            Destroy(old);
        }
    }

    WorldLayerType GetNextLayer() {
        List<WorldLayerType> allTypes = System.Enum.GetValues(typeof(WorldLayerType)).Cast<WorldLayerType>().ToList();
        allTypes.Remove(curLayer); 
        return allTypes[Random.Range(0, allTypes.Count)];
    }

    GameObject SpawnLayer(WorldLayerType t) {
        string prefabLoc = "";
        switch (t) {
            case WorldLayerType.Grass:
                prefabLoc = "WorldLayers/Grass";
                break;
            case WorldLayerType.Logs:
                prefabLoc = "WorldLayers/Log";
                break;
            case WorldLayerType.Road:
                prefabLoc = "WorldLayers/Road";
                break;
        }
        GameObject layer = (GameObject)Instantiate(Resources.Load(prefabLoc));
        layer.transform.position = new Vector3(0, layer.transform.position.y, curSpawnZ);
        world.Add(layer);
        curSpawnZ++;
        return layer;
    }
}

public enum WorldLayerType {
    Grass, Logs, Road
}
