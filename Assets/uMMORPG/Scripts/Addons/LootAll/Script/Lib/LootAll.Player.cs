public partial class Player
{
    public void TakeAllLootItem()
    {
        Player player = Player.localPlayer;
        player.looting.CmdTakeGold();
        for (int i = 0; i < ((Monster)player.target).inventory.slots.Count; ++i)
        {
            int icopy = i;
            player.looting.CmdTakeItem(icopy);
        }
    }
}
