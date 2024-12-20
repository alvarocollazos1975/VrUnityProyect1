using UnityEngine;

public class CustomHandPose : MonoBehaviour
{
    public HandPoseotro leftHandPose;
    public HandPoseotro rightHandPose;

    public Transform[] leftHandBones;
    public Transform[] rightHandBones;

    private void Start()
    {
        InitializeHandPoses();
    }

    private void InitializeHandPoses()
    {
        if (leftHandBones != null && leftHandBones.Length > 0)
        {
            leftHandPose = new HandPoseotro(leftHandBones);
            leftHandPose.InitializePoseArrays();
        }

        if (rightHandBones != null && rightHandBones.Length > 0)
        {
            rightHandPose = new HandPoseotro(rightHandBones);
            rightHandPose.InitializePoseArrays();
        }
    }

    public void CaptureLeftHandPose()
    {
        if (leftHandPose == null)
        {
            leftHandPose = new HandPoseotro(leftHandBones);
            leftHandPose.InitializePoseArrays();
        }
        leftHandPose.CapturePose();
    }

    public void CaptureRightHandPose()
    {
        if (rightHandPose == null)
        {
            rightHandPose = new HandPoseotro(rightHandBones);
            rightHandPose.InitializePoseArrays();
        }
        rightHandPose.CapturePose();
    }

    public void ApplyLeftHandPose()
    {
        leftHandPose?.ApplyPose();
    }

    public void ApplyRightHandPose()
    {
        rightHandPose?.ApplyPose();
    }
}