using System;
using UnityEngine;

public enum ContaminationCause
{
    TurnAround,
    FastLook,
    LongObservation,
    RepeatCheck
}

public sealed class ObservationContamination : MonoBehaviour
{
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private float decayDelay = 4f;
    [SerializeField] private float decayPerSecond = 2f;
    [SerializeField] private float[] stageThresholds = { 18f, 38f, 62f, 82f };

    private float value;
    private float lastIncreaseTime = -999f;
    private float[] causeAmounts;
    private int[] causeCounts;
    private ContaminationCause lastCause;
    private bool hasCauseProfile;

    public event Action<float, int, ContaminationCause> Increased;
    public event Action<float, int> Changed;

    public float Value => value;
    public float Normalized => maxValue <= 0f ? 0f : Mathf.Clamp01(value / maxValue);
    public int Stage => CalculateStage(value);
    public bool IsOverloaded => value >= maxValue;
    public ContaminationCause LastCause => lastCause;
    public bool HasCauseProfile => hasCauseProfile;
    public ContaminationCause DominantCause => CalculateDominantCause();

    private void Awake()
    {
        int causeCount = Enum.GetValues(typeof(ContaminationCause)).Length;
        causeAmounts = new float[causeCount];
        causeCounts = new int[causeCount];
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

        RecordCause(amount, cause);
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
        int index = (int)cause;
        return causeAmounts == null || index < 0 || index >= causeAmounts.Length ? 0f : causeAmounts[index];
    }

    public int GetCauseCount(ContaminationCause cause)
    {
        int index = (int)cause;
        return causeCounts == null || index < 0 || index >= causeCounts.Length ? 0 : causeCounts[index];
    }

    public string GetCauseLabel(ContaminationCause cause)
    {
        return cause switch
        {
            ContaminationCause.TurnAround => "뒤돌아봄",
            ContaminationCause.FastLook => "급시선",
            ContaminationCause.LongObservation => "응시",
            ContaminationCause.RepeatCheck => "반복 확인",
            _ => "불명"
        };
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

    private void RecordCause(float amount, ContaminationCause cause)
    {
        int index = (int)cause;
        if (causeAmounts == null || causeCounts == null || index < 0 || index >= causeAmounts.Length || index >= causeCounts.Length)
        {
            return;
        }

        causeAmounts[index] += amount;
        causeCounts[index]++;
        lastCause = cause;
        hasCauseProfile = true;
    }

    private ContaminationCause CalculateDominantCause()
    {
        if (!hasCauseProfile || causeAmounts == null)
        {
            return lastCause;
        }

        ContaminationCause dominant = lastCause;
        float bestAmount = -1f;

        for (int i = 0; i < causeAmounts.Length; i++)
        {
            float amount = causeAmounts[i];
            if (amount > bestAmount || Mathf.Approximately(amount, bestAmount) && i == (int)lastCause)
            {
                bestAmount = amount;
                dominant = (ContaminationCause)i;
            }
        }

        return dominant;
    }
}
