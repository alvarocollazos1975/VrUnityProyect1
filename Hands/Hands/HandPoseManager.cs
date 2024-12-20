using UnityEngine;

public class HandPoseManager : MonoBehaviour
{
    [SerializeField] private GameObject handPosePrefab; // Prefab de la mano con la pose
    [HideInInspector] public GameObject leftHandPose;   // Pose de la mano izquierda
    [HideInInspector] public GameObject rightHandPose;  // Pose de la mano derecha (espejo)

    // Método para crear la pose de la mano izquierda
    public void CreateLeftHandPose()
    {
        if (handPosePrefab == null)
        {
            Debug.LogWarning("No se ha asignado el prefab de la pose de la mano.");
            return;
        }

        // Instanciar la pose de la mano izquierda sin padre y configurar la escala correctamente
        rightHandPose = Instantiate(handPosePrefab);
        rightHandPose.name = "rightHandPose";
        
        // Configurar posición, rotación y escala sin distorsionar
        rightHandPose.transform.SetParent(transform, false); // Mantener las transformaciones locales
        rightHandPose.transform.localPosition = Vector3.zero;
        rightHandPose.transform.localRotation = Quaternion.identity;
        rightHandPose.transform.localScale = Vector3.one;

        Debug.Log("Pose de la mano Derecha creada.");
    }

    // Método para crear la pose espejo de la mano derecha
    public void CreateRightHandMirrorPose()
    {
        if (rightHandPose == null)
        {
            Debug.LogWarning("Primero debes crear la pose de la mano Derecha.");
            return;
        }

        // Instanciar la pose de la mano derecha y reflejarla sin cambiar escala
        leftHandPose = Instantiate(handPosePrefab);
        leftHandPose.name = "leftHandPose";
        
        // Ajustar posición, rotación y escala para evitar distorsiones
        leftHandPose.transform.SetParent(transform, false);
        leftHandPose.transform.localPosition = rightHandPose.transform.localPosition;
        leftHandPose.transform.localRotation = rightHandPose.transform.localRotation;
        leftHandPose.transform.localScale = new Vector3(-1, 1, 1); // Reflejar en el eje X

        Debug.Log("Pose espejo de la mano Izquierda creada.");
    }

    // Método para borrar las poses si es necesario
    public void ClearHandPoses()
    {
        if (leftHandPose != null) DestroyImmediate(leftHandPose);
        if (rightHandPose != null) DestroyImmediate(rightHandPose);
        
        Debug.Log("Poses de manos eliminadas.");
    }
}
