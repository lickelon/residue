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
    [SerializeField] private Light[] lightsToDim;
    [SerializeField] private Color earlyAmbient = new(0.18f, 0.19f, 0.22f);
    [SerializeField] private Color lateAmbient = new(0.04f, 0.045f, 0.055f);
    [SerializeField] private ContaminationChange[] changes;

    private bool[] applied;
    private float[] baseLightIntensities;

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

        applied = new bool[changes == null ? 0 : changes.Length];
        baseLightIntensities = new float[lightsToDim == null ? 0 : lightsToDim.Length];

        for (int i = 0; i < baseLightIntensities.Length; i++)
        {
            baseLightIntensities[i] = lightsToDim[i] == null ? 1f : lightsToDim[i].intensity;
        }
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

            lightsToDim[i].intensity = Mathf.Lerp(baseLightIntensities[i], baseLightIntensities[i] * 0.32f, t);
        }
    }
}
