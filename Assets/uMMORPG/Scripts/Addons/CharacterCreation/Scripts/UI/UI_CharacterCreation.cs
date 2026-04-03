using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if _iMMOUMACHARACTERS
using UMA;
using UMA.CharacterSystem;
#endif

public partial class UI_CharacterCreation : MonoBehaviour
{
    [Header("[-=-[ Character Creation ]-=-]")]
    public GameObject panel;
    public GameObject centerPanel, centerPanel2;

#if _iMMOTRAITS
    public UI_CharacterTraits traitsPanel;
#endif

    public Transform content;
    public UI_CharacterSlot characterSlot;
    public bool lookAtCamera;
    public GameObject SpawnPoint;
    public NetworkManagerMMO manager;

    public Transform creationCameraLocation;
    public Transform creationCameraHeadLocation;

    public Quaternion rotationDefault;

    [Header("[-=-[ Creation Panel ]-=-]")]
    public InputField nameInput;

    public Button createButton;
    public Button cancelButton;
    public Toggle gameMasterToggle;

    protected List<Player> players;
    protected int classIndex = 0;
    protected bool bInit = false;

    [Header("[-=-[ UMA UI Configuration ]-=-]")]
    public Button btnSwitchMale;
    public Button btnSwitchFemale;
    public GameObject beardPanel;


    private GameObject go = null;
   // private player player = null;
   // public Slider heightScale;
#if _iMMOUMACHARACTERS

    [HideInInspector]
    public DynamicCharacterAvatar dca;
    [Header("[-=-[ Default Skin ]-=-]")]
    [Tooltip("In order not to have naked people you can force the display of a default skin to the player")]
    public bool enableChangeDefaultSkin = true;
    public GameObject[] panelSkin; 

    [HideInInspector] public int maleIndex = 0;
    [HideInInspector] public int femaleIndex = 0;
    [HideInInspector] public int maleClothingIndex = 0;
    [HideInInspector] public int femaleClothingIndex = 0;

    public TMP_Text skinName;
#endif



    // -----------------------------------------------------------------------------------
    // currentPlayer
    // -----------------------------------------------------------------------------------
    public Player currentPlayer
    {
        get
        {
            return players[classIndex];
        }
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show()
    {
#if _CLIENT

        if (creationCameraLocation != null && Camera.main != null)
        {
            Camera.main.transform.position = creationCameraLocation.position;
            Camera.main.transform.rotation = creationCameraLocation.rotation;
        }

        rotationDefault = SpawnPoint.transform.rotation;

        players = new List<Player>();
        players = manager.playerClasses;
        Debug.Log(players.Count + " player prefab");

        if (players == null || players.Count <= 0)
        {
            return;
        }
#if _iMMOUMACHARACTERS
        if (players[0].playerAddonsConfigurator.tmpl_UMACharacterCreation)
        {
            centerPanel.SetActive(true);
            centerPanel2.SetActive(true);
        }
#else
        centerPanel.SetActive(false);
        centerPanel2.SetActive(false);
#endif


        UIUtils.BalancePrefabs(characterSlot.gameObject, players.Count, content);
        for (int c = 0; c < players.Count; c++)
        {

            UI_CharacterSlot slot = content.GetChild(c).GetComponent<UI_CharacterSlot>();

            slot.characterName.text = players[c].name;
            int temp = c;

            slot.button.onClick.SetListener(() => {
                SetCharacterClass(temp);
            });
            
#if _iMMOUNLOCKABLECLASSES
            if (manager.networkManagerMMOUnlockableClasses.HasUnlockedClass(players[temp]))
            {
                slot.button.gameObject.SetActive(true);
            }
            else
            {
                slot.button.gameObject.SetActive(false);
            }
#else
            slot.button.gameObject.SetActive(true);
#endif
        }

#if _iMMOUNLOCKABLECLASSES
        for (int c = 0; c < players.Count; c++)
        {
            int selectedClass = c;
            if (manager.networkManagerMMOUnlockableClasses.HasUnlockedClass(players[selectedClass]))
            {
                SetCharacterClass(selectedClass);
                break;
            }
        }
#else
        SetCharacterClass(0);
#endif

        createButton.onClick.SetListener(() =>
        {
            CreateCharacter();
            nameInput.text = "";
        });

        cancelButton.onClick.SetListener(() =>
        {
            Hide();
            nameInput.text = "";
        });

        panel.SetActive(true);

        bInit = true;
#endif
    }

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        if (!bInit) return;
        createButton.interactable = manager.IsAllowedCharacterName(nameInput.text);

        //gameMasterToggle.gameObject.SetActive(NetworkServer.activeHost);
#if _iMMOUMACHARACTERS
        if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation)
        {
            btnSwitchMale.gameObject.SetActive(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.dcaRaceMale != string.Empty);
            btnSwitchFemale.gameObject.SetActive(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.dcaRaceFemale != string.Empty);
        }
#endif
    }


    public void CamGoToHead()
    {
        if(currentPlayer != null) {

            // Obtenez la position relative du GameObject par rapport � son parent
            Vector3 positionRelative = currentPlayer.headPosition.transform.position;

            // Utilisez Quaternion * Vector3 pour prendre en compte la rotation
            Vector3 offsetRotation = Vector3.zero;

            // Ajoutez la position du parent pour obtenir la position globale dans le monde
            Vector3 positionDansMonde = SpawnPoint.transform.position + positionRelative + SpawnPoint.transform.forward * 1f;
            // Obtenez la rotation dans le monde ind�pendamment du parent
            Quaternion rotationDansMonde = currentPlayer.headPosition.transform.parent.rotation * currentPlayer.headPosition.transform.rotation;

            // D�marrez la transition de la cam�ra vers la nouvelle position et rotation
            StartCoroutine(TransitionCameraCreation(Camera.main.transform.position, positionDansMonde, Camera.main.transform.rotation, rotationDansMonde));

        }

    }

    IEnumerator TransitionCameraCreation(Vector3 positionStart, Vector3 positionRequired, Quaternion rotationStart, Quaternion rotationRequired)
    {
        float elapsedTime = 0f;
        float dureeTransition = 1f; // Vous pouvez ajuster cette valeur selon votre besoin

        while (elapsedTime < dureeTransition)
        {
            // Utilisez Mathf.Lerp pour interpoler en douceur entre 0 et 1 en fonction du temps �coul�
            float t = elapsedTime / dureeTransition;

            // Calculez la nouvelle position de la cam�ra en tenant compte de la rotation
            Vector3 nouvellePosition = Vector3.Lerp(positionStart, positionRequired, t);

            // Calculez la nouvelle rotation de la cam�ra en tenant compte de la rotation
            Quaternion nouvelleRotation = Quaternion.Slerp(rotationStart, rotationRequired, t);

            // D�finissez la position et la rotation de la cam�ra sur les nouvelles valeurs
            Camera.main.transform.position = nouvellePosition;
            Camera.main.transform.rotation = nouvelleRotation;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Assurez-vous que la position et la rotation finale sont atteintes
        Camera.main.transform.position = positionRequired;
        Camera.main.transform.rotation = rotationRequired;

        // La transition est termin�e, vous pouvez ajouter ici d'autres actions si n�cessaire
        Debug.Log("Transition termin�e");
    }

    // -----------------------------------------------------------------------------------
    // CreateCharacter
    // -----------------------------------------------------------------------------------
    public virtual void CreateCharacter()
    {
        if (SpawnPoint.transform.childCount > 0)
            Destroy(SpawnPoint.transform.GetChild(0).gameObject);

#if _iMMOTRAITS
        int[] iTraits = new int[traitsPanel.currentTraits.Count];

        for (int i = 0; i < traitsPanel.currentTraits.Count; i++)
        {
            iTraits[i] = traitsPanel.currentTraits[i].name.GetStableHashCode();
        }

        CharacterCreateMsg message = new CharacterCreateMsg
        {
            name = nameInput.text,
            classIndex = classIndex,
#if !_iMMO2D
            //gameMaster = gameMasterToggle.isOn,
#endif
            traits = iTraits
#if _iMMOUMACHARACTERS
            , dna = CompressedString()
#endif
        };
#else
        CharacterCreateMsg message = new CharacterCreateMsg
        {
            name 		= nameInput.text,
            classIndex 	= classIndex
#if _iMMOUMACHARACTERS
            , dna = CompressedString()
#endif
        };

#endif

        NetworkClient.Send(message);
        Hide();
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public virtual void SetCharacterClass(int _classIndex)
    {

        Camera.main.transform.position = creationCameraLocation.position;
        Camera.main.transform.rotation = creationCameraLocation.rotation;

        classIndex = _classIndex;

        if (SpawnPoint.transform.childCount > 0)
            Destroy(SpawnPoint.transform.GetChild(0).gameObject);

        go = Instantiate(currentPlayer.gameObject, SpawnPoint.transform.position, SpawnPoint.transform.rotation);

        go.transform.parent = SpawnPoint.transform;

        if (lookAtCamera)
            go.transform.LookAt(creationCameraLocation);

        Player player = go.GetComponent<Player>();

        player.nameOverlay.gameObject.SetActive(false); // hide player name
#if _iMMOTITLES
        if(player.playerTitles)
            player.playerTitles.titleOverlay.gameObject.SetActive(false); // hide player title
#endif

#if _iMMOUMACHARACTERS
        if (player.playerAddonsConfigurator.tmpl_UMACharacterCreation)
        {
            centerPanel.SetActive(false);
            centerPanel.SetActive(true);
            centerPanel2.SetActive(true);
        }
        else
        {
            centerPanel.SetActive(false);
            centerPanel2.SetActive(false);
        }
#endif

        ItemSlot item = new ItemSlot();
        for (int i = 0; i < ((PlayerEquipment)currentPlayer.equipment).slotInfo.Length; ++i)
        {
            EquipmentInfo info = ((PlayerEquipment)currentPlayer.equipment).slotInfo[i];
            // No display weapons for character creation
            if (info.defaultItem.item != null &&
                info.requiredCategory != "MainHand" &&
                info.requiredCategory != "OffHand" &&
                info.requiredCategory != "Ammo" &&
                info.requiredCategory != "WeaponBow" &&
                info.requiredCategory != "WeaponSword" &&
                info.requiredCategory != "Shield")
            {
                item = new ItemSlot(new Item(info.defaultItem.item), info.defaultItem.amount);
            }
            player.equipment.slots.Add(item);
            ((PlayerEquipment)player.equipment).RefreshLocation(i);
        }

#if _iMMOTRAITS
        traitsPanel.Show();
#endif
#if _iMMOUMACHARACTERS
        if(player.playerAddonsConfigurator.tmpl_UMACharacterCreation)
            SetupAll();
#endif
    }

    // -----------------------------------------------------------------------------------
    // Hide
    // -----------------------------------------------------------------------------------
    public void Hide()
    {
        if (SpawnPoint.transform.childCount > 0)
            Destroy(SpawnPoint.transform.GetChild(0).gameObject);

#if _iMMOTRAITS
        traitsPanel.Hide();
#endif

        panel.SetActive(false);
        bInit = false;

        if (manager != null && manager.selectionCameraLocation != null && Camera.main != null)
        {
            Camera.main.transform.position = manager.selectionCameraLocation.position;
            Camera.main.transform.rotation = manager.selectionCameraLocation.rotation;
        }

#if _iMMOUMACHARACTERS
        dca = null;
#endif
    }

    public bool IsVisible()
    {
        return panel.activeSelf;
    }


#if _iMMOUMACHARACTERS

    public void SetupAll()
    {
        if (!enableChangeDefaultSkin)
            foreach (GameObject item in panelSkin)
                item.SetActive(false);

        dca = FindObjectOfType<DynamicCharacterAvatar>();
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        yield return new WaitForSeconds(0.1f);
        //yield return new WaitForSeconds(0.1f);
        dca.ChangeRace(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.dcaRaceMale);
        //yield return new WaitForSeconds(0.1f);
        if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale.Length > 0)
            SelectClothing(0);
        //yield return new WaitForSeconds(0.1f);
        if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.maleHairStyles.Count > 0)
            SelectHair(0);
    }

    public void SelectClothing(int index)
    {
        bool male = dca.activeRace.name == currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.dcaRaceMale;
        dca.ClearSlot("Underwear");
        // Male
        if (male)
        {

            skinName.text = currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].SkinName;

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].helmet != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].helmet);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].chest != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].chest);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].shoulder != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].shoulder);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].arms != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].arms);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].Hands != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].Hands);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].legs != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].legs);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].feets != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsMale[index].feets);

        }
        // Female
        else
        {
            skinName.text = currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].SkinName;

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].helmet != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].helmet);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].chest != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].chest);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].shoulder != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].shoulder);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].arms != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].arms);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].Hands != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].Hands);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].legs != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].legs);

            if (currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].feets != null)
                dca.SetSlot(currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.clothingsFemale[index].feets);
        }
        dca.BuildCharacter();
    }

    public void SelectHair(int index)
    {
        bool male = dca.activeRace.name == currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.dcaRaceMale;
        if(male && currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.maleHairStyles.Count > 0|| !male && currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.femaleHairStyles.Count > 0)
            dca.SetSlot((male) ? currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.maleHairStyles[index] : currentPlayer.playerAddonsConfigurator.tmpl_UMACharacterCreation.femaleHairStyles[index]);
        dca.BuildCharacter();
    }


    public void SwitchRace(string raceName)
    {
        dca.ChangeRace(raceName);
        dca.BuildCharacter();
    }

    public void SwitchGender(string genderName)
    {
        dca.ChangeRace(genderName);
        
        SelectClothing(0);
        SelectHair(0);
    }

    public void ChangeColor(UmaColorTypes colorTypes , Color col)
    {
        dca.SetColor(colorTypes.ToString(), col);
        dca.UpdateColors(true);
    }

    private String CompressedString()
    {
        return  (dca) ? CompressUMA.Compressor.CompressDna(dca.GetCurrentRecipe()) : "";
    }

#endif
    // -----------------------------------------------------------------------------------
}