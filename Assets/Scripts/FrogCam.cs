using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogCam : MonoBehaviour {
    [SerializeField]
    Transform target;
    [SerializeField]
    float smooth = 5;
    [SerializeField]
    Vector3 offset = new Vector3(0, 5, -5);
    [SerializeField]
    Vector2 XZoffset = new Vector2(0, 0);

    Vector3 tgtPos, lookPos;
    float lookSmooth = 2;

    bool activated = false; //activates when the target gets to the center of the screen 

    // Update is called once per frame
    void Update() {
        lookPos = Vector3.Lerp(lookPos, new Vector3(target.position.x / 2, 0, target.position.z) + new Vector3(XZoffset.x, 0, XZoffset.y), Time.deltaTime * lookSmooth);
        tgtPos = lookPos + offset;
        transform.position = tgtPos;// Vector3.Lerp(transform.position, tgtPos, Time.deltaTime * smooth);
        transform.rotation = Quaternion.LookRotation(lookPos - transform.position);
    }
}
