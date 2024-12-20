using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    
    
        [Header("Camera Options : ")]

        [Tooltip("If true the CharacterController will move along with the HMD, as long as there are no obstacle's in the way")]
        public bool MoveCharacterWithCamera = true;

        [Tooltip("If true the CharacterController will rotate it's Y angle to match the HMD's Y angle")]
        public bool RotateCharacterWithCamera = true;

        [Tooltip("If true the CharacterController will resize to match the calculated player height (distance from floor to camera)")]
        public bool ResizeCharacterHeightWithCamera = true;

        [Header("Ground checks : ")]
        [Tooltip("Raycast against these layers to check if player is grounded")]
        public LayerMask GroundedLayers;

        /// <summary>
        /// 0 means we are grounded
        /// </summary>
        [Tooltip("How far off the ground the player currently is. 0 = Grounded, 1 = 1 Meter in the air.")]
        public float DistanceFromGround = 0;

        [Tooltip("DistanceFromGround will subtract this value when determining distance from ground")]
        public float DistanceFromGroundOffset = 0;

        [Header("Player Capsule Settings : ")]

        /// <summary>
        /// Minimum Height our Player's capsule collider can be (in meters)
        /// </summary>
        [Tooltip("Minimum Height our Player's capsule collider can be (in meters)")]
        public float MinimumCapsuleHeight = 0.4f;

        /// <summary>
        /// Maximum Height our Player's capsule collider can be (in meters)
        /// </summary>
        [Tooltip("Maximum Height our Player's capsule collider can be (in meters)")]
        public float MaximumCapsuleHeight = 3f;        

         [Header("Player Y Offset : ")]
        /// <summary>
        /// Offset the height of the CharacterController by this amount
        /// </summary>
        [Tooltip("Offset the height of the CharacterController by this amount")]
        public float CharacterControllerYOffset = -0.025f;

        /// <summary>
        /// Height of our camera in local coords
        /// </summary>
        [HideInInspector]
        public float CameraHeight;

         [Header("Misc : ")]                

        [Tooltip("If true the Camera will be offset by ElevateCameraHeight if no HMD is active or connected. This prevents the camera from falling to the floor and can allow you to use keyboard controls.")]
        public bool ElevateCameraIfNoHMDPresent = true;

        [Tooltip("How high (in meters) to elevate the player camera if no HMD is present and ElevateCameraIfNoHMDPresent is true. 1.65 = about 5.4' tall. ")]
        public float ElevateCameraHeight = 1.65f;

        /// <summary>
        /// If player goes below this elevation they will be reset to their initial starting position.
        /// If the player goes too far away from the center they may start to jitter due to floating point precisions.
        /// Can also use this to detect if player somehow fell through a floor. Or if the "floor is lava".
        /// </summary>
        [Tooltip("Minimum Y position our player is allowed to go. Useful for floating point precision and making sure player didn't fall through the map.")]
        public float MinElevation = -6000f;

        /// <summary>
        /// If player goes above this elevation they will be reset to their initial starting position.
        /// If the player goes too far away from the center they may start to jitter due to floating point precisions.
        /// </summary>
        public float MaxElevation = 6000f;

         [HideInInspector]
        public float LastPlayerMoveTime;

         // Use smooth movement if available
        protected AlcSmoothLocomotion smoothLocomotion;

        // The controller to manipulate
        protected CharacterController characterController;

        // The controller to manipulate
        protected Rigidbody playerRigid;
        protected CapsuleCollider playerCapsule;

        // Optional components can be used to update LastMoved Time
        protected PlayerClimbing playerClimbing;
        protected bool isClimbing, wasClimbing = false;

        // This the object that is currently beneath us
        public RaycastHit groundHit;

        // Stored for GC
        protected RaycastHit hit;

        protected Transform mainCamera;

        private Vector3 _initialPosition;

          [SerializeField] private Transform xrOrigin; // XR Origin que incluye la cámara
     
    // Start is called before the first frame update
    void Start()
    {
         characterController = GetComponentInChildren<CharacterController>();
            playerRigid = GetComponent<Rigidbody>();
            playerCapsule = GetComponent<CapsuleCollider>();
            smoothLocomotion = GetComponentInChildren<AlcSmoothLocomotion>();

            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;

            if (characterController) {
                _initialPosition = characterController.transform.position;
            }
            else if(playerRigid) {
                _initialPosition = playerRigid.position;
            }
            else {
                _initialPosition = transform.position;
            }

            playerClimbing = GetComponentInChildren<PlayerClimbing>();
        
    }

 private void Update()
    {
        
          // Sanity check for camera
            if (mainCamera == null && Camera.main != null) {
                mainCamera = Camera.main.transform;
            }

            UpdateCharacterHeight();
        // Actualizar la posición y altura
        AdjustHeightAndPosition();

        CheckCharacterCollisionMove();

       

        // Rotación del jugador
        //playerRotacion.HandleRotation();
    }

     void FixedUpdate() {

            UpdateDistanceFromGround();

            CheckPlayerElevationRespawn();
        }

    /// <summary>
        /// Check if the player has moved beyond the specified min / max elevation
        /// Player should never go above or below 6000 units as physics can start to jitter due to floating point precision
        /// Maybe they clipped through a floor, touched a set "lava" height, etc.
        /// </summary>

     public virtual void CheckPlayerElevationRespawn() {

            // No need for elevation checks
            if(MinElevation == 0 && MaxElevation == 0) {
                return;
            }

            // Check Elevation based on Character Controller height
            if(characterController != null && (characterController.transform.position.y < MinElevation || characterController.transform.position.y > MaxElevation)) {
                Debug.Log("Player out of bounds; Returning to initial position.");
                characterController.transform.position = _initialPosition;
            }
			
            // Check Elevation based on Character Controller height
            if(playerRigid != null && (playerRigid.transform.position.y < MinElevation || playerRigid.transform.position.y > MaxElevation)) {
                Debug.Log("Player out of bounds; Returning to initial position.");
                playerRigid.transform.position = _initialPosition;
            }			
        }


    private void AdjustHeightAndPosition()
    {
        Vector3 currentPosition = transform.position;
        Vector3 newPosition = new Vector3(currentPosition.x, xrOrigin.position.y, currentPosition.z);
        transform.position = newPosition;
    }

     public virtual void UpdateDistanceFromGround() {

            if(characterController) {
                if (Physics.Raycast(characterController.transform.position, -characterController.transform.up, out groundHit, 20, GroundedLayers, QueryTriggerInteraction.Ignore)) {
                    DistanceFromGround = Vector3.Distance(characterController.transform.position, groundHit.point);
                    DistanceFromGround += characterController.center.y;
                    DistanceFromGround -= (characterController.height * 0.5f) + characterController.skinWidth;

                    // Round to nearest thousandth
                    DistanceFromGround = (float)Math.Round(DistanceFromGround * 1000f) / 1000f;
                }
                else {
                    DistanceFromGround = float.MaxValue;
                }
            }
			
            if(playerRigid) {
                if (Physics.Raycast(playerCapsule.transform.position, -playerCapsule.transform.up, out groundHit, 20, GroundedLayers, QueryTriggerInteraction.Ignore)) {
                    DistanceFromGround = Vector3.Distance(playerCapsule.transform.position, groundHit.point);
                    DistanceFromGround += playerCapsule.center.y;
                    DistanceFromGround -= (playerCapsule.height * 0.5f);

                    // Round to nearest thousandth
                    DistanceFromGround = (float)Math.Round(DistanceFromGround * 1000f) / 1000f;
                }
                else {
                    DistanceFromGround = float.MaxValue;
                }
            }
			
            // No CharacterController found. Update Distance based on current transform position
            else {
                if (Physics.Raycast(transform.position, -transform.up, out groundHit, 20, GroundedLayers, QueryTriggerInteraction.Ignore)) {
                    DistanceFromGround = Vector3.Distance(transform.position, groundHit.point) - 0.0875f;
                    // Round to nearest thousandth
                    DistanceFromGround = (float)Math.Round(DistanceFromGround * 1000f) / 1000f;
                }
                else {
                    DistanceFromGround = float.MaxValue;
                }
            }

            if (DistanceFromGround != float.MaxValue) {
                DistanceFromGround -= DistanceFromGroundOffset;
            }

            // Smooth floating point issues from thousandths
            if(DistanceFromGround < 0.001f && DistanceFromGround > -0.001f) {
                DistanceFromGround = 0;
            }
        }


    /// <summary>
        /// Move the character controller to new camera position
        /// </summary>
        public virtual void CheckCharacterCollisionMove()
        {

             // Calcular delta para movimiento
        Vector3 cameraPosition = xrOrigin.position;
        Vector3 delta = cameraPosition - transform.position;
        smoothLocomotion.MoveCharacter(delta);            
        }

         public virtual bool IsGrounded() {

            // Immediately check for a positive from a CharacterController if it's present
            if(characterController != null) {
                if(characterController.isGrounded) {
                    return true;
                }
            }
			
            // DistanceFromGround is a bit more reliable as we can give a bit of leniency in what's considered grounded
            return DistanceFromGround <= 0.007f;
        }

          public float SphereColliderRadius = 0.08f;

         public virtual void UpdateCharacterHeight() {
            float minHeight = MinimumCapsuleHeight;
            // Increase Min Height if no HMD is present. This prevents our character from being really small
            if(minHeight < 1f) {
                minHeight = 1f;
            }

            // Update Character Height based on Camera Height.
            if(characterController) {
                characterController.height = Mathf.Clamp(CameraHeight + CharacterControllerYOffset - characterController.skinWidth, minHeight, MaximumCapsuleHeight);

                // If we are climbing set the capsule center upwards
              
            }
            else if(playerRigid && playerCapsule) {
                playerCapsule.height = Mathf.Clamp(CameraHeight + CharacterControllerYOffset, minHeight, MaximumCapsuleHeight);
                playerCapsule.center = new Vector3(0, playerCapsule.height / 2 + (SphereColliderRadius * 2), 0);
            }
        }

        

}
