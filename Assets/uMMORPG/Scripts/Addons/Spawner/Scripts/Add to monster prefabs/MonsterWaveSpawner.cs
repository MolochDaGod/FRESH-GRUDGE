using Mirror;

public class MonsterWaveSpawner : NetworkBehaviour
{
    public Monster monster;

    public override void OnStartServer()
    {
        monster.health.onEmpty.AddListener(monster.OnDeath_WaveSpawnerEntity);
    }
}
