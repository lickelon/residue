using UnityEngine;
using UnityEngine.UI;

public sealed class PrototypeHud : MonoBehaviour
{
    [SerializeField] private ObservationContamination contamination;
    [SerializeField] private PrototypeRunState runState;
    [SerializeField] private Text statusText;
    [SerializeField] private Text messageText;
    [SerializeField] private float messageDuration = 4f;
    [SerializeField] private Color normalMessageColor = new(0.94f, 0.82f, 0.58f);
    [SerializeField] private Color warningMessageColor = new(1f, 0.52f, 0.32f);
    [SerializeField] private Color criticalMessageColor = new(1f, 0.18f, 0.16f);

    private float messageUntil;
    private int messagePriority;

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
            statusText.text = $"{anchors}\n관측 오염 {contamination.Value:0} / 단계 {contamination.Stage}\n{GetCauseStatusText()}\n문은 방 안쪽으로 열리고, 오래 바라보면 닫힌다.\n앵커를 모두 회수하고 오염 2단계 이하로 출구에 도달하라.";
        }

        if (messageText != null && Time.time > messageUntil)
        {
            messageText.text = string.Empty;
            messagePriority = 0;
        }
    }

    public void ShowMessage(string message)
    {
        ShowMessage(message, 0);
    }

    public void ShowMessage(string message, int priority)
    {
        if (messageText == null)
        {
            return;
        }

        if (Time.time < messageUntil && priority < messagePriority)
        {
            return;
        }

        messageText.text = message;
        messageText.color = GetMessageColor(priority);
        messageUntil = Time.time + messageDuration;
        messagePriority = priority;
    }

    private Color GetMessageColor(int priority)
    {
        if (priority >= 2)
        {
            return criticalMessageColor;
        }

        return priority == 1 ? warningMessageColor : normalMessageColor;
    }

    private string GetCauseStatusText()
    {
        return $"뒤돌아봄 {GetCauseStatus(ContaminationCause.TurnAround)} / 급시선 {GetCauseStatus(ContaminationCause.FastLook)}\n응시 {GetCauseStatus(ContaminationCause.LongObservation)} / 반복 확인 {GetCauseStatus(ContaminationCause.RepeatCheck)}";
    }

    private string GetCauseStatus(ContaminationCause cause)
    {
        return $"{contamination.GetCauseAmount(cause):0}({contamination.GetCauseCount(cause)})";
    }
}
