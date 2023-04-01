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
    [SerializeField] private float projectileScaleSmoothValue = 1.175f;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float areaToClearLength;
    [SerializeField] private float jumpDuration;
    [SerializeField] float projectileMaxScaleMultiplier = 3f;
    [SerializeField] float projectileMinScaleMultiplier = 1.2f;

    public static UnityAction OnJumpStarted;
    public static UnityAction<Vector3> OnJumpFinished;
    public static UnityAction<Vector3> OnShot;

    private bool canShoot = true;
    private Vector3 minBallScale;
    private GameObject areaToClear;

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
        float minScaleLose = 0.1f;
        float maxScaleLose = 0.85f;
        float projectileScaleMultiplier = 0;

        Vector3 ballScaleBeforeCharging = transform.localScale;
        Vector3 ballScaleAfterMinLose = transform.localScale - transform.localScale * minScaleLose;

        GameObject projectile = Instantiate(projectileBall, projectileStartTransform.position, Quaternion.identity);

        while (Input.GetMouseButton(0))
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();

            float ballScaleDecrease = Mathf.Lerp(0, maxScaleLose, timer / maxChargeTime);
            projectileScaleMultiplier = Mathf.Lerp(projectileMaxScaleMultiplier, projectileMinScaleMultiplier, timer / maxChargeTime / projectileScaleSmoothValue);

            if (transform.localScale.magnitude <= minBallScale.magnitude)
            {
                GameManager.instance.LoseGame();
                break;
            }

            transform.localScale = ballScaleBeforeCharging - ballScaleDecrease * ballScaleBeforeCharging;

            if (projectile)
            {
                projectile.transform.localScale = ballScaleDecrease * ballScaleBeforeCharging * projectileScaleMultiplier;
            }        
        }

        if (projectile && transform.localScale.magnitude > ballScaleAfterMinLose.magnitude)
        {
            float shotDelay = minShotDelay - timer;
            transform.DOScale(ballScaleAfterMinLose, shotDelay);
            projectile.transform.DOScale(ballScaleBeforeCharging * minScaleLose * projectileScaleMultiplier, shotDelay);
            yield return new WaitForSeconds(shotDelay);
        }
        OnShot?.Invoke((targetTransform.position - transform.position).normalized);
    }

    private void CheckIfAreaCleared(float delay)
    {
        Vector3 lineStartPoint = transform.position;
        Vector3 areaToClearEndPoint = lineStartPoint + areaToClearLength * (targetTransform.position - lineStartPoint).normalized;
        areaToClearEndPoint.y = 0;

        Vector3 areaCenter = (areaToClearEndPoint + lineStartPoint) / 2;
        areaCenter.y = transform.position.y;
        float areaToClearWidth = lineRenderer.startWidth;
        Vector3 directionToTarget = targetTransform.position - areaCenter;
        Quaternion rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        areaToClear.transform.position = areaCenter;
        areaToClear.transform.rotation = rotation;

        //Creates collider to see overlap area in gizmos 
        BoxCollider boxCollider = areaToClear.GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(areaToClearWidth, 1f, areaToClearLength);
        boxCollider.isTrigger = true;

        Collider[] colliders = Physics.OverlapBox(areaCenter, new Vector3(areaToClearWidth / 2f, 10f, areaToClearLength / 2f), rotation);

        foreach (Collider collider in colliders)
        {
            if (collider.tag == "Obstacle")
            {
                canShoot = true;
                return;
            }
        }
        StartJumping((areaCenter + transform.position) / 2, delay);
    }

    private void StartJumping(Vector3 targetPos, float delay)
    {
        float jumpPower = 4f;
        int numOfJumps = 2;

        if (GameManager.instance.isPlaying)
        {
            OnJumpStarted?.Invoke();

            Sequence jumpSequence = DOTween.Sequence();
            jumpSequence.AppendInterval(delay);
            jumpSequence.Append(transform.DOJump(targetPos, jumpPower, numOfJumps, jumpDuration).SetEase(Ease.Linear));
            jumpSequence.AppendCallback(() => OnJumpFinished?.Invoke(transform.position))
                .AppendCallback(() => CheckIfAreaCleared(0));

            jumpSequence.Play();
        }
    }

    private void JumpInPortal(float delay)
    {
        float portalJumpPower = 8.75f;
        int numOfJumps = 1;
        float jumpDuration = 1.25f;
        float showWinScreenDelay = 1f;

        GameManager.instance.isPlaying = false;
        OnJumpStarted?.Invoke();
        Sequence jumpSequence = DOTween.Sequence();
        jumpSequence.AppendInterval(delay);

        float distanceToPortal = Vector3.Distance(transform.position, targetTransform.position);
        Vector3 jumpTargetPos = transform.position + 2 * distanceToPortal * (targetTransform.position - transform.position).normalized;
        jumpTargetPos.y = transform.position.y;

        jumpSequence.Append(transform.DOJump(jumpTargetPos, portalJumpPower, numOfJumps, jumpDuration).SetEase(Ease.Linear));
        jumpSequence.AppendInterval(showWinScreenDelay);
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

