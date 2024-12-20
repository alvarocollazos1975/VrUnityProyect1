using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlcPlayerRotacion : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // Transform del XR Origin
    [SerializeField] private float snapAngle = 45f; // Ángulo de rotación por pasos
    [SerializeField] private float smoothRotationSpeed = 5f; // Velocidad de rotación suave

    private bool useSnapRotation = true; // Cambiar entre suave y por pasos

    public void HandleRotation()
    {
        if (useSnapRotation)
        {
            SnapRotation();
        }
        else
        {
            SmoothRotation();
        }
    }

    private void SnapRotation()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Ejemplo de input para rotar a la derecha
        {
            playerTransform.Rotate(0, snapAngle, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Q)) // Ejemplo de input para rotar a la izquierda
        {
            playerTransform.Rotate(0, -snapAngle, 0);
        }
    }

    private void SmoothRotation()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Suponer input del joystick
        float targetAngle = horizontalInput * smoothRotationSpeed * Time.deltaTime;
        playerTransform.Rotate(0, targetAngle, 0);
    }

   
     
}
