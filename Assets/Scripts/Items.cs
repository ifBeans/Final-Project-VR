
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Items : MonoBehaviour
{
    [Header("Item Information")]
    public string itemName;

    [TextArea(2, 4)]
    public string description;

    [Header("Reference")]
    public RequestorManager requestorManager;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable =
            GetComponent<XRGrabInteractable>();

        if (requestorManager == null)
        {
            requestorManager =
                FindFirstObjectByType<RequestorManager>();
        }
    }

    private void OnEnable()
    {
        if (grabInteractable == null)
        {
            grabInteractable =
                GetComponent<XRGrabInteractable>();
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(
                HandleGrab
            );
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(
                HandleGrab
            );
        }
    }

    private void HandleGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Grab detected: " + itemName);

        if (requestorManager == null)
        {
            Debug.LogError(
                name + ": RequestorManager not found."
            );
            return;
        }

        requestorManager.CheckItem(this);
    }
}
