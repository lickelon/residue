using UnityEngine;
using UnityEngine.InputSystem;

public sealed class SimpleDoor : MonoBehaviour
{
    [SerializeField] private Transform hinge;
    [SerializeField] private Transform watchTarget;
    [SerializeField] private Renderer doorRenderer;
    [SerializeField] private ObservationContamination contamination;
    [SerializeField] private Camera viewCamera;
    [SerializeField] private float openAngle = -92f;
    [SerializeField] private float openSpeed = 68f;
    [SerializeField] private float closeSpeed = 105f;
    [SerializeField] private float openHoldDuration = 2.2f;
    [SerializeField] private float closeWatchDuration = 1.4f;
    [SerializeField] private float observationPenaltyCooldown = 1.2f;
    [SerializeField] private Color idleColor = new(0.34f, 0.36f, 0.37f);
    [SerializeField] private Color resistingColor = new(0.72f, 0.26f, 0.22f);
    [SerializeField] private Color openingColor = new(0.26f, 0.58f, 0.52f);
    [SerializeField] private PrototypeHud hud;

    private bool playerNear;
    private bool openingRequested;
    private float currentAngle;
    private float lastPenaltyAt = -999f;
    private float holdOpenUntil = -999f;
    private float watchedOpenTime;
    private bool fullyOpened;

    private void Awake()
    {
        if (hinge == null)
        {
            hinge = transform;
        }

        if (hud == null)
        {
            hud = FindFirstObjectByType<PrototypeHud>();
        }

        if (contamination == null)
        {
            contamination = FindFirstObjectByType<ObservationContamination>();
        }

        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }

        if (doorRenderer == null)
        {
            doorRenderer = hinge.GetComponentInChildren<Renderer>();
        }

        if (watchTarget == null)
        {
            watchTarget = doorRenderer == null ? hinge : doorRenderer.transform;
        }
    }

    private void Update()
    {
        if (playerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            openingRequested = !openingRequested;
            if (!openingRequested)
            {
                holdOpenUntil = -999f;
                watchedOpenTime = 0f;
                fullyOpened = false;
            }

            hud?.ShowMessage(openingRequested ? "문은 방 안쪽으로 열린다. 열린 문을 오래 보면 닫힌다." : "문을 닫았다.");
        }

        bool heldOpen = Time.time < holdOpenUntil;
        bool watched = openingRequested && fullyOpened && !heldOpen && IsWatched();
        watchedOpenTime = watched ? watchedOpenTime + Time.deltaTime : 0f;
        bool closingFromWatch = watchedOpenTime >= closeWatchDuration;
        float targetAngle = openingRequested && !closingFromWatch ? openAngle : 0f;
        float speed = closingFromWatch ? closeSpeed : (openingRequested ? openSpeed : closeSpeed);
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, speed * Time.deltaTime);
        hinge.localRotation = Quaternion.Euler(0f, currentAngle, 0f);

        if (openingRequested && !fullyOpened && Mathf.Approximately(currentAngle, openAngle))
        {
            fullyOpened = true;
            holdOpenUntil = Time.time + openHoldDuration;
            hud?.ShowMessage("문이 안쪽으로 밀려 열렸다. 지금 통과하라.");
        }

        if (closingFromWatch && Mathf.Approximately(currentAngle, 0f))
        {
            openingRequested = false;
            watchedOpenTime = 0f;
            fullyOpened = false;
            hud?.ShowMessage("문이 시선을 견디지 못하고 닫혔다.");
        }

        if (watched && Time.time - lastPenaltyAt >= observationPenaltyCooldown)
        {
            contamination?.Add(5f, ContaminationCause.LongObservation);
            hud?.ShowMessage("문이 시선을 느낀다. 계속 보면 닫힌다.");
            lastPenaltyAt = Time.time;
        }

        UpdateColor(watched);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out FirstPersonController _))
        {
            return;
        }

        playerNear = true;
        hud?.ShowMessage("E: 문 깨우기. 열린 문을 오래 보지 마라.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out FirstPersonController _))
        {
            return;
        }

        playerNear = false;
    }

    private bool IsWatched()
    {
        if (viewCamera == null)
        {
            return false;
        }

        Vector3 targetPosition = watchTarget == null ? transform.position : watchTarget.position;
        Vector3 viewport = viewCamera.WorldToViewportPoint(targetPosition);
        return viewport.z > 0f && viewport.x > 0.12f && viewport.x < 0.88f && viewport.y > 0.12f && viewport.y < 0.88f;
    }

    private void UpdateColor(bool watched)
    {
        if (doorRenderer == null)
        {
            return;
        }

        Color target = watched ? resistingColor : (openingRequested ? openingColor : idleColor);
        doorRenderer.material.color = Color.Lerp(doorRenderer.material.color, target, 10f * Time.deltaTime);
    }
}
