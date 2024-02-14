using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Player GameObject")]
    public Transform playerCharacter;

    private float senseX; // default = 400
    private float senseY; // default = 400
    private float xRotation;
    private float yRotation;

    void Start()
    {
        senseX = 400;
        senseY = 400;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * senseX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * senseY;
        
        yRotation += mouseX;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerCharacter.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
