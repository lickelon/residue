using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ObservationPrototypeBuilder
{
    private const string ScenePath = "Assets/Scenes/ObservationPrototype.unity";

    [MenuItem("Tools/Residue/Build Observation Prototype")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material wall = CreateMaterial("Prototype_Wall", new Color(0.34f, 0.36f, 0.37f));
        Material floor = CreateMaterial("Prototype_Floor", new Color(0.13f, 0.14f, 0.15f));
        Material objectMat = CreateMaterial("Prototype_Object", new Color(0.68f, 0.58f, 0.42f));
        Material changedMat = CreateMaterial("Prototype_Changed", new Color(0.22f, 0.55f, 0.52f));
        Material doorMat = CreateMaterial("Prototype_Exit", new Color(0.25f, 0.75f, 0.55f));
        Material anchorMat = CreateMaterial("Prototype_Anchor", new Color(0.35f, 0.9f, 0.8f));

        Light mainLight = CreateLight("Main Light", new Vector3(0f, 6f, 8f), Quaternion.Euler(55f, -30f, 0f), 1.8f);
        CreateLight("Hall Light A", new Vector3(0f, 2.8f, 3.5f), Quaternion.identity, 2.6f, LightType.Point, 7f);
        Light hallLightB = CreateLight("Hall Light B", new Vector3(0f, 2.8f, 15f), Quaternion.identity, 2.1f, LightType.Point, 7f);

        BuildLayout(wall, floor);

        GameObject systems = new("Prototype Systems");
        ObservationContamination contamination = systems.AddComponent<ObservationContamination>();
        PrototypeRunState runState = systems.AddComponent<PrototypeRunState>();

        GameObject player = CreatePlayer(contamination);
        Camera camera = player.GetComponentInChildren<Camera>();

        GameObject respawnPoint = new("Reality Collapse Respawn");
        respawnPoint.transform.SetPositionAndRotation(new Vector3(0f, 0.1f, 1.2f), Quaternion.identity);

        PrototypeHud hud = CreateHud(contamination, runState);
        SetObject(runState, "contamination", contamination);
        SetObject(runState, "hud", hud);
        SetObject(runState, "player", player.transform);
        SetObject(runState, "respawnPoint", respawnPoint.transform);

        GameObject portrait = CreateObservable("Crooked Portrait", new Vector3(-3.82f, 1.55f, 8.5f), new Vector3(0.08f, 1.15f, 1.3f), objectMat);
        GameObject chair = CreateObservable("Waiting Chair", new Vector3(2.7f, 0.45f, 12.2f), new Vector3(0.9f, 0.9f, 0.9f), objectMat);
        GameObject cabinet = CreateObservable("Tall Cabinet", new Vector3(-2.9f, 1.1f, 18f), new Vector3(0.8f, 2.2f, 0.6f), objectMat);
        GameObject duplicatePortrait = CreateObservable("Returned Portrait", new Vector3(3.82f, 1.55f, 20f), new Vector3(0.08f, 1.15f, 1.3f), changedMat);
        duplicatePortrait.SetActive(false);

        GameObject doorA = CreateDoor("Side Room Door", new Vector3(-3.95f, 1.15f, 5.3f), Quaternion.identity, hud, wall, contamination, camera);
        GameObject doorB = CreateDoor("Back Room Door", new Vector3(3.95f, 1.15f, 18.4f), Quaternion.Euler(0f, 180f, 0f), hud, wall, contamination, camera);
        CreateAnchor("Anchor A", new Vector3(-9.7f, 0.65f, 6.5f), anchorMat, runState, hud);
        CreateAnchor("Anchor B", new Vector3(9.7f, 0.65f, 17.5f), anchorMat, runState, hud);
        CreateAnchor("Anchor C", new Vector3(0f, 0.65f, 21.8f), anchorMat, runState, hud);

        GameObject blocker = Cube("Unstable Exit Blocker", new Vector3(0f, 1.1f, 24.8f), new Vector3(2.2f, 2.2f, 0.25f), wall);
        GameObject exit = Cube("Final Exit", new Vector3(0f, 1.1f, 26f), new Vector3(2.4f, 2.2f, 0.2f), doorMat);
        BoxCollider exitCollider = exit.GetComponent<BoxCollider>();
        exitCollider.size = new Vector3(1.2f, 1.2f, 1.8f);
        ExitController exitController = exit.AddComponent<ExitController>();

        SetObject(exitController, "contamination", contamination);
        SetObject(exitController, "runState", runState);
        SetObject(exitController, "doorRenderer", exit.GetComponent<Renderer>());
        SetObject(exitController, "unstableBlocker", blocker);
        SetObject(exitController, "hud", hud);

        ContaminationResponder responder = systems.AddComponent<ContaminationResponder>();
        SetObject(responder, "contamination", contamination);
        SetObject(responder, "viewCamera", camera);
        SetObject(responder, "hud", hud);
        SetLightArray(responder, new[] { mainLight, hallLightB });
        SetChanges(responder, new[]
        {
            Change(portrait.transform, 1, new Vector3(0f, 0.18f, 0f), new Vector3(0f, 0f, 7f), Vector3.one, false, true),
            Change(chair.transform, 2, new Vector3(0.75f, 0f, -0.35f), new Vector3(0f, 38f, 0f), new Vector3(1.05f, 1f, 1.05f), false, true),
            Change(cabinet.transform, 3, new Vector3(0f, 0f, -1.2f), Vector3.zero, new Vector3(1f, 1.28f, 1f), false, true),
            Change(duplicatePortrait.transform, 4, Vector3.zero, Vector3.zero, Vector3.one, true, true),
            Change(doorA.transform, 3, new Vector3(0f, 0f, 1.4f), new Vector3(0f, 6f, 0f), Vector3.one, false, true),
            Change(doorB.transform, 4, new Vector3(0f, 0f, -1.6f), new Vector3(0f, -8f, 0f), Vector3.one, false, true)
        });

        RenderSettings.ambientLight = new Color(0.18f, 0.19f, 0.22f);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.OpenScene(ScenePath);
        Selection.activeGameObject = player;
    }

    private static void BuildLayout(Material wall, Material floor)
    {
        Cube("Main Corridor Floor", new Vector3(0f, -0.05f, 12f), new Vector3(7.6f, 0.1f, 28f), floor);
        Cube("Main Corridor Ceiling", new Vector3(0f, 3.05f, 12f), new Vector3(7.6f, 0.12f, 28f), wall);
        Cube("Left Corridor Wall A", new Vector3(-4f, 1.5f, 1.65f), new Vector3(0.18f, 3f, 7.3f), wall);
        Cube("Left Corridor Wall B", new Vector3(-4f, 1.5f, 16.55f), new Vector3(0.18f, 3f, 18.9f), wall);
        Cube("Left Door Header", new Vector3(-4f, 2.65f, 6.2f), new Vector3(0.2f, 0.75f, 1.8f), wall);
        Cube("Right Corridor Wall A", new Vector3(4f, 1.5f, 7.3f), new Vector3(0.18f, 3f, 18.6f), wall);
        Cube("Right Corridor Wall B", new Vector3(4f, 1.5f, 22.2f), new Vector3(0.18f, 3f, 7.6f), wall);
        Cube("Right Door Header", new Vector3(4f, 2.65f, 17.5f), new Vector3(0.2f, 0.75f, 1.8f), wall);
        Cube("Start Back Wall", new Vector3(0f, 1.5f, -2f), new Vector3(7.8f, 3f, 0.18f), wall);

        Cube("Left Room Floor", new Vector3(-8f, -0.05f, 6.5f), new Vector3(8f, 0.1f, 7f), floor);
        Cube("Left Room Ceiling", new Vector3(-8f, 3.05f, 6.5f), new Vector3(8f, 0.12f, 7f), wall);
        Cube("Left Room Back Wall", new Vector3(-8f, 1.5f, 3f), new Vector3(8f, 3f, 0.18f), wall);
        Cube("Left Room Side Wall", new Vector3(-12f, 1.5f, 6.5f), new Vector3(0.18f, 3f, 7f), wall);
        Cube("Left Room Front Wall", new Vector3(-8f, 1.5f, 10f), new Vector3(8f, 3f, 0.18f), wall);

        Cube("Right Room Floor", new Vector3(8f, -0.05f, 17.5f), new Vector3(8f, 0.1f, 7f), floor);
        Cube("Right Room Ceiling", new Vector3(8f, 3.05f, 17.5f), new Vector3(8f, 0.12f, 7f), wall);
        Cube("Right Room Back Wall", new Vector3(8f, 1.5f, 14f), new Vector3(8f, 3f, 0.18f), wall);
        Cube("Right Room Side Wall", new Vector3(12f, 1.5f, 17.5f), new Vector3(0.18f, 3f, 7f), wall);
        Cube("Right Room Front Wall", new Vector3(8f, 1.5f, 21f), new Vector3(8f, 3f, 0.18f), wall);

        Cube("Exit Frame Left", new Vector3(-1.8f, 1.5f, 25f), new Vector3(0.25f, 3f, 0.35f), wall);
        Cube("Exit Frame Right", new Vector3(1.8f, 1.5f, 25f), new Vector3(0.25f, 3f, 0.35f), wall);
        Cube("Exit Frame Top", new Vector3(0f, 2.65f, 25f), new Vector3(3.8f, 0.75f, 0.35f), wall);
    }

    private static GameObject CreatePlayer(ObservationContamination contamination)
    {
        GameObject player = new("Player");
        player.transform.position = new Vector3(0f, 0.1f, 0f);
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.28f;
        controller.center = new Vector3(0f, 0.9f, 0f);

        FirstPersonController movement = player.AddComponent<FirstPersonController>();
        PlayerBehaviorTracker tracker = player.AddComponent<PlayerBehaviorTracker>();

        GameObject cameraObject = new("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.nearClipPlane = 0.03f;
        camera.fieldOfView = 68f;
        cameraObject.AddComponent<AudioListener>();

        SetObject(movement, "viewCamera", camera);
        SetObject(tracker, "viewCamera", camera);
        SetObject(tracker, "contamination", contamination);

        return player;
    }

    private static PrototypeHud CreateHud(ObservationContamination contamination, PrototypeRunState runState)
    {
        GameObject canvasObject = new("Prototype HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        Text status = CreateText("Status", canvasObject.transform, new Vector2(28f, -28f), TextAnchor.UpperLeft, 26, new Color(0.84f, 0.9f, 0.86f));
        status.rectTransform.sizeDelta = new Vector2(760f, 180f);

        Text message = CreateText("Message", canvasObject.transform, new Vector2(0f, 88f), TextAnchor.MiddleCenter, 34, new Color(0.94f, 0.82f, 0.58f));
        message.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        message.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        message.rectTransform.sizeDelta = new Vector2(1100f, 120f);

        PrototypeHud hud = canvasObject.AddComponent<PrototypeHud>();
        SetObject(hud, "contamination", contamination);
        SetObject(hud, "runState", runState);
        SetObject(hud, "statusText", status);
        SetObject(hud, "messageText", message);
        return hud;
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, TextAnchor alignment, int fontSize, Color color)
    {
        GameObject textObject = new(name);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = GetBuiltinFont();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.rectTransform.anchorMin = new Vector2(0f, 1f);
        text.rectTransform.anchorMax = new Vector2(0f, 1f);
        text.rectTransform.pivot = new Vector2(0f, 1f);
        text.rectTransform.anchoredPosition = anchoredPosition;
        return text;
    }

    private static Font GetBuiltinFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static GameObject CreateObservable(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obj = Cube(name, position, scale, material);
        obj.AddComponent<ObservableTarget>();
        return obj;
    }

    private static GameObject CreateDoor(string name, Vector3 position, Quaternion rotation, PrototypeHud hud, Material material, ObservationContamination contamination, Camera camera)
    {
        GameObject root = new(name);
        root.transform.SetPositionAndRotation(position, rotation);
        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(2.8f, 2.7f, 2.3f);

        GameObject hingePivot = new(name + " Hinge");
        hingePivot.transform.SetParent(root.transform, false);

        GameObject panel = Cube(name + " Panel", Vector3.zero, new Vector3(0.2f, 2.3f, 1.8f), material);
        panel.transform.SetParent(hingePivot.transform, false);
        panel.transform.localPosition = new Vector3(0f, 0f, 0.9f);

        GameObject handle = Cube(name + " Handle", Vector3.zero, new Vector3(0.08f, 0.12f, 0.16f), material);
        handle.transform.SetParent(panel.transform, false);
        handle.transform.localPosition = new Vector3(-0.62f, 0f, 0.22f);

        GameObject slit = Cube(name + " Eye Slit", Vector3.zero, new Vector3(0.03f, 0.24f, 0.78f), material);
        slit.transform.SetParent(panel.transform, false);
        slit.transform.localPosition = new Vector3(-0.085f, 0.45f, 0f);

        SimpleDoor door = root.AddComponent<SimpleDoor>();
        SetObject(door, "hinge", hingePivot.transform);
        SetObject(door, "watchTarget", panel.transform);
        SetObject(door, "doorRenderer", panel.GetComponent<Renderer>());
        SetObject(door, "contamination", contamination);
        SetObject(door, "viewCamera", camera);
        SetObject(door, "hud", hud);
        return root;
    }

    private static GameObject CreateAnchor(string name, Vector3 position, Material material, PrototypeRunState runState, PrototypeHud hud)
    {
        GameObject root = new(name);
        root.transform.position = position;
        SphereCollider trigger = root.AddComponent<SphereCollider>();
        trigger.radius = 1.15f;
        trigger.isTrigger = true;

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = name + " Core";
        core.transform.SetParent(root.transform, false);
        core.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        Object.DestroyImmediate(core.GetComponent<Collider>());
        core.GetComponent<Renderer>().sharedMaterial = material;

        Light light = CreateLight(name + " Light", position + Vector3.up * 0.65f, Quaternion.identity, 1.8f, LightType.Point, 4.5f);
        StabilityAnchor anchor = root.AddComponent<StabilityAnchor>();
        SetObject(anchor, "runState", runState);
        SetObject(anchor, "hud", hud);
        SetObject(anchor, "anchorRenderer", core.GetComponent<Renderer>());
        SetObject(anchor, "anchorLight", light);
        return root;
    }

    private static Light CreateLight(string name, Vector3 position, Quaternion rotation, float intensity, LightType type = LightType.Directional, float range = 10f)
    {
        GameObject obj = new(name);
        obj.transform.SetPositionAndRotation(position, rotation);
        Light light = obj.AddComponent<Light>();
        light.type = type;
        light.intensity = intensity;
        light.range = range;
        light.color = new Color(0.78f, 0.86f, 1f);
        return light;
    }

    private static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;

        if (material != null)
        {
            obj.GetComponent<Renderer>().sharedMaterial = material;
        }

        return obj;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string folder = "Assets/Materials";
        Directory.CreateDirectory(folder);
        string path = $"{folder}/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static ContaminationChange Change(Transform target, int stage, Vector3 positionOffset, Vector3 eulerOffset, Vector3 scaleMultiplier, bool changeActive, bool active)
    {
        return new ContaminationChange
        {
            target = target,
            requiredStage = stage,
            requireOutOfView = true,
            localPositionOffset = positionOffset,
            localEulerOffset = eulerOffset,
            localScaleMultiplier = scaleMultiplier,
            changeActiveState = changeActive,
            activeState = active
        };
    }

    private static void SetObject(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetLightArray(ContaminationResponder responder, Light[] lights)
    {
        SerializedObject serialized = new(responder);
        SerializedProperty array = serialized.FindProperty("lightsToDim");
        array.arraySize = lights.Length;
        for (int i = 0; i < lights.Length; i++)
        {
            array.GetArrayElementAtIndex(i).objectReferenceValue = lights[i];
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetChanges(ContaminationResponder responder, ContaminationChange[] changes)
    {
        SerializedObject serialized = new(responder);
        SerializedProperty array = serialized.FindProperty("changes");
        array.arraySize = changes.Length;

        for (int i = 0; i < changes.Length; i++)
        {
            SerializedProperty item = array.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("target").objectReferenceValue = changes[i].target;
            item.FindPropertyRelative("requiredStage").intValue = changes[i].requiredStage;
            item.FindPropertyRelative("requireOutOfView").boolValue = changes[i].requireOutOfView;
            item.FindPropertyRelative("localPositionOffset").vector3Value = changes[i].localPositionOffset;
            item.FindPropertyRelative("localEulerOffset").vector3Value = changes[i].localEulerOffset;
            item.FindPropertyRelative("localScaleMultiplier").vector3Value = changes[i].localScaleMultiplier;
            item.FindPropertyRelative("changeActiveState").boolValue = changes[i].changeActiveState;
            item.FindPropertyRelative("activeState").boolValue = changes[i].activeState;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
