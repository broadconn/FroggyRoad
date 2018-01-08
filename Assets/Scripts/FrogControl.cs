using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FrogControl : MonoBehaviour {
    static FrogControl instance;
    public static FrogControl Instance {
        get { return instance; }
    }

    [SerializeField]
    AnimationCurve jumpXZAnim;
    [SerializeField]
    AnimationCurve jumpYAnim;
    [SerializeField]
    AnimationCurve waterBob;
    [SerializeField]
    AnimationCurve jumpScaleAnim;

    Transform model;
    Rigidbody rb;

    FrogState state = FrogState.Idle;
    Vector3 jumpDir = Vector3.forward;
    Vector3 lookDir = Vector3.forward;

    const float jumpTime = 0.18f;
    float timeTriggeredJump;
    Vector3 jumpOrigin;
    float jumpHeight = 0.5f;
    GameObject movingThing;

    float timeDied = 0;
    const float timeDeadBeforeReset = 1.0f;

    List<Action> moveCommands = new List<Action>();

    ParticleSystem splashParticles, deathParticles;

    float modelY;
    float timeTriggeredBob = -10;
    float bobHeight = 0.2f;
    float bobTime = 0.2f;

    public AnimationCurve WaterBob {
        get { return waterBob; }
    }

    public float BobTime {
        get { return bobTime; }
    }

    private void Awake() {
        instance = this;
        model = transform.GetChild(0);
        modelY = model.localPosition.y;
        rb = GetComponent<Rigidbody>();
        splashParticles = GameObject.Find("Splash").GetComponent<ParticleSystem>();
        deathParticles = GameObject.Find("Death").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update() {
        HandleInput();

        float jumpY = 0;

        //move
        Vector3 tgtMovePos = transform.position;

        //handle jump
        if (state == FrogState.Jumping) {
            float timeThroughJump = Time.time - timeTriggeredJump;
            float jumpTimeMod = jumpTime / ((moveCommands.Count / 2f) + 1);
            float percThroughJump = timeThroughJump / jumpTimeMod;
            float adjustedTimePos = jumpXZAnim.Evaluate(percThroughJump);
            Vector3 tgtPos = jumpOrigin + jumpDir;

            //if on moving thing, move towards its center point
            if (movingThing != null) {
                tgtPos = new Vector3(movingThing.transform.position.x, transform.position.y, movingThing.transform.position.z);
                tgtMovePos = Vector3.Lerp(jumpOrigin, tgtPos, adjustedTimePos);
            }
            //otherwise move towards the int point
            else {
                tgtPos = new Vector3(Mathf.RoundToInt(tgtPos.x), tgtPos.y, Mathf.RoundToInt(tgtPos.z));
                tgtMovePos = Vector3.Lerp(jumpOrigin, tgtPos, adjustedTimePos);
            }
            jumpY = jumpYAnim.Evaluate(percThroughJump) * jumpHeight;

            if (percThroughJump >= 1) JumpEnded();
        }
        else if (state == FrogState.Idle) {
            CheckForQueuedCommands();
        }
        rb.MovePosition(tgtMovePos);

        //rotate 
        float ang = (lookDir.x > 0 ? 1 : -1) * Vector3.Angle(Vector3.forward, lookDir);
        model.eulerAngles = new Vector3(0, Mathf.LerpAngle(model.eulerAngles.y, ang, Time.deltaTime * 15), 0);

        //water bob
        float bobTimePassed = Time.time - timeTriggeredBob;
        float bobMod = bobHeight * WaterBob.Evaluate(bobTimePassed / bobTime);
        model.transform.localPosition = new Vector3(model.transform.localPosition.x, modelY + jumpY + bobMod, model.transform.localPosition.z);

        //restart if dead
        if (state == FrogState.Dead) {
            float timePassed = Time.time - timeDied;
            if (timePassed > timeDeadBeforeReset)
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    void CheckForQueuedCommands() {
        if (state == FrogState.Dead) return;
        if (moveCommands.Count > 0) {
            moveCommands[0]();
            moveCommands.RemoveAt(0);
        }
    }

    void HandleInput() {
        //check for look 
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
            lookDir = Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            lookDir = Vector3.left;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            lookDir = Vector3.forward;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            lookDir = Vector3.back;
        }
        //check for jump 
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D)) {
            moveCommands.Add(() => { Jump(Vector3.right); });
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A)) {
            moveCommands.Add(() => { Jump(Vector3.left); });
        }
        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)) {
            moveCommands.Add(() => { Jump(Vector3.forward); });
        }
        if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) {
            moveCommands.Add(() => { Jump(Vector3.back); });
        }
    }

    void Jump(Vector3 dir) {
        jumpDir = dir;
        lookDir = dir;

        //test for jumping onto a log
        Vector3 testInDir = transform.position + new Vector3(0, -0.5f, 0) + jumpDir;
        Collider[] hitColliders = Physics.OverlapSphere(testInDir, 0.5f);
        movingThing = null;
        List<Transform> logsInRange = new List<Transform>();
        for (int i = 0; i < hitColliders.Length; i++) {
            if (hitColliders[i].tag == "Moving")
                logsInRange.Add(hitColliders[i].transform);
        }
        if (logsInRange.Count > 0) { //if we have logs...
            //order by closest
            float closestDist = float.MaxValue;
            Transform closest = logsInRange[0];
            foreach (Transform t in logsInRange) {
                float dist = Vector2.Distance(transform.position, t.position);
                if (t.GetComponent<Log>().Active && !closest.GetComponent<Log>().Active || dist < closestDist) { //if this is closer 
                    closest = t;
                    closestDist = dist;
                }
            }
            movingThing = closest.gameObject;
        }

        //check for attempting to jump into tree or other obstruction
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), dir, 1f)) {
            jumpDir = Vector3.zero;
            //TODO animate hit-a-wall
            return;
        }

        state = FrogState.Jumping;
        timeTriggeredJump = Time.time;
        jumpOrigin = transform.position;
    }

    void JumpEnded() {
        state = FrogState.Idle;

        //check for jumped into water, kill the frog dead >:D
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f)) {
            if (hit.transform.tag == "Moving") {
                if (!hit.transform.GetComponent<Log>().Active) {
                    Destroy(hit.transform.gameObject);
                    rb.velocity = Vector3.zero;
                    rb.AddForce(Vector3.down * 10, ForceMode.Impulse);
                    state = FrogState.Dead;
                    timeDied = Time.time;
                    splashParticles.transform.position = transform.position;
                    splashParticles.Play();
                    return; //die
                }
            }
        }

        //check for jumped-onto-log waterbob
        if (movingThing != null) {
            timeTriggeredBob = Time.time;
            movingThing.GetComponent<Log>().WaterBobLog();
        }

        CheckForQueuedCommands();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Vehicle") {
            transform.localScale = new Vector3(transform.localScale.x, 0.07f, transform.localScale.z);
            rb.velocity = Vector3.zero; 
            state = FrogState.Dead;
            timeDied = Time.time;
            deathParticles.transform.position = transform.position;
            deathParticles.Play();
        }
    }
}

public enum FrogState {
    Idle, Jumping, Dead
}

