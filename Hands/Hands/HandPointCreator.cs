using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPointCreator : MonoBehaviour
{
    [SerializeField] private GameObject attachPointPrefab;
    [SerializeField] private Transform Wrist;

    public void CreateAttachPoint()
    {
        var point = Instantiate(attachPointPrefab, this.transform);
        point.transform.localPosition = this.transform.InverseTransformPoint(this.Wrist.position);
    }

    [ContextMenu("Create Attach Point")]
    private void CreateAttachPointFromContextMenu()
    {
        CreateAttachPoint();
    }
}