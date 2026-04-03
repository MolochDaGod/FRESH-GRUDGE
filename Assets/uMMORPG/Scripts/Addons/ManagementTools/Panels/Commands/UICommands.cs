#if _SQLITE
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICommands : MonoBehaviour
{
    public static UICommands singleton;
    public Canvas canvas;

    #region Commands - Buttons
    public Button banButton;                    // access 0
    public Button downButton;                   // access 1
    public Button upButton;                     // access 2

    public Button immortalButton;
    public TMP_Text immortalButtonText;
    
    public Button hideButton;
    public TMP_Text hideButtonText;
    
    public Button freezeButton;
    public TMP_Text freezeButtonText;

    public Button muteButton;
    public TMP_Text muteButtonText;

    public Button kickButton;

    public Button killButton;
    public TMP_Text killButtonText;

    public Button fetchButton;

    public Button goButton;

    public Button goldButton;
    public TMP_InputField goldInput;

    public Button coinsButton;
    public TMP_InputField coinsInput;

    public Button expButton;
    public TMP_InputField expInput;

    public Button levelButton;
    public TMP_InputField levelInput;

    public Button strengthButton;
    public TMP_InputField strengthInput;

    public Button intelligenceButton;
    public TMP_InputField intelligenceInput;
    #endregion

    #region Player Information
    public TMP_Text nameText;
    public TMP_Text accountText;
    public TMP_Text accessText;
    public TMP_Text levelText;
    public TMP_Text goldText;
    public TMP_Text coinsText;
    public TMP_Text classText;

    public Slider healthSlider;

    public TMP_Text healthText;
    public TMP_Text healthMaxText;

    public Slider manaSlider;

    public TMP_Text manaText;
    public TMP_Text manaMaxText;

    public TMP_Text strengthText;
    public TMP_Text intelligenceText;
    public TMP_Text immortalText;
    public TMP_Text invisibleText;
    public TMP_Text frozenText;
    public TMP_Text mutedText;
    #endregion

    #region Accessors - Health
    int _health;
    public int Health
    {
        get { return _health; }
        set
        {
            if (_health != value)
            {
                _health = value;
                healthText.text = value.ToString();
                killButtonText.text = value == 0 ? "Resurrect" : "Kill";
                //Debug.Log("health[done]");
            }
        }
    }

    int _healthMax;
    public int HealthMax
    {
        get { return _healthMax; }
        set
        {
            if (_healthMax != value)
            {
                _healthMax = value;
                healthMaxText.text = value.ToString();
                //Debug.Log("healthMax[done]");
            }
        }
    }
    #endregion

    #region Accessors - Mana
    int _mana;
    public int Mana
    {
        get { return _mana; }
        set
        {
            if (_mana != value)
            {
                _mana = value;
                manaText.text = value.ToString();
                //Debug.Log("mana[done]");
            }
        }
    }

    int _manaMax;
    public int ManaMax
    {
        get { return _manaMax; }
        set
        {
            if (_manaMax != value)
            {
                _manaMax = value;
                manaMaxText.text = value.ToString();
                //Debug.Log("manaMax[done]");
            }
        }
    }
    #endregion

    #region Accessors - Account
    string _account;
    public string Account
    {
        get { return _account; }
        set
        {
            if (_account != value)
            {
                _account = value;
                accountText.text = value;
                //Debug.Log("account[done]");
            }
        }
    }
    #endregion

    #region Accessors - Access
    int _access;
    public int Access
    {
        get { return _access; }
        set
        {
            if (_access != value)
            {
                _access = value;
                accessText.text = value.ToString();
                banButton.interactable = (UIManagement.singleton.currentName != UIManagement.singleton.player.name) && value == 1;
                downButton.interactable = value == 2;
                upButton.interactable = value == 1;
                //Debug.Log("access[done]");
            }
        }
    }
    #endregion

    #region Accessors - Class
    string _class;
    public string ClassName
    {
        get { return _class; }
        set
        {
            if (_class != value)
            {
                _class = value;
                classText.text = value;
                //Debug.Log("class[done]");
            }
        }
    }
    #endregion

    #region Accessors - Level
    int _level;
    public int Level
    {
        get { return _level; }
        set
        {
            if (_level != value)
            {
                _level = value;
                levelText.text = value.ToString();
                //Debug.Log("level[done]");
            }
        }
    }
    #endregion

    #region Accessors - Coins
    long _coins;
    public long Coins
    {
        get { return _coins; }
        set
        {
            if (_coins != value)
            {
                _coins = value;
                coinsText.text = value.ToString();
                //Debug.Log("coins[done]");
            }
        }
    }
    #endregion

    #region Accessors - Gold
    long _gold;
    public long Gold
    {
        get { return _gold; }
        set
        {
            if (_gold != value)
            {
                _gold = value;
                goldText.text = value.ToString();
                //Debug.Log("gold[done]");
            }
        }
    }
    #endregion

    #region Accessors - Strength
    int _strength;
    public int Strength
    {
        get { return _strength; }
        set
        {
            if (_strength != value)
            {
                _strength = value;
                strengthText.text = value.ToString();
                //Debug.Log("strength[done]");
            }
        }
    }
    #endregion

    #region Accessors - Intelligence
    int _intelligence;
    public int Intelligence
    {
        get { return _intelligence; }
        set
        {
            if (_intelligence != value)
            {
                _intelligence = value;
                intelligenceText.text = value.ToString();
                //Debug.Log("intelligence[done]");
            }
        }
    }
    #endregion

    #region Accessors - Immortal
    bool _immortal;
    public bool Immortal
    {
        get { return _immortal; }
        set
        {
            if (_immortal != value)
            {
                _immortal = value;
                immortalText.text = value == false ? "no" : "yes";
                immortalButtonText.text = value == false ? "Immortal" : "Mortal";
                //Debug.Log("immortal[done]");
            }
        }
    }
    #endregion

    #region Accessors - Invisible
    bool _invisible;
    public bool Invisible
    {
        get { return _invisible; }
        set
        {
            if (_invisible != value)
            {
                _invisible = value;
                invisibleText.text = value == false ? "no" : "yes";
                hideButtonText.text = value == false ? "Hide" : "Unhide";
                //Debug.Log("invisible[done]");
            }
        }
    }
    #endregion

    #region Accessors - Frozen
    bool _frozen;
    public bool Frozen
    {
        get { return _frozen; }
        set
        {
            if (_frozen != value)
            {
                _frozen = value;
                frozenText.text = value == false ? "no" : "yes";
                freezeButtonText.text = value == false ? "Freeze" : "Unfreeze";
                //Debug.Log("frozen[done]");
            }
        }
    }
    #endregion

    #region Accessors - Muted
    bool _muted;
    public bool Muted
    {
        get { return _muted; }
        set
        {
            if (_muted != value)
            {
                _muted = value;
                mutedText.text = value == false ? "no" : "yes";
                muteButtonText.text = value == false ? "Mute" : "Unmute";
                //Debug.Log("muted[done]");
            }
        }
    }   
    #endregion

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    // triggered by clicking the commands button on the toolbar
    public void PanelActivation()
    {
        if (!canvas.enabled)
        {
            InvokeInspect();
        }
        else
        {
            CancelInspect();
        }
        UIManagement.singleton.PanelActivation(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    public void InformationControl()
    {
        // only the owner can change access
        if (UIManagement.singleton.player.OwnerAccess())
        {
            banButton.gameObject.SetActive(true);
            downButton.gameObject.SetActive(true);
            upButton.gameObject.SetActive(true);
        }

        nameText.text = UIManagement.singleton.currentName;
#if UMMORPG_3D_C || UMMORPG_2D_C
        killButtonText.text = UIManagement.singleton.player.health == 0 ? "Resurrect" : "Kill";
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
        killButtonText.text = UIManagement.singleton.player.health.current == 0 ? "Resurrect" : "Kill";
#endif
        kickButton.interactable = UIManagement.singleton.currentName != UIManagement.singleton.player.name;
        fetchButton.interactable = UIManagement.singleton.currentName != UIManagement.singleton.player.name;
        goButton.interactable = UIManagement.singleton.currentName != UIManagement.singleton.player.name;
    }

    void UpdateCharacterInfo()
    {
        UIManagement.singleton.player.CmdCharacterInfo(UIManagement.singleton.currentName);
        //Debug.Log("UpdateCharacterInfo[refresh]");
    }
    
    void InvokeInspect()
    {
        InformationControl();

        InvokeRepeating(nameof(UpdateCharacterInfo), 0.0f, 0.25f);
        //Debug.Log("UpdateCharacterInfo[start]");
    }

    public void CancelInspect()
    {
        CancelInvoke(nameof(UpdateCharacterInfo));
        //Debug.Log("UpdateCharacterInfo[stop]");
    }

    #region Input Field - Control
    // control the range of numbers in the input fields for commands from 1-10
    string ReplaceAtIndex10(string text)
    {
        StringBuilder stringBuilder = new StringBuilder(text);
        if (stringBuilder.Length == 1)
        {
            if (stringBuilder[0] == '0')
            {
                //Debug.Log("1");
                stringBuilder[0] = '1';
            }
        }
        else if (stringBuilder.Length == 2)
        {
            if (stringBuilder[0] != '1')
            {
                //Debug.Log("1");
                stringBuilder[0] = '1';
            }
            if (stringBuilder[1] != '0')
            {
                //Debug.Log("0");
                stringBuilder[1] = '0';
            }
        }
        return stringBuilder.ToString();
    }

    // control the range of numbers in the input fields for commands from 0-100
    string ReplaceAtIndex100(string text, bool zero = false)
    {
        StringBuilder stringBuilder = new StringBuilder(text);
        if ((stringBuilder.Length == 1) && !zero)
        {
            if (stringBuilder[0] == '0')
            {
                //Debug.Log("1");
                stringBuilder[0] = '1';
            }            
        }
        else if ((stringBuilder.Length == 2) && zero)
        {
            if (stringBuilder[0] == '0')
            {
                //Debug.Log("1-9");
                stringBuilder[0] = stringBuilder[1];
                stringBuilder.Length = 1;
            }
        }
        else if (stringBuilder.Length == 3)
        {
            if (stringBuilder[0] != '1')
            {
                //Debug.Log("1");
                stringBuilder[0] = '1';
            }
            if (stringBuilder[1] != '0')
            {
                //Debug.Log("0");
                stringBuilder[1] = '0';
            }
            if (stringBuilder[2] != '0')
            {
                //Debug.Log("0");
                stringBuilder[2] = '0';
            }
        }
        return stringBuilder.ToString();
    }

    // control the range of numbers in the input fields for commands from 1-1000
    string ReplaceAtIndex1000(string text)
    {
        StringBuilder stringBuilder = new StringBuilder(text);
        if (stringBuilder.Length == 1)
        {
            if (stringBuilder[0] == '0')
            {
                //Debug.Log("1");
                stringBuilder[0] = '1';
            }
        }
        else if (stringBuilder.Length == 4)
        {
            if (stringBuilder[0] != '1')
            {
                //Debug.Log("1");
                stringBuilder[0] = '1';
            }
            if (stringBuilder[1] != '0')
            {
                //Debug.Log("0");
                stringBuilder[1] = '0';
            }
            if (stringBuilder[2] != '0')
            {
                //Debug.Log("0");
                stringBuilder[2] = '0';
            }
            if (stringBuilder[3] != '0')
            {
                //Debug.Log("0");
                stringBuilder[3] = '0';
            }
        }
        return stringBuilder.ToString();
    }
    #endregion

    #region Commands - Activation
    public void CommandAccess(int access)
    {
        string message = "";

        access = Mathf.Clamp(access, 0, 2);
        switch (access)
        {
            case 0:
                // ban                
                message = $"Ban <color=#00FA9A>{UIManagement.singleton.currentName}</color>?";
                break;
            case 1:
                // player                
                message = $"Set <color=#00FA9A>{UIManagement.singleton.currentName}</color>'s access level to player?";
                break;
            case 2:
                // special access
                message = $"Give <color=#00FA9A>{UIManagement.singleton.currentName}</color> special access?";
                break;
        }

        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        UICommandConfirmation.singleton.Show(message, () => { UIManagement.singleton.player.CmdMsgChangeAccess(UIManagement.singleton.currentName, access, infoWindow); });
    }

    public void CommandImmortal()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        UIManagement.singleton.player.CmdMsgImmortal(UIManagement.singleton.currentName, infoWindow);
    }

    public void CommandHide()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        UIManagement.singleton.player.CmdMsgHide(UIManagement.singleton.currentName, infoWindow);
    }

    public void CommandFreeze()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        UIManagement.singleton.player.CmdMsgFreeze(UIManagement.singleton.currentName, infoWindow);
    }

    public void CommandMute()
    {
        UIManagement.singleton.player.CmdMute(UIManagement.singleton.currentName);
    }

    public void CommandKick()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        UIManagement.singleton.player.CmdMsgKick(UIManagement.singleton.currentName, infoWindow);
    }

    public void CommandKill()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        UIManagement.singleton.player.CmdMsgKill(UIManagement.singleton.currentName, infoWindow);
    }

    public void CommandFetch()
    {
        UIManagement.singleton.player.CmdMsgFetch(UIManagement.singleton.currentName);
    }

    public void CommandGoTo()
    {
        UIManagement.singleton.player.CmdMsgGoTo(UIManagement.singleton.currentName);
    }

    public void CommandGold()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        int amount = Mathf.Clamp(int.Parse(goldInput.text), 1, 1000);
        UIManagement.singleton.player.CmdMsgGold(UIManagement.singleton.currentName, amount, infoWindow);

        UIManagement.singleton.DeleteText(goldInput);
    }

    public void CommandCoins()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        int amount = Mathf.Clamp(int.Parse(coinsInput.text), 1, 1000);
        UIManagement.singleton.player.CmdMsgCoins(UIManagement.singleton.currentName, amount, infoWindow);

        UIManagement.singleton.DeleteText(coinsInput);
    }

    public void CommandExp()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        int amount = Mathf.Clamp(int.Parse(expInput.text), 1, 1000);
        UIManagement.singleton.player.CmdMsgExp(UIManagement.singleton.currentName, amount, infoWindow);

        UIManagement.singleton.DeleteText(expInput);
    }

    public void CommandLevel()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        int value = Mathf.Clamp(int.Parse(levelInput.text), 1, 100);
        UIManagement.singleton.player.CmdMsgLevel(UIManagement.singleton.currentName, value, infoWindow);

        UIManagement.singleton.DeleteText(levelInput);
    }

    public void CommandStrength()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        int value = Mathf.Clamp(int.Parse(strengthInput.text), 0, 100);
        UIManagement.singleton.player.CmdMsgStrength(UIManagement.singleton.currentName, value, infoWindow);

        UIManagement.singleton.DeleteText(strengthInput);
    }

    public void CommandIntelligence()
    {
        // check whether to show the information window
        bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;
        int value = Mathf.Clamp(int.Parse(intelligenceInput.text), 0, 100);
        UIManagement.singleton.player.CmdMsgIntelligence(UIManagement.singleton.currentName, value, infoWindow);

        UIManagement.singleton.DeleteText(intelligenceInput);
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheckGold()
    {
        goldInput.text = ReplaceAtIndex1000(goldInput.text).Replace("-", "").Trim();

        // gold button works only if you enter text
        if (string.IsNullOrWhiteSpace(goldInput.text))
        {
            //Debug.Log("gold[inactive]");
            goldButton.interactable = false;
        }
        else if (!goldButton.IsInteractable())
        {
            //Debug.Log("gold[active]");
            goldButton.interactable = true;
        }
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheckCoins()
    {
        coinsInput.text = ReplaceAtIndex1000(coinsInput.text).Replace("-", "").Trim();

        // coins button works only if you enter text
        if (string.IsNullOrWhiteSpace(coinsInput.text))
        {
            //Debug.Log("coins[inactive]");
            coinsButton.interactable = false;
        }
        else if (!coinsButton.IsInteractable())
        {
            //Debug.Log("coins[active]");
            coinsButton.interactable = true;
        }
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheckExp()
    {
        expInput.text = ReplaceAtIndex1000(expInput.text).Replace("-", "").Trim();

        // exp button works only if you enter text
        if (string.IsNullOrWhiteSpace(expInput.text))
        {
            //Debug.Log("exp[inactive]");
            expButton.interactable = false;
        }
        else if (!expButton.IsInteractable())
        {
            //Debug.Log("exp[active]");
            expButton.interactable = true;
        }
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheckLevel()
    {
        levelInput.text = ReplaceAtIndex100(levelInput.text).Replace("-", "").Trim();

        // exp button works only if you enter text
        if (string.IsNullOrWhiteSpace(levelInput.text))
        {
            //Debug.Log("level[inactive]");
            levelButton.interactable = false;
        }
        else if (!levelButton.IsInteractable())
        {
            //Debug.Log("level[active]");
            levelButton.interactable = true;
        }
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheckStrength()
    {
        strengthInput.text = ReplaceAtIndex100(strengthInput.text, true).Replace("-", "").Trim();

        // strength button works only if you enter text
        if (string.IsNullOrWhiteSpace(strengthInput.text))
        {
            //Debug.Log("strength[inactive]");
            strengthButton.interactable = false;
        }
        else if (!strengthButton.IsInteractable())
        {
            //Debug.Log("strength[active]");
            strengthButton.interactable = true;
        }
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheckIntelligence()
    {
        intelligenceInput.text = ReplaceAtIndex100(intelligenceInput.text, true).Replace("-", "").Trim();

        // intelligence button works only if you enter text
        if (string.IsNullOrWhiteSpace(intelligenceInput.text))
        {
            //Debug.Log("intelligence[inactive]");
            intelligenceButton.interactable = false;
        }
        else if (!intelligenceButton.IsInteractable())
        {
            //Debug.Log("intelligence[active]");
            intelligenceButton.interactable = true;
        }
    }
    #endregion
}

public struct PlayerInfo
{
    public string account, className;
    public int healthValue, healthMax, manaValue, manaMax, access, level, strength, intelligence;
    public long coins, gold;
    public float healthPercent, manaPercent;
    public bool immortal, invisible, frozen, muted;

    public PlayerInfo(int _healthValue, int _healthMax, float _healthPercent, int _manaValue, int _manaMax, float _manaPercent, string _account, int _access, string _className, int _level, long _coins, long _gold, int _strength, int _intelligence, bool _immortal, bool _invisible, bool _frozen, bool _muted)
    {
        healthValue = _healthValue;
        healthMax = _healthMax;
        healthPercent = _healthPercent;
        manaValue = _manaValue;
        manaMax = _manaMax;
        manaPercent = _manaPercent;
        account = _account;
        access = _access;
        className = _className;
        level = _level;
        coins = _coins;
        gold = _gold;
        strength = _strength;
        intelligence = _intelligence;
        immortal = _immortal;
        invisible = _invisible;
        frozen = _frozen;
        muted = _muted;
    }
}
#endif
