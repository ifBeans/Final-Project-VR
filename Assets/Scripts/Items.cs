using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Items : MonoBehaviour
{
    private static readonly FieldInfo wasKinematicField =
        typeof(XRGrabInteractable).GetField("m_WasKinematic", BindingFlags.NonPublic | BindingFlags.Instance);

    [Header("Item Data (Optional Preset)")]
    public SearchItemData itemData;

    [Header("Item Information")]
    public string itemName;

    [TextArea(2, 4)]
    public string description;

    [Header("Visual Representation")]
    public Sprite shirtIcon;
    public Texture shirtTexture;

    [Header("Gameplay Options")]
    [Tooltip("If true, checks immediately when grabbed. If false (default), the player must bring the item back to the NPC.")]
    public bool checkOnGrab = false;

    [Header("Reference")]
    public RequestorManager requestorManager;

    private XRGrabInteractable grabInteractable;

    public bool IsHeld => grabInteractable != null && grabInteractable.isSelected;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.forceGravityOnDetach = true;
        }

        if (itemData != null)
        {
            if (string.IsNullOrEmpty(itemName)) itemName = itemData.itemName;
            if (string.IsNullOrEmpty(description)) description = itemData.description;
            if (shirtIcon == null) shirtIcon = itemData.itemIcon;
            if (shirtTexture == null) shirtTexture = itemData.itemTexture;
        }

        if (requestorManager == null)
        {
            requestorManager = FindFirstObjectByType<RequestorManager>();
        }
    }

    private void OnEnable()
    {
        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        if (grabInteractable != null)
        {
            grabInteractable.forceGravityOnDetach = true;
            grabInteractable.selectEntered.AddListener(HandleGrab);
            grabInteractable.selectExited.AddListener(HandleDrop);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(HandleGrab);
            grabInteractable.selectExited.RemoveListener(HandleDrop);
        }
    }

    private void HandleGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Grab detected: " + itemName);

        if (grabInteractable != null)
        {
            grabInteractable.forceGravityOnDetach = true;
            wasKinematicField?.SetValue(grabInteractable, false);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (!checkOnGrab)
        {
            // By default, gameplay requires bringing the shirt to the NPC
            return;
        }

        if (requestorManager == null)
        {
            Debug.LogError(name + ": RequestorManager not found.");
            return;
        }

        requestorManager.CheckItem(this);
    }

    private void HandleDrop(SelectExitEventArgs args)
    {
        StartCoroutine(EnsureDropPhysicsRoutine());
    }

    private IEnumerator EnsureDropPhysicsRoutine()
    {
        // Check if item was locked by NPC delivery
        if (requestorManager != null && requestorManager.IsGameWon && requestorManager.RequestedItem == this)
        {
            yield break;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Wait until end of physics step so XRGrabInteractable's internal detach has completed
        yield return new WaitForFixedUpdate();

        if (rb != null && !IsHeld)
        {
            if (requestorManager != null && requestorManager.IsGameWon && requestorManager.RequestedItem == this)
            {
                yield break;
            }

            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
