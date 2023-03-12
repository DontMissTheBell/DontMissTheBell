using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour // used MC_ for main character variables to cause less confusion later on
{
    public CharacterController controller; // creates a input on our player in unity called controller so we can input our character controller so we can link that to this script and interact with it through here

    [SerializeField] private Camera playerCamera;
    private MouseLookMainCharacter cameraScript;

    [SerializeField] private int MC_Health; 
    [SerializeField] private  float MC_PlayerSpeed;
    [SerializeField] private  float MC_gravity;
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



    public Transform groundCheck; // creates an input in unity we can put our epty ground check object into
    public float groundDistance = 0.4f; // will be used later to make the sphere check for 0.4 towards the ground
    public LayerMask groundMask, vaultMask; // a layer mask is in unity and is just a layer you can create

    private enum movementStates{Run, WallRun, Slide, Dodge, Vault}
    private movementStates mState = movementStates.Run;


    [Header("Dodge")]
    [SerializeField]private float dodgeDuration;
    private bool dodging;
    [SerializeField] private float dodgePower;

    [Header("Jump")]
    [SerializeField] private float MC_JumpHeight;
    private float jumpBuffer;
    [SerializeField] private float jumpBufferMax = 0.25f; // The time frame that the game will store the players jump input
    private float jumpDelay;

    [Header("Slide")]
    private bool isSliding;
    private float crouchDelay;
    [SerializeField] private float slidePower;
    [SerializeField] private float slideDuration;
    private float slideTime;
    private Vector3 slideStartPos;
    private Vector3 slideEndPos;
    private Vector3 slideDirection;

    [Header("Crouch")]
    private bool isCrouching;
    [Header("Roll")]
    [SerializeField] private Image DamageTint;
    private bool failRoll;
    private float failRollTimer;
    private float failRollDuration = 2.0f;
    [Header("Grapple")]
    private RaycastHit grapData;
    private float grapDistance;
    [Header("Vault")]
    private RaycastHit vaultData;
    private Vector3 vaultPosition;
    private Vector3 startVaultPosition;
    private Vector3 endVaultPosition;
    [SerializeField] private float vaultDuration;
    private float vaultTime;

    [Header("Wall Run")]
    [SerializeField] private float wallJumpForce;

    private RaycastHit leftWallData;
    private RaycastHit rightWallData;
    private bool canWallRunRight;
    private bool canWallRunLeft;
    private bool canWallRun;
    private Vector3 wallJumpDistance;
    private float wallJumpTime;
    [SerializeField] private float wallJumpTimeMax;
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private float wallJumpDelay;
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

        if (Input.GetKey("left shift") && (!isCrouching || (isCrouching && isSliding)) && !failRoll)
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

        // If the player has failed a roll
        if (failRoll)
        {
            failRollTimer += Time.deltaTime;
            if(failRollTimer > failRollDuration)
            {
                failRoll = false;
                MC_PlayerSpeed *= 2;
            }
        }

        // Smoothly changes the cameras FOV depending on if the value of the targetFov value
        playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, targetFov, ref dampingVelocity, 0.1f);
                        
    }

    public void ResetScene()
    {
        if (MC_Health <= 0)
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
                if (Input.GetButton("Jump") && jumpDelay <= 0 && !failRoll)
                {
                    jumpBuffer = jumpBufferMax;
                }
                else
                {
                    jumpBuffer -= Time.deltaTime;
                }

                crouchDelay -= Time.deltaTime;

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
                        StartCoroutine(cameraScript.QuickTiltScreen(dodgeDuration,7.5f));
                    }
                    if (Input.GetAxisRaw("Horizontal") == 1 && Input.GetMouseButtonDown(1))
                    {
                        StartCoroutine(Dodge(transform.right*dodgePower+transform.position));
                        StartCoroutine(cameraScript.QuickTiltScreen(dodgeDuration,-7.5f));
                    }
                }

                if (Input.GetKey(KeyCode.V) && CheckVault())
                    {
                        Vault();
                    }


                if (!OnGround())
                    {
                        if (jumpBuffer > 0 && CheckGrap())
                        {
                            Grap();
                        }
                        if (CheckWallRun() && canWallRun)
                        {
                            StartWallRun();
                        }
                    }

                if (controller.isGrounded)//
                {
                    if(Input.GetKey(KeyCode.F) && crouchDelay <= 0 && !isCrouching)
                    {
                        if (isSprinting)
                        {
                        StartSlide(transform.forward*slidePower+transform.position);
                        }
                        StartCrouch();
                    }
                    if(!Input.GetKey(KeyCode.F))
                    {
                        if(isCrouching)
                        {
                            EndCrouch();
                        }
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
            if (wallRunTime >= wallJumpDelay)
                {
                if (Input.GetButton("Jump"))
                    {
                        WallJump();
                        EndWallRun();
                    }
                }
            
        }
        if (mState == movementStates.Slide)
        {
            slideTime += Time.deltaTime;
            float t = slideTime/slideDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            controller.Move(slideDirection * Time.deltaTime * Mathf.Lerp(slidePower,1,t));

            if(slideTime >= 0.1f && !Input.GetKey(KeyCode.F))
            {
                EndCrouch();
                EndSlide();
            }

            if(slideTime >= slideDuration)
            {
                if (!Input.GetKey(KeyCode.F))
                {
                EndCrouch();
                EndSlide();
                }
                else
                {
                EndSlide();
                }
            }

        }
        if (mState == movementStates.Vault)
        {
            vaultTime += Time.deltaTime;
            vaultPosition = Vector3.Lerp(startVaultPosition, endVaultPosition,vaultTime/vaultDuration);

            float vaultY = 0.1f;

            vaultPosition.y += vaultY;

            transform.position = vaultPosition;


            if(vaultTime >= vaultDuration)
            {
            mState = movementStates.Run;
            }
        }

    }

    private void Vault()
    {
        float vaultWidth = 0;
        if(vaultData.normal.x != 0)
        {
            vaultWidth = ((vaultData.transform.gameObject.GetComponent<BoxCollider>().size.x * vaultData.transform.localScale.x));
        }
        if(vaultData.normal.z != 0)
        {
            vaultWidth = ((vaultData.transform.gameObject.GetComponent<BoxCollider>().size.z * vaultData.transform.localScale.z));
        }
        startVaultPosition = transform.position;
        endVaultPosition = startVaultPosition + ((transform.forward) * (vaultWidth+(vaultData.distance * 2)));
        mState = movementStates.Vault;

        vaultTime = 0;
    }

    private bool CheckVault()
    {
        if (Physics.Raycast(groundCheck.transform.position, groundCheck.transform.TransformDirection(Vector3.forward), out vaultData, 1.5f, vaultMask.value) && !Physics.CheckSphere(groundCheck.transform.position + groundCheck.transform.forward * 6, 1, vaultMask.value))
        {
            return true;
        }
        return false;
    }

    private bool CheckGrap()
    {
        // Here it performs a raycast, to check if the player is infront of an object they can grapple
        if (Physics.Raycast(groundCheck.transform.position, groundCheck.transform.TransformDirection(Vector3.forward), out grapData, 2, vaultMask.value))
        {
            // Here it does some math, with the output being the distance of the player from the top of the wall.
            // This is then used to determine if the player is high enough to grapple, as well as how high they need to go in order to be moved ontop of the object
            Vector3 grapLocation = grapData.point;
            float grapHeight = ((grapData.transform.gameObject.GetComponent<BoxCollider>().size.y * grapData.transform.localScale.y));
            float grapHeightWorld = (grapHeight/2) + grapData.transform.position.y;
            GameObject grapObject = grapData.transform.gameObject;

            grapDistance = grapHeightWorld - grapLocation.y;

            if (grapDistance <= 2) // If the player is high enough to grapple, then return true
            {
                return true;
            }
        }
        return false;
    }

    private void Grap()
    {
        jumpBuffer = 0f;

        transform.position += (transform.forward*grapData.distance);
        transform.position += new Vector3(0, grapDistance + 0.25f, 0);
        ySpeed = -0.5f;
        jumpDelay = 0.25f;
    }

    private void StartCrouch()
    {
        isCrouching = true;

        crouchDelay = 0.25f;

        controller.height = 1;
        controller.center = new Vector3(0,-0.5f,0); 

        playerCamera.transform.DOLocalMoveY(playerCamera.transform.localPosition.y-0.5f,0.1f).SetEase(Ease.InOutSine);
    }

    private void EndCrouch()
    {
        isCrouching = false;
        
        crouchDelay = 0.25f;

        controller.height = 2;
        controller.center = new Vector3(0,0,0); 

        playerCamera.transform.DOLocalMoveY(playerCamera.transform.localPosition.y+0.5f,0.1f).SetEase(Ease.InOutSine);
    }

    private void StartSlide(Vector3 endPos)
    {
        mState = movementStates.Slide;

        slideDirection = playerCamera.transform.forward;

        // This section ensures the direction is straight and not up or down
        float slideMag = slideDirection.magnitude;
        slideDirection.y = 0;

        slideDirection = Vector3.Normalize(slideDirection) * slideMag;

        slideTime = 0;

        isSliding = true;


    }

    private void EndSlide()
    {
        mState = movementStates.Run;

        isSliding = false;
    }

    private void StartWallRun()
    {
        mState = movementStates.WallRun;
        ySpeed = 0;
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
        canWallRun = false;
        cameraScript.StartTiltScreen(0.25f, wallRunTilt, true);
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

    public void OnControllerColliderHit(ControllerColliderHit MC_FallDamage)
    {

        if (ySpeed <= -20f) // If the player falls from high enough to need to roll
        {
            if(Input.GetKey(KeyCode.F))
            {
                StartCoroutine(Roll());
            }
            else
            {
                FailRoll();
            }
        }
        if (ySpeed <= -40f)
        {
            MC_Health = MC_Health -= 1;
        }
//        else
//        {
//            MC_Health = MC_Health;         // This code does nothing lol
//        }
        return;        
    }

    IEnumerator Roll()
    {

        float startRotation = transform.eulerAngles.x;
        float endRotation = startRotation + 360.0f;

        float yRotation = transform.eulerAngles.y;
        float zRotation = transform.eulerAngles.z;
        
        float duration = 0.5f;
        float t = 0.0f;

        cameraScript.StartRoll(duration);

        while (t < duration)
        {
            t += Time.deltaTime;

            float xRotation = Mathf.Lerp(startRotation, endRotation, t/ duration) % 360;

            transform.eulerAngles = new Vector3(xRotation, yRotation, zRotation);


            yield return null;


        }



    }

    private void FailRoll()
    {
        MC_PlayerSpeed /= 2;
        DamageTint.DOFade(1,0.1f).OnComplete(()=> DamageTint.DOFade(0,failRollDuration-0.1f));

        playerCamera.DOShakePosition(0.35f,1,20,45,true,randomnessMode:ShakeRandomnessMode.Harmonic);

        failRoll = true;
        failRollTimer = 0;

        jumpBuffer = 0;
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