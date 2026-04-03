// Understanding the Addon System:
// (Comments remain unchanged for brevity)

using System.Text;
using System.Collections.Generic; // Moved from bottom
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class AddonExample : NetworkBehaviour
{
    // hook this into the Health component's OnEmpty method in the Inspector
    public void OnDeath()
    {
        Debug.LogWarning("ExampleAddon: OnDeath!");
    }

    // we can also provide our own hooks for additional addons/components to use
    public UnityEvent TestEvent;
    void Start()
    {
        TestEvent.Invoke();
    }

    // we can also use UnityEvents with parameters, for example:
    public UnityEventString TestEventString;
    void Update()
    {
        TestEventString.Invoke("42");
    }
}

// items ///////////////////////////////////////////////////////////////////////
public partial class ItemTemplate
{
    //[Header("My Addon")]
    //public int addonVariable = 0;
}

// note: can't add variables yet without modifying original constructor
public partial struct Item
{
    //public int addonVariable {
    //    get { return template.addonVariable; }
    //}

    void ToolTip_Example(StringBuilder tip)
    {
        //tip.Append("");
    }
}

// skills //////////////////////////////////////////////////////////////////////
public partial class SkillTemplate
{
    //[Header("My Addon")]
    //public int addonVariable = 0;

    public partial struct SkillLevel
    {
        // note: adding variables here will give lots of warnings, but it works.
        //public int addonVariable;
    }
}

// note: can't add variables yet without modifying original constructor
public partial struct Skill
{
    //public int addonVariable
    //{
    //    get { return template.addonVariable; }
    //}

    void ToolTip_Example(StringBuilder tip)
    {
        //tip.Append("");
    }
}

// quests //////////////////////////////////////////////////////////////////////
public partial class QuestTemplate
{
    //[Header("My Addon")]
    //public int addonVariable = 0;
}

// note: can't add variables yet without modifying original constructor
public partial struct Quest
{
    //public int addonVariable
    //{
    //    get { return template.addonVariable; }
    //}

    void ToolTip_Example(StringBuilder tip)
    {
        //tip.Append("");
    }
}

// network messages ////////////////////////////////////////////////////////////
public partial class LoginMsg
{
}

// here is how to pass more data to the available message to show health, mana
// etc. in the character selection UI if necessary
public partial class CharactersAvailableMsg
{
    public partial class CharacterPreview
    {
        public int health;
    }

    public CharacterPreview[] characters; // Add this line to declare the characters array

    void Load_Example(List<Player> players)
    {
        characters = new CharacterPreview[players.Count]; // Initialize the characters array
        for (int i = 0; i < players.Count; ++i)
            characters[i].health = players[i].health.current; // Access the current property of health
    }
}

#if _iMMOTOOL
#endif