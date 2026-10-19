
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RequestorManager : MonoBehaviour
{
    [Header("Available Items")]
    public List<Items> availableItems =
        new List<Items>();

    [Header("Dialogue UI")]
    public TMP_Text dialogueText;

    [Header("Replay UI")]
    public GameObject replayButton;

    [Header("Settings")]
    [Min(0.1f)]
    public float wrongMessageDuration = 2f;

    private Items requestedItem;
    private Coroutine messageRoutine;
    private bool gameWon;

    private void Start()
    {
        if (dialogueText == null)
        {
            Debug.LogError(
                "RequestorManager: Dialogue Text is not assigned."
            );
            return;
        }

        if (replayButton != null)
        {
            replayButton.SetActive(false);
        }

        SelectRandomItem();
    }

    private void SelectRandomItem()
    {
        List<Items> validItems =
            availableItems.FindAll(item => item != null);

        if (validItems.Count == 0)
        {
            Debug.LogError(
                "RequestorManager: No valid items assigned."
            );
            return;
        }

        requestedItem = validItems[
            Random.Range(0, validItems.Count)
        ];

        Debug.Log(
            "Requested item: " + requestedItem.itemName
        );

        ShowRequest();
    }

    private void ShowRequest()
    {
        if (dialogueText == null || requestedItem == null)
            return;

        dialogueText.text =
            "Please bring me " +
            requestedItem.description + ".";
    }

    public void CheckItem(Items grabbedItem)
    {
        if (gameWon || requestedItem == null ||
            grabbedItem == null)
            return;

        if (grabbedItem == requestedItem)
        {
            WinGame();
        }
        else if (messageRoutine == null)
        {
            messageRoutine = StartCoroutine(
                WrongMessageRoutine()
            );
        }
    }

    private IEnumerator WrongMessageRoutine()
    {
        dialogueText.text = "WRONG ITEM!";

        yield return new WaitForSeconds(
            wrongMessageDuration
        );

        ShowRequest();
        messageRoutine = null;
    }

    private void WinGame()
    {
        gameWon = true;

        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }

        dialogueText.text =
            "Correct! You found the " +
            requestedItem.itemName + "!";

        if (replayButton != null)
        {
            replayButton.SetActive(true);
        }

        Debug.Log("PLAYER WINS!");
    }

    public void Replay()
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }

        gameWon = false;
        requestedItem = null;

        if (replayButton != null)
        {
            replayButton.SetActive(false);
        }

        SelectRandomItem();

        Debug.Log("New round started!");
    }
}
