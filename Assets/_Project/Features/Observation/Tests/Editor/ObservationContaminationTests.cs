using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class ObservationContaminationTests
{
    private readonly List<GameObject> createdObjects = new();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < createdObjects.Count; i++)
        {
            Object.DestroyImmediate(createdObjects[i]);
        }

        createdObjects.Clear();
    }

    [Test]
    public void AddIncreasesValueCauseProfileAndEvents()
    {
        ObservationContamination contamination = CreateContamination();
        int changedCount = 0;
        int increasedCount = 0;
        float changedValue = 0f;
        int changedStage = 0;
        float increasedValue = 0f;
        int increasedStage = 0;
        ContaminationCause increasedCause = ContaminationCause.TurnAround;

        contamination.Changed += (value, stage) =>
        {
            changedCount++;
            changedValue = value;
            changedStage = stage;
        };
        contamination.Increased += (value, stage, cause) =>
        {
            increasedCount++;
            increasedValue = value;
            increasedStage = stage;
            increasedCause = cause;
        };

        contamination.Add(20f, ContaminationCause.FastLook);

        Assert.AreEqual(20f, contamination.Value);
        Assert.AreEqual(0.2f, contamination.Normalized);
        Assert.AreEqual(1, contamination.Stage);
        Assert.IsTrue(contamination.HasCauseProfile);
        Assert.AreEqual(ContaminationCause.FastLook, contamination.LastCause);
        Assert.AreEqual(ContaminationCause.FastLook, contamination.DominantCause);
        Assert.AreEqual(20f, contamination.GetCauseAmount(ContaminationCause.FastLook));
        Assert.AreEqual(1, contamination.GetCauseCount(ContaminationCause.FastLook));
        Assert.AreEqual(1, changedCount);
        Assert.AreEqual(20f, changedValue);
        Assert.AreEqual(1, changedStage);
        Assert.AreEqual(1, increasedCount);
        Assert.AreEqual(20f, increasedValue);
        Assert.AreEqual(1, increasedStage);
        Assert.AreEqual(ContaminationCause.FastLook, increasedCause);
    }

    [Test]
    public void AddIgnoresNonPositiveAmount()
    {
        ObservationContamination contamination = CreateContamination();
        int changedCount = 0;
        int increasedCount = 0;
        contamination.Changed += (_, _) => changedCount++;
        contamination.Increased += (_, _, _) => increasedCount++;

        contamination.Add(0f, ContaminationCause.FastLook);
        contamination.Add(-1f, ContaminationCause.TurnAround);

        Assert.AreEqual(0f, contamination.Value);
        Assert.IsFalse(contamination.HasCauseProfile);
        Assert.AreEqual(0, changedCount);
        Assert.AreEqual(0, increasedCount);
    }

    [Test]
    public void AddClampsValueToMaximum()
    {
        ObservationContamination contamination = CreateContamination();

        contamination.Add(150f, ContaminationCause.LongObservation);

        Assert.AreEqual(100f, contamination.Value);
        Assert.AreEqual(1f, contamination.Normalized);
        Assert.AreEqual(4, contamination.Stage);
        Assert.IsTrue(contamination.IsOverloaded);
        Assert.AreEqual(150f, contamination.GetCauseAmount(ContaminationCause.LongObservation));
    }

    [Test]
    public void StabilizeLowersValueWithoutChangingCauseProfile()
    {
        ObservationContamination contamination = CreateContamination();

        contamination.Add(20f, ContaminationCause.RepeatCheck);
        contamination.Stabilize(7f);

        Assert.AreEqual(13f, contamination.Value);
        Assert.AreEqual(20f, contamination.GetCauseAmount(ContaminationCause.RepeatCheck));
        Assert.AreEqual(1, contamination.GetCauseCount(ContaminationCause.RepeatCheck));
        Assert.AreEqual(ContaminationCause.RepeatCheck, contamination.LastCause);
    }

    [Test]
    public void ResetValueClearsCurrentValueOnly()
    {
        ObservationContamination contamination = CreateContamination();

        contamination.Add(20f, ContaminationCause.TurnAround);
        contamination.ResetValue();

        Assert.AreEqual(0f, contamination.Value);
        Assert.AreEqual(0f, contamination.Normalized);
        Assert.AreEqual(0, contamination.Stage);
        Assert.IsTrue(contamination.HasCauseProfile);
        Assert.AreEqual(20f, contamination.GetCauseAmount(ContaminationCause.TurnAround));
    }

    private ObservationContamination CreateContamination()
    {
        var gameObject = new GameObject("ObservationContaminationTest");
        createdObjects.Add(gameObject);
        ObservationContamination contamination = gameObject.AddComponent<ObservationContamination>();
        typeof(ObservationContamination)
            .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(contamination, null);

        return contamination;
    }
}
