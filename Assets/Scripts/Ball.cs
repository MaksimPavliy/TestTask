using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Ball : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform projectileStartTransform;
    [SerializeField] private GameObject projectileBall;
    [SerializeField] private float maxChargeTime = 10f;
    [SerializeField] private float minScaleValue = 0.25f;
    [SerializeField] private float minShotDelay = 2f;
    [SerializeField] private float projectileScaleSmoothed = 1.175f;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float areaToClearLength;

    public static UnityAction OnJumpStarted;
    public static UnityAction<Vector3> OnJumpFinished;
    public static UnityAction<Vector3> OnShot;

    private bool canShoot = true;
    private Vector3 minBallScale;
    private GameObject areaToClear;
    private Collider[] colliders;
    private Sequence jumpSequence;

    void Start()
    {
        BallTarget.OnPortalOpened += JumpInPortal;
        areaToClear = new GameObject();
        areaToClear.AddComponent<BoxCollider>();
        Projectile.OnExplosion += CheckIfAreaCleared;
        transform.LookAt(targetTransform);
        minBallScale = transform.localScale * minScaleValue;
    }

    private void Update()
    {
        if (canShoot && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Shoot());
        }
    }

    private IEnumerator Shoot()
    {
        canShoot = false;
        float timer = 0;
        float minScaleLose = 0.075f;
        float maxScaleLose = 0.85f;
        float projectileScaleMultiplier = 1;
        Vector3 ballScaleBeforeCharging = transform.localScale;
        Vector3 ballScaleAfterMinLose = transform.localScale - transform.localScale * minScaleLose;

        GameObject projectile = Instantiate(projectileBall, projectileStartTransform.position, Quaternion.identity);

        while (Input.GetMouseButton(0))
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();

            float ballScaleDecrease = Mathf.Lerp(0, maxScaleLose, timer / maxChargeTime);
            projectileScaleMultiplier = Mathf.Lerp(3, 1.2f, timer / maxChargeTime / projectileScaleSmoothed);

            if (transform.localScale.magnitude <= minBallScale.magnitude)
            {
                GameManager.instance.LoseGame();
                break;
            }

            transform.localScale = ballScaleBeforeCharging - ballScaleDecrease * ballScaleBeforeCharging;

            if (!projectile)
            {
                break;
            }
            projectile.transform.localScale = ballScaleDecrease * ballScaleBeforeCharging * projectileScaleMultiplier;
        }

        if (projectile && transform.localScale.magnitude > ballScaleAfterMinLose.magnitude)
        {
            float delayLeft = minShotDelay - timer;
            transform.DOScale(ballScaleAfterMinLose, delayLeft);
            projectile.transform.DOScale(ballScaleBeforeCharging * minScaleLose * projectileScaleMultiplier, delayLeft);
            yield return new WaitForSeconds(delayLeft);
        }
        OnShot?.Invoke((targetTransform.position - transform.position).normalized);
    }

    private void CheckIfAreaCleared()
    {
        Vector3 lineStartPoint = lineRenderer.GetComponent<LineRenderer>().GetPosition(0);
        Vector3 areaToClearEndPoint = lineStartPoint + areaToClearLength * (targetTransform.position - lineStartPoint).normalized;
        areaToClearEndPoint.y = 0;

        Vector3 areaCenter = (areaToClearEndPoint + lineStartPoint) / 2;
        areaCenter.y = transform.position.y;
        float areaToClearWidth = lineRenderer.startWidth;
        Vector3 directionToTarget = targetTransform.position - areaCenter;
        Quaternion rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        areaToClear.transform.position = areaCenter;
        areaToClear.transform.rotation = rotation;
        BoxCollider boxCollider = areaToClear.GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(areaToClearWidth, 1f, areaToClearLength);
        boxCollider.isTrigger = true;

        colliders = Physics.OverlapBox(areaCenter, new Vector3(areaToClearWidth / 2f, 8f, areaToClearLength / 2f), rotation);

        foreach (Collider collider in colliders)
        {
            if (collider.tag == "Obstacle")
            {
                canShoot = true;
                return;
            }
        }
        StartJumping(areaCenter);
    }

    private void StartJumping(Vector3 targetPos)
    {
        if (GameManager.instance.isPlaying)
        {
            OnJumpStarted?.Invoke();

            jumpSequence = DOTween.Sequence();
            jumpSequence.AppendInterval(1f);
            jumpSequence.Append(transform.DOJump(targetPos, 4f, 3, 1.2f).SetEase(Ease.Linear));
            jumpSequence.AppendCallback(() => OnJumpFinished?.Invoke(transform.position))
                .AppendCallback(() => CheckIfAreaCleared());

            jumpSequence.Play();
        }
    }

    private void JumpInPortal(float delay)
    {
        foreach (var collider in colliders)
        {
            if (collider && collider.tag == "Obstacle")
            {
                return;
            }
        }

        GameManager.instance.isPlaying = false;
        DOTween.Kill(jumpSequence);
        OnJumpStarted?.Invoke();
        jumpSequence = DOTween.Sequence();
        jumpSequence.AppendInterval(delay);

        float distanceToPortal = Vector3.Distance(transform.position, targetTransform.position);
        Vector3 jumpTargetPos = transform.position + 2 * distanceToPortal * (targetTransform.position - transform.position).normalized;
        jumpTargetPos.y = transform.position.y;

        jumpSequence.Append(transform.DOJump(jumpTargetPos, 7.75f, 1, 1.25f).SetEase(Ease.Linear));
        jumpSequence.AppendCallback(() => GameManager.instance.WinGame());
        jumpSequence.Play();
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
        Projectile.OnExplosion -= CheckIfAreaCleared;
        BallTarget.OnPortalOpened -= JumpInPortal;
    }
}

