using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequestorManager : MonoBehaviour
{
    [Header("Available Items")]
    [Tooltip("List of all shirts available in the store that can be requested.")]
    public List<Items> availableItems = new List<Items>();

    [Header("Speech Bubble UI (Drawing Style)")]
    [Tooltip("UI Image displaying the T-Shirt with the requested pattern.")]
    public Image shirtImage;

    [Tooltip("Question mark text displayed beside the T-shirt (changes to checkmark on win).")]
    public TMP_Text questionMarkText;

    [Tooltip("Status and feedback text in the speech bubble.")]
    public TMP_Text dialogueText;

    [Tooltip("Optional header / title text.")]
    public TMP_Text titleText;

    [Header("Delivery Mechanics")]
    [Tooltip("Point where the delivered shirt will be neatly placed upon correct delivery.")]
    public Transform deliveryPoint;

    [Tooltip("Replay / Next Request button.")]
    public GameObject replayButton;

    [Min(0.1f)]
    public float wrongMessageDuration = 2.5f;

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip requestSound;
    public AudioClip successSound;
    public AudioClip wrongSound;

    [Header("Visual Effects")]
    public GameObject confettiEffect;

    [Header("Gameplay State")]
    [SerializeField] private Items requestedItem;
    private Items deliveredItem;
    private Coroutine feedbackRoutine;
    private bool gameWon;

    public Items RequestedItem => requestedItem;
    public bool IsGameWon => gameWon;

    private void Start()
    {
        if (replayButton != null)
        {
            replayButton.SetActive(false);
        }

        if (confettiEffect != null)
        {
            confettiEffect.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (availableItems.Count == 0)
        {
            availableItems.AddRange(FindObjectsByType<Items>(FindObjectsSortMode.None));
        }

        SelectRandomItem();
    }

    public void SelectRandomItem()
    {
        List<Items> validItems = availableItems.FindAll(item => item != null);

        if (validItems.Count == 0)
        {
            Debug.LogError("RequestorManager: No valid items assigned.");
            if (dialogueText != null) dialogueText.text = "No shirts found!";
            return;
        }

        if (validItems.Count > 1 && requestedItem != null)
        {
            List<Items> otherItems = validItems.FindAll(item => item != requestedItem);
            requestedItem = otherItems[Random.Range(0, otherItems.Count)];
        }
        else
        {
            requestedItem = validItems[Random.Range(0, validItems.Count)];
        }

        Debug.Log("NPC Requested item: " + requestedItem.itemName);

        ShowRequest();

        // Audio playback disabled per user request
        // if (audioSource != null && requestSound != null)
        // {
        //     audioSource.PlayOneShot(requestSound);
        // }
    }

    public void ShowRequest()
    {
        if (requestedItem == null) return;

        // Ensure shirt image icon and question mark are deactivated
        if (shirtImage != null)
        {
            shirtImage.gameObject.SetActive(false);
        }

        if (questionMarkText != null)
        {
            questionMarkText.gameObject.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.text = "";
        }

        if (dialogueText != null)
        {
            string desc = !string.IsNullOrEmpty(requestedItem.description)
                ? requestedItem.description
                : (requestedItem.itemData != null ? requestedItem.itemData.description : "");

            if (!string.IsNullOrEmpty(desc))
            {
                dialogueText.text = $"<size=20>Please bring me \"{desc}\"</size>";
            }
            else
            {
                dialogueText.text = $"<b>{requestedItem.itemName}</b>";
            }
        }
    }

    /// <summary>
    /// Attempts to deliver an item to the NPC. ONLY the requested shirt is accepted!
    /// </summary>
    public bool DeliverItem(Items broughtItem)
    {
        if (gameWon || requestedItem == null || broughtItem == null)
            return false;

        // Strict verification: only the requested shirt can be delivered!
        if (broughtItem == requestedItem)
        {
            AcceptDelivery(broughtItem);
            return true;
        }
        else
        {
            RejectDelivery(broughtItem);
            return false;
        }
    }

    public void CheckItem(Items broughtItem)
    {
        DeliverItem(broughtItem);
    }

    private void AcceptDelivery(Items item)
    {
        gameWon = true;
        deliveredItem = item;

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        // Snap and lock the delivered shirt into the delivery stand/point
        if (deliveryPoint != null)
        {
            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            item.transform.position = deliveryPoint.position;
            item.transform.rotation = deliveryPoint.rotation;
        }

        if (questionMarkText != null)
        {
            questionMarkText.gameObject.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.text = "<size=26><color=#2ECC71><b>Thank you!</b></color></size>";
        }

        if (titleText != null)
        {
            titleText.text = "";
        }

        // Play success audio
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        if (confettiEffect != null)
        {
            confettiEffect.SetActive(true);
            var ps = confettiEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);
            }
        }

        // Replay / Next Request button removed per user request
        // if (replayButton != null)
        // {
        //     replayButton.SetActive(true);
        // }

        Debug.Log($"NPC: Successfully received requested shirt {item.itemName}! Delivery completed.");
    }

    private void RejectDelivery(Items wrongItem)
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(WrongItemRoutine(wrongItem));
    }

    private IEnumerator WrongItemRoutine(Items wrongItem)
    {
        // Audio playback disabled per user request
        // if (audioSource != null && wrongSound != null)
        // {
        //     audioSource.PlayOneShot(wrongSound);
        // }

        if (questionMarkText != null)
        {
            questionMarkText.gameObject.SetActive(false);
        }

        if (dialogueText != null)
        {
            string wrongName = wrongItem != null ? wrongItem.itemName : "this shirt";
            dialogueText.text = $"<size=22><color=#E74C3C><b>Not this one!</b></color></size>\n\n<size=18>That's the <i>{wrongName}</i>.</size>";
        }

        Debug.Log($"NPC: Rejected wrong item {wrongItem.itemName}. Only {requestedItem.itemName} is accepted!");

        yield return new WaitForSeconds(wrongMessageDuration);

        ShowRequest();
        feedbackRoutine = null;
    }

    public void Replay()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        // Unlock previous delivered shirt if it was locked
        if (deliveredItem != null)
        {
            var rb = deliveredItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            deliveredItem = null;
        }

        gameWon = false;
        requestedItem = null;

        if (confettiEffect != null)
        {
            confettiEffect.SetActive(false);
        }

        if (replayButton != null)
        {
            replayButton.SetActive(false);
        }

        SelectRandomItem();
        Debug.Log("New NPC request started!");
    }

    private void OnTriggerEnter(Collider other)
    {
        Items item = other.GetComponentInParent<Items>();
        if (item != null)
        {
            DeliverItem(item);
        }
    }
}
