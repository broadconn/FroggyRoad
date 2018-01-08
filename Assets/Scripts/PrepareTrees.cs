using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareTrees : MonoBehaviour {
    List<int> playFieldIdxs = new List<int>();

    // Use this for initialization
    void Start() {
        for (int i = 5; i < 14; i++) playFieldIdxs.Add(i);

        //randomly remove eligible indexes from the pool
        int emptySpaces = Random.Range(2, 9);
        for (int i = 0; i < emptySpaces; i++) {
            playFieldIdxs.RemoveAt(Random.Range(0, playFieldIdxs.Count)); 
        }

        //randomize tree heights + hide hidden ones
        for (int i = 0; i < transform.childCount; i++) {
            GameObject tree = transform.GetChild(i).gameObject;
            tree.SetActive(playFieldIdxs.Contains(i));

            Transform leaves = tree.transform.Find("Leaves");
            if(leaves) leaves.localScale = new Vector3(leaves.localScale.x, Random.Range(0.7f, 1.2f), leaves.localScale.z);
        }
    }
}
