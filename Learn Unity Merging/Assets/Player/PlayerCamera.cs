using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private InputActionReference action;
    [SerializeField] private Vector2 sensitivity;

    Vector2 lookAngles;

    private void Start()
    {
        lookAngles = transform.localEulerAngles;

        //Capture cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //Calculate input
        Vector2 rawInput = action.action.ReadValue<Vector2>();
        Vector2 scaledInput = new Vector2(rawInput.x*sensitivity.x, rawInput.y*sensitivity.y);

        //Apply
        lookAngles += new Vector2(scaledInput.y, scaledInput.x); //Swizzle otherwise our axes are inverted
        lookAngles.x = Mathf.Clamp(lookAngles.x, -90, 90);
        transform.localEulerAngles = lookAngles;
    }
}
