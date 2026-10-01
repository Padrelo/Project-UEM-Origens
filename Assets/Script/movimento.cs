using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movimento : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5f;

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        float verticalInput = Input.GetAxis("Vertical");

        if (horizontalInput > 0 && horizontalInput < 1) horizontalInput = 0;
        else if (horizontalInput > 0) horizontalInput = 1;
        else if (horizontalInput < 0 && horizontalInput > -1) horizontalInput = 0;
        else if (horizontalInput < 0) horizontalInput = -1;

        if (verticalInput > 0 && verticalInput < 1) verticalInput = 0;
        else if (verticalInput > 0) verticalInput = 1;
        else if (verticalInput < 0 && verticalInput > -1) verticalInput = 0;
        else if (verticalInput < 0) verticalInput = -1;

        transform.position = transform.position + new Vector3(horizontalInput * movementSpeed * Time.deltaTime, verticalInput * movementSpeed * Time.deltaTime, 0);
        transform.position.Normalize();
    }
}