using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
    /// An example hand controller that sets animation values depending on Grabber state
    /// </summary>
    public class HandController : MonoBehaviour {

        [Header("Setup : ")]
        [Tooltip("HandController parent will be set to this on Start if specified")]
        public Transform HandAnchor;

        [Tooltip("If true, this transform will be parented to HandAnchor and it's position / rotation set to 0,0,0.")]
        public bool ResetHandAnchorPosition = true;

        public Animator HandAnimator;

       // [Tooltip("(Optional) If specified, this HandPoser can be used when setting poses retrieved from a grabbed Grabbable.")]
        //public HandPoser handPoser;

      //  [Tooltip("(Optional) If specified, this AutoPoser component can be used when if set on the Grabbable, or if AutoPose is set to true")]
       // public AutoPoser autoPoser;

        // We can use the HandPoseBlender to blend between an open and closed hand pose, using controller inputs such as grip and trigger as the blend values
        //HandPoseBlender poseBlender;

        [Tooltip("How to handle the hand when nothing is being grabbed / idle. Ex : Can use an Animator to control the hand via blending, a HandPoser to control via blend states, AutoPoser to continually auto pose while nothing is being held, or 'None' if you want to handle the idle state yourself.")]
        public HandPoserType IdlePoseType = HandPoserType.HandPoser;

        [Tooltip("If true, the idle hand pose will be determined by the connected Valve Index Controller's finger tracking. Requires the SteamVR SDK. Make sure IdlePoseType is set to 'HandPoser'")]
        public bool UseIndexFingerTracking = true;

        /// <summary>
        /// How fast to Lerp the Layer Animations
        /// </summary>
        [Tooltip("How fast to Lerp the Layer Animations")]
        public float HandAnimationSpeed = 20f;

        [Tooltip("Check the state of this grabber to determine animation state. If null, a child Grabber component will be used.")]
        public HandPose HandPoseOverride;

        [Header("Shown for Debug : ")]
        /// <summary>
        /// 0 = Open Hand, 1 = Full Grip
        /// </summary>
        [Range(0f, 1f)]
        public float GripAmount;
        private float _prevGrip;

        /// <summary>
        /// 0 = Index Curled in,  1 = Pointing Finger
        /// </summary>
        [Range(0f, 1f)]
        public float PointAmount;
        private float _prevPoint;

        /// <summary>
        /// 0 = Thumb Down, 1 = Thumbs Up
        /// </summary>
        [Range(0f, 1f)]
        public float ThumbAmount;
        private float _prevThumb;
        
        // Raw input values
        private bool _thumbIsNear = false;
        private bool _indexIsNear = false;
        private float _triggerValue = 0f;
        private float _gripValue = 0f;

        public int PoseId;

        ControllerOffsetHelper offset;
        InputBridge input;
        Rigidbody rigid;
        Transform offsetTransform;

        public bool handPoser;

        Vector3 offsetPosition {
            get {
                if(offset) {
                    return offset.OffsetPosition;
                }
                return Vector3.zero;
            }
        }

        Vector3 offsetRotation {
            get {
                if (offset) {
                    return offset.OffsetRotation;
                }
                return Vector3.zero;
            }
        }

        void Start() {

            rigid = GetComponent<Rigidbody>();
            offset = GetComponent<ControllerOffsetHelper>();
            offsetTransform = new GameObject("OffsetHelper").transform;
            offsetTransform.parent = transform;

            if (HandAnchor) {
                transform.parent = HandAnchor;
                offsetTransform.parent = HandAnchor;

                if (ResetHandAnchorPosition) {
                    transform.localPosition = offsetPosition;
                    transform.localEulerAngles = offsetRotation;
                }
            }
         

            input = InputBridge.Instance;
        }

        public void Update() {

        

            // Set Hand state according to InputBridge
            UpdateFromInputs();
            
         
          
        }

        

        /// <summary>
        /// Dropped our held item - nothing currently in our hands
        /// </summary>
       

        public virtual void SetHandAnimator() {
            if (HandAnimator == null || !HandAnimator.gameObject.activeInHierarchy) {
                HandAnimator = GetComponentInChildren<Animator>();
            }
        }

        /// <summary>
        /// Update GripAmount, PointAmount, and ThumbAmount based raw input from InputBridge
        /// </summary>
        public virtual void UpdateFromInputs() {

            // // Grabber may have been deactivated
            // if (grabber == null || !grabber.isActiveAndEnabled) {
            //     grabber = GetComponentInChildren<Grabber>();
            //     GripAmount = 0;
            //     PointAmount = 0;
            //     ThumbAmount = 0;
            //     return;
            // }

            // // Update raw values based on hand side
            // if (grabber.HandSide == ControllerHand.Left) {
            //     _indexIsNear = input.LeftTriggerNear;
            //     _thumbIsNear = input.LeftThumbNear;
            //     _triggerValue = input.LeftTrigger;
            //     _gripValue = input.LeftGrip;
            // }
            // else if (grabber.HandSide == ControllerHand.Right) {
            //     _indexIsNear = input.RightTriggerNear;
            //     _thumbIsNear = input.RightThumbNear;
            //     _triggerValue = input.RightTrigger;
            //     _gripValue = input.RightGrip;
            // }

            // Massage raw values to get a better value set the animator can use
            GripAmount = _gripValue;
            ThumbAmount = _thumbIsNear ? 0 : 1;

            // Point Amount can vary depending on if touching or our input source
            PointAmount = 1 - _triggerValue; // Range between 0 and 1. 1 == Finger all the way out
            PointAmount *= InputBridge.Instance.InputSource == XRInputSource.SteamVR ? 0.25F : 0.5F; // Reduce the amount our finger points out if Oculus or XRInput

            // If not near the trigger, point finger all the way out
            if (input.SupportsIndexTouch && _indexIsNear == false && PointAmount != 0) {
                PointAmount = 1f;
            }
            // Does not support touch, stick finger out as if pointing if no trigger found
            else if (!input.SupportsIndexTouch && _triggerValue == 0) {
                PointAmount = 1;
            }
        }

       

        // public virtual HandPose GetDefaultOpenPose() {
        //     return Resources.Load<HandPose>("Open");
        // }

        // public virtual HandPose GetDefaultClosedPose() {
        //     return Resources.Load<HandPose>("Closed");
        // }

        public virtual void EnableHandPoser() {
            // Disable the hand animator if we have a valid hand pose to use
            // if(handPoser != null) {
            //     // Just need to make sure animator isn't enabled
            //     DisableHandAnimator();
            // }
        }

       
    }
    
    public enum HandPoserType {
        HandPoser,
        Animator,
        AutoPoser,
        None
    }
