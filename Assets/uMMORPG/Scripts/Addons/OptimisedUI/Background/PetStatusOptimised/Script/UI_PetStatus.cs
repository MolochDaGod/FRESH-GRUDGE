using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_PetStatus : MonoBehaviour
{
    public GameObject panel;
    public Button backgroundButton;
    public Image image;
    public Slider healthSlider;
    public Slider experienceSlider;
    public Text nameText;
    public Text levelText;
    public Toggle attackToggle;
    public Toggle stayToggle;
    public Toggle followToggle;
    public KeyCode attackKey;
    public KeyCode stayKey;
    public KeyCode followKey;
    public Button unsummonButton;


    [Header("[-=-=-[ Refresh Time ]-=-=-]")]
    public float refreshSpeed = .5f;

    public void Start()
    {
        StartCoroutine(CR_PetStatus());
    }
    private IEnumerator CR_PetStatus()
    {
        while (true)
        {

            UpdatePetStatus();

            yield return new WaitForSeconds(refreshSpeed);

        }
    }
    void Update()
    {
        // Check if the attack key is pressed
        if (Input.GetKeyDown(attackKey))
        {
            ToggleAttack();
        }

        // Check if the stay key is pressed
        if (Input.GetKeyDown(stayKey))
        {
            ToggleStay();
        }

        // Check if the follow key is pressed
        if (Input.GetKeyDown(followKey))
        {
            ToggleFollow();
        }
    }

    void ToggleAttack()
    {
        attackToggle.isOn = !attackToggle.isOn; // Toggle the state
    }

    void ToggleStay()
    {
        stayToggle.isOn = !stayToggle.isOn; // Toggle the state
    }

    void ToggleFollow()
    {
        followToggle.isOn = !followToggle.isOn; // Toggle the state
    }

    void UpdatePetStatus()
    {
        Player player = Player.localPlayer;

        if (player != null &&
            player.petControl.activePet != null)
        {
            Pet pet = player.petControl.activePet;
            panel.SetActive(true);

            image.sprite = pet.portraitIcon;

            backgroundButton.onClick.SetListener(() => {
                // pet variable might be null by the TimeLogout button gets
                // clicked. can't target null, otherwise we get a
                // MissingReferenceException.
                if (pet != null)
                    player.CmdSetTarget(pet.netIdentity);
            });

            healthSlider.value = pet.health.Percent();
            healthSlider.GetComponent<UIShowToolTip>().text = "Health: " + pet.health.current + " / " + pet.health.max;

            experienceSlider.value = pet.experience.Percent();
            experienceSlider.GetComponent<UIShowToolTip>().text = "Experience: " + pet.experience.current + " / " + pet.experience.max;

            nameText.text = pet.name;
            levelText.text = "Lv." + pet.level.current;

            attackToggle.onValueChanged.SetListener((value) =>
            {
                if (pet != null && player.target != null)
                {
                    pet.CmdAttackTarget(player.target);
                }
                else if (pet != null && (player.target == null || Vector3.Distance(player.target.transform.position, pet.transform.position) > pet.returnDistance))
                {
                    if (!pet.followingOwner && !pet.defendOwner)
                    {
                        pet.CmdFollowOnly(true);
                    }
                    else if (pet.followingOwner)
                    {
                        pet.CmdSetDefendOwner(true);
                        pet.CmdFollowOnly(false);
                    }
                    else if (pet.defendOwner)
                    {
                        pet.CmdSetDefendOwner(false);
                        pet.CmdFollowOnly(true);
                    }
                }
            });
            stayToggle.onValueChanged.SetListener((value) =>
            {
                if (pet != null)
                    pet.CmdSetStayPut(value);
            });
            followToggle.onValueChanged.SetListener((value) =>
            {
                pet.CmdFollowOnly(value);
            });

            //unsummonButton.interactable = player.CanUnsummonPet(); <- looks too annoying if button flashes rapidly
            unsummonButton.onClick.SetListener(() => {
                if (player.petControl.CanUnsummon())
                    player.petControl.CmdUnsummon();
            });
        }
        else panel.SetActive(false);
    }
}
