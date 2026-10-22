using UnityEngine;

public class NPCDeliveryZone : MonoBehaviour
{
    [Header("Manager Reference")]
    public RequestorManager requestorManager;

    [Header("Visual Feedback")]
    public GameObject dropPromptVisual;

    private void Awake()
    {
        if (requestorManager == null)
        {
            requestorManager = FindFirstObjectByType<RequestorManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (requestorManager == null) return;

        Items item = other.GetComponentInParent<Items>();
        if (item != null)
        {
            // Only the requested shirt will be accepted by DeliverItem!
            bool delivered = requestorManager.DeliverItem(item);
            if (delivered)
            {
                Debug.Log($"[NPCDeliveryZone] Correct shirt delivered: {item.itemName}");
            }
            else
            {
                Debug.Log($"[NPCDeliveryZone] Wrong item rejected: {item.itemName}");
            }
        }
    }
}
