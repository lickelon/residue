using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ExitController : MonoBehaviour
{
    [SerializeField] private ObservationContamination contamination;
    [SerializeField] private PrototypeRunState runState;
    [SerializeField] private int maximumStableStage = 2;
    [SerializeField] private Renderer doorRenderer;
    [SerializeField] private GameObject unstableBlocker;
    [SerializeField] private PrototypeHud hud;
    [SerializeField] private Color stableColor = new(0.25f, 0.75f, 0.55f);
    [SerializeField] private Color unstableColor = new(0.75f, 0.2f, 0.25f);

    private bool completed;

    public bool IsStable => contamination != null && contamination.Stage <= maximumStableStage && !contamination.IsOverloaded;
    public bool CanExit => IsStable && runState != null && runState.HasAllAnchors;

    private void Awake()
    {
        if (contamination == null)
        {
            contamination = FindFirstObjectByType<ObservationContamination>();
        }

        if (hud == null)
        {
            hud = FindFirstObjectByType<PrototypeHud>();
        }

        if (runState == null)
        {
            runState = FindFirstObjectByType<PrototypeRunState>();
        }

        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void Update()
    {
        bool stable = CanExit;

        if (doorRenderer != null)
        {
            doorRenderer.material.color = stable ? stableColor : unstableColor;
        }

        if (unstableBlocker != null)
        {
            unstableBlocker.SetActive(!stable);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (completed || !other.TryGetComponent(out FirstPersonController _))
        {
            return;
        }

        if (CanExit)
        {
            completed = true;
            hud?.ShowMessage("탈출 성공: 필요한 것만 확인했고, 현실이 버텼다.");
        }
        else if (runState != null && !runState.HasAllAnchors)
        {
            hud?.ShowMessage("출구가 잠겨 있다. 안정화 앵커를 모두 회수해야 한다.");
        }
        else
        {
            hud?.ShowMessage("출구가 흔들린다. 너무 많이 확인했다.");
        }
    }
}
