
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[RequireComponent(typeof(TeleportationAnchor))]
public class ResetGameOnTeleport: MonoBehaviour
{
    private TeleportationAnchor anchor;
    private bool isResetting;

    private void Awake()
    {
        anchor = GetComponent<TeleportationAnchor>();
    }

    private void OnEnable()
    {
        anchor.teleporting.AddListener(OnTeleporting);
    }

    private void OnDisable()
    {
        anchor.teleporting.RemoveListener(OnTeleporting);
    }

    private void OnTeleporting(
        TeleportingEventArgs args)
    {
        if (isResetting)
            return;

        isResetting = true;
        StartCoroutine(ResetScene());
    }

    private IEnumerator ResetScene()
    {
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
