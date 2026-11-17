using MoreMountains.Feedbacks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-click setup for the attack feedbacks (Feel). Creates the MMF_Players under the player,
/// adds the shakers the feedbacks need and assigns everything to PlayerCombat.
/// Safe to run again: it rebuilds the feedback players from scratch.
/// </summary>
public static class CombatFeedbackSetup
{
    private const string FeedbacksRootName = "Feedbacks";

    [MenuItem("Tools/Combat/Setup Feel Feedbacks")]
    private static void Setup()
    {
        PlayerCombat combat = Object.FindAnyObjectByType<PlayerCombat>();
        if (combat == null)
        {
            Debug.LogError("[CombatFeedbackSetup] No PlayerCombat found in the open scene.");
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("[CombatFeedbackSetup] No camera tagged MainCamera found.");
            return;
        }

        // Shakers that listen to the camera feedbacks.
        GetOrAdd<MMCameraShaker>(camera.gameObject);
        GetOrAdd<MMCameraOrthographicSizeShaker>(camera.gameObject);
        SetupCameraWiggle(GetOrAdd<MMWiggle>(camera.gameObject));

        // Character trembles while charging. Shake the visual child so it does not fight the dash movement.
        Transform visual = combat.transform.Find("Visual");
        MMPositionShaker positionShaker = GetOrAdd<MMPositionShaker>(visual != null ? visual.gameObject : combat.gameObject);

        Transform feedbacksRoot = combat.transform.Find(FeedbacksRootName);
        if (feedbacksRoot == null)
        {
            GameObject rootObject = new GameObject(FeedbacksRootName);
            Undo.RegisterCreatedObjectUndo(rootObject, "Create Combat Feedbacks");
            rootObject.transform.SetParent(combat.transform, false);
            feedbacksRoot = rootObject.transform;
        }

        MMF_Player charge = CreatePlayer(feedbacksRoot, "ChargeFeedback");
        AddCameraShake(charge, duration: 0.60f, amplitude: 0.15f, frequency: 35.00f);
        AddCameraPullBack(charge, duration: 0.60f, extraOrthographicSize: 0.70f);
        AddTremble(charge, positionShaker, duration: 0.60f);

        MMF_Player dash = CreatePlayer(feedbacksRoot, "DashFeedback");
        AddCameraShake(dash, duration: 0.12f, amplitude: 0.12f, frequency: 60.00f);

        MMF_Player attack = CreatePlayer(feedbacksRoot, "AttackFeedback");
        AddCameraShake(attack, duration: 0.20f, amplitude: 0.25f, frequency: 50.00f);

        MMF_Player back = CreatePlayer(feedbacksRoot, "ReturnFeedback");

        SerializedObject serializedCombat = new SerializedObject(combat);
        serializedCombat.FindProperty("chargeFeedback").objectReferenceValue = charge;
        serializedCombat.FindProperty("dashFeedback").objectReferenceValue = dash;
        serializedCombat.FindProperty("attackFeedback").objectReferenceValue = attack;
        serializedCombat.FindProperty("returnFeedback").objectReferenceValue = back;
        serializedCombat.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(combat.gameObject.scene);
        Selection.activeGameObject = feedbacksRoot.gameObject;
        Debug.Log("[CombatFeedbackSetup] Combat feedbacks created and assigned. Save the scene to keep them.", feedbacksRoot);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(target);
    }

    /// <summary>
    /// MMCameraShaker drives the camera through MMWiggle, whose position wiggle is off by default.
    /// Enable it, but keep it idle until a shake event arrives.
    /// </summary>
    private static void SetupCameraWiggle(MMWiggle wiggle)
    {
        Undo.RecordObject(wiggle, "Setup Camera Wiggle");
        wiggle.PositionActive = true;
        wiggle.PositionWiggleProperties = new WiggleProperties
        {
            WigglePermitted = false,
            WiggleType = WiggleTypes.Noise,
            StartWigglingAutomatically = false,
        };
        EditorUtility.SetDirty(wiggle);
    }

    private static MMF_Player CreatePlayer(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            Undo.DestroyObjectImmediate(existing.gameObject);

        GameObject playerObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(playerObject, "Create " + name);
        playerObject.transform.SetParent(parent, false);

        MMF_Player player = playerObject.AddComponent<MMF_Player>();
        EditorUtility.SetDirty(player);
        return player;
    }

    private static void AddCameraShake(MMF_Player player, float duration, float amplitude, float frequency)
    {
        MMF_CameraShake shake = (MMF_CameraShake)player.AddFeedback(typeof(MMF_CameraShake));
        shake.CameraShakeProperties = new MMCameraShakeProperties(duration, amplitude, frequency);
        EditorUtility.SetDirty(player);
    }

    /// <summary>Camera slowly moves "back" = orthographic size grows over the charge, then resets.</summary>
    private static void AddCameraPullBack(MMF_Player player, float duration, float extraOrthographicSize)
    {
        MMF_CameraOrthographicSize zoom = (MMF_CameraOrthographicSize)player.AddFeedback(typeof(MMF_CameraOrthographicSize));
        zoom.Label = "Camera Pull Back";
        zoom.Duration = duration;
        zoom.RelativeOrthographicSize = true;
        zoom.ShakeOrthographicSize = AnimationCurve.EaseInOut(0.00f, 0.00f, 1.00f, 1.00f);
        zoom.RemapOrthographicSizeZero = 0.00f;
        zoom.RemapOrthographicSizeOne = extraOrthographicSize;
        zoom.ResetTargetValuesAfterShake = true;
        EditorUtility.SetDirty(player);
    }

    private static void AddTremble(MMF_Player player, MMPositionShaker shaker, float duration)
    {
        MMF_PositionShake tremble = (MMF_PositionShake)player.AddFeedback(typeof(MMF_PositionShake));
        tremble.Label = "Character Tremble";
        tremble.TargetShaker = shaker;
        tremble.Duration = duration;
        tremble.ShakeSpeed = 45.00f;
        tremble.ShakeRange = 0.04f;
        tremble.ShakeMainDirection = Vector3.right;
        tremble.AddDirectionalNoise = false;
        tremble.UseAttenuation = false;
        EditorUtility.SetDirty(player);
    }
}
