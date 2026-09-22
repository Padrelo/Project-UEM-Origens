using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movimento : MonoBehaviour
{
    //movement speed in units per second
    [SerializeField] private float movementSpeed = 5f;

    void Update()
    {
        //get the Input from Horizontal axis
        float horizontalInput = Input.GetAxis("Horizontal");
        //get the Input from Vertical axis
        float verticalInput = Input.GetAxis("Vertical");

        //update the position
        if (horizontalInput > 0) horizontalInput = 1;
        else if (horizontalInput < 0) horizontalInput = -1;
        //if (horizontalInput == 1 || horizontalInput == -1)
        //{
        //    transform.position = transform.position + new Vector3(horizontalInput * movementSpeed * Time.deltaTime, 0);
        //}
        transform.position = transform.position + new Vector3(horizontalInput * movementSpeed * Time.deltaTime, verticalInput * movementSpeed * Time.deltaTime, 0);
        transform.position.Normalize();
    }
}
