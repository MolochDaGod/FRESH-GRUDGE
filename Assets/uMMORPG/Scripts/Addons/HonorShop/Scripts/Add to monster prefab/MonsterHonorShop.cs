using Mirror;

// NPC
public class MonsterHonorShop : NetworkBehaviour
{
    public Monster monster;
    public Health health;

    public override void OnStartServer()
    {
        health.onEmpty.AddListener(monster.OnDeath_HonorShop);
    }
}