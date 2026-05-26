using System;

public sealed class ContaminationProfile
{
    private readonly float[] causeAmounts;
    private readonly int[] causeCounts;
    private ContaminationCause lastCause;
    private bool hasCauseProfile;

    public ContaminationCause LastCause => lastCause;
    public bool HasCauseProfile => hasCauseProfile;
    public ContaminationCause DominantCause => CalculateDominantCause();

    public ContaminationProfile()
    {
        int causeCount = Enum.GetValues(typeof(ContaminationCause)).Length;
        causeAmounts = new float[causeCount];
        causeCounts = new int[causeCount];
    }

    public void Record(float amount, ContaminationCause cause)
    {
        int index = (int)cause;
        if (index < 0 || index >= causeAmounts.Length || index >= causeCounts.Length)
        {
            return;
        }

        causeAmounts[index] += amount;
        causeCounts[index]++;
        lastCause = cause;
        hasCauseProfile = true;
    }

    public float GetAmount(ContaminationCause cause)
    {
        int index = (int)cause;
        return index < 0 || index >= causeAmounts.Length ? 0f : causeAmounts[index];
    }

    public int GetCount(ContaminationCause cause)
    {
        int index = (int)cause;
        return index < 0 || index >= causeCounts.Length ? 0 : causeCounts[index];
    }

    public string GetLabel(ContaminationCause cause)
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

    private ContaminationCause CalculateDominantCause()
    {
        if (!hasCauseProfile)
        {
            return lastCause;
        }

        ContaminationCause dominant = lastCause;
        float bestAmount = -1f;

        for (int i = 0; i < causeAmounts.Length; i++)
        {
            float amount = causeAmounts[i];
            if (amount > bestAmount || Math.Abs(amount - bestAmount) < 0.0001f && i == (int)lastCause)
            {
                bestAmount = amount;
                dominant = (ContaminationCause)i;
            }
        }

        return dominant;
    }
}
