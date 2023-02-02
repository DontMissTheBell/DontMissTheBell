using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour // used MC_ for main character variables to cause less confusion later on
{
    public CharacterController controller; // creates a input on our player in unity called controller so we can input our character controller so we can link that to this script and interact with it through here

    [SerializeField] private Camera playerCamera;

    [SerializeField] private  float MC_PlayerSpeed;
    [SerializeField] private  float MC_gravity;
    [SerializeField] private float MC_JumpHeight;
    [SerializeField] private float MC_SprintSpeed;
    [SerializeField] private float ySpeed;
    public Vector3 playerVelocity;

    [SerializeField]
    private GameObject powerupParticle;
    private ParticleSystem powerupParticleSystem;

    private float defaultFov;
    private  float dampingVelocity = 0f;
    private float targetFov;

    private bool hasVaulted = false;

    private float jumpBuffer;
    private float jumpBufferMax = 0.25f;
    private float jumpDelay;

    [SerializeField]private float dodgeDuration;
    private bool dodging;
    [SerializeField] private float dodgePower;
    


    public Transform groundCheck; // creates an input in unity we can put our epty ground check object into
    public float groundDistance = 0.4f; // will be used later to make the sphere check for 0.4 towards the ground
    public LayerMask groundMask; // a layer mask is in unity and is just a layer you can create
    

    bool MC_isGrounded; // boolean to see if its grounded, this is all to make sure our velocity isnt still gradually increasing if we are on the ground

    Vector3 velocity;

    void Start()
    {
        powerupParticleSystem = powerupParticle.GetComponent<ParticleSystem>();
        defaultFov = playerCamera.fieldOfView;
    }



    void Update() // used a normal update for things that use a character controller as fixed update is mainly for built in physics whereas normal update i created my own gravity etc so this just smoothes things out slightly
    {


        float x = Input.GetAxis("Horizontal"); // checks if there is a horizontal input from either a or d these are values unity is already aware of, and assings them to x (a is -1 and d is 1)
        float z = Input.GetAxis("Vertical"); // checks if there is a vertical input from either w or s these are values unity is already aware of, and assings them to z (w is -1 and s is 1)

        Vector3 move = transform.right * x + transform.forward * z; // creates a Vector3 which is a variable called move with 3 presets of coordinates for us already, so we transform the right by x variable and forward by z variable these are on the same line so we can move in either direction at the same time
        //transform.right and transform.forward are things unity already recognises this also makes sure that if we move forward but look right we will move where our camera is 
        if (Input.GetKey("left shift")){
            move *= MC_SprintSpeed;
            Vector3 fovVelocity = Vector3.zero;
            targetFov = defaultFov + 20;
        }
        else
        {
            targetFov = defaultFov;
        }
        float magnitude = Mathf.Clamp01(move.magnitude) * MC_PlayerSpeed; // Clamps the magnitude of the move vector so that u dont go faster diagonally, also times by movement speed

        jumpDelay -= Time.deltaTime;

        if (Input.GetButton("Jump") && jumpDelay <= 0)
        {
            jumpBuffer = jumpBufferMax;
        }
        else
        {
            jumpBuffer -= Time.deltaTime;
        }

        ySpeed += MC_gravity * Time.deltaTime;
        if (controller.isGrounded){
            hasVaulted = false;
            ySpeed = -0.5f;
            if (jumpBuffer > 0f)
            {
                ySpeed = MC_JumpHeight;
                jumpBuffer = 0f;
            }
        }

        Vector3 velocity = move * MC_PlayerSpeed;
        velocity.y = ySpeed;
        controller.Move(velocity * Time.deltaTime); // moves our character controller we inputted by the vector3 variable multiplied by the speed we initialised then multiplied by time delta, this ensures we move at a constant speed in correlation to our fps so there is no stuttering etc
        
        // Changes the cameras FOV depending on if the value of the targetFov value
        playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, targetFov, ref dampingVelocity, 0.1f);

        if (!controller.isGrounded && jumpBuffer > 0 && !hasVaulted)
        {
        jumpBuffer = 0f;
        RaycastHit hitData;
        if (Physics.Raycast(groundCheck.transform.position, groundCheck.transform.TransformDirection(Vector3.forward), out hitData, 2, ~7))
            {
            Vector3 vaultLocation = hitData.point;
            float vaultHeight = ((hitData.transform.gameObject.GetComponent<BoxCollider>().size.y * hitData.transform.localScale.y));
            float vaultHeightWorld = (vaultHeight/2) + hitData.transform.position.y;
            GameObject vaultObject = hitData.transform.gameObject;
            float vaultDistance = vaultHeightWorld - vaultLocation.y;
            if (vaultDistance <= 2)
                {
                    hasVaulted = true;
                    transform.position += (transform.forward*hitData.distance);
                    transform.position += new Vector3(0, vaultDistance + 0.25f, 0);
                    ySpeed = -0.5f;
                    jumpDelay = 0.25f;
                }
            }
        }

        if (!dodging)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                StartCoroutine(Dodge(-transform.right*dodgePower+transform.position));
//                playerCamera.transform.DORotate(new Vector3(0,0,10), dodgeDuration/2, RotateMode.LocalAxisAdd).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
                StartCoroutine(CameraTilt(dodgeDuration, Quaternion.Euler(0,0,19f)));
            }
            if (Input.GetKey(KeyCode.E))
            {
                StartCoroutine(Dodge(transform.right*dodgePower+transform.position));
                StartCoroutine(CameraTilt(dodgeDuration, Quaternion.Euler(0,0,-10f)));
            }
        }

            

    }

    IEnumerator CameraTilt(float duration, Quaternion tilt)
    {
        float tiltTime = 0f;

        Quaternion startTilt = playerCamera.transform.rotation;
        Quaternion endTilt = startTilt * tilt;

        Quaternion newRot = playerCamera.transform.rotation;
        Quaternion oldRot = playerCamera.transform.rotation;

        while (tiltTime < duration)
            if (tiltTime <= duration/2)
                {
                    playerCamera.transform.rotation = Quaternion.Slerp(startTilt, endTilt,Mathf.SmoothStep(0.0f,1.0f,tiltTime/duration));
                    tiltTime += Time.deltaTime;
                    yield return null;
                }
            else
                {
                    playerCamera.transform.rotation = Quaternion.Slerp(endTilt, startTilt,Mathf.SmoothStep(0.0f,1.0f,tiltTime/duration));
                    tiltTime += Time.deltaTime;
                    yield return null;
                }
    }

    IEnumerator Dodge(Vector3 endPos)
    {
        dodging = true;
        Vector3 startPos = transform.position;
        Vector3 newPos = transform.position;
        Vector3 oldPos = transform.position;

        float dodgeTime = 0f;

        while (dodgeTime < dodgeDuration)
        {
            newPos = transform.position;
            Vector3 diff = newPos - oldPos;
            startPos += diff;
            endPos += diff;
            // This chunk of code will move the start and end positions of the dodge movement each frame to match with the movement of the player. This means their position wont freeze when they dodge

            transform.position = Vector3.Lerp(startPos, endPos,Mathf.SmoothStep(0.0f,1.0f,Mathf.SmoothStep(0.0f,1.0f,dodgeTime/dodgeDuration)));
            dodgeTime += Time.deltaTime;

            oldPos = transform.position
            ;
            yield return null;
        }
        dodging = false;
    }

    IEnumerator Powerup_Effect()
    {
        powerupParticleSystem.Play();
        yield return new WaitForSeconds(4.5f);
        powerupParticleSystem.Stop();
    }
    IEnumerator Powerup_SpeedBoost()
    {
        MC_PlayerSpeed *= 2;
        yield return new WaitForSeconds(5);
        MC_PlayerSpeed /= 2;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Powerup")
        {
            var otherScript = other.GetComponent<Powerup>();
            otherScript.particles.Play();
            if (otherScript.variant == 1)
                {
                    StartCoroutine(Powerup_SpeedBoost());
                }
            otherScript.powerupLoop1.Kill();
            otherScript.powerupLoop2.Kill();
            otherScript.particles.transform.parent = null;
            Destroy(other.gameObject);
            Destroy(otherScript.particles.gameObject, 1f);
            StartCoroutine(Powerup_Effect());
        }

    }
        

}