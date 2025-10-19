using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmPopup : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text messageText;
    public Button confirmButton;
    public Button cancelButton;

    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        gameObject.SetActive(false); 
    }

    public void Show(string message, Action confirmAction, Action cancelAction = null)
    {
        messageText.text = message;
        onConfirm = confirmAction;
        onCancel = cancelAction;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Confirm()
    {
        onConfirm?.Invoke();
        Hide();
    }

    public void Cancel()
    {
        onCancel?.Invoke();
        Hide();
    }
}