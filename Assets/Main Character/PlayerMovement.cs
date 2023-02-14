using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour // used MC_ for main character variables to cause less confusion later on
{
    public CharacterController controller; // creates a input on our player in unity called controller so we can input our character controller so we can link that to this script and interact with it through here

    [SerializeField] private Camera playerCamera;
    private MouseLookMainCharacter cameraScript;

    [SerializeField] private int MC_Health; 
    [SerializeField] private  float MC_PlayerSpeed;
    [SerializeField] private  float MC_gravity;
    [SerializeField] private float MC_JumpHeight;
    [SerializeField] private float MC_SprintSpeed;
    [SerializeField] private float ySpeed; // The speed the player is falling
    public Vector3 playerVelocity;

    private bool isSprinting;


    [SerializeField] // The particle objects attached to the camera
    private GameObject powerupParticle; 
    private ParticleSystem powerupParticleSystem;

    private float defaultFov;
    private  float dampingVelocity = 0f;
    private float targetFov;

    private float jumpBuffer;
    [SerializeField] private float jumpBufferMax = 0.25f; // The time frame that the game will store the players jump input
    private float jumpDelay;

    [SerializeField]private float dodgeDuration;
    private bool dodging;
    [SerializeField] private float dodgePower;


    public Transform groundCheck; // creates an input in unity we can put our epty ground check object into
    public float groundDistance = 0.4f; // will be used later to make the sphere check for 0.4 towards the ground
    public LayerMask groundMask, vaultMask; // a layer mask is in unity and is just a layer you can create

    private enum movementStates{Run, WallRun, Slide, Dodge}
    private movementStates mState = movementStates.Run;

    private RaycastHit vaultData;

    [Header("Wall Run")]
    [SerializeField] private float wallJumpForce;

    private RaycastHit leftWallData;
    private RaycastHit rightWallData;
    private bool canWallRunRight;
    private bool canWallRunLeft;
    private bool canWallJump;
    private bool canWallRun;
    private Vector3 wallJumpDistance;
    private float wallJumpTime;
    [SerializeField] private float wallJumpTimeMax;
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private float wallJumpBufferMax = 0.5f;
    private float wallRunTime;
    private float extraWallRunSpeed;
    private float wallRunTilt;
    [SerializeField] private float wallRunTiltMax;


    

    bool MC_isGrounded; // boolean to see if its grounded, this is all to make sure our velocity isnt still gradually increasing if we are on the ground

    Vector3 velocity;

    void Start()
    {
        powerupParticleSystem = powerupParticle.GetComponent<ParticleSystem>();
        defaultFov = playerCamera.fieldOfView;
        targetFov = defaultFov;

        cameraScript = playerCamera.GetComponent<MouseLookMainCharacter>();
    }



    void Update() // used a normal update for things that use a character controller as fixed update is mainly for built in physics whereas normal update i created my own gravity etc so this just smoothes things out slightly
    {
        ResetScene();

        if (Input.GetKey("left shift"))
            { // Checks if the player is holding down the sprint key
                targetFov = defaultFov + 20;
                isSprinting = true;
            }
            else
            {
                targetFov = defaultFov;
                isSprinting = false;
            }

        MovementState();

        // Smoothly changes the cameras FOV depending on if the value of the targetFov value
        playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, targetFov, ref dampingVelocity, 0.1f);
                        
    }

    public void ResetScene()
    {
        if (MC_Health == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            //Debug.Log("Health is: " + MC_Health);
        }
        
    }

    private void MovementState()
    {
        if (mState == movementStates.Run)
        {
                float x = Input.GetAxis("Horizontal"); // checks if there is a horizontal input from either a or d these are values unity is already aware of, and assings them to x (a is -1 and d is 1)
                float z = Input.GetAxis("Vertical"); // checks if there is a vertical input from either w or s these are values unity is already aware of, and assings them to z (w is -1 and s is 1)

                Vector3 move = transform.right * x + transform.forward * z; // creates a Vector3 which is a variable called move with 3 presets of coordinates for us already, so we transform the right by x variable and forward by z variable these are on the same line so we can move in either direction at the same time
                //transform.right and transform.forward are things unity already recognises this also makes sure that if we move forward but look right we will move where our camera is 

                if (isSprinting)
                {
                    move *= MC_SprintSpeed;
                }

                float magnitude = Mathf.Clamp01(move.magnitude) * MC_PlayerSpeed; // Clamps the magnitude of the move vector so that u dont go faster diagonally, also times by movement speed

                Vector3 velocity = move * MC_PlayerSpeed;

                velocity.y = ySpeed;

                // This code handles the gravity of the player
                ySpeed += MC_gravity * Time.deltaTime;


                // If the player has walljumped
                if (wallJumpTime <= wallJumpTimeMax)
                {
                    Vector3 currentJumpDistance = Vector3.Lerp(wallJumpDistance, Vector3.zero, wallJumpTime/wallJumpTimeMax);
                    velocity += currentJumpDistance;


                    wallJumpTime += Time.deltaTime;


                }
                
                controller.Move(velocity * Time.deltaTime); // moves our character controller we inputted by the vector3 variable multiplied by the speed we initialised then multiplied by time delta, this ensures we move at a constant speed in correlation to our fps so there is no stuttering etc

                jumpDelay -= Time.deltaTime; // This code handles the jump buffering system
                if (Input.GetButton("Jump") && jumpDelay <= 0)
                {
                    jumpBuffer = jumpBufferMax;
                }
                else
                {
                    jumpBuffer -= Time.deltaTime;
                }
                if (controller.isGrounded)
                {
                    ySpeed = -0.5f;
                    canWallRun = true;
                    if (jumpBuffer > 0f)
                    {
                        ySpeed = MC_JumpHeight;
                        jumpBuffer = 0f;
                    }
                }

                if (!dodging)
                {
                    // Checks if the player is going left or right by checking their horizontal axis, and also checks if they click their mouse
                    if (Input.GetAxisRaw("Horizontal") == -1 && Input.GetMouseButtonDown(1))
                    {
                        StartCoroutine(Dodge(-transform.right*dodgePower+transform.position));
                        StartCoroutine(cameraScript.QuickTiltScreen(dodgeDuration,10f));
                    }
                    if (Input.GetAxisRaw("Horizontal") == 1 && Input.GetMouseButtonDown(1))
                    {
                        StartCoroutine(Dodge(transform.right*dodgePower+transform.position));
                        StartCoroutine(cameraScript.QuickTiltScreen(dodgeDuration,-10f));
                    }
                }


                if (!OnGround())
                    {
                        if (CheckVault() && jumpBuffer > 0)
                        {
                            Vault();
                        }
                        if (CheckWallRun() && canWallRun && jumpBuffer > 0)
                        {
                            StartWallRun();
                        }
                    }
        }
        if (mState == movementStates.WallRun)
        {
            WallRun();
            if (!CheckWallRun())
                {
                    EndWallRun();
                }
            if (!canWallJump)
                {
                if (Input.GetButton("Jump"))
                {
                    jumpBuffer = wallJumpBufferMax;
                }
                else
                {
                    jumpBuffer -= Time.deltaTime;
                    if ((wallJumpBufferMax - jumpBuffer) > 0.1)
                    {
                        canWallJump = true;
                    }
                }
            }
            else
            {
                jumpBuffer -= Time.deltaTime;
                if (Input.GetButton("Jump"))
                {
                    WallJump();
                    EndWallRun();
                }
                if (jumpBuffer <= 0)
                {
                    EndWallRun();
                }
            }
        }

    }

    private void StartWallRun()
    {
        mState = movementStates.WallRun;
        ySpeed = 0;
        canWallJump = false;
        wallRunTime = 0f;
        wallRunTilt = canWallRunRight ? wallRunTiltMax : -wallRunTiltMax;
        cameraScript.StartTiltScreen(0.25f, wallRunTilt, false);
        if (isSprinting)
        {
            extraWallRunSpeed = 2;
        }
        else
        {
            extraWallRunSpeed = 1;
        }

    }

    private void EndWallRun()
    {
        mState = movementStates.Run;
        canWallJump = false;
        canWallRun = false;
        cameraScript.StartTiltScreen(0.25f, wallRunTilt, true);
    }

    private bool CheckVault()
    {
        // Here it performs a raycast, to check if the player is infront of an object they can vault
        return Physics.Raycast(groundCheck.transform.position, groundCheck.transform.TransformDirection(Vector3.forward), out vaultData, 2, vaultMask.value);
    }
    private bool CheckWallRun()
    {
        canWallRunLeft = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out leftWallData, 2f, vaultMask.value);
        canWallRunRight = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out rightWallData, 2f, vaultMask.value);
        if (canWallRunRight)
        {
            return canWallRunRight;
        }
        if (canWallRunLeft)
        {
            return canWallRunLeft;
        }
        else
        {
            return false;
        }
    }

    private void WallJump()
    {
        Vector3 normal = canWallRunRight ? rightWallData.normal : leftWallData.normal;

        wallJumpDistance = transform.up * MC_JumpHeight + normal * wallJumpForce;
        wallJumpTime = 0;
        jumpDelay = 0.25f;
        jumpBuffer = 0;


    }

    private void WallRun()
    {
        Vector3 normal = canWallRunRight ? rightWallData.normal : leftWallData.normal;
        float length = canWallRunRight ? rightWallData.distance : leftWallData.distance;
        Vector3 forward = Vector3.Cross(normal, transform.up);

        if((transform.forward - forward).magnitude > (transform.forward - -forward).magnitude)
            forward = -forward;

        if (wallRunTime <= 0.15){
        forward = Vector3.Lerp(forward,forward + (-normal*(length-1f)),wallRunTime/0.15f);
        }

        wallRunTime += Time.deltaTime;

        controller.Move(new Vector3(forward.x,0,forward.z)*(wallRunSpeed*extraWallRunSpeed*Time.deltaTime));//
    }

    private void Vault()
    {
        jumpBuffer = 0f;
        // Here it does some math, with the output being the distance of the player from the top of the wall.
        // This is then used to determine if the player is high enough to vault, as well as how high they need to go in order to be moved ontop of the object
        Vector3 vaultLocation = vaultData.point;
        float vaultHeight = ((vaultData.transform.gameObject.GetComponent<BoxCollider>().size.y * vaultData.transform.localScale.y));
        float vaultHeightWorld = (vaultHeight/2) + vaultData.transform.position.y;
        GameObject vaultObject = vaultData.transform.gameObject;
        float vaultDistance = vaultHeightWorld - vaultLocation.y;
        if (vaultDistance <= 2) // If the player is high enough to vault, then they are moved the correct distance to the top of the object
            {
                transform.position += (transform.forward*vaultData.distance);
                transform.position += new Vector3(0, vaultDistance + 0.25f, 0);
                ySpeed = -0.5f;
                jumpDelay = 0.25f;
            }
            
    }

    public void OnControllerColliderHit(ControllerColliderHit MC_FallDamage)
    {
        if (ySpeed <= -40f)
        {
            MC_Health = MC_Health -= 1;
        }
        else
        {
            MC_Health = MC_Health;         
        }
        return;        
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

    private bool OnGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, 2, groundMask.value);
    }
        

}