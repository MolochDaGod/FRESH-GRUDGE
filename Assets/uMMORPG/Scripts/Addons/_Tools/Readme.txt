// =======================================================================================
//                            Thank you for your download.
//                 Do not hesitate to post your feedback on Asset Store
// =======================================================================================
// * Discord Support Server........: https://discord.gg/aRCBPGMr7A
// * Website Instructions..........: https://mmo-indie.com/addon.php?name=tools
// =======================================================================================
// * Addon name....................: Tools
// * Asset Store link..............: https://u3d.as/2FcL
// * Require Asstets...............: https://assetstore.unity.com/packages/templates/systems/ummorpg-remastered-mmorpg-engine-159401
// * Core edit.....................: Yes
// * Prefab Edit...................: Yes
// * Recommendations...............: unmodified ummorpg.
// =======================================================================================
//                                 * Description *
//========================================================================================
// This utility AddOn is required by other AddOns, as it contains several shared functions and UI elements. 
// Most notably are the universal “CastBarUI”, “InfoBoxUI” and a universal “PopupUI”. 
// Those UI elements also come with options that can be edited via the Inspector. 
// Besides that, this AddOn adds a lot of new functions to the core asset, those
// functions are required in order for the AddOns to work as expected.
// =======================================================================================


// =======================================================================================
//                        * Installation and Configuration *
//========================================================================================

1. Open in menu "Tools/Auto Patch System"
  a) Click on install and wait recompile project

2. Open Scene "World" scene in Assets/uMMORPG/Scenes/World
  a) Select NetworkManager GameObject
  b) Add new component "ToolsConfigurationManager"
  c) In inspector in script ToolsConfigurationManager
   - A) Click on circle "Config Template" and select scriptable "Configuration"
   - B) Click on circle "Addon Template" and select scriptable "Define"
   - C) Click on cicle "Rules Template" and select scriptable "GameRules"

3. Edit all player prefabs (Warrior, Archer)
  a) add script "Tools_InfoBox"
  b) add script "Tools_PopUp"

4. Open hierarchy Assets/uMMORPG/Scripts/Addons/_Tools/
  a) Add all prefabs in "Required UI [Drag to Canvas]" in Canvas
  b) (Optional) Drag and drop optional prefab to the scene



// =======================================================================================
//                                  * Optional *
//========================================================================================

// ======================== Installation Distance Checker ================================

1. Open Scene "World" scene in Assets/uMMORPG/Scenes/World
    a) Select GameObject on scene, for Exemple a house
    b) Add new component "DistanceChecker"
    c) an Sphere collider is added at the same time as DistanceChecker, Drag & drop this "sphere collider" in component "DistanceChecker", in "Collider Triger"
    d) Use slider for chose a distance maximum


// ============================= Installation Floater ====================================

1. Open hierarchy Assets/uMMORPG/Scripts/Addons/_Tools/Utils/Floater
    a) Exemple 1 
     - a) open Prefab folder (in sale folder hierarchy)
     - b) Drag & Drop prefab in the scene

    b) Exemple 2
     - a) in Scene Select a Npc (Alchemist for exemple)
     - b) extend hierarchy and find "NameOverlayPosition"
     - c) Add component Floater component


// ======================================= MySQL =========================================

1. Open Scene "World" scene in Assets/uMMORPG/Scenes/World
    a) Find Network Manager
    b) Find component Database Look at Database Type and Select MySQL
    c) Enter all information Required (Host name, user, password)
     - a) if you do not have MySQL install https://www.wampserver.com/en/
     - b) for the mysql password, empty is not allowed. Use this link for help https://docs.phpmyadmin.net/en/latest/privileges.html
    d) Now uMMORPG uses MySQL


// ================================ Latency And FPS ======================================

1. Open hierarchy Assets/uMMORPG/Scripts/Addons/_Tools/Utils/LatencyAndFps
    a) Extend all folder
    b) Drag and Drop prefab to Canvas in Scene

    
// =============================== Framerate Limiter =====================================
1. Open Scene "World" scene in Assets/uMMORPG/Scenes/World
    a) Search Main Camera
    b) Add new Component "FpsLimiter"
    c) Chose your target FPS and enable/disable VSync


// ============================== Sleep Screen Control ===================================
1. Open Scene "World" scene in Assets/uMMORPG/Scenes/World
    a) Search Main Camera
    b) Add new Component "SleepScreenControl"
    c) Choose number of minutes for screen sleep, or click on NeverSleepControl to disable sleeping


// ================================= UI Visibility =======================================
1. Open Scene "World" scene in Assets/uMMORPG/Scenes/World
    a) Search Canvas
    b) Add new Component "UIVisibility"
    c) Select your conbo key "Left Alt+Z" is default
    d) Add all the children of the Canvas you want hidden


// ============================== New attack detection system ============================
1) Open all entity prefab 
      a) add new GameObject empty and moved it to eye level
      b) in compotenent (Player,Monster,Pet,Mount,Npc) Add this GameObject in HeadPosition


// ============================== Usable Sound ============================
1) Open script UsableItem.cs

      a) search
    public virtual void Use(Player player, int inventoryIndex)
    {
        // start cooldown (if any)
        // -> no need to set sync dict dirty if we have no cooldown
        if (cooldown > 0)
            player.SetItemCooldown(cooldownCategory, cooldown);
    }


      b) and Replace by
    public virtual void Use(Player player, int inventoryIndex)
    {
        // start cooldown (if any)
        // -> no need to set sync dict dirty if we have no cooldown
        if (cooldown > 0)
            player.SetItemCooldown(cooldownCategory, cooldown);

#if _iMMOTOOLS
        if (usableSound)
            player.audioSource.PlayOneShot(usableSound, 100);
#endif
    }

2) Open UILoot.cs
    a) Search
                    slot.button.onClick.SetListener(() => {
                        player.looting.CmdTakeItem(icopy);
                    });

    b) Replace to 
                    slot.button.onClick.SetListener(() => {
                        player.looting.CmdTakeItem(icopy);
                        if(itemSlot.item.data.dropSound)
                            player.audioSource.PlayOneShot(itemSlot.item.data.dropSound, 1);
                    });