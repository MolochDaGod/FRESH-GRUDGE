***1) add in Canvas :***
 - UI_GuildWarehouse

***2) open player prefab***
- 2.1) add assign GuildWarehouseConfiguration in component PlayerAddonsConfigurator


***3) open prefab npc (OPTIONAL)***
- 3.1) open npc prefab, add NpcGuildWarehouse component
- 3.2) in same npc prefab go to component Npc and asign component NpcGuildWarehouse in field `NpcGuildWarehouse`
- 3.3) in same npc prefab assign scriptables game event `UIEventGuildWarehouse` and check if yes or not this npc offert Guild Warehouse


****Advice :****
- 1) I advise you to create your own scriptables and not to use those present in the addons
- 2) I also advise you to create your own prefabs, this will avoid any subsequent overwriting

****Tips:****
  - copying a file on Unity is very simple, just press `Ctrl+d`