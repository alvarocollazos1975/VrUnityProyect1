using UnityEngine;

namespace BIMOS
{
    public class PhysicsHand : MonoBehaviour
    {
        [Header("Target References")]
        public Transform Target;          // Objeto objetivo (controlador VR)
        public Transform Controller;      // Transform del controlador VR

        [Header("Offsets")]
        public Vector3 TargetOffsetPosition;      // Offset de posición
        public Quaternion TargetOffsetRotation;   // Offset de rotación

        private ConfigurableJoint _handJoint;     // ConfigurableJoint para la mano física
        private Rigidbody _rigidbody;             // Rigidbody de la mano física

        private void Awake()
        {
            // Obtener referencias
            _handJoint = GetComponent<ConfigurableJoint>();
            _rigidbody = GetComponent<Rigidbody>();

            // Configurar las iteraciones del solver para mayor precisión
            _rigidbody.solverIterations = 60;
            _rigidbody.solverVelocityIterations = 10;

            // Inicializar la rotación del offset
            TargetOffsetRotation = Quaternion.identity;

            // Configurar el ConfigurableJoint
            //SetupJoint();
        }

        private void FixedUpdate()
        {
            // 1. Sincronizar la posición
            Vector3 targetPosition = Target.TransformPoint(TargetOffsetPosition);
            Vector3 localPositionOffset = targetPosition - transform.position;

            // Aplicar la posición relativa al ConfigurableJoint
            _handJoint.targetPosition = localPositionOffset;

            // 2. Sincronizar la rotación
            Quaternion targetRotation = Target.rotation * TargetOffsetRotation;

            // Configurar la rotación en el ConfigurableJoint
            _handJoint.targetRotation = Quaternion.Inverse(targetRotation) * transform.rotation;
        }

        /// <summary>
        /// Configura las propiedades del ConfigurableJoint.
        /// </summary>
        private void SetupJoint()
        {
            // Configuración del Joint Drive para posición
            JointDrive positionDrive = new JointDrive
            {
                positionSpring = 5000f,  // Fuerza para mover la mano
                positionDamper = 100f,   // Amortiguación
                maximumForce = Mathf.Infinity
            };

            _handJoint.xDrive = positionDrive;
            _handJoint.yDrive = positionDrive;
            _handJoint.zDrive = positionDrive;

            // Configuración del Joint Drive para rotación
            JointDrive rotationDrive = new JointDrive
            {
                positionSpring = 5000f,  // Fuerza para rotar la mano
                positionDamper = 100f,
                maximumForce = Mathf.Infinity
            };

            _handJoint.angularXDrive = rotationDrive;
            _handJoint.angularYZDrive = rotationDrive;

            // Bloquear otras configuraciones no necesarias
            _handJoint.rotationDriveMode = RotationDriveMode.Slerp;
        }
    }
}