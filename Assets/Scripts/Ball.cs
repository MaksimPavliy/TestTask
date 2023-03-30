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
    [SerializeField] private float maxChargeTime = 5f;
    [SerializeField] private int shotsRequiredToWin;
    [SerializeField] private float minScaleValue = 0.25f;
    [SerializeField] private float minShotDelay = 1f;
    [SerializeField] private float projectileScaleMultiplier = 2f;

    public static UnityAction OnJumpStarted;
    public static UnityAction OnJumpFinished;
    public static UnityAction<Vector3> OnShot;

    private bool canShoot = true;
    private Vector3 singleMoveDistance;
    private Vector3 minBallScale;

    void Start()
    {
        transform.LookAt(targetTransform);
        Projectile.OnExplosion += StartJumping;
        minBallScale = transform.localScale * minScaleValue;

        singleMoveDistance = (targetTransform.position - transform.position) / shotsRequiredToWin;
        singleMoveDistance.y = 0;
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
        float minScaleLose = 0.05f;
        float maxScaleLose = 0.2f;
        Vector3 ballScaleBeforeCharging = transform.localScale;
        Vector3 ballScaleAfterMinLose = transform.localScale - transform.localScale * minScaleLose;

        GameObject projectile = Instantiate(projectileBall, projectileStartTransform.position, Quaternion.identity);

        while (Input.GetMouseButton(0))
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();

            float ballScaleDecrease = Mathf.Lerp(0, maxScaleLose, timer / maxChargeTime);

            if (transform.localScale.magnitude <= minBallScale.magnitude)
            {
                Debug.Log("wtf");
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
            transform.DOScale(ballScaleAfterMinLose, minShotDelay);
            projectile.transform.DOScale(ballScaleBeforeCharging * minScaleLose * projectileScaleMultiplier, minShotDelay);
            yield return new WaitForSeconds(minShotDelay);
        }
        OnShot?.Invoke((targetTransform.position - transform.position).normalized);
    }

    private void StartJumping(Vector3 explosionPosition)
    {
        OnJumpStarted?.Invoke();
        Sequence jumpSequence = DOTween.Sequence();

        Vector3 newPosition = transform.position + singleMoveDistance;

        jumpSequence.AppendInterval(1f);
        jumpSequence.Append(transform.DOJump(newPosition, 4f, 3, 1.2f).SetEase(Ease.Linear));
        jumpSequence.AppendCallback(() => OnJumpFinished?.Invoke())
        .AppendCallback(() => CheckForWin());

        jumpSequence.Play();

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            Debug.Log(collision.gameObject);
            Destroy(gameObject);
        }
    }

    private void CheckForWin()
    {
        Debug.Log(targetTransform.position.x - transform.position.x - transform.position.y);

        if (targetTransform.position.x - transform.position.x - transform.position.y < 5)
        {
            /*transform.DOJump(targetTransform.position, 7f, 2, 1f);*/
        }
        else
        {
            canShoot = true;
        }
    }

    private void OnDestroy()
    {
        Projectile.OnExplosion -= StartJumping;
        DOTween.KillAll();
    }
}

