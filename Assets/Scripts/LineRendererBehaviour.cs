using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererBehaviour : MonoBehaviour
{
    [SerializeField] private Ball ball;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 lineEndOffset;
    [SerializeField] private float fadelineMultiplier;
    [SerializeField] private float lineWidthSmoothValue;

    private LineRenderer lineRenderer;
    private Vector3 distance;
    private float boundsSizeMultiplier = 0.8f;

    private void Start()
    {
        Ball.OnJumpStarted += DisableLineOnJumping;
        Ball.OnJumpFinished += UpdateLine;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.alignment = LineAlignment.TransformZ;
        transform.localEulerAngles = new Vector3(90, 0, 0);
        lineRenderer.endWidth = target.transform.GetComponent<MeshRenderer>().bounds.size.z * boundsSizeMultiplier;

        UpdateLine(ball.transform.position);
    }

    private void Update()
    {
        if (ball)
        {
            float lineWidth = ball.transform.localScale.magnitude * lineWidthSmoothValue;
            lineRenderer.startWidth = lineWidth;
            Vector3 lineStartPos = new Vector3(ball.transform.position.x, ball.transform.position.y - ball.transform.localScale.y / 2, ball.transform.position.z);
            lineRenderer.SetPosition(0, lineStartPos - distance * ball.transform.localScale.magnitude * fadelineMultiplier);
        }
    }

    private void UpdateLine(Vector3 ballPos)
    {
        distance = (target.transform.position - ballPos).normalized;
        distance.z = -distance.z;
        distance.y = 0;

        lineRenderer.SetPosition(1, target.transform.position - lineEndOffset);
        lineRenderer.enabled = true;
    }

    private void DisableLineOnJumping()
    {
        lineRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        Ball.OnJumpStarted -= DisableLineOnJumping;
        Ball.OnJumpFinished -= UpdateLine;
    }
}
