using System;
using UnityEngine;

public sealed class ObservationContamination : MonoBehaviour
{
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private float decayDelay = 4f;
    [SerializeField] private float decayPerSecond = 2f;
    [SerializeField] private float[] stageThresholds = { 18f, 38f, 62f, 82f };

    private float value;
    private float lastIncreaseTime = -999f;
    private ContaminationProfile profile;

    public event Action<float, int, ContaminationCause> Increased;
    public event Action<float, int> Changed;

    public float Value => value;
    public float Normalized => maxValue <= 0f ? 0f : Mathf.Clamp01(value / maxValue);
    public int Stage => CalculateStage(value);
    public bool IsOverloaded => value >= maxValue;
    public ContaminationCause LastCause => profile.LastCause;
    public bool HasCauseProfile => profile.HasCauseProfile;
    public ContaminationCause DominantCause => profile.DominantCause;

    private void Awake()
    {
        profile = new ContaminationProfile();
    }

    private void Update()
    {
        if (value <= 0f || Time.time - lastIncreaseTime < decayDelay)
        {
            return;
        }

        SetValue(value - decayPerSecond * Time.deltaTime);
    }

    public void Add(float amount, ContaminationCause cause)
    {
        if (amount <= 0f)
        {
            return;
        }

        profile.Record(amount, cause);
        SetValue(value + amount);
        lastIncreaseTime = Time.time;
        Increased?.Invoke(value, Stage, cause);
    }

    public void Stabilize(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetValue(value - amount);
    }

    public void ResetValue()
    {
        SetValue(0f);
    }

    public float GetCauseAmount(ContaminationCause cause)
    {
        return profile.GetAmount(cause);
    }

    public int GetCauseCount(ContaminationCause cause)
    {
        return profile.GetCount(cause);
    }

    public string GetCauseLabel(ContaminationCause cause)
    {
        return profile.GetLabel(cause);
    }

    private void SetValue(float nextValue)
    {
        float clamped = Mathf.Clamp(nextValue, 0f, maxValue);
        if (Mathf.Approximately(value, clamped))
        {
            return;
        }

        value = clamped;
        Changed?.Invoke(value, Stage);
    }

    private int CalculateStage(float currentValue)
    {
        int stage = 0;
        for (int i = 0; i < stageThresholds.Length; i++)
        {
            if (currentValue >= stageThresholds[i])
            {
                stage = i + 1;
            }
        }

        return stage;
    }

}
