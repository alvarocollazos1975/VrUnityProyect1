using System.Collections;
using System.Collections.Generic;
using BIMOS;
using UnityEngine;

 public class HandPhysics : MonoBehaviour {

        /// <summary>
        /// This is the object our physical hand should try to follow / match
        /// </summary>
        [Tooltip("This is the object our physical hand should try to follow / match. Should typically be an object on the controller Transform")]
        public Transform AttachTo;

        [Tooltip("Amount of Velocity to apply to hands when trying to reach anchor point")]
        public float HandVelocity = 1500f;

         [SerializeField]
        private Hand _hand;

        [Tooltip("If true, Hand COlliders will be disabled while grabbing an object")]
        public bool DisableHandCollidersOnGrab = true;

        [Tooltip("If the hand exceeds this distance from it's origin it will snap back to the original position. Specified in meters.")]
        public float SnapBackDistance = 1f;

       

        [Tooltip("Assign Hand Colliders this material if provided")]
        public PhysicsMaterial ColliderMaterial;

        public Transform HandModel;
        public Transform HandModelOffset;

       

        // Colliders that live in the hand model
        List<Collider> handColliders;
        Rigidbody rigid;
        ConfigurableJoint configJoint;
      

        Vector3 localHandOffset;
        Vector3 localHandOffsetRotation;

        bool wasHoldingObject = false;

        void Start() {

            rigid = GetComponent<Rigidbody>();
            configJoint = GetComponent<ConfigurableJoint>();
            

            // Create Attach Point based on current position and rotation
            if(AttachTo == null) {
                AttachTo = new GameObject("AttachToTransform").transform;
            }
            
            AttachTo.parent = transform.parent;
            AttachTo.SetPositionAndRotation(transform.position, transform.rotation);

            // Connect config joint to our AttachPoint's Rigidbody
            Rigidbody attachRB = AttachTo.gameObject.AddComponent<Rigidbody>();
            attachRB.useGravity = false;
            attachRB.isKinematic = true;
            attachRB.constraints = RigidbodyConstraints.FreezeAll;
             configJoint.connectedBody = attachRB;
            Destroy(configJoint);

            localHandOffset = HandModel.localPosition;
            localHandOffsetRotation = HandModel.localEulerAngles;

           

           

            

            _priorParent = transform.parent;
            // Physics Hands typically want to have no parent at all
            transform.parent = null;
        }
       

        void Update() {
           
           
           
            // Our root object is disabled
            if (!AttachTo.gameObject.activeSelf) {
                transform.parent = AttachTo;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                return;
            }

            // If we are holding something, move the hands in Update, ignoring physics. 
          
        }

        

        void FixedUpdate() {

                // Move using Velocity
                Vector3 positionDelta = AttachTo.position - transform.position;
                rigid.linearVelocity = Vector3.MoveTowards(rigid.linearVelocity, (positionDelta * HandVelocity) * Time.fixedDeltaTime, 5f);

                // Rotate using angular velocity
                float angle;
                Vector3 axis;
                Quaternion rotationDelta = AttachTo.rotation * Quaternion.Inverse(transform.rotation);
                rotationDelta.ToAngleAxis(out angle, out axis);

                // Fix rotation angle
                if (angle > 180) {
                    angle -= 360;
                }

                if (angle != 0) {
                    Vector3 angularTarget = angle * axis;
                    angularTarget = (angularTarget * 60f) * Time.fixedDeltaTime;
                    rigid.angularVelocity = Vector3.MoveTowards(rigid.angularVelocity, angularTarget, 20f);
                }
      
       

        
        }


       

       



        Transform _priorParent;

        public virtual void LockLocalPosition() {
            _priorParent = transform.parent;
            transform.parent = AttachTo;
        }

        public virtual void UnlockLocalPosition() {
            transform.parent = _priorParent;
        }

       
        void OnEnable() {
           

            PlayerRotation.OnBeforeRotate += LockLocalPosition;
            PlayerRotation.OnAfterRotate += UnlockLocalPosition;

            SmoothLocomotion.OnBeforeMove += LockOffset;
            SmoothLocomotion.OnAfterMove += UnlockOffset;
        }

        Vector3 _priorLocalOffsetPosition;

        public virtual void LockOffset() {
            _priorLocalOffsetPosition = AttachTo.InverseTransformPoint(transform.position);
        }

        public virtual void UnlockOffset() {
            Vector3 dest = AttachTo.TransformPoint(_priorLocalOffsetPosition);
            float dist = Vector3.Distance(transform.position, dest);
            // Only move if gone far enough
            if (dist > 0.0005f) {
                transform.position = dest;
            }
        }

        void OnDisable() {
           

            PlayerRotation.OnBeforeRotate -= LockLocalPosition;
            PlayerRotation.OnAfterRotate -= UnlockLocalPosition;

            SmoothLocomotion.OnBeforeMove -= LockOffset;
            SmoothLocomotion.OnAfterMove -= UnlockOffset;
        }

      
    }