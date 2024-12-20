using UnityEngine;

[System.Serializable]
public class HandPoseotro
{
    public Transform[] fingerBones;
    public Vector3[] fingerPositions;
    public Quaternion[] fingerRotations;

    public HandPoseotro(Transform[] bones)
    {
        fingerBones = bones;
        InitializePoseArrays();
    }

    public void InitializePoseArrays()
    {
        if (fingerBones != null)
        {
            fingerPositions = new Vector3[fingerBones.Length];
            fingerRotations = new Quaternion[fingerBones.Length];
        }
    }

    public void CapturePose()
    {
        if (fingerBones == null || fingerPositions == null || fingerRotations == null)
        {
            Debug.LogError("Finger bones or pose arrays are not initialized.");
            return;
        }

        for (int i = 0; i < fingerBones.Length; i++)
        {
            fingerPositions[i] = fingerBones[i].localPosition;
            fingerRotations[i] = fingerBones[i].localRotation;
        }
    }

    public void ApplyPose()
    {
        if (fingerBones == null || fingerPositions == null || fingerRotations == null)
        {
            Debug.LogError("Finger bones or pose arrays are not initialized.");
            return;
        }

        for (int i = 0; i < fingerBones.Length; i++)
        {
            fingerBones[i].localPosition = fingerPositions[i];
            fingerBones[i].localRotation = fingerRotations[i];
        }
    }
}