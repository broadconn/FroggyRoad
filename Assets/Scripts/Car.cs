using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {
    int scrollDir = 1; //1 or -1 = right or left
    float scrollSpeed = 1;
    const float roadWidth = 18;

    bool going = false;
    GameObject road;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (!going) return;

        float xPos = transform.position.x + (scrollSpeed * Mathf.Sign(scrollDir)) * Time.deltaTime;

        //check for despawn
        if (scrollDir > 0 && xPos > roadWidth || scrollDir < 0 && xPos < -roadWidth
            || road == null) {
            Destroy(gameObject);
        }

        Vector3 tgtPos = new Vector3(xPos, transform.position.y, transform.position.z);
        transform.position = tgtPos;
    }

    public void SetSettings(int dir, float speed, GameObject road) {
        scrollDir = dir;
        scrollSpeed = speed;
        going = true;
        this.road = road;
    }
}
