using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{

    
    public InputActionReference jumpInputSource;
    public InputActionReference moveInputSource; // Referencia a la acci�n de entrada para el movimiento
    public InputActionReference rotationInputSource; // Referencia a la acci�n de entrada para la rotaci�n
    public Vector2 moveInput;
    public Vector2 rotationInput;
    public float rotationSpeed = 100f;

    [Header("Movimiento")]
    
    public bool Sprinting = false;
    public bool MovementEnabled = true;
   
    public bool IsClimbing = false;
    

    public LayerMask GroundedLayerMask;
    public float GroundedDistance = .02f;

    [Tooltip("Grounded sphere case radius factor multiplied by the capsules radius for ground checking.")]
    public float GroundedRadiusFactor = .5f;

    [Tooltip("Minimum Player Capsule Height.")]
    public float MinHeight = .3f;

    [Header("Locomotion")]
    public bool InstantAcceleration = true;

    [Tooltip("Walking speed in m/s.")]
    public float Acceleration = 15;

    public float Deacceleration = 15f;
    public float MoveSpeed = 1.5f;
    public float SprintAcceleration = 20f;

    [Tooltip("Sprinting speed in m/s.")]
    public float RunSpeed = 3.5f;

    public float Gravity = 9.81f;
    public float MaxFallSpeed = 2f;
    public float JumpVelocity = 5f;
    public bool CanJump = false;

    public bool CanSteerWhileJumping = true;
    public bool CanSprint = true;
    public bool CanCrouch = true;

    [Tooltip("Double click timeout for sprinting.")]
    public float DoubleClickThreshold = .25f;
    public bool IsGrounded;// { get; private set; }
    public Vector3 GroundNormal { get; private set; }
    public bool MouseTurning = true;
    public float MouseSensitivityX = 2f;

    // Variables para la rotaci�n
    public enum RotationType { Smooth, Snap }
    public RotationType RotationMode = RotationType.Smooth;
    public bool RotationEnabled { get; set; } = true;

    public float SmoothTurnThreshold = 0.1f;
    public float SmoothTurnSpeed = 180f;
    public float SnapThreshold = 0.9f;
    public float SnapAmount = 45f;

    [Header("Crouching")]
    [Tooltip("Player height must be above this to toggle crouch.")]
    public float CrouchMinHeight = 1.2f;
    private float _crouchOffset;
    public bool saltando=false;

    // Referencias
    public Rigidbody RigidBody { get; private set; }
    public CharacterController CharacterController { get; private set; }


    [Tooltip("If true the player will ignore the first HMD movement on detection. " +
             "If the HMD is not centered the player would move away from it's placed position to where the HMD is.")]
    public bool InitialHMDAdjustment = true;



    [Header("Transforms")]

    public Transform Root;
    
    public Transform Neck { get; private set; }
    public Transform Camera;
    public Transform LeftControllerTransform;
    public Transform RightControllerTransform;
    public Transform NeckPivot;    
    public Transform FloorOffset;

    [Header("Head Collision")]
    public HVRHeadCollision HeadCollision;
    public float HeadCollisionFadeSpeed = 1f;
    [Tooltip("If true, limits the head distance from the body by MaxLean amount.")]
    public bool LimitHeadDistance = true;

    [Tooltip("If LimitHeadDistance is true, the max distance your head can be from your body.")]
    public float MaxLean = .5f;

    [Tooltip("Screen fades when leaning to far into something.")]
    public bool FadeFromLean = true;

    [Tooltip("If true, when your head collides it returns your head to the body's position")]
    public bool HeadCollisionPushesBack = true;
    [Header("Components")]
    public HVRCameraRig CameraRig;
    public HVRScreenFade ScreenFader;
    // public HandPhysics LeftHand;
    // public HandPhysics RightHand;

    private float _previousTurnAxis;
    private Vector3 xzVelocity = Vector3.zero;
    private float yVelocity = 0f;
    private bool _waitingForCameraMovement;

    private Vector3 _cameraStartingPosition;
    public Vector3 PreviousPosition { get; set; }

    private bool _isCameraCorrecting;

    [SerializeField]
    private float _actualVelocity;

    private Vector3 _previousLeftControllerPosition;
    private Vector3 _previousRightControllerPosition;
    public virtual float CameraHeight
    {
        get { return CameraRig.AdjustedCameraHeight; }
    }

    public bool IsCrouching => CameraHeight < CrouchMinHeight;
    private Vector3 _previousVelocity;



    public enum PlayerDirectionMode
    {
        Camera,
        LeftController,
        RightController
    }
    public PlayerDirectionMode DirectionStyle = PlayerDirectionMode.Camera;

    #region Events
    public delegate void OnBeforeMoveAction();
    public static event OnBeforeMoveAction OnBeforeMove;

    public delegate void OnAfterMoveAction();
    //public static event OnAfterMoveAction OnAfterMove;

     public delegate void OnBeforeRotateAction();
        public static event OnBeforeRotateAction OnBeforeRotate;

        public delegate void OnAfterRotateAction();
        public static event OnAfterRotateAction OnAfterRotate;
    #endregion

    private void Awake()
    {
        RigidBody = GetComponent<Rigidbody>();
        CharacterController = GetComponent<CharacterController>();


        if (NeckPivot)
            Neck = NeckPivot;
        else
            Neck = Camera;

        _cameraStartingPosition = Camera.localPosition;
    }
    private void Start()
    {
       // jumpInputSource.action.performed += ctx => HandleVerticalMovement();

        Reset();
    }
    private void OnDestroy()
    {
        if (jumpInputSource != null && jumpInputSource.action != null)
        {
            // Aseg�rate de eliminar la asociaci�n del evento cuando el objeto se destruye
            jumpInputSource.action.performed -= ctx => HandleVerticalMovement();
            
        }
        saltando = false;
    }

    private void Jump()
    {

        
        //return saltando = true;
    }

    public virtual void Reset()
    {
        _waitingForCameraMovement = InitialHMDAdjustment;
    }

    protected virtual void Update()
    {

       
        CheckCameraCorrection();
        UpdateHeight();
        HandleMovement();
        HandleRotation();
        CameraRig.PlayerControllerYOffset = _crouchOffset;
    }

    void FixedUpdate()
    {
        if (_waitingForCameraMovement)
            CheckCameraMovement();

        if (CharacterController.enabled)
        {

            HandleMovement();

            if (CanRotate())
            {
                HandleRotation();
            }
        }
       
        CheckLean();
        CheckGrounded();


        _actualVelocity = ((transform.position - PreviousPosition) / Time.deltaTime).magnitude;

        _previousLeftControllerPosition = LeftControllerTransform.position;
        _previousRightControllerPosition = RightControllerTransform.position;

        PreviousPosition = transform.position;
    }
    private IEnumerator CorrectCamera()
    {
        _isCameraCorrecting = true;

        var delta = transform.position - Neck.position;
        delta.y = 0f;

        if (!ScreenFader)
        {
            CameraRig.transform.position += delta;
            _isCameraCorrecting = false;
            yield break;
        }

        ScreenFader.Fade(1, HeadCollisionFadeSpeed);

        while (ScreenFader.CurrentFade < .9)
        {
            yield return null;
        }

        delta = transform.position - Neck.position;
        delta.y = 0f;
        CameraRig.transform.position += delta;

        ScreenFader.Fade(0, HeadCollisionFadeSpeed);

        while (ScreenFader.CurrentFade > .1)
        {
            yield return null;
        }

        _isCameraCorrecting = false;
    }
    private void CheckCameraCorrection()
    {
        if (HeadCollisionPushesBack && HeadCollision && HeadCollision.IsColliding && !_isCameraCorrecting)
        {
            StartCoroutine(CorrectCamera());
        }
    }

    protected virtual bool CanRotate()
    {
        if (!RotationEnabled)
            return false;

        //if (_hasTeleporter && Teleporter.IsAiming && !RotateWhileTeleportAiming)
        //{
        //    return false;
        //}

        return true;
    }

    protected virtual void CheckCameraMovement()
    {
        if (Vector3.Distance(_cameraStartingPosition, Camera.transform.localPosition) < .05f)
        {
            return;
        }

        var delta = Camera.transform.position - CharacterController.transform.position;
        delta.y = 0f;
        CameraRig.transform.position -= delta;
        _waitingForCameraMovement = false;
        PreviousPosition = transform.position;
    }

    protected virtual void CheckGrounded()
    {
        var radius = CharacterController.radius * GroundedRadiusFactor;
        var origin = CharacterController.center - Vector3.up * (.5f * CharacterController.height - radius);
        IsGrounded = Physics.SphereCast(
            transform.TransformPoint(origin) + Vector3.up * CharacterController.contactOffset,
            radius,
            Vector3.down,
            out var hit,
            GroundedDistance + CharacterController.contactOffset,
            GroundedLayerMask, QueryTriggerInteraction.Ignore);

        GroundNormal = hit.normal;
    }

    protected virtual void CheckLean()
    {
        if (_isCameraCorrecting || !LimitHeadDistance)
            return;

        var delta = Neck.transform.position - CharacterController.transform.position;
        delta.y = 0;

        if (delta.sqrMagnitude < .01f || delta.magnitude < MaxLean) return;

        if (FadeFromLean)
        {
            StartCoroutine(CorrectCamera());
            return;
        }

        var allowedPosition = CharacterController.transform.position + delta.normalized * MaxLean;
        var difference = allowedPosition - Neck.transform.position;
        difference.y = 0f;
        CameraRig.transform.position += difference;
    }

    protected virtual void UpdateHeight()
    {
        CharacterController.height = Mathf.Clamp(CameraRig.AdjustedCameraHeight, MinHeight, CameraRig.AdjustedCameraHeight);
        CharacterController.center = new Vector3(0, CharacterController.height * .5f + CharacterController.skinWidth, 0f);
    }
       

    protected virtual void HandleHMDMovement()
    {
        var originalCameraPosition = CameraRig.transform.position;
        var originalCameraRotation = CameraRig.transform.rotation;

        var delta = Neck.transform.position - CharacterController.transform.position;
        delta.y = 0f;
        if (delta.magnitude > 0.0f && CharacterController.enabled)
        {
            delta = Vector3.ProjectOnPlane(delta, GroundNormal);
            CharacterController.Move(delta);
        }

        transform.rotation = Quaternion.Euler(0.0f, Neck.rotation.eulerAngles.y, 0.0f);

        CameraRig.transform.position = originalCameraPosition;
        var local = CameraRig.transform.localPosition;
        local.y = 0f;
        CameraRig.transform.localPosition = local;
        CameraRig.transform.rotation = originalCameraRotation;
    }
    
    protected virtual void HandleRotation()
    {
        if (RotationMode == RotationType.Smooth)
        {
            HandleSmoothRotation();
        }
        else if (RotationMode == RotationType.Snap)
        {
            HandleSnapRotation();
        }

        //HandlMouseRotation();

        _previousTurnAxis = GetTurnAxis().x;
    }

    private Vector2 GetTurnAxis()
    {
        return rotationInput = rotationInputSource.action.ReadValue<Vector2>();
    }

    private Vector2 GetMovementAxis()
    {
        return moveInput = moveInputSource.action.ReadValue<Vector2>();
    }

    //protected virtual void HandlMouseRotation()
    //{
    //    if (MouseTurning)
    //    {
    //        if (Inputs.IsMouseDown)
    //        {
    //            var offset = Quaternion.Euler(new Vector3(0, Inputs.MouseAxis.x * MouseSensitivityX, 0));
    //            transform.rotation *= offset;

    //            Cursor.lockState = CursorLockMode.Locked;
    //        }
    //        else
    //        {
    //            Cursor.lockState = CursorLockMode.None;
    //        }
    //    }
    //}

    protected virtual void HandleSnapRotation()
    {
        var input = GetTurnAxis().x;
        if (Math.Abs(input) < SnapThreshold || Mathf.Abs(_previousTurnAxis) > SnapThreshold)
            return;

        
         OnBeforeRotate?.Invoke();
        var rotation = Quaternion.Euler(0, Mathf.Sign(input) * SnapAmount, 0);
        transform.rotation *= rotation;
        OnAfterRotate?.Invoke();
    }

    protected virtual void HandleSmoothRotation()
    {
        var input = GetTurnAxis().x;
        if (Math.Abs(input) < SmoothTurnThreshold)
            return;

        var rotation = input * SmoothTurnSpeed * Time.deltaTime;
        var rotationVector = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotation, transform.eulerAngles.z);
        transform.rotation = Quaternion.Euler(rotationVector);
    }

    protected virtual void HandleMovement()
    {
        if (IsClimbing)
        {
           // HandleClimbing();
            return;
        }

        if (!_waitingForCameraMovement)
        {
            HandleHMDMovement();
        }

        HandleHorizontalMovement();
        HandleVerticalMovement();
        //SyncHands();

        AdjustHandAcceleration();
    }

    private void AdjustHandAcceleration()
    {
        // var v = (transform.position - PreviousPosition) / Time.deltaTime;
        // var acceler = (v - _previousVelocity) / Time.deltaTime;
        // _previousVelocity = v;

        // LeftHand.rigid.AddForce(acceler * LeftHand.rigid.mass, ForceMode.Force);
        // RightHand.rigid.AddForce(acceler * RightHand.rigid.mass, ForceMode.Force);

        // Rigidbody leftRB = null;
        // Rigidbody rightRb = null;

        // if (LeftHand.ThisGrabber && LeftHand.ThisGrabber.body)
        // {
        //     leftRB = LeftHand.ThisGrabber.body;
        // }

        // if (RightHand.ThisGrabber && RightHand.ThisGrabber.body)
        // {
        //     rightRb = RightHand.ThisGrabber.body;
        // }

        // if (leftRB && rightRb && leftRB == rightRb)
        // {
        //     LeftHand.rigid.AddForce(acceler * (.5f * leftRB.mass), ForceMode.Force);
        //     RightHand.rigid.AddForce(acceler * (.5f * rightRb.mass), ForceMode.Force);
        // }
        // else
        // {
        //     if (leftRB)
        //     {
        //         LeftHand.rigid.AddForce(acceler * leftRB.mass, ForceMode.Force);
        //     }

        //     if (rightRb)
        //     {
        //         RightHand.rigid.AddForce(acceler * rightRb.mass, ForceMode.Force);
        //     }
        // }
    }

    private void SyncHands()
    {

        // Obtener la posici�n del CharacterController
        // Vector3 controllerPosition = CharacterController.transform.position;

        // // Sincronizar las posiciones de las manos con el CharacterController
        // LeftHand.transform.position = controllerPosition + LeftHand.Offset;
        // RightHand.transform.position = controllerPosition + RightHand.Offset;
        //// Obtener la posici�n y rotaci�n del CharacterController
        //Vector3 controllerPosition = CharacterController.transform.position;
        //Quaternion controllerRotation = CharacterController.transform.rotation;

        //// Obtener la posici�n relativa de las manos al CharacterController
        //Vector3 leftHandRelativePosition = LeftHand.transform.position - controllerPosition;
        //Vector3 rightHandRelativePosition = RightHand.transform.position - controllerPosition;

        //// Sincronizar las posiciones de las manos con el CharacterController sin afectar su seguimiento
        //LeftHand.transform.position = controllerPosition + leftHandRelativePosition;
        //RightHand.transform.position = controllerPosition + rightHandRelativePosition;

        //// Mantener la rotaci�n de las manos igual a la rotaci�n del CharacterController, pero solo en el eje y
        //Quaternion leftHandRotation = Quaternion.Euler(0f, controllerRotation.eulerAngles.y, 0f);
        //Quaternion rightHandRotation = Quaternion.Euler(0f, controllerRotation.eulerAngles.y, 0f);

        //// Aplicar rotaci�n solo en el eje y para mantener la orientaci�n original en los otros ejes
        //LeftHand.transform.rotation = leftHandRotation * LeftHand.transform.rotation;
        //RightHand.transform.rotation = rightHandRotation * RightHand.transform.rotation;
        //// Obtener la posici�n y rotaci�n del CharacterController
        //Vector3 controllerPosition = CharacterController.transform.position;
        //Quaternion controllerRotation = CharacterController.transform.rotation;

        //// Obtener la posici�n relativa de las manos al CharacterController
        //Vector3 leftHandRelativePosition = LeftHand.transform.position - controllerPosition;
        //Vector3 rightHandRelativePosition = RightHand.transform.position - controllerPosition;

        //// Sincronizar las posiciones de las manos con el CharacterController sin afectar su seguimiento
        //LeftHand.transform.position = controllerPosition + leftHandRelativePosition;
        //RightHand.transform.position = controllerPosition + rightHandRelativePosition;

        //// Mantener la rotaci�n de las manos igual a la rotaci�n del CharacterController
        //LeftHand.transform.rotation = controllerRotation;
        //RightHand.transform.rotation = controllerRotation;
        //// Obtener la posici�n del CharacterController
        //Vector3 controllerPosition = CharacterController.transform.position;

        //// Sincronizar las posiciones de las manos con la posici�n del CharacterController
        //if (LeftHand != null)
        //{
        //    LeftHand.transform.position = controllerPosition + LeftHand.Offset;
        //}
        //if (RightHand != null)
        //{
        //    RightHand.transform.position = controllerPosition + RightHand.Offset;
        //}
    }

    private void HandleClimbing()
    {
        throw new NotImplementedException();
    }

    protected virtual void GetMovementDirection(out Vector3 forwards, out Vector3 right)
    {
        var t = transform;

        switch (DirectionStyle)
        {
            case PlayerDirectionMode.Camera:
                if (Camera)
                    t = Camera;
                break;
            case PlayerDirectionMode.LeftController:
                if (LeftControllerTransform)
                    t = LeftControllerTransform;
                break;
            case PlayerDirectionMode.RightController:
                if (RightControllerTransform)
                    t = RightControllerTransform;
                break;
        }

        forwards = t.forward;
        right = t.right;
        forwards.y = 0;
        forwards.Normalize();
        right.y = 0;
        right.Normalize();
    }

    protected virtual void HandleVerticalMovement()
    {
        
        Vector3 velocity = xzVelocity;

        if (IsGrounded)
        {
            if (CanJump && MovementEnabled)
            {
                yVelocity = JumpVelocity;
                Debug.Log("Bot�n A presionado");
            }
            else
            {
                yVelocity += -Gravity * Time.deltaTime;
            }

            yVelocity = Mathf.Clamp(yVelocity, -Gravity * Time.deltaTime, yVelocity);
        }
        else
        {
            yVelocity += -Gravity * Time.deltaTime;
            yVelocity = Mathf.Clamp(yVelocity, -MaxFallSpeed, yVelocity);
        }

        velocity.y += yVelocity;

        CharacterController.Move(velocity * Time.deltaTime);
    }
    public float Magnitude;

    protected virtual void HandleHorizontalMovement()
    {
        var speed = MoveSpeed;
        var runSpeed = RunSpeed;

        if (Sprinting)
            speed = runSpeed;

        var movement = GetMovementAxis();

        Magnitude = (float)Math.Round(movement.magnitude * 1000f) / 1000f;

        if (!MovementEnabled)
        {
            movement = Vector2.zero;
        }
        bool callEvents = Magnitude > 0.0f;

        // Call any Before Move Events
        if (callEvents)
        {
            OnBeforeMove?.Invoke();
           // Debug.Log("Se llamo el evento onbefore01");
        }

        GetMovementDirection(out var forward, out var right);
        var direction = (forward * movement.y + right * movement.x);

        if (IsGrounded || CanSteerWhileJumping)
        {
            if (IsGrounded)
            {
                direction = Vector3.ProjectOnPlane(direction, GroundNormal);
            }

            if (InstantAcceleration)
            {
                xzVelocity = speed * direction;
            }
            else
            {
                var noMovement = Mathf.Abs(movement.x) < .1f && Mathf.Abs(movement.y) < .1f;
                if (noMovement)
                {
                    var dir = xzVelocity.normalized;
                    var deacceleration = Deacceleration * Time.deltaTime;
                    if (deacceleration > xzVelocity.magnitude)
                    {
                        xzVelocity = Vector3.zero;
                    }
                    else
                    {
                        xzVelocity -= dir * deacceleration;
                    }
                }
                else
                {
                    var acceleration = (Sprinting ? SprintAcceleration : Acceleration) * Time.deltaTime;
                    xzVelocity += acceleration * direction;
                    xzVelocity = Vector3.ClampMagnitude(xzVelocity, speed);
                }
            }
        }
        if (callEvents)
        {
            OnBeforeMove?.Invoke();
           // Debug.Log("Se llamo el evento onbefore01");
        }
    }

   

    //private void OnEnable()
    //{
    //    jumpInputSource.action.Enable();
    //}

    //private void OnDisable()
    //{
    //    jumpInputSource.action.Disable();
    //}

    public virtual void IgnoreCollision(IEnumerable<Collider> colliders)
    {
        foreach (var otherCollider in colliders)
        {
            if (otherCollider && CharacterController)
                Physics.IgnoreCollision(CharacterController, otherCollider, true);
        }
    }

}