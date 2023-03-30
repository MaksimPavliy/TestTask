using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float followSpeed;
    [SerializeField] private float scaleZoomMultiplier;

    private Vector3 offset;
    private Vector3 desiredPos;
    private Vector3 targetStartingScale;

    private void Start()
    {
        offset = followTarget.position - transform.position;
        targetStartingScale = followTarget.transform.localScale;
    }

    void LateUpdate()
    {
        if (followTarget)
        {
            float scaleOffset = followTarget.transform.localScale.magnitude / targetStartingScale.magnitude;

            desiredPos = followTarget.position - offset + (1 - scaleOffset) * scaleZoomMultiplier * Vector3.one;
            desiredPos.y = transform.position.y;
            Vector3 followPos = Vector3.Lerp(transform.position, desiredPos, followSpeed);
            transform.position = followPos;
        }
    }
}
