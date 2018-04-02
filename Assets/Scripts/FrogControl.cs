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

    //jump
    Vector3 jumpDir = Vector3.forward;
    Vector3 lookDir = Vector3.forward;
    const float jumpTime = 0.18f;
    const float jumpHeight = 0.5f;
    float timeTriggeredJump;
    Vector3 jumpOrigin;
    Log logTarget;

    //model animation
    float modelY;
    float timeTriggeredBob = -10;
    const float bobHeight = 0.2f;
    const float bobTime = 0.2f;

    //death
    float timeDied = 0;
    const float timeDeadBeforeReset = 1.0f;

    Rigidbody rb;
    Transform model;
    FrogState state = FrogState.Idle;
    List<Action> moveCommands = new List<Action>();
    ParticleSystem splashParticles, deathParticles;

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

        //record initial position
        Vector3 tgtMovePos = transform.position;
        
        //update states
        if (state == FrogState.Idle) { //just wait for commands
            CheckForQueuedCommands(); 
        } 
        if (state == FrogState.Jumping) {
            //calc position through jump as time through animation
            float timeThroughJump = Time.time - timeTriggeredJump;
            float jumpTimeMod = jumpTime / ((moveCommands.Count / 2f) + 1);
            float percThroughJump = timeThroughJump / jumpTimeMod;
            float adjustedTimePos = jumpXZAnim.Evaluate(percThroughJump);
            Vector3 tgtPos = jumpOrigin + jumpDir;

            //if we are jumping onto a log, target its center point
            if (logTarget != null) {
                tgtPos = new Vector3(logTarget.transform.position.x, transform.position.y, logTarget.transform.position.z); //maintain our Y pos so we dont jump inside the log
                tgtMovePos = Vector3.Lerp(jumpOrigin, tgtPos, adjustedTimePos);
            }
            //otherwise move towards the int point
            else {
                tgtPos = new Vector3(Mathf.RoundToInt(tgtPos.x), tgtPos.y, Mathf.RoundToInt(tgtPos.z));
                tgtMovePos = Vector3.Lerp(jumpOrigin, tgtPos, adjustedTimePos);
            }
            jumpY = jumpYAnim.Evaluate(percThroughJump) * jumpHeight;

            //detect jump end
            if (percThroughJump >= 1) JumpEnded();
        }

        //apply any movement
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
        //execute queued commands, remove from queue
        if (moveCommands.Count > 0) {
            moveCommands[0]();
            moveCommands.RemoveAt(0);
        }
    }

    void HandleInput() {
        //look + jump on arrow key press
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
            Vector3 cmdDir = Vector3.right;
            lookDir = cmdDir;
            moveCommands.Add(() => { Jump(cmdDir); });
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            Vector3 cmdDir = Vector3.left;
            lookDir = cmdDir;
            moveCommands.Add(() => { Jump(cmdDir); });
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            Vector3 cmdDir = Vector3.forward;
            lookDir = cmdDir;
            moveCommands.Add(() => { Jump(cmdDir); });
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            Vector3 cmdDir = Vector3.back;
            lookDir = cmdDir;
            moveCommands.Add(() => { Jump(cmdDir); });
        }
    }

    void Jump(Vector3 dir) {
        jumpDir = dir;
        lookDir = dir;

        //test for jumping onto a log 
        logTarget = null;
        Vector3 testInDir = transform.position + new Vector3(0, -0.5f /*log test height*/, 0) + jumpDir;
        Collider[] hitColliders = Physics.OverlapSphere(testInDir, 0.5f);
        List<Transform> logsInRange = new List<Transform>();
        for (int i = 0; i < hitColliders.Length; i++) {
            if (hitColliders[i].tag == "Moving")
                logsInRange.Add(hitColliders[i].transform);
        }
        //if we have logs in jump range...
        if (logsInRange.Count > 0) {
            //find the closest one
            float closestDist = float.MaxValue;
            Transform closest = logsInRange[0];
            foreach (Transform t in logsInRange) {
                float dist = Vector2.Distance(transform.position, t.position);
                //if log is active and closer, consider it
                if (t.GetComponent<Log>().Active && !closest.GetComponent<Log>().Active || dist < closestDist) {
                    closest = t;
                    closestDist = dist;
                }
            }
            logTarget = closest.GetComponent<Log>();
        }

        //check for attempting to jump into an obstruction (tree, wall)
        //TODO: check for vehicle + die 
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), dir, 1f)) {
            jumpDir = Vector3.zero; //do nothing
            //TODO animate hit-a-wall
            return;
        }

        state = FrogState.Jumping;
        timeTriggeredJump = Time.time;
        jumpOrigin = transform.position;
    }

    void JumpEnded() {
        state = FrogState.Idle;

        //check for jumped into water, kill the frog >:D
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f)) {
            if (hit.transform.tag == "Moving") {
                if (!hit.transform.GetComponent<Log>().Active) {
                    Destroy(hit.transform.gameObject);
                    rb.velocity = Vector3.zero; //stop any xz velocity
                    rb.AddForce(Vector3.down * 10, ForceMode.Impulse); //fall into water
                    state = FrogState.Dead;
                    timeDied = Time.time;
                    splashParticles.transform.position = transform.position;
                    splashParticles.Play();
                    return; //die
                }
            }
        }

        //check for jumped-onto-log waterbob
        if (logTarget != null) {
            timeTriggeredBob = Time.time;
            logTarget.TriggerLogWaterBob();
        }

        //see if there is a next command to execute
        CheckForQueuedCommands();
    }

    private void OnTriggerEnter(Collider other) {
        //vehicle death
        if (other.tag == "Vehicle") {
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

