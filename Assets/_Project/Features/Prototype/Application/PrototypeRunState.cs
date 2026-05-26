using UnityEngine;

public sealed class PrototypeRunState : MonoBehaviour
{
    [SerializeField] private ObservationContamination contamination;
    [SerializeField] private PrototypeHud hud;
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private int requiredAnchors = 3;
    [SerializeField] private float overloadPenaltyCooldown = 5f;
    [SerializeField] private float overloadStabilizeAmount = 36f;

    private int collectedAnchors;
    private float lastPenaltyAt = -999f;

    public int RequiredAnchors => requiredAnchors;
    public int CollectedAnchors => collectedAnchors;
    public bool HasAllAnchors => collectedAnchors >= requiredAnchors;

    private void Awake()
    {
        if (contamination == null)
        {
            contamination = FindFirstObjectByType<ObservationContamination>();
        }

        if (hud == null)
        {
            hud = FindFirstObjectByType<PrototypeHud>();
        }
    }

    private void Update()
    {
        if (contamination == null || !contamination.IsOverloaded || Time.time - lastPenaltyAt < overloadPenaltyCooldown)
        {
            return;
        }

        CollapseReality();
    }

    public void CollectAnchor(StabilityAnchor anchor)
    {
        collectedAnchors++;
        contamination?.Stabilize(anchor.StabilizeAmount);
        hud?.ShowMessage($"앵커 회수 {collectedAnchors}/{requiredAnchors}. 현실이 잠깐 안정됐다.");
    }

    private void CollapseReality()
    {
        lastPenaltyAt = Time.time;
        contamination.Stabilize(overloadStabilizeAmount);

        CharacterController controller = player == null ? null : player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        if (player != null && respawnPoint != null)
        {
            player.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
        }

        if (controller != null)
        {
            controller.enabled = true;
        }

        hud?.ShowMessage("현실 붕괴. 공간이 너를 입구로 되밀었다.");
    }
}
