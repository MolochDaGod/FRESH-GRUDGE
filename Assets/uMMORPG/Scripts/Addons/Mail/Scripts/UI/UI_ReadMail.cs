using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// UI READ MAIL
public partial class UI_ReadMail : MonoBehaviour
{
    public static UI_ReadMail singleton;
    public GameObject panel;
    public RectTransform messagesContent;
    public RectTransform readContent;
    public GameObject messageSlot;
    public GameObject readMailPanel;
    public UI_SendMail sendMailPanel;
    public Button newMailButton;
    public Button takeItemButton;
    public Button deleteButton;
    public Text receivedText;
    public Text expiresText;
    public Text fromText;
    public Text subjectText;
    public Text bodyText;

    private int readingIndex = -1;
    private int cnt = 0;

    [Header("[-=-=-[ Refresh Time ]-=-=-]")]
    public float refreshSpeed = .2f;

    public void Start()
    {
        StartCoroutine(CR_ReadMail());
    }
    private IEnumerator CR_ReadMail()
    {
        while (true)
        {

            UpdateReadMail();

            yield return new WaitForSeconds(refreshSpeed);

        }
    }
    public void Awake()
    {
        if (singleton == null) singleton = this;
    }
    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void UpdateReadMail()
    {
        Player player = Player.localPlayer;
        if (player == null) return;
        if (player.playerAddonsConfigurator && player.playerAddonsConfigurator.mailSettings != null)
        {
            long current = Epoch.Current();

            //-- setup send mail button
            newMailButton.gameObject.SetActive(player.playerAddonsConfigurator.mailSettings.mailSendFromAnywhere);
            /*newMailButton.onClick.SetListener(() =>
            {
                sendMailPanel.Show();
                readMailPanel.SetActive(false);
            });*/

            // -- setup delete button
            /*deleteButton.onClick.SetListener(() =>
            {
                if (NetworkTime.time >= player.nextRiskyActionTime)
                {
                    readingIndex = -1;

                    for (int i = 0; i < messagesContent.childCount; ++i)
                    {
                        int idx = i;
                        UIMailMessageSlot slot = messagesContent.GetChild(idx).GetComponent<UIMailMessageSlot>();
                        if (slot.toggle.isOn)
                            player.playerMailComponent.CmdMail_DeleteMessage(slot.mailIndex);
                    }
                }
            });*/

            //-- setup take item button
            takeItemButton.gameObject.SetActive(readingIndex > -1);

            //count messages that haven't expired yet
            int mailCount = player.playerAddonsConfigurator.mailMessages.Count((m) => current <= m.expires);
            UIUtils.BalancePrefabs(messageSlot, mailCount, messagesContent);

            if (mailCount != cnt)
            {
                cnt = mailCount;
            }

            int slotIndex = -1;

            //loop over messages backwards because we add to the synclist so newer messages appear at end, we want to display newer on top
            for (int mailIndex = player.playerAddonsConfigurator.mailMessages.Count - 1; mailIndex >= 0; mailIndex--)
            {
                MailMessage message = player.playerAddonsConfigurator.mailMessages[mailIndex];

                //if message has expired, skip it
                if (current > message.expires) continue;

                int tmpIndex = mailIndex;

                slotIndex++;
                UIMailMessageSlot slot = messagesContent.GetChild(slotIndex).GetComponent<UIMailMessageSlot>();
                slot.textReceived.text = message.sentAt;
                slot.textFrom.text = message.from;
                slot.textSubject.text = message.subject;
                slot.mailIndex = tmpIndex;
                UI_MailItemSlot slotItem = slot.GetComponentInChildren<UI_MailItemSlot>();
                if (message.item != null)
                {
#if _iMMOITEMRARITY
                    slotItem.RatityOutline.color = RarityColor.SetRarityColorResult(message.item);
#endif
                    ItemSlot item = new ItemSlot(new Item(message.item));
                    slot.itemSlot.interactable = false;
                    slot.itemSlot.GetComponent<Image>().color = Color.white;
                    slot.itemSlot.GetComponent<Image>().sprite = message.item.image;
                    slot.itemSlot.GetComponent<UIShowToolTip>().enabled = true;
                    slot.itemSlot.GetComponent<UIShowToolTip>().text = item.ToolTip();
                }
                else
                {

#if _iMMOITEMRARITY
                    slotItem.RatityOutline.color = Color.clear;// RarityColor.SetRarityColorResult(itemData);
#endif
                    slot.itemSlot.interactable = false;
                    slot.itemSlot.GetComponent<Image>().color = Color.clear;
                    slot.itemSlot.GetComponent<Image>().sprite = null;
                    slot.itemSlot.GetComponent<UIShowToolTip>().enabled = false;
                }

                //if the message has been read, show normal font, else bold text
                if (message.read > 0)
                {
                    slot.textReceived.fontStyle = FontStyle.Normal;
                    slot.textFrom.fontStyle = FontStyle.Normal;
                    slot.textSubject.fontStyle = FontStyle.Normal;
                }
                else
                {
                    slot.textReceived.fontStyle = FontStyle.Bold;
                    slot.textFrom.fontStyle = FontStyle.Bold;
                    slot.textSubject.fontStyle = FontStyle.Bold;
                }

                //click on slot, read message
                slot.readButton.onClick.SetListener(() =>
                {
                    readingIndex = tmpIndex;
                    if (message.read == 0)
                    {
                        player.playerAddonsConfigurator.CmdMail_ReadMessage(tmpIndex);
                    }
                });
            }

            // currently selected message
            if (readingIndex > -1)
            {
                MailMessage reading = player.playerAddonsConfigurator.mailMessages[readingIndex];

                readContent.gameObject.SetActive(true);
                receivedText.text = reading.sentAt;
                expiresText.text = reading.expiresAt;
                fromText.text = reading.from;
                subjectText.text = reading.subject;
                bodyText.text = reading.body;
                UI_MailItemSlot slot = takeItemButton.GetComponentInParent<UI_MailItemSlot>();
                if (reading.item != null)
                {

                    ItemSlot item = new ItemSlot(new Item(reading.item));
                    ScriptableItem itemData = reading.item;
                    takeItemButton.onClick.SetListener(() =>
                    {
                        player.playerAddonsConfigurator.CmdMail_TakeItem(readingIndex);
                        takeItemButton.interactable = false;
                    });
#if _iMMOITEMRARITY
                    slot.RatityOutline.color = RarityColor.SetRarityColorResult(itemData);
#endif
                    // refresh item
                    slot.tooltip.text = item.ToolTip();
                    slot.image.color = Color.white;
                    slot.image.sprite = itemData.image;
                    //slot.nameText.text = itemData.name;
                    /*takeItemButton.interactable = true;
                    takeItemButton.GetComponent<Image>().color = Color.white;
                    takeItemButton.GetComponent<Image>().sprite = reading.item.image;
                    takeItemButton.GetComponent<UIShowToolTip>().enabled = true;
                    takeItemButton.GetComponent<UIShowToolTip>().text = item.ToolTip();*/
                }
                else
                {
#if _iMMOITEMRARITY
                    slot.RatityOutline.color = Color.clear;
#endif
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    //                takeItemButton.GetComponent<Image>().color = Color.clear;
                    //                takeItemButton.GetComponent<Image>().sprite = null;
                    //                takeItemButton.GetComponent<UIShowToolTip>().enabled = false;
                }
            }
            else
            {
                readContent.gameObject.SetActive(false);
            }
        }
    }

    public void btnNewMail()
    {
        sendMailPanel.Show();
        readMailPanel.SetActive(false);
    }

    public void btnDeleteMail()
    {
        Player player = Player.localPlayer;
        if (player == null) return;

        if (NetworkTime.time >= player.nextRiskyActionTime)
        {
            readingIndex = -1;

            for (int i = 0; i < messagesContent.childCount; ++i)
            {
                int idx = i;
                UIMailMessageSlot slot = messagesContent.GetChild(idx).GetComponent<UIMailMessageSlot>();
                if (slot.toggle.isOn)
                    player.playerAddonsConfigurator.CmdMail_DeleteMessage(slot.mailIndex);
            }
        }
    }
    // -----------------------------------------------------------------------------------
}