using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareLog : MonoBehaviour {

    // Use this for initialization
    void Start() {
        int diff = WorldGen.Difficulty;
        int scrollDir = transform.position.z % 2 == 0 ? 1 : -1;
        float scrollSpeed = Random.Range(0.7f, 1.5f);

        int i = -1;
        while (i < transform.childCount) {
            //place + setup log
            int logSize = Random.Range(1, 4);
            List<Log> logPieces = new List<Log>();
            for (int j = i + 1; j <= i + logSize; j++) {
                if (j < transform.childCount) {
                    Log piece = transform.GetChild(j).GetComponent<Log>();
                    piece.SetStats(true, scrollDir, scrollSpeed);
                    logPieces.Add(piece);
                }
            }

            //tell each piece the other parts of the same log
            for (int j = i + 1; j <= i + logSize; j++) {
                if (j < transform.childCount) {
                    Log piece = transform.GetChild(j).GetComponent<Log>();
                    piece.SetBrothers(logPieces);
                }
            }
            i += logSize;

            //add gap
            int gapSize = Random.Range(1, 3);
            for (int j = i + 1; j <= i + gapSize; j++) {
                if (j < transform.childCount) {
                    transform.GetChild(j).GetComponent<Log>().SetStats(false, scrollDir, scrollSpeed);
                }
            }
            i += gapSize;
        }
        
        //always have the last log be an empty slot so  it doesnt wrap around as a different log but look like one log
        Log piece2 = transform.GetChild(transform.childCount - 1).GetComponent<Log>();
        piece2.SetStats(false, scrollDir, scrollSpeed);
    }
}
