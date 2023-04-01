using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallTarget : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float distanceOffset;
    [SerializeField] private float openDelay = 2.25f;

    public static UnityAction<float> OnPortalOpened;

    void Start()
    {
        Ball.OnJumpFinished += TryToOpenPortal;
    }

    private void TryToOpenPortal(Vector3 ballPos)
    {
        if (Vector2.Distance(transform.position, ballPos) - distanceOffset < 5)
        {
            animator.SetBool("isOpened", true);
            OnPortalOpened?.Invoke(openDelay);
        }
    }

    private void OnDestroy()
    {
        Ball.OnJumpFinished -= TryToOpenPortal;
    }
}
