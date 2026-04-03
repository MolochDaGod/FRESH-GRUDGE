***1) add in Canvas :***
 - UI_PlayerWarehouse

***2) open player prefab***
- 2.1) add assign warehouseConfiguration in component PlayerAddonsConfigurator


***3) open prefab npc (OPTIONAL)***
- 3.1) open npc prefab, add NpcWarehouse component
- 3.2) in same npc prefab go to component Npc and asign component NpcWarehouse in field `NpcWarehouse`
- 3.3) in same npc prefab assign scriptables game event `UIEventPlayerWarehouse` and check if yes or not this npc offert warehouse


****Advice :****
- 1) I advise you to create your own scriptables and not to use those present in the addons
- 2) I also advise you to create your own prefabs, this will avoid any subsequent overwriting

****Tips:****
  - copying a file on Unity is very simple, just press `Ctrl+d`