using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMov : MonoBehaviour
{
    public float moveSmoothness;
    public float rotSmoothness;

    public Vector3 movOffset;
    public Vector3 rotOffset;

    public Transform playerTarget;
    private Vector3 originalPosition; // To store the original position of the camera

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        FollowTarget();
    }

    public void FollowTarget()
    {
        HandleMovement();
       // HandleRotation();
    }

    public void HandleMovement()
    {
        Vector3 targetPos = new Vector3();
        targetPos = playerTarget.TransformPoint(movOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);

        originalPosition = transform.localPosition; // Store the initial position of the camera
    }

    public void HandleRotation()
    {
        var direction = playerTarget.position - transform.position;
        var rotation = new Quaternion();

        rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSmoothness * Time.deltaTime);
    }
}
