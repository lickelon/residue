using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public sealed class StabilityAnchor : MonoBehaviour
{
    [SerializeField] private PrototypeRunState runState;
    [SerializeField] private PrototypeHud hud;
    [SerializeField] private float stabilizeAmount = 24f;
    [SerializeField] private Renderer anchorRenderer;
    [SerializeField] private Light anchorLight;
    [SerializeField] private float pulseSpeed = 4.5f;
    [SerializeField] private float pulseScale = 0.16f;

    private bool playerNear;
    private bool collected;
    private Vector3 baseScale;

    public float StabilizeAmount => stabilizeAmount;

    private void Awake()
    {
        if (runState == null)
        {
            runState = FindFirstObjectByType<PrototypeRunState>();
        }

        if (hud == null)
        {
            hud = FindFirstObjectByType<PrototypeHud>();
        }

        if (anchorRenderer == null)
        {
            anchorRenderer = GetComponentInChildren<Renderer>();
        }

        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (collected)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
        transform.localScale = baseScale * pulse;

        if (anchorLight != null)
        {
            anchorLight.intensity = 1.3f + Mathf.Sin(Time.time * pulseSpeed) * 0.45f;
        }

        if (playerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Collect();
        }
    }

    private void Collect()
    {
        collected = true;
        runState?.CollectAnchor(this);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out FirstPersonController _))
        {
            return;
        }

        playerNear = true;
        hud?.ShowMessage("E: 안정화 앵커 회수. 회수하면 오염이 낮아진다.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out FirstPersonController _))
        {
            return;
        }

        playerNear = false;
    }
}
