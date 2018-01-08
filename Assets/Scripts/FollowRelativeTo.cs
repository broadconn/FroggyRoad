using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRelativeTo : MonoBehaviour {
    [SerializeField]
    Transform target;
    [SerializeField]
    bool x = true;
    [SerializeField]
    bool y = true;
    [SerializeField]
    bool z = true;

    Vector3 targetLastPos;
    Vector3 offset;

    // Use this for initialization
    void Start() {
        targetLastPos = target.position;
        offset = target.position - transform.position;
    }

    // Update is called once per frame
    void Update() {
        Vector3 diff = target.position - targetLastPos;
        diff = new Vector3((x ? 1 : 0) * diff.x, (y ? 1 : 0) * diff.y, (z ? 1 : 0) * diff.z);
        transform.position += diff;
        targetLastPos = target.position;
    }
}
