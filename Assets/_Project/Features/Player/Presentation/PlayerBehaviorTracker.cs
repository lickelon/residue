using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerBehaviorTracker : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField] private ObservationContamination contamination;
    [SerializeField] private LayerMask observableMask = ~0;
    [SerializeField] private float lookDistance = 8f;
    [SerializeField] private float turnWindow = 0.9f;
    [SerializeField] private float turnAroundDegrees = 155f;
    [SerializeField] private float fastLookDegreesPerSecond = 300f;
    [SerializeField] private float eventCooldown = 1.6f;
    [SerializeField] private float longLookThreshold = 2.2f;
    [SerializeField] private float longLookInterval = 1.15f;
    [SerializeField] private float repeatMinimumGap = 0.35f;
    [SerializeField] private float repeatMaximumGap = 6f;

    private readonly Dictionary<ObservableTarget, float> lastReleasedAt = new();
    private ObservableTarget currentTarget;
    private float currentTargetTime;
    private float nextLongLookAt;
    private float previousYaw;
    private float accumulatedYaw;
    private float turnWindowStartedAt;
    private float lastTurnAroundAt = -999f;
    private float lastFastLookAt = -999f;

    private void Awake()
    {
        if (viewCamera == null)
        {
            viewCamera = GetComponentInChildren<Camera>();
        }

        if (contamination == null)
        {
            contamination = FindFirstObjectByType<ObservationContamination>();
        }

        previousYaw = transform.eulerAngles.y;
        turnWindowStartedAt = Time.time;
    }

    private void Update()
    {
        TrackRotation();
        TrackObservation();
    }

    private void TrackRotation()
    {
        float yaw = transform.eulerAngles.y;
        float delta = Mathf.DeltaAngle(previousYaw, yaw);
        float absDelta = Mathf.Abs(delta);
        float speed = Time.deltaTime > 0f ? absDelta / Time.deltaTime : 0f;

        if (Time.time - turnWindowStartedAt > turnWindow || Mathf.Sign(accumulatedYaw) != Mathf.Sign(delta) && Mathf.Abs(accumulatedYaw) > 8f)
        {
            accumulatedYaw = 0f;
            turnWindowStartedAt = Time.time;
        }

        accumulatedYaw += delta;

        if (Mathf.Abs(accumulatedYaw) >= turnAroundDegrees && Time.time - lastTurnAroundAt >= eventCooldown)
        {
            contamination?.Add(13f, ContaminationCause.TurnAround);
            lastTurnAroundAt = Time.time;
            accumulatedYaw = 0f;
            turnWindowStartedAt = Time.time;
        }
        else if (speed >= fastLookDegreesPerSecond && Time.time - lastFastLookAt >= eventCooldown)
        {
            contamination?.Add(3f, ContaminationCause.FastLook);
            lastFastLookAt = Time.time;
        }

        previousYaw = yaw;
    }

    private void TrackObservation()
    {
        ObservableTarget observed = RaycastObservable();
        if (observed != currentTarget)
        {
            ReleaseCurrentTarget();
            AcquireTarget(observed);
        }

        if (currentTarget == null)
        {
            return;
        }

        currentTargetTime += Time.deltaTime;
        if (currentTargetTime >= longLookThreshold && Time.time >= nextLongLookAt)
        {
            contamination?.Add(6f, ContaminationCause.LongObservation);
            nextLongLookAt = Time.time + longLookInterval;
        }
    }

    private ObservableTarget RaycastObservable()
    {
        if (viewCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, lookDistance, observableMask, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        return hit.collider.GetComponentInParent<ObservableTarget>();
    }

    private void ReleaseCurrentTarget()
    {
        if (currentTarget == null)
        {
            return;
        }

        lastReleasedAt[currentTarget] = Time.time;
        currentTarget = null;
        currentTargetTime = 0f;
    }

    private void AcquireTarget(ObservableTarget target)
    {
        currentTarget = target;
        currentTargetTime = 0f;
        nextLongLookAt = Time.time + longLookThreshold;

        if (target == null || !lastReleasedAt.TryGetValue(target, out float releasedAt))
        {
            return;
        }

        float gap = Time.time - releasedAt;
        if (gap >= repeatMinimumGap && gap <= repeatMaximumGap)
        {
            contamination?.Add(8f, ContaminationCause.RepeatCheck);
        }
    }
}
