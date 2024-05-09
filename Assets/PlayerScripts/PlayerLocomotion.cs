using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    InputManager inputManager;
    PlayerManager playerManager;
    AnimatorManager animatorManager;
    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRigidbody;
    [Header("Falling")]
    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public float rayCastHeightOffset = 0.5f;
     public float maxDistance = 1;
    public LayerMask groundLayer;
    [Header("Movement Flags")]
    public bool isSprinting;
    public bool isGrounded;

    [Header("Movement Speeds")]
    public float walkingSpeed =1.5f; 
    public float runningSpeed = 5;
    public float sprintingSpeed = 7;
    public float rotationSpeed = 15;
    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;

    }
    public void HandleAllMovement()
    {


      HandleFallingAndLanding();
      if (playerManager.isInteracting)
         return;
      HandleMovement();
      HandleRotation();
    }
    private void HandleMovement()
    {
       moveDirection = cameraObject.forward *  inputManager.verticalInput;
       moveDirection = moveDirection + cameraObject.right * inputManager.horizontalInput;
       moveDirection.Normalize();
       moveDirection.y = 0;

       if (isSprinting)
       {
          moveDirection = moveDirection * sprintingSpeed;
       }
       else
       {
            if (inputManager.moveAmount >= 0.5f)
       {
            moveDirection = moveDirection * runningSpeed;
       }
            else
       {
            moveDirection =moveDirection * walkingSpeed;
       }
       }
       

       
     

      Vector3 movementVelocity = moveDirection;
       playerRigidbody.velocity = movementVelocity;
    }
    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;
        targetDirection = cameraObject.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;
        
        if(targetDirection == Vector3.zero)
            targetDirection = transform.forward;
        

      
           

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation,targetRotation, rotationSpeed * Time.deltaTime);
        
        transform.rotation = playerRotation;
    }

    private void HandleFallingAndLanding()
    {
       RaycastHit hit;
       Vector3 rayCastOrigin = transform.position; 
       rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;
       if (!isGrounded)
       {
         if (playerManager.isInteracting)
         {
            animatorManager.PlayTargetAnimation("Falling",true);
         }

         inAirTimer = inAirTimer + Time.deltaTime;
         playerRigidbody.AddForce(transform.forward * leapingVelocity);
         playerRigidbody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
       }

       if (Physics.SphereCast(rayCastOrigin,0.2f,-Vector3.up,out hit,maxDistance,groundLayer))
       {
          if (!isGrounded && !playerManager.isInteracting)
          {
            animatorManager.PlayTargetAnimation("Land",true);
          }
          inAirTimer = 0;
          isGrounded = true;
          playerManager.isInteracting = false;
       }
       else
       {
          isGrounded = false;
          playerManager.isInteracting = true;

       }
    }
}
