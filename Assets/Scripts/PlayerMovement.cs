using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour // used MC_ for main character variables to cause less confusion later on
{
    public CharacterController
        controller; // creates a input on our player in unity called controller so we can input our character controller so we can link that to this script and interact with it through here

    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject cameraPivot;
    [SerializeField] private GameObject playerMesh;

    [SerializeField] private int health;

    [SerializeField] private float defaultWalkSpeed;

    public float walkSpeed;

    [SerializeField] private float sprintSpeed;

    [SerializeField] private float gravity;

    [SerializeField] private float ySpeed; // The speed the player is falling

    public enum CameraState
    {
        Standard,
        Crouching,
        Rolling
    }

    public CameraState cameraState;


    [SerializeField] // The particle objects attached to the camera
    private GameObject powerupParticleSpeed;
    [SerializeField]
    private GameObject powerupParticleJump;


    public Transform groundCheck; // creates an input in unity we can put our epty ground check object into
    public float groundDistance = 0.4f; // will be used later to make the sphere check for 0.4 towards the ground
    public LayerMask groundMask, vaultMask, uncrouchMask; // a layer mask is in unity and is just a layer you can create


    [Header("Dodge")] [SerializeField] private float dodgeDuration;

    [SerializeField] private float dodgePower;

    [Header("Jump")] [SerializeField] private float mcJumpHeight;

    [SerializeField]
    private float jumpBufferMax = 0.25f; // The time frame that the game will store the players jump input

    [SerializeField] private float jumpDelayMax;

    [SerializeField] private float coyoteTime = 0.2f;

    [SerializeField] private float slidePower;
    [SerializeField] private float slideDuration;

    [Header("Roll")] [SerializeField] private Image damageTint;

    [SerializeField] private float maxFallHeight;

    [SerializeField] private float rollDurationMax;

    private bool isRolling;


    [Header("Wall Run")] [SerializeField] private float wallJumpForce;

    [SerializeField] private float wallJumpTimeMax;
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private float wallJumpDelay;
    [SerializeField] private float wallRunTiltMax;

    [SerializeField] private float wallRunDelayMax;
    private float wallRunDelay;

    [Header("Grapple")] [SerializeField] private float grapJumpDelay;
    [Header("Vault")] [SerializeField] private float vaultDuration;
    [SerializeField] private float maxVaultDistance;
    [SerializeField] private float vaultMaxAngle;
    [SerializeField] private AnimationCurve vaultUpCurve;
    [SerializeField] private AnimationCurve vaultDownCurve;
    private readonly float failRollDuration = 2.0f;
    private MouseLookMainCharacter cameraScript;
    private bool canWallRun;
    private bool canWallRunLeft;
    private bool canWallRunRight;
    private float coyoteTimeCounter;
    private float crouchDelay;
    private float dampingVelocity;

    private float defaultFov;
    private bool dodging;
    private Vector3 endVaultPosition;
    private float extraWallRunSpeed;
    private bool failRoll;
    private float failRollTimer;
    private RaycastHit grapData;

    private float grapDistance;
    private bool hasDied;

    [Header("Crouch")] private bool isCrouching;

    [Header("Slide")] 
    [SerializeField] private float slideDelayMax;
    private float slideDelay;
    private bool isSliding;

    private bool isSprinting;
    private float jumpBuffer;
    private float jumpDelay;
    private Vector3 lastWallRunNormal;

    private RaycastHit leftWallData;


    private bool
        mcIsGrounded; // boolean to see if its grounded, this is all to make sure our velocity isnt still gradually increasing if we are on the ground

    private MovementStates mState = MovementStates.Run;

    private bool playerOnGround;
    private ParticleSystem powerupParticleSystemSpeed;
    private ParticleSystem powerupParticleSystemJump;
    private RaycastHit rightWallData;
    private Vector3 slideDirection;
    private Vector3 slideEndPos;
    private Vector3 slideStartPos;
    private float slideTime;
    private Vector3 startVaultPosition;
    private float targetFov;
    private RaycastHit vaultData;
    private Vector3 vaultDirection;
    private float vaultHeight;

    private Vector3 vaultPosition;
    private float vaultTime;
    private float vaultWidth;


    private Vector3 velocity;
    private Vector3 wallJumpDistance;
    private float wallJumpTime;
    private Vector3 wallRunNormal;
    private float wallRunTilt;
    private float wallRunTime;


    private void Start()
    {
        powerupParticleSystemSpeed = powerupParticleSpeed.GetComponent<ParticleSystem>();
        powerupParticleSystemJump = powerupParticleJump.GetComponent<ParticleSystem>();
        defaultFov = playerCamera.fieldOfView;
        targetFov = defaultFov;

        cameraScript = playerCamera.GetComponent<MouseLookMainCharacter>();

        walkSpeed = defaultWalkSpeed;
    }

    // used a normal update for things that use a character controller as fixed update is mainly for built in physics
    // whereas normal update i created my own gravity etc so this just smoothes things out slightly
    private void Update()
    {
        // If we are at 0 health, die (reload level)
        if (health <= 0 && !hasDied)
        {
            hasDied = true;
            Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen(SceneManager.GetActiveScene().name));
        }

        if (Input.GetKey("left shift") && (!isCrouching || (isCrouching && isSliding)) && !failRoll)
        {
            // Checks if the player is holding down the sprint key
            targetFov = defaultFov + 20;
            isSprinting = true;
        }
        else
        {
            targetFov = defaultFov;
            isSprinting = false;
        }

        if (!Globals.Instance.cutsceneActive)
        {
        MovementState();
        }

        // If the player has failed a roll
        if (failRoll)
        {
            failRollTimer += Time.deltaTime;
            if (failRollTimer > failRollDuration)
            {
                failRoll = false;
                walkSpeed *= 2;
            }
        }

        // Smoothly changes the cameras FOV depending on if the value of the targetFov value
        playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, targetFov, ref dampingVelocity, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject
            .CompareTag("Powerup")) // This will trigger if the player goes inside the hitbox of a powerup object
        {
            var otherScript = other.GetComponent<Powerup>();
            otherScript.particles.Play();
            // The variant variable is stored on each powerup object and determines which type of powerup it is
            if (otherScript.variant == 1) StartCoroutine(Powerup_SpeedBoost());
            if (otherScript.variant == 2) StartCoroutine(Powerup_JumpBoost());
            // These 2 kill functions are used to stop the idle animations of the powerup object or else DOTween throws a fit
            otherScript.powerupLoop1.Kill();
            otherScript.powerupLoop2.Kill();
            otherScript.particles.transform.parent =
                null; // The particle object is removed as a child so that it isnt immediately destroyed and has time to display the particles 
            Destroy(other.gameObject);
            Destroy(otherScript.particles.gameObject, 1f);
            StartCoroutine(Powerup_Effect(otherScript.variant));
        }

        if (other.gameObject
            .CompareTag("Homework"))
        {
            var otherScript = other.GetComponent<Objectives>();
            otherScript.tweener1.Kill();
            otherScript.tweener2.Kill();
            Destroy(other.gameObject);
        }
    }

    private void MovementState()
    {
        if (mState == MovementStates.Run)
        {
            var
                x = Input.GetAxis(
                    "Horizontal"); // checks if there is a horizontal input from either a or d these are values unity is already aware of, and assings them to x (a is -1 and d is 1)
            var
                z = Input.GetAxis(
                    "Vertical"); // checks if there is a vertical input from either w or s these are values unity is already aware of, and assings them to z (w is -1 and s is 1)

            var
                move = transform.right * x +
                       transform.forward *
                       z; // creates a Vector3 which is a variable called move with 3 presets of coordinates for us already, so we transform the right by x variable and forward by z variable these are on the same line so we can move in either direction at the same time
            //transform.right and transform.forward are things unity already recognises this also makes sure that if we move forward but look right we will move where our camera is 

            if (isSprinting) move *= sprintSpeed;

            var magnitude =
                Mathf.Clamp01(move.magnitude) *
                walkSpeed; // Clamps the magnitude of the move vector so that u dont go faster diagonally, also times by movement speed

            var velocity = move * walkSpeed;

            velocity.y = ySpeed;

            // This code handles the gravity of the player
            ySpeed += gravity * Time.deltaTime;


            // If the player has walljumped
            if (wallJumpTime <= wallJumpTimeMax)
            {
                var currentJumpDistance = Vector3.Lerp(wallJumpDistance, Vector3.zero, wallJumpTime / wallJumpTimeMax);
                velocity += currentJumpDistance;


                wallJumpTime += Time.deltaTime;
            }

            controller.Move(velocity *
                            Time.deltaTime); // moves our character controller we inputted by the vector3 variable multiplied by the speed we initialised then multiplied by time delta, this ensures we move at a constant speed in correlation to our fps so there is no stuttering etc



            // Checks if the player lands on the ground
            if (!controller.isGrounded) playerOnGround = false;
            if (controller.isGrounded && !playerOnGround)
            {
                if (ySpeed <= maxFallHeight) // If the player falls from high enough to need to roll
                {
                    if (Input.GetKey(KeyCode.C))
                        StartCoroutine(Roll());
                    else
                        FailRoll();
                    health = health -= 1;
                }

                playerOnGround = true;
            }

            jumpDelay -= Time.deltaTime; // This code handles the jump buffering system
            if (Input.GetButton("Jump") && jumpDelay <= 0 && !failRoll)
                jumpBuffer = jumpBufferMax;
            else
                jumpBuffer -= Time.deltaTime;

            if (controller.isGrounded)
            {
                ySpeed = -0.5f;
                canWallRun = true;
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // If player is in air and pressed jump
            if (coyoteTimeCounter > 0 && jumpBuffer > 0f && jumpDelay <= 0)
            {
                ySpeed = mcJumpHeight;
                jumpBuffer = 0f;
                coyoteTimeCounter = 0;
                
                jumpDelay = jumpDelayMax;

                wallRunDelay = wallRunDelayMax;
            }

            if (!dodging)
            {
                // Checks if the player is going left or right by checking their horizontal axis, and also checks if they click their mouse
                if (Input.GetAxisRaw("Horizontal") == -1 && Input.GetMouseButtonDown(1))
                {
                    StartCoroutine(Dodge(-transform.right * dodgePower + transform.position));
                    StartCoroutine(cameraScript.QuickTiltScreen(dodgeDuration, 7.5f));
                }

                if (Input.GetAxisRaw("Horizontal") == 1 && Input.GetMouseButtonDown(1))
                {
                    StartCoroutine(Dodge(transform.right * dodgePower + transform.position));
                    StartCoroutine(cameraScript.QuickTiltScreen(dodgeDuration, -7.5f));
                }
            }

            // Disabled because we don't use it in any level at the moment
            // if (Input.GetKey(KeyCode.V) && CheckVault()) Vault();

            wallRunDelay -= Time.deltaTime;
            // Wallrun
            if (!OnGround())
            {
                if (Input.GetButtonDown("Jump") && CheckGrap()) Grap();
                if (CheckWallRun() && canWallRun && wallRunDelay <= 0 && Input.GetButton("Jump")) StartWallRun();
            }

            if (controller.isGrounded && !failRoll && !isRolling) //
            {
                if (Input.GetKey(KeyCode.C) && crouchDelay <= 0 && !isCrouching && slideDelay <= 0)
                {
                    if (isSprinting) StartSlide(transform.forward * slidePower + transform.position);
                    StartCrouch();
                }
//
                if (!Input.GetKey(KeyCode.C) && crouchDelay <= 0 && isCrouching)
                    if (isCrouching && !Physics.CheckCapsule(transform.position, transform.position + (Vector3.up*2),0.5f, ~uncrouchMask))
                        EndCrouch();
            }

            crouchDelay -= Time.deltaTime;
            slideDelay -= Time.deltaTime;
        }

        if (mState == MovementStates.WallRun)
        {
            WallRun();
            if (!CheckWallRun()) EndWallRun();
            if (wallRunTime >= wallJumpDelay)
                if (Input.GetButtonDown("Jump"))
                {
                    WallJump();
                    EndWallRun();
                }
        }

        if (mState == MovementStates.Slide)
        {
            
            slideTime += Time.deltaTime;
            var t = slideTime / slideDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            controller.Move(slideDirection * Time.deltaTime * Mathf.Lerp(slidePower, 1, t));

            if (slideTime >= 0.1f && !Input.GetKey(KeyCode.C))
            {
                if (!Physics.CheckCapsule(transform.position, transform.position + (Vector3.up*2),0.5f, ~uncrouchMask))
                {
                    EndCrouch();
                    EndSlide();
                }
                else
                {
                    EndSlide();
                }
            }

            if (slideTime >= slideDuration)
            {
                if (!Input.GetKey(KeyCode.C) && !Physics.CheckCapsule(transform.position, transform.position + (Vector3.up*2),0.5f, ~uncrouchMask))
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

        if (mState == MovementStates.Vault)
        {
            vaultTime += Time.deltaTime;
            
            vaultPosition = Vector3.Lerp(startVaultPosition, endVaultPosition, vaultTime / vaultDuration);

            var vaultY = 0f;

            // First half of the vault
            if (vaultTime < vaultDuration / 2)
                vaultY = Mathf.Lerp(0, vaultHeight, vaultUpCurve.Evaluate((vaultTime / vaultDuration))*2);
            // Second half
            else
                vaultY = Mathf.Lerp(0, vaultHeight, vaultDownCurve.Evaluate((vaultTime / vaultDuration))*2);

            vaultPosition.y += vaultY;

            transform.position = vaultPosition;


            if (vaultTime >= vaultDuration) mState = MovementStates.Run;
        }
    }

    private void Vault()
    {
        if (vaultData.normal.x != 0)
            vaultWidth = vaultData.transform.gameObject.GetComponent<BoxCollider>().size.x *
                         vaultData.transform.localScale.x;
        if (vaultData.normal.z != 0)
            vaultWidth = vaultData.transform.gameObject.GetComponent<BoxCollider>().size.z *
                         vaultData.transform.localScale.z;


        startVaultPosition = transform.position;
        endVaultPosition = startVaultPosition + transform.forward * (vaultWidth + vaultData.distance * 2);

        RaycastHit exitPoint;

        Physics.Raycast(endVaultPosition, -transform.forward,
            out exitPoint, vaultWidth * 2, vaultMask.value);

        endVaultPosition = exitPoint.point + transform.forward * vaultData.distance;


        mState = MovementStates.Vault;

        // This part calculates the height that the player needs to go to travel over the object
        var objectHeight = vaultData.transform.gameObject.GetComponent<BoxCollider>().size.y *
                           vaultData.transform.localScale.y;

        var vaultHeightWorld = objectHeight / 2 + vaultData.transform.position.y;

        vaultHeight = vaultHeightWorld - vaultData.point.y;


        vaultTime = 0;
        jumpBuffer = 0;
        ySpeed = -0.5f;
    }

    private bool CheckVault()
    {
        // Checks if there is a vaultable object close enough to the player
        if (Physics.Raycast(groundCheck.transform.position, groundCheck.transform.TransformDirection(Vector3.forward),
                out vaultData, 1.5f, vaultMask.value) &&
            // It then checks further forwards to make sure that the object is thin enough for the player to vault over
            !Physics.CheckSphere(groundCheck.transform.position + groundCheck.transform.forward * maxVaultDistance, 1,
                vaultMask.value) &&
            // Lastly it checks the angle that the player is look at, as well as the normal of the wall.
            // This ensures that the player cant slide across long angles causing buggy behaviour
            VaultAngleCheck()) return true;
        return false;
    }

    // private bool VaultLengthCheck()
    // {

    //     if (vaultData.normal.x != 0)
    //         vaultWidth = vaultData.transform.gameObject.GetComponent<BoxCollider>().size.x *
    //                      vaultData.transform.localScale.x;
    //     if (vaultData.normal.z != 0)
    //         vaultWidth = vaultData.transform.gameObject.GetComponent<BoxCollider>().size.z *
    //                      vaultData.transform.localScale.z;

    //     var vaultAngle = Vector3.Angle(vaultData.normal,-vaultDirection);

    //     vaultAngle = 90 - vaultAngle;

    //     var vaultLength = vaultWidth / Mathf.Sin(vaultAngle);

    //     //print("Box Width:   " + vaultWidth);
    //     //print("Line Angle:   " + vaultAngle);
    //     //print("Vault Length:   " + vaultLength);
    //     //print("");

    //     return true;
    // }

    private bool VaultAngleCheck()
    {
        vaultDirection = transform.forward;

        var vaultDirectionCheck = vaultDirection - -vaultData.normal;

        if (vaultDirectionCheck.x <= -vaultMaxAngle || vaultDirectionCheck.x >= vaultMaxAngle ||
            vaultDirectionCheck.z <= -vaultMaxAngle || vaultDirectionCheck.z >= vaultMaxAngle) return false;

        return true;
    }

    private bool CheckGrap()
    {
        // Here it performs a raycast, to check if the player is infront of an object they can grapple
        if (Physics.Raycast(groundCheck.transform.position, groundCheck.transform.TransformDirection(Vector3.forward),
                out grapData, 2, vaultMask.value))
        {
            // Here it does some math, with the output being the distance of the player from the top of the wall.
            // This is then used to determine if the player is high enough to grapple, as well as how high they need to go in order to be moved ontop of the object
            var grapLocation = grapData.point;
            var grapHeight = grapData.transform.gameObject.GetComponent<BoxCollider>().size.y *
                             grapData.transform.localScale.y;
            var grapHeightWorld = grapHeight / 2 + grapData.transform.position.y;
            var grapObject = grapData.transform.gameObject;

            grapDistance = grapHeightWorld - grapLocation.y;

            if (grapDistance <= 5 && CheckGrapHeight()) // If the player is high enough to grapple, then return true
                return true;
        }

        return false;
    }

    private bool CheckGrapHeight()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, 2, groundMask.value)) return false;
        return true;
    }

    private void Grap()
    {
        jumpBuffer = 0f;

        transform.position += transform.forward * grapData.distance;
        transform.position += new Vector3(0, grapDistance + 0.25f, 0);
        ySpeed = -0.5f;
        jumpDelay = grapJumpDelay;
    }

    private void StartCrouch()
    {
        isCrouching = true;

        crouchDelay = 0.1f;

        controller.height = 1;
        controller.center = new Vector3(0, -0.5f, 0);

        cameraState = CameraState.Crouching;

        playerCamera.transform.DOLocalMoveY(playerCamera.transform.localPosition.y - 0.9f, 0.1f)
            .SetEase(Ease.InOutSine);
    }

    private void EndCrouch()
    {
        isCrouching = false;

        crouchDelay = 0.1f;

        controller.height = 2;
        controller.center = new Vector3(0, 0, 0);

        cameraState = CameraState.Standard;

        playerCamera.transform.DOLocalMoveY(playerCamera.transform.localPosition.y + 0.9f, 0.1f)
            .SetEase(Ease.InOutSine);

    }

    private void StartSlide(Vector3 endPos)
    {
        mState = MovementStates.Slide;

        slideDirection = transform.forward;

        slideTime = 0;

        slideDelay = slideDelayMax;

        isSliding = true;
    }

    private void EndSlide()
    {
        mState = MovementStates.Run;

        isSliding = false;
    }

    private void StartWallRun()
    {
        mState = MovementStates.WallRun;
        ySpeed = 0;
        jumpBuffer = 0;
        wallRunTime = 0f;
        wallRunTilt = canWallRunRight ? wallRunTiltMax : -wallRunTiltMax;
        cameraScript.StartTiltScreen(0.25f, wallRunTilt, false);
        if (isSprinting)
            extraWallRunSpeed = 2;
        else
            extraWallRunSpeed = 1;
    }

    private void EndWallRun()
    {
        mState = MovementStates.Run;
        canWallRun = false;
        cameraScript.StartTiltScreen(0.25f, wallRunTilt, true);
    }

    private bool CheckWallRun()
    {
        canWallRunLeft = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left),
            out leftWallData, 3f, vaultMask.value);
        canWallRunRight = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right),
            out rightWallData, 3f, vaultMask.value);

        // Makes sure the player hasnt gone around a wall
        wallRunNormal = canWallRunRight ? rightWallData.normal : leftWallData.normal;
        var diff = wallRunNormal - lastWallRunNormal;

        lastWallRunNormal = wallRunNormal;
        // If the difference between the normals between this frame and the last frame are more than a quarter then stop the wallrun
        if (diff.x > 0.50f || diff.x < -0.50 ||
            diff.z > 0.50f || diff.z < -0.50)
            return false; 


        if (canWallRunRight) return canWallRunRight;
        if (canWallRunLeft)
            return canWallRunLeft;
        return false;
    }

    private void WallJump()
    {
        var normal = canWallRunRight ? rightWallData.normal : leftWallData.normal;

        wallJumpDistance = transform.up * mcJumpHeight + normal * wallJumpForce;
        wallJumpTime = 0;
        jumpDelay = 0.1f;
        jumpBuffer = 0;
    }

    private void WallRun()
    {
        var normal = canWallRunRight ? rightWallData.normal : leftWallData.normal;
        var length = canWallRunRight ? rightWallData.distance : leftWallData.distance;
        var forward = Vector3.Cross(normal, transform.up);

        if ((transform.forward - forward).magnitude > (transform.forward - -forward).magnitude)
            forward = -forward;

        if (wallRunTime <= 0.15)
            forward = Vector3.Lerp(forward, forward + -normal * (length - 1f), wallRunTime / 0.15f);

        wallRunTime += Time.deltaTime;

        controller.Move(new Vector3(forward.x, 0, forward.z) * (wallRunSpeed * extraWallRunSpeed * Time.deltaTime)); //
    }

    private IEnumerator Roll()
    {
        var startRotation = cameraPivot.transform.eulerAngles.x;
        var endRotation = startRotation + 360.0f;

        var yRotation = cameraPivot.transform.eulerAngles.y;
        var zRotation = cameraPivot.transform.eulerAngles.z;

        var duration = rollDurationMax;
        var t = 0.0f;

        // This function will ensure that the player will finish the roll looking straight ahead
        cameraScript.StartRoll(duration);

        walkSpeed /= 1.5f;
        isRolling = true;
        playerMesh.SetActive(false);
        cameraState = CameraState.Rolling;

        while (t < duration)
        {
            t += Time.deltaTime;

            var newT = t / duration;
            newT = Mathf.Sin((newT * Mathf.PI) / 2); 

            var xRotation = Mathf.Lerp(startRotation, endRotation, newT) % 360;

            cameraPivot.transform.eulerAngles = new Vector3(xRotation, transform.eulerAngles.y, transform.eulerAngles.z); 


            yield return null;
        }

        walkSpeed *= 1.5f;
        isRolling = false;
        playerMesh.SetActive(true);
        cameraState = CameraState.Standard;
    }

    private void FailRoll()
    {
        walkSpeed /= 2;
        
        damageTint.DOFade(1, 0.1f).OnComplete(() => 
            damageTint.DOFade(0, failRollDuration - 0.1f));
        playerCamera.DOShakePosition(0.35f, 1, 20, 45, randomnessMode: ShakeRandomnessMode.Harmonic);

        failRoll = true;
        failRollTimer = 0;

        jumpBuffer = 0;
    }


    private IEnumerator Dodge(Vector3 endPos)
    {
        dodging = true;
        var startPos = transform.position;
        var oldPos = transform.position;

        var dodgeTime = 0f;

        while (dodgeTime < dodgeDuration)
        {
            // This chunk of code will move the start and end positions of the dodge movement each frame to match with the movement of the player. This means their position wont freeze when they dodge
            var newPos = transform.position;
            var diff = newPos - oldPos;
            startPos += diff;
            endPos += diff;
            //

            // Smoothly moves the player towards their new dodge position
            transform.position = Vector3.Lerp(startPos, endPos,
                Mathf.SmoothStep(0.0f, 1.0f, 
                    Mathf.SmoothStep(0.0f, 1.0f, dodgeTime / dodgeDuration)));
            dodgeTime += Time.deltaTime;

            oldPos = transform.position
                ;
            yield return null;
        }

        dodging = false;
    }

    private IEnumerator Powerup_Effect(float variant) // Plays the powerup UI particles
    {
        if (variant == 1)
        {
            powerupParticleSystemSpeed.Play();
        }
        else
        {
            powerupParticleSystemJump.Play();
        }
        yield return new WaitForSeconds(7f);
                if (variant == 1)
        {
            powerupParticleSystemSpeed.Stop();
        }
        else
        {
            powerupParticleSystemJump.Stop();
        }
    }

    private IEnumerator Powerup_SpeedBoost() // Doubles the players speed for 5 seconds
    {
        walkSpeed += defaultWalkSpeed/2.5f;
        yield return new WaitForSeconds(7.5f);
        walkSpeed -= defaultWalkSpeed/2.5f;
    }

    private IEnumerator Powerup_JumpBoost() // Increases the players jump height for 5 seconds
    {
        mcJumpHeight *= 1.25f;
        yield return new WaitForSeconds(7.5f);
        mcJumpHeight /= 1.25f;
    }

    private bool OnGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, 2, groundMask.value);
    }

    private enum MovementStates
    {
        Run,
        WallRun,
        Slide,
        Dodge,
        Vault
    }
}