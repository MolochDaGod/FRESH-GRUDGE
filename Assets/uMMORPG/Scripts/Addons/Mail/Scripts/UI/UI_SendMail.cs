
// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

// UI SEND MAIL

public partial class UI_SendMail : MonoBehaviour
{

    protected NetworkManagerMMO manager;
    protected NetworkAuthenticatorMMO auth;
    
    public static UI_SendMail singleton;
    public GameObject panel;

    public RectTransform searchPanel;
    public InputField search;
    public RectTransform searchContent;
    public UIMailSearchSlot searchSlot;
    public Button searchButton;

    public RectTransform messagePanel;
    public Text recipientText;
    public InputField subject;
    public InputField body;

    public Text dialogMessage;
    public GameObject dialog;
    public Button dialogButton;

    public Button acceptButton;
    public Button cancelButton;

    public UIDragAndDropable mailItemSlot;

    private bool sending = false;
    [HideInInspector] public string recipient;
    [HideInInspector] public int itemMailIndex = -1;


    [Header("[-=-[ Refresh Time ]-=-]")]
    public float refreshSpeed = .2f;

    public void Start()
    {
        StartCoroutine(CR_SendMail());
    }
    private IEnumerator CR_SendMail()
    {
        while (true)
        {

            UpdateSendMail();

            yield return new WaitForSeconds(refreshSpeed);

        }
    }
    // -----------------------------------------------------------------------------------
    // UI_SendMail
    // -----------------------------------------------------------------------------------
    public UI_SendMail()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show()
    {
        itemMailIndex = -1;
        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    void UpdateSendMail()
    {
        // Debug.Log(" La ? " + itemMailIndex);
        Player player = Player.localPlayer;
        if (!player) return;

        if (manager == null)
            manager = FindObjectOfType<NetworkManagerMMO>();

        if (auth == null)
            auth = manager.GetComponent<NetworkAuthenticatorMMO>();

        if (manager != null && panel.activeSelf)
        {
            // item slot
            if (itemMailIndex != -1 && itemMailIndex < player.inventory.slots.Count &&
                player.inventory.slots[itemMailIndex].amount > 0) 
            {
                ItemSlot itemSlot = player.inventory.slots[itemMailIndex];
                //ScriptableItem item = player.inventory.slots[itemMailIndex].item.data;
                mailItemSlot.GetComponent<Image>().color = Color.white;
                //itemSlot.GetComponent<Image>().sprite = item.image;
                mailItemSlot.GetComponent<Image>().sprite = itemSlot.item.image;
                mailItemSlot.GetComponent<UIShowToolTip>().enabled = true;
                mailItemSlot.GetComponent<UIShowToolTip>().text = itemSlot.ToolTip();
                mailItemSlot.dragable = true;
            }
            else
            {
                // show default mailItemSlot panel in UI
                mailItemSlot.GetComponent<Image>().color = Color.clear;
                mailItemSlot.GetComponent<Image>().sprite = null;
                mailItemSlot.GetComponent<UIShowToolTip>().enabled = false;
                mailItemSlot.dragable = false;
            }
            mailItemSlot.dragable = true;
            // no one selected yet, show search box
            if (string.IsNullOrEmpty(recipient))
            {
                searchPanel.gameObject.SetActive(true);
                messagePanel.gameObject.SetActive(false);

                //itemMailIndex = -1;
                if (search.text.Length > 0 &&
                    search.text.Length <= auth.accountMaxLength &&
                    Regex.IsMatch(search.text, @"^[a-zA-Z0-9_]+$"))
                {
                    searchButton.interactable = true;
                }
                else
                {
                    searchButton.interactable = false;
                }
                searchButton.onClick.SetListener(() =>
                {
                    if (NetworkTime.time >= player.nextRiskyActionTime)
                    {
                        //prepare and send search request, get response
                        sending = true;
                        search.interactable = false;
                        searchButton.interactable = false;
                        player.playerAddonsConfigurator.CmdMail_Search(search.text);
                        dialogMessage.text = "Searching...";
                        dialog.SetActive(true);
                    }
                });
            }
            else
            {
                Debug.Log("else " + itemMailIndex);
                searchPanel.gameObject.SetActive(false);
                messagePanel.gameObject.SetActive(true);
                recipientText.text = recipient;

                acceptButton.interactable = !string.IsNullOrEmpty(subject.text) && player.playerAddonsConfigurator.mailSettings.costPerMail.CheckCost(player);
                acceptButton.onClick.SetListener(() =>
                {
                    if (NetworkTime.time >= player.nextRiskyActionTime)
                    {
                        sending = true;
                        dialogMessage.text = "Sending Mail...";
                        dialog.SetActive(true);
                        player.playerAddonsConfigurator.CmdMail_Send(recipient, subject.text, body.text, itemMailIndex);
                        itemMailIndex = -1;
                    }
                });

                dialogButton.onClick.SetListener(() =>
                {
                    cancelButton.onClick.Invoke();
                });
            }

            //show the dialog button if we are not sending
            dialogButton.gameObject.SetActive(!sending);

            // cancel
            cancelButton.interactable = !sending;
            cancelButton.onClick.SetListener(() =>
            {
                recipient = "";
                search.text = "";
                subject.text = "";
                body.text = "";
                sending = false;

                itemMailIndex = -1;
                UIUtils.BalancePrefabs(searchSlot.gameObject, 0, searchContent);
                dialog.SetActive(false);
                panel.SetActive(false);
            });
        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public void UpdateSearchResults(List<MailSearch> results)
    {
        UIUtils.BalancePrefabs(searchSlot.gameObject, results.Count, searchContent);

        for (int i = 0; i < results.Count; ++i)
        {
            UIMailSearchSlot slot = searchContent.GetChild(i).GetComponent<UIMailSearchSlot>();
            slot.nameText.text = results[i].name;
            slot.levelText.text = results[i].level.ToString();
            slot.guildText.text = results[i].guild == null ? "" : results[i].guild;
            slot.actionButton.onClick.SetListener(() =>
            {
                recipient = slot.nameText.text;
            });
        }

        searchButton.interactable = true;
        search.interactable = true;
        sending = false;
        dialog.SetActive(false);
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public void MailMessageSent(string status)
    {
        dialogMessage.text = status;
        sending = false;
        itemMailIndex = -1;

        dialog.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
}
