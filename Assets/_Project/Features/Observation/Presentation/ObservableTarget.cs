using UnityEngine;

[DisallowMultipleComponent]
public sealed class ObservableTarget : MonoBehaviour
{
    [SerializeField] private string targetId;

    public string TargetId => string.IsNullOrWhiteSpace(targetId) ? gameObject.name : targetId;

    private void Reset()
    {
        targetId = gameObject.name;
    }
}
