using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour // used MC_ for main character variables to cause less confusion later on
{
    public CharacterController controller; // creates a input on our player in unity called controller so we can input our character controller so we can link that to this script and interact with it through here

    public float MC_PlayerSpeed = 20f;
    public float MC_gravity = -12f;
    public float MC_JumpHeight = 150000f;
    public Vector3 playerVelocity;

    [SerializeField]
    private GameObject powerupParticle;
    private ParticleSystem powerupParticleSystem;
    


    public Transform groundCheck; // creates an input in unity we can put our epty ground check object into
    public float groundDistance = 0.4f; // will be used later to make the sphere check for 0.4 towards the ground
    public LayerMask groundMask; // a layer mask is in unity and is just a layer you can create
    public Rigidbody MC_rbody;

    bool MC_isGrounded; // boolean to see if its grounded, this is all to make sure our velocity isnt still gradually increasing if we are on the ground

    Vector3 velocity;

    void Start()
    {
        powerupParticleSystem = powerupParticle.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        MC_isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // this is the boolean that we check the ground for, Physics.CheckSphere is in unity already and will create a sphere for us at the ground check position with ground check distance and check for the ground mask layer we want

        if (MC_isGrounded && playerVelocity.y < 0) // checks if we are grounded and if our velocity is at less than 0 then make our velocity to -2, -2 is just a nice sweet spot for grounded velocity
        {
            playerVelocity.y = 20f;
            Mathf.Sqrt(MC_JumpHeight * -3.0f * MC_gravity);
        }

        if (Input.GetButtonDown("Jump") && MC_isGrounded == true)
        {
            playerVelocity.y += Mathf.Sqrt(MC_JumpHeight * -3.0f * MC_gravity);
        }

        playerVelocity.y += MC_gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);


        float x = Input.GetAxis("Horizontal"); // checks if there is a horizontal input from either a or d these are values unity is already aware of, and assings them to x (a is -1 and d is 1)
        float z = Input.GetAxis("Vertical"); // checks if there is a vertical input from either w or s these are values unity is already aware of, and assings them to z (w is -1 and s is 1)

        Vector3 move = transform.right * x + transform.forward * z; // creates a Vector3 which is a variable called move with 3 presets of coordinates for us already, so we transform the right by x variable and forward by z variable these are on the same line so we can move in either direction at the same time
        //transform.right and transform.forward are things unity already recognises this also makes sure that if we move forward but look right we will move where our camera is 


        controller.Move(move * MC_PlayerSpeed * Time.deltaTime); // moves our character controller we inputted by the vector3 variable multiplied by the speed we initialised then multiplied by time delta, this ensures we move at a constant speed in correlation to our fps so there is no stuttering etc

        velocity.y += MC_gravity * Time.deltaTime; //velocity is gravity * speed your going so thats the maths eqation we add

        controller.Move(velocity * Time.deltaTime); // weve already done our velocity eqaution here to say how much on the y axis we can fall if we are in the air how fast etc, 
                                                    // but we have to times it by time delta again because the amount we want to move on the y is gravity * time squared but we only multiplied it once in our velocity eqation so we just simply multiply it again here
                                                    // sorry thats just maths, thats just the eqation we have to do to find our velocity
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