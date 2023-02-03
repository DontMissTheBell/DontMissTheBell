using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MouseLookMainCharacter : MonoBehaviour
{
    public float mouseSensitivity = 1000f;

    public Transform playerBody; // creates a transform we can put our player into so when we rotate horizontally the player will also rotate with the camera

    float xRotation = 0f; // for the vertical camera movement

    private float tiltTime;
    private float tiltDuration;
    private float startTilt;
    private float tiltTarget;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // hides the cursor
    }

    void Update()
    {                                                                                     // changed to fixedDeltaTime to correct the jittering that was happening
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.fixedDeltaTime; // creates a variable saying to move the camera along x axis if mouse is moving along x axis at the mouseSensitivity rate multiplied by time delta
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.fixedDeltaTime; // Time Delta ensures that we will be moving our camera in correlation with our fps essentially

        xRotation -= mouseY; // takes xRotation and turns it negative to mouseY because in 2d Space y is y however in 3D space x is what we want to rotate around so we rotate y around x, negative becasue it just is lmao 
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // ensures we cant look below or behind us


        if (tiltTime < tiltDuration){ //If the screen if being tilted then this chunk of code is ran instead to alter the Z rotation of the camera
            if (tiltTime <= tiltDuration/2) // This will smoothly tilt the screen back and forward for the duration of the tiltDuration
               {
                   transform.localRotation = Quaternion.Euler(xRotation,transform.localRotation.y,Mathf.Lerp(startTilt, tiltTarget,Mathf.SmoothStep(0.0f,1.0f,tiltTime/tiltDuration)));
               }
           else
               {
                   transform.localRotation = Quaternion.Euler(xRotation,transform.localRotation.y,Mathf.Lerp(tiltTarget, startTilt,Mathf.SmoothStep(0.0f,1.0f,tiltTime/tiltDuration)));
               }
            tiltTime += Time.deltaTime;
        }
        else // If the screen isnt being tilted, then it runs the default rotation code
        {
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // is making sure we can only roate the player along the correct axis so it isnt all 3 axis at once and only the z axis
        }

        playerBody.Rotate(Vector3.up * mouseX); // rotates whatever transform we put into the Transform variable at the start for us it will be the player, then it will rotate the player alongside the camera
    }

    public void TiltScreen(float duration, float tilt)
    {
        tiltTime = 0f;
        tiltDuration = duration;

        startTilt = transform.rotation.z;
        tiltTarget = startTilt + tilt;

    }

}
    

