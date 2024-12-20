using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestearPose : MonoBehaviour
{
    public Transform[] handJoints;
    public HandPose openPose;
    public HandPose grabPose;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CurrentPose(HandPose targetPose)
    {
        if (targetPose == null || handJoints == null || handJoints.Length == 0)
        {
            Debug.LogWarning("Target pose or hand joints are not assigned correctly.");
            return;
        }

        for (int i = 0; i < handJoints.Length; i++)
        {
            // handJoints[i].localPosition = targetPose.positions[i];
            // handJoints[i].localRotation = targetPose.rotations[i];
        }
    }
}
