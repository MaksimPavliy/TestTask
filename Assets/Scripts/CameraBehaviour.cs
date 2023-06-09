using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float followSpeed;
    [SerializeField] private float scaleZoomMultiplier;
    [SerializeField] private Transform portal;
    [SerializeField] private Vector3 endPos;
    [SerializeField] private Quaternion endRotation;

    private Vector3 offset;
    private Vector3 desiredPos;
    private Vector3 targetStartingScale;

    private void Start()
    {
        offset = followTarget.position - transform.position;
        targetStartingScale = followTarget.transform.localScale;
    }

    private void LateUpdate()
    {
        if (followTarget && GameManager.instance.isPlaying)
        {
            float scaleOffset = followTarget.transform.localScale.magnitude / targetStartingScale.magnitude;

            desiredPos = followTarget.position - offset + (1 - scaleOffset) * scaleZoomMultiplier * Vector3.one;
            desiredPos.y = transform.position.y;
            Vector3 followPos = Vector3.Lerp(transform.position, desiredPos, followSpeed);
            transform.position = followPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, endPos, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, followSpeed * Time.deltaTime);
        }
    }
}
