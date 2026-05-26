using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ExitController : MonoBehaviour
{
    private enum ExitFeedbackState
    {
        MissingAnchors,
        Unstable,
        Ready
    }

    [SerializeField] private ObservationContamination contamination;
    [SerializeField] private PrototypeRunState runState;
    [SerializeField] private int maximumStableStage = 2;
    [SerializeField] private Renderer doorRenderer;
    [SerializeField] private GameObject unstableBlocker;
    [SerializeField] private PrototypeHud hud;
    [SerializeField] private Color stableColor = new(0.25f, 0.75f, 0.55f);
    [SerializeField] private Color unstableColor = new(0.75f, 0.2f, 0.25f);
    [SerializeField] private Color lockedColor = new(0.38f, 0.4f, 0.42f);
    [SerializeField] private float statePulseDuration = 0.75f;
    [SerializeField] private float statePulseIntensity = 1.35f;

    private bool completed;
    private ExitFeedbackState lastState;
    private bool hasState;
    private float statePulseUntil = -999f;

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
        ExitFeedbackState state = GetState();

        if (!hasState)
        {
            lastState = state;
            hasState = true;
        }
        else if (state != lastState)
        {
            lastState = state;
            statePulseUntil = Time.time + statePulseDuration;
            hud?.ShowMessage(GetStateMessage(state), state == ExitFeedbackState.Ready ? 2 : 1);
        }

        if (doorRenderer != null)
        {
            float pulse = Mathf.Clamp01((statePulseUntil - Time.time) / statePulseDuration);
            Color target = GetStateColor(state);
            doorRenderer.material.color = Color.Lerp(target, Color.white, pulse * (statePulseIntensity - 1f));
        }

        if (unstableBlocker != null)
        {
            unstableBlocker.SetActive(state != ExitFeedbackState.Ready);
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
            hud?.ShowMessage("탈출 성공: 필요한 것만 확인했고, 현실이 버텼다.", 2);
        }
        else if (runState != null && !runState.HasAllAnchors)
        {
            hud?.ShowMessage("출구가 잠겨 있다. 안정화 앵커를 모두 회수해야 한다.", 1);
        }
        else
        {
            hud?.ShowMessage("출구가 흔들린다. 너무 많이 확인했다.", 1);
        }
    }

    private ExitFeedbackState GetState()
    {
        if (runState == null || !runState.HasAllAnchors)
        {
            return ExitFeedbackState.MissingAnchors;
        }

        return IsStable ? ExitFeedbackState.Ready : ExitFeedbackState.Unstable;
    }

    private Color GetStateColor(ExitFeedbackState state)
    {
        return state switch
        {
            ExitFeedbackState.Ready => stableColor,
            ExitFeedbackState.Unstable => unstableColor,
            _ => lockedColor
        };
    }

    private string GetStateMessage(ExitFeedbackState state)
    {
        return state switch
        {
            ExitFeedbackState.Ready => "출구가 안정됐다. 지금 나갈 수 있다.",
            ExitFeedbackState.Unstable => "출구가 다시 흔들린다. 오염을 낮춰야 한다.",
            _ => "출구가 다시 잠겼다. 앵커가 부족하다."
        };
    }
}
