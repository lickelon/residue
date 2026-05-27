using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ObservationPrototypeSmokeTests
{
    [UnityTest]
    public IEnumerator ObservationPrototypeLoadsRequiredRuntimeObjects()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync("ObservationPrototype", LoadSceneMode.Single);
        while (!load.isDone)
        {
            yield return null;
        }

        yield return null;

        Scene scene = SceneManager.GetActiveScene();
        Assert.AreEqual("ObservationPrototype", scene.name);
        Assert.IsTrue(scene.isLoaded);
        Assert.IsNotNull(Camera.main);
        AssertComponent("Prototype Systems", "ObservationContamination");
        AssertComponent("Prototype Systems", "PrototypeRunState");
        AssertComponent("Prototype Systems", "ContaminationResponder");
        AssertComponent("Prototype HUD", "PrototypeHud");
        AssertComponent("Player", "FirstPersonController");
        AssertComponent("Player", "PlayerBehaviorTracker");
        AssertComponent("Final Exit", "ExitController");
        AssertComponent("Side Room Door", "SimpleDoor");
        AssertComponent("Back Room Door", "SimpleDoor");
        AssertComponent("Anchor A", "StabilityAnchor");
        AssertComponent("Anchor B", "StabilityAnchor");
        AssertComponent("Anchor C", "StabilityAnchor");
    }

    private static void AssertComponent(string objectName, string componentName)
    {
        GameObject target = GameObject.Find(objectName);
        Assert.IsNotNull(target, objectName);
        Assert.IsNotNull(target.GetComponent(componentName), $"{objectName}.{componentName}");
    }
}
