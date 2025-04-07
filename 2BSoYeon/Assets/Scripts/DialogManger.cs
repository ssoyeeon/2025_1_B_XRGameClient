using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManger : MonoBehaviour
{
    public static DialogManger Instance { get; private set; }

    [Header("Dialog References")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;

    [Header("Dialog Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffext = true;

    [Header("Dialog Choices")]
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private GameObject choicesButtonPrefab;

    private bool isTyping = false;
    private Coroutine typingCorouine;

    private DialogSO currentDialog;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if(dialogDatabase != null)
        {
            dialogDatabase.Initailize();
        }
        else
        {
            Debug.LogError("Dialog Database is not assinged to Dialog Manager");
        }
        if(NextButton != null)
        {
            NextButton.onClick.AddListener(NextDialog);
        }
        else
        {
            Debug.LogError("Next Button is not assigend!");
        }
    }
    void Start()
    {
        CloseDialog();
        StartDialog(1);
    }

    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        foreach(char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }    

    private void StopTypingEffect()
    {
        if(typingCorouine != null)
        {
            StopCoroutine(typingCorouine);
            typingCorouine = null;
        }
    }

    private void StartTypingEffect(string text)
    {
        isTyping = true;
        if(typingCorouine != null)
        {
            StopCoroutine (typingCorouine);

        }
        typingCorouine = StartCoroutine(TypeText(text));

    }

    public void StartDialog(int dialogId)
    {
        DialogSO dialog = dialogDatabase.GetDialogByld(dialogId);
        if(dialog == null)
        {
            StartDialog(dialogId);
        }
        else
        {
            Debug.LogError($"Dialog with ID {dialogId} not found!");
        }

    }
    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDialog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }

    public void ShowDialog()
    {
        if (currentDialog == null) return;
        characterNameText.text = currentDialog.characterName;

        if(useTypewriterEffext)
        {
            StartTypingEffect(currentDialog.text);
        }
        else
        {
            dialogText.text = currentDialog.text;

        }
        ClearChoices();
        if(currentDialog != null && currentDialog.choices.Count > 0)
        {
            ShowChoices();
            NextButton.gameObject.SetActive(false);
        }
        else
        {
            NextButton.gameObject.SetActive(true);
        }
    }
    public void CloseDialog()
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
        StopTypingEffect();
    }

    private void ClearChoices()
    {
        foreach(Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }
        choicesPanel.SetActive(false);
    }
    public void NextDialog()
    {
        if(isTyping)
        {
            StopTypingEffect();
            dialogText.text = currentDialog.text;
            isTyping = false;
            return;
        }
        if (currentDialog != null && currentDialog.nextiId > 0 )
        {
            DialogSO nextDialog = dialogDatabase.GetDialogByld(currentDialog.nextiId);
            if(nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();

            }

        }
        else
        {
            CloseDialog();
        }
    }
    public void SelectChoice(DialogChoiceSO choice)
    {
        if(choice != null && choice.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogByld(choice.nextId);
            if (nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }
    private void ShowChoices()
    {
        choicesPanel.SetActive(true);
        foreach(var choice in currentDialog.choices)
        {
            GameObject choiceGO = Instantiate(choicesButtonPrefab, choicesPanel.transform);
            TextMeshProUGUI buttonText = choiceGO.GetComponentInChildren<TextMeshProUGUI>();
            Button button = choiceGO.GetComponent<Button>();

            if(buttonText != null)
            {
                buttonText.text = choice.text;

            }
            if(button != null)
            {
                DialogChoiceSO choiceSO = choice;
                button.onClick.AddListener(() => SelectChoice(choiceSO));
            }
        }
    }
}
