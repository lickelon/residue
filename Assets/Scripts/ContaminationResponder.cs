using System;
using UnityEngine;

[Serializable]
public sealed class ContaminationChange
{
    public Transform target;
    public int requiredStage = 1;
    public bool requireOutOfView = true;
    public Vector3 localPositionOffset;
    public Vector3 localEulerOffset;
    public Vector3 localScaleMultiplier = Vector3.one;
    public bool changeActiveState;
    public bool activeState = true;
}

public sealed class ContaminationResponder : MonoBehaviour
{
    [SerializeField] private ObservationContamination contamination;
    [SerializeField] private Camera viewCamera;
    [SerializeField] private PrototypeHud hud;
    [SerializeField] private Light[] lightsToDim;
    [SerializeField] private Color earlyAmbient = new(0.18f, 0.19f, 0.22f);
    [SerializeField] private Color lateAmbient = new(0.04f, 0.045f, 0.055f);
    [SerializeField] private Color pulseColor = new(0.82f, 0.18f, 0.15f);
    [SerializeField] private float pulseDuration = 0.8f;
    [SerializeField] private float pulseIntensity = 1.45f;
    [SerializeField] private ContaminationChange[] changes;

    private bool[] applied;
    private float[] baseLightIntensities;
    private Color[] baseLightColors;
    private int lastStage;
    private float lastValue;
    private float pulseUntil = -999f;

    private void Awake()
    {
        if (contamination == null)
        {
            contamination = FindFirstObjectByType<ObservationContamination>();
        }

        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }

        if (hud == null)
        {
            hud = FindFirstObjectByType<PrototypeHud>();
        }

        applied = new bool[changes == null ? 0 : changes.Length];
        baseLightIntensities = new float[lightsToDim == null ? 0 : lightsToDim.Length];
        baseLightColors = new Color[lightsToDim == null ? 0 : lightsToDim.Length];

        for (int i = 0; i < baseLightIntensities.Length; i++)
        {
            baseLightIntensities[i] = lightsToDim[i] == null ? 1f : lightsToDim[i].intensity;
            baseLightColors[i] = lightsToDim[i] == null ? Color.white : lightsToDim[i].color;
        }

        lastStage = contamination == null ? 0 : contamination.Stage;
        lastValue = contamination == null ? 0f : contamination.Value;
    }

    private void OnEnable()
    {
        if (contamination == null)
        {
            contamination = FindFirstObjectByType<ObservationContamination>();
        }

        if (contamination == null)
        {
            return;
        }

        contamination.Increased += OnContaminationIncreased;
        contamination.Changed += OnContaminationChanged;
    }

    private void OnDisable()
    {
        if (contamination == null)
        {
            return;
        }

        contamination.Increased -= OnContaminationIncreased;
        contamination.Changed -= OnContaminationChanged;
    }

    private void Update()
    {
        if (contamination == null)
        {
            return;
        }

        ApplyPendingChanges();
        ApplyLighting();
    }

    private void ApplyPendingChanges()
    {
        for (int i = 0; i < changes.Length; i++)
        {
            if (applied[i])
            {
                continue;
            }

            ContaminationChange change = changes[i];
            if (change.target == null || contamination.Stage < change.requiredStage)
            {
                continue;
            }

            if (change.requireOutOfView && IsVisible(change.target))
            {
                continue;
            }

            Apply(change);
            applied[i] = true;
            hud?.ShowMessage($"{change.target.name}이 시야 밖에서 달라졌다.", 1);
        }
    }

    private void Apply(ContaminationChange change)
    {
        change.target.localPosition += change.localPositionOffset;
        change.target.localRotation *= Quaternion.Euler(change.localEulerOffset);
        change.target.localScale = Vector3.Scale(change.target.localScale, change.localScaleMultiplier);

        if (change.changeActiveState)
        {
            change.target.gameObject.SetActive(change.activeState);
        }
    }

    private bool IsVisible(Transform target)
    {
        if (viewCamera == null)
        {
            return false;
        }

        Vector3 viewport = viewCamera.WorldToViewportPoint(target.position);
        return viewport.z > 0f && viewport.x > 0.04f && viewport.x < 0.96f && viewport.y > 0.04f && viewport.y < 0.96f;
    }

    private void ApplyLighting()
    {
        float t = contamination.Normalized;
        RenderSettings.ambientLight = Color.Lerp(earlyAmbient, lateAmbient, t);

        for (int i = 0; i < lightsToDim.Length; i++)
        {
            if (lightsToDim[i] == null)
            {
                continue;
            }

            float pulse = Mathf.Clamp01((pulseUntil - Time.time) / pulseDuration);
            float baseIntensity = Mathf.Lerp(baseLightIntensities[i], baseLightIntensities[i] * 0.32f, t);
            lightsToDim[i].intensity = baseIntensity * Mathf.Lerp(1f, pulseIntensity, pulse);
            lightsToDim[i].color = Color.Lerp(baseLightColors[i], pulseColor, pulse);
        }
    }

    private void OnContaminationIncreased(float value, int stage, ContaminationCause cause)
    {
        pulseUntil = Time.time + pulseDuration;
        hud?.ShowMessage(GetCauseMessage(cause), 1);
    }

    private void OnContaminationChanged(float value, int stage)
    {
        if (stage > lastStage)
        {
            pulseUntil = Time.time + pulseDuration;
            hud?.ShowMessage($"관측 오염 {stage}단계. 공간 반응이 강해진다.", 2);
        }
        else if (value < lastValue - 0.5f)
        {
            hud?.ShowMessage("오염이 낮아졌다. 공간이 잠깐 안정된다.", 1);
        }

        lastStage = stage;
        lastValue = value;
    }

    private string GetCauseMessage(ContaminationCause cause)
    {
        return cause switch
        {
            ContaminationCause.TurnAround => "급하게 뒤돌아본 흔적이 공간에 남았다.",
            ContaminationCause.FastLook => "시선이 흔들리자 오염이 번졌다.",
            ContaminationCause.LongObservation => "너무 오래 본 것이 너를 알아챘다.",
            ContaminationCause.RepeatCheck => "같은 것을 다시 확인한 순간 위치가 어긋났다.",
            _ => "관측 오염이 증가했다."
        };
    }
}
