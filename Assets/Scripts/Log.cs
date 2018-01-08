using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Log : MonoBehaviour {
    public int scrollDir = 1; //1 or -1 = right or left
    public float scrollSpeed = 1;
    Rigidbody rb;
    bool active = true; //log can be a gap if not active
    List<Log> restOfLog = new List<Log>();
    Transform model;

    float logWidth = 0;
    float modelY = 0;

    float timeTriggeredBob = -10;
    float bobHeight = 0.2f;

    float zWidth = 0.75f;

    public bool Active {
        get { return active; }
    }

    private void Awake() {
        model = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        logWidth = transform.parent.childCount;
        modelY = model.position.y;
        model.localScale = new Vector3(model.localScale.x, model.localScale.y, zWidth);
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        //move
        float xPos = transform.position.x + (scrollSpeed * Mathf.Sign(scrollDir)) * Time.deltaTime;
        if (scrollDir > 0 && xPos > logWidth / 2f) {
            float diff = xPos - (logWidth / 2f);
            xPos = -logWidth / 2f + diff;
        }
        else if (scrollDir < 0 && xPos < -logWidth / 2f) {
            float diff = (-logWidth / 2f) - xPos;
            xPos = logWidth / 2f - diff;
        }

        //finalize movement
        Vector3 tgtPos = new Vector3(xPos, transform.position.y, transform.position.z);
        rb.MovePosition(tgtPos);
        
        //water bob (only affects the model)
        float bobTimePassed = Time.time - timeTriggeredBob; 
        float bobMod = bobHeight * FrogControl.Instance.WaterBob.Evaluate(bobTimePassed / FrogControl.Instance.BobTime);
        model.transform.localPosition = new Vector3(0, modelY + bobMod, 0);
    }

    public void SetStats(bool active, int scrollDir, float scrollSpeed) {
        this.active = active;
        transform.GetChild(0).gameObject.SetActive(active);
        this.scrollDir = scrollDir;
        this.scrollSpeed = scrollSpeed;
    }

    public void SetBrothers(List<Log> brothers) {
        restOfLog = brothers;
    }

    public void WaterBobLog() {
        foreach (Log l in restOfLog)
            l.SetWaterBobTime();
    }

    public void SetWaterBobTime() {
        timeTriggeredBob = Time.time;
    }
}
