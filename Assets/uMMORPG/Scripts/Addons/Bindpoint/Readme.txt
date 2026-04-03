***1) add in Canvas :***
 - 1.1) UI_bindpoint prefab
 - 1.2) search respawn panel and add new button "ButtonRespawnToBindpoint" ajust position if needed
 - 1.3) add UI_Bindpoint_RespawnDialogue component in respawn panel

***2) open prefab npc ***
- 2.1) open npc prefab, add NpcBindpoint component
- 2.2) in same npc prefab go to component Npc and asign component NpcBindpoint in field `NpcBindpoint`
- 2.3) in same npc prefab check if yes or not this npc offert bindpoint

***3) add bindpoint in scene***
- 3.1) extract 3D Only Bindpoint or 2D Only indpoint (depending on your version of ummorpg)
- 3.2) add in scene prefab exemple recenlty extracted

****Advice :****
- 1) I advise you to create your own scriptables and not to use those present in the addons
- 2) I also advise you to create your own prefabs, this will avoid any subsequent overwriting

****Tips:****
  - copying a file on Unity is very simple, just press `Ctrl+d`
