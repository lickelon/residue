using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class ContaminationResponderTests
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
    public void DoesNotApplyChangeBelowRequiredStage()
    {
        ObservationContamination contamination = CreateContamination();
        Transform target = CreateTarget("Stage Target", new Vector3(0f, 0f, -4f));
        ContaminationResponder responder = CreateResponder(contamination, null, StageChange(target, 1));

        InvokeUpdate(responder);

        Assert.AreEqual(new Vector3(0f, 0f, -4f), target.position);
    }

    [Test]
    public void DoesNotApplyChangeForDifferentDominantCause()
    {
        ObservationContamination contamination = CreateContamination();
        contamination.Add(20f, ContaminationCause.LongObservation);
        Transform target = CreateTarget("Cause Target", new Vector3(0f, 0f, -4f));
        ContaminationResponder responder = CreateResponder(
            contamination,
            null,
            CauseChange(target, ContaminationCause.FastLook, 8f));

        InvokeUpdate(responder);

        Assert.AreEqual(new Vector3(0f, 0f, -4f), target.position);
    }

    [Test]
    public void DoesNotApplyChangeBelowMinimumCauseAmount()
    {
        ObservationContamination contamination = CreateContamination();
        contamination.Add(20f, ContaminationCause.FastLook);
        Transform target = CreateTarget("Minimum Target", new Vector3(0f, 0f, -4f));
        ContaminationResponder responder = CreateResponder(
            contamination,
            null,
            CauseChange(target, ContaminationCause.FastLook, 25f));

        InvokeUpdate(responder);

        Assert.AreEqual(new Vector3(0f, 0f, -4f), target.position);
    }

    [Test]
    public void DoesNotApplyOutOfViewChangeWhenTargetIsVisible()
    {
        ObservationContamination contamination = CreateContamination();
        contamination.Add(20f, ContaminationCause.FastLook);
        Camera camera = CreateCamera();
        Transform target = CreateTarget("Visible Target", new Vector3(0f, 0f, 5f));
        ContaminationResponder responder = CreateResponder(
            contamination,
            camera,
            CauseChange(target, ContaminationCause.FastLook, 8f));

        InvokeUpdate(responder);

        Assert.AreEqual(new Vector3(0f, 0f, 5f), target.position);
    }

    [Test]
    public void AppliesMatchingOutOfViewChangeOnce()
    {
        ObservationContamination contamination = CreateContamination();
        contamination.Add(20f, ContaminationCause.FastLook);
        Camera camera = CreateCamera();
        Transform target = CreateTarget("Hidden Target", new Vector3(0f, 0f, -4f));
        ContaminationResponder responder = CreateResponder(
            contamination,
            camera,
            CauseChange(target, ContaminationCause.FastLook, 8f));

        InvokeUpdate(responder);
        InvokeUpdate(responder);

        Assert.AreEqual(new Vector3(1f, 0f, -4f), target.position);
        Assert.That(target.eulerAngles.y, Is.EqualTo(45f).Within(0.001f));
        Assert.AreEqual(new Vector3(2f, 1f, 1f), target.localScale);
    }

    private ObservationContamination CreateContamination()
    {
        var gameObject = new GameObject("ObservationContaminationTest");
        createdObjects.Add(gameObject);
        ObservationContamination contamination = gameObject.AddComponent<ObservationContamination>();
        InvokePrivate(contamination, "Awake");
        return contamination;
    }

    private ContaminationResponder CreateResponder(ObservationContamination contamination, Camera camera, ContaminationChange change)
    {
        var gameObject = new GameObject("ContaminationResponderTest");
        createdObjects.Add(gameObject);
        ContaminationResponder responder = gameObject.AddComponent<ContaminationResponder>();
        SetField(responder, "contamination", contamination);
        SetField(responder, "viewCamera", camera);
        SetField(responder, "lightsToDim", new Light[0]);
        SetField(responder, "changes", new[] { change });
        InvokePrivate(responder, "Awake");
        return responder;
    }

    private Camera CreateCamera()
    {
        var gameObject = new GameObject("Responder Camera");
        createdObjects.Add(gameObject);
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        return gameObject.AddComponent<Camera>();
    }

    private Transform CreateTarget(string name, Vector3 position)
    {
        var gameObject = new GameObject(name);
        createdObjects.Add(gameObject);
        gameObject.transform.position = position;
        return gameObject.transform;
    }

    private static ContaminationChange StageChange(Transform target, int requiredStage)
    {
        return new ContaminationChange
        {
            target = target,
            requiredStage = requiredStage,
            requireOutOfView = false,
            localPositionOffset = Vector3.right,
            localEulerOffset = new Vector3(0f, 45f, 0f),
            localScaleMultiplier = new Vector3(2f, 1f, 1f)
        };
    }

    private static ContaminationChange CauseChange(Transform target, ContaminationCause cause, float minimumCauseAmount)
    {
        ContaminationChange change = StageChange(target, 1);
        change.requireOutOfView = true;
        change.requireDominantCause = true;
        change.dominantCause = cause;
        change.minimumCauseAmount = minimumCauseAmount;
        return change;
    }

    private static void InvokeUpdate(ContaminationResponder responder)
    {
        InvokePrivate(responder, "Update");
    }

    private static void InvokePrivate(object target, string methodName)
    {
        target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, null);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }
}
