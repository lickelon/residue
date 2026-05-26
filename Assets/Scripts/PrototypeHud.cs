using UnityEngine;
using UnityEngine.UI;

public sealed class PrototypeHud : MonoBehaviour
{
    [SerializeField] private ObservationContamination contamination;
    [SerializeField] private PrototypeRunState runState;
    [SerializeField] private Text statusText;
    [SerializeField] private Text messageText;
    [SerializeField] private float messageDuration = 4f;

    private float messageUntil;

    private void Awake()
    {
        if (contamination == null)
        {
            contamination = FindFirstObjectByType<ObservationContamination>();
        }

        if (runState == null)
        {
            runState = FindFirstObjectByType<PrototypeRunState>();
        }
    }

    private void Update()
    {
        if (statusText != null && contamination != null)
        {
            string anchors = runState == null ? "앵커 0/3" : $"앵커 {runState.CollectedAnchors}/{runState.RequiredAnchors}";
            statusText.text = $"{anchors}\n관측 오염 {contamination.Value:0} / 단계 {contamination.Stage}\nE로 앵커와 문을 조작한다.\n앵커를 모두 회수하고 오염 2단계 이하로 출구에 도달하라.";
        }

        if (messageText != null && Time.time > messageUntil)
        {
            messageText.text = string.Empty;
        }
    }

    public void ShowMessage(string message)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
        messageUntil = Time.time + messageDuration;
    }
}
