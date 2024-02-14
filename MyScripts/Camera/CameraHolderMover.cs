using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolderMover : MonoBehaviour
{
    public GameObject playerCharacterHead;
    public Transform cameraPosition;
    
    void Update()
    {
        cameraPosition.position = new Vector3(cameraPosition.position.x, playerCharacterHead.transform.position.y, cameraPosition.position.z);
        transform.position = cameraPosition.position;
    }
}
