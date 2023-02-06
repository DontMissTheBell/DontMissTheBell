using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour // used MC_ for main character variables to cause less confusion later on
{
    public CharacterController controller; // creates a input on our player in unity called controller so we can input our character controller so we can link that to this script and interact with it through here

    [SerializeField] private Camera playerCamera;
    private MouseLookMainCharacter cameraScript;

    [SerializeField] private  float MC_PlayerSpeed;
    [SerializeField] private  float MC_gravity;
    [SerializeField] private float MC_JumpHeight;
    [SerializeField] private float MC_SprintSpeed;
    [SerializeField] private float ySpeed; // The speed the player is falling
    public Vector3 playerVelocity;


    [SerializeField] // The particle objects attached to the camera
    private GameObject powerupParticle; 
    private ParticleSystem powerupParticleSystem;

    private float defaultFov;
    private  float dampingVelocity = 0f;
    private float targetFov;

    private bool hasVaulted = false;

    private float jumpBuffer;
    [SerializeField] private float jumpBufferMax = 0.25f; // The time frame that the game will store the players jump input
    private float jumpDelay;

    [SerializeField]private float dodgeDuration;
    private bool dodging;
    [SerializeField] private float dodgePower;


    public Transform groundCheck; // creates an input in unity we can put our epty ground check object into
    public float groundDistance = 0.4f; // will be used later to make the sphere check for 0.4 towards the ground
    public LayerMask groundMask, vaultMask; // a layer mask is in unity and is just a layer you can create
    

    bool MC_isGrounded; // boolean to see if its grounded, this is all to make sure our velocity isnt still gradually increasing if we are on the ground

    Vector3 velocity;

    void Start()
    {
        powerupParticleSystem = powerupParticle.GetComponent<ParticleSystem>();
        defaultFov = playerCamera.fieldOfView;

        cameraScript = playerCamera.GetComponent<MouseLookMainCharacter>();
    }



    void Update() // used a normal update for things that use a character controller as fixed update is mainly for built in physics whereas normal update i created my own gravity etc so this just smoothes things out slightly
    {


        float x = Input.GetAxis("Horizontal"); // checks if there is a horizontal input from either a or d these are values unity is already aware of, and assings them to x (a is -1 and d is 1)
        float z = Input.GetAxis("Vertical"); // checks if there is a vertical input from either w or s these are values unity is already aware of, and assings them to z (w is -1 and s is 1)

        Vector3 move = transform.right * x + transform.forward * z; // creates a Vector3 which is a variable called move with 3 presets of coordinates for us already, so we transform the right by x variable and forward by z variable these are on the same line so we can move in either direction at the same time
        //transform.right and transform.forward are things unity already recognises this also makes sure that if we move forward but look right we will move where our camera is 


        if (Input.GetKey("left shift")){ // If shift is held, then it increases the vector used to determine the distance the player travels the next frame
            move *= MC_SprintSpeed;
            targetFov = defaultFov + 20;
        }
        else
        {
            targetFov = defaultFov;
        }
        float magnitude = Mathf.Clamp01(move.magnitude) * MC_PlayerSpeed; // Clamps the magnitude of the move vector so that u dont go faster diagonally, also times by movement speed


        jumpDelay -= Time.deltaTime; // This code handles the jump buffering system
        if (Input.GetButton("Jump") && jumpDelay <= 0)
        {
            jumpBuffer = jumpBufferMax;
        }
        else
        {
            jumpBuffer -= Time.deltaTime;
        }

        // This code handles the gravity of the player
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
        
        // Smoothly changes the cameras FOV depending on if the value of the targetFov value
        playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, targetFov, ref dampingVelocity, 0.1f);

        // This code handles the vaulting mechanic. First it checks if the player jumps while in the air
        if (!controller.isGrounded && jumpBuffer > 0 && !hasVaulted)
        {
        jumpBuffer = 0f;
        // Next it performs a raycast, to check if the player is infront of an object they can vault
        RaycastHit hitData;
        if (Physics.Raycast(groundCheck.transform.position, groundCheck.transform.TransformDirection(Vector3.forward), out hitData, 2, vaultMask.value))
            {
            // Then it does some math, with the output being the distance of the player from the top of the wall.
            // This is then used to determine if the player is high enough to vault, as well as how high they need to go in order to be moved ontop of the object
            Vector3 vaultLocation = hitData.point;
            float vaultHeight = ((hitData.transform.gameObject.GetComponent<BoxCollider>().size.y * hitData.transform.localScale.y));
            float vaultHeightWorld = (vaultHeight/2) + hitData.transform.position.y;
            GameObject vaultObject = hitData.transform.gameObject;
            float vaultDistance = vaultHeightWorld - vaultLocation.y;
            if (vaultDistance <= 2) // If the player is high enough to vault, then they are moved the correct distance to the top of the object
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
            // Checks if the player is going left or right by checking their horizontal axis, and also checks if they click their mouse
            if (Input.GetAxisRaw("Horizontal") == -1 && Input.GetMouseButtonDown(1))
            {
                StartCoroutine(Dodge(-transform.right*dodgePower+transform.position));
                cameraScript.TiltScreen(dodgeDuration,10f);
            }
            if (Input.GetAxisRaw("Horizontal") == 1 && Input.GetMouseButtonDown(1))
            {
                StartCoroutine(Dodge(transform.right*dodgePower+transform.position));
                cameraScript.TiltScreen(dodgeDuration,-10f);
            }
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
            // This chunk of code will move the start and end positions of the dodge movement each frame to match with the movement of the player. This means their position wont freeze when they dodge
            newPos = transform.position;
            Vector3 diff = newPos - oldPos;
            startPos += diff;
            endPos += diff;
            //

            // Smoothly moves the player towards their new dodge position
            transform.position = Vector3.Lerp(startPos, endPos,Mathf.SmoothStep(0.0f,1.0f,Mathf.SmoothStep(0.0f,1.0f,dodgeTime/dodgeDuration)));
            dodgeTime += Time.deltaTime;

            oldPos = transform.position
            ;
            yield return null;
        }
        dodging = false;
    }

    IEnumerator Powerup_Effect() // Plays the powerup UI particles
    {
        powerupParticleSystem.Play();
        yield return new WaitForSeconds(4.5f);
        powerupParticleSystem.Stop();
    }
    IEnumerator Powerup_SpeedBoost() // Doubles the players speed for 5 seconds
    {
        MC_PlayerSpeed *= 2;
        yield return new WaitForSeconds(5);
        MC_PlayerSpeed /= 2;
    }

    IEnumerator Powerup_JumpBoost() // Increases the players jump height for 5 seconds
    {
        MC_JumpHeight *= 1.5f;
        yield return new WaitForSeconds(5);
        MC_JumpHeight /= 1.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Powerup") // This will trigger if the player goes inside the hitbox of a powerup object
        {
            var otherScript = other.GetComponent<Powerup>();
            otherScript.particles.Play();
            // The variant variable is stored on each powerup object and determines which type of powerup it is
            if (otherScript.variant == 1)
                {
                    StartCoroutine(Powerup_SpeedBoost());
                }
            if (otherScript.variant == 2)
                {
                    StartCoroutine(Powerup_JumpBoost());
                }
            // These 2 kill functions are used to stop the idle animations of the powerup object or else DOTween throws a fit
            otherScript.powerupLoop1.Kill();
            otherScript.powerupLoop2.Kill();
            otherScript.particles.transform.parent = null; // The particle object is removed as a child so that it isnt immediately destroyed and has time to display the particles 
            Destroy(other.gameObject);
            Destroy(otherScript.particles.gameObject, 1f);
            StartCoroutine(Powerup_Effect());
        }

    }
        

}