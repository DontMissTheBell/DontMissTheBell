using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLookMainCharacter : MonoBehaviour
{
    public float mouseSensitivity = 1000f;

    public Transform playerBody; // creates a transform we can put our player into so when we rotate horizontally the player will also rotate with the camera

    float xRotation = 0f; // for the vertical camera movement

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // hides the cursor
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; // creates a variable saying to move the camera along x axis if mouse is moving along x axis at the mouseSensitivity rate multiplied by time delta
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // Time Delta ensures that we will be moving our camera in correlation with our fps essentially

        xRotation -= mouseY; // takes xRotation and turns it negative to mouseY because in 2d Space y is y however in 3D space x is what we want to rotate around so we rotate y around x, negative becasue it just is lmao 
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // ensures we cant look below or behind us

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // is making sure we can only roate the player along the correct axis so it isnt all 3 axis at once and only the z axis

        playerBody.Rotate(Vector3.up * mouseX); // rotates whatever transform we put into the Transform variable at the start for us it will be the player, then it will rotate the player alongside the camera
    }

}
    

