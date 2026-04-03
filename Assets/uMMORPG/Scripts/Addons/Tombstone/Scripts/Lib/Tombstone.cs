using Mirror;
using TMPro;
using UnityEngine;

// TOMBSTONE
public partial class Tombstone : NetworkBehaviour
{   
    [SyncVar]public Player owner;
    public TMP_Text playerName;
    public long lostExperience;
    public long restoreExperience;
    public float destroyAfter;
    public TargetBuffSkill praySkill;

    public bool pickupComplete = false;
    public bool detected = false;

    [ClientRpc]
    public void DisplayName()
    {
        playerName.text = " " + owner.name;
    }
#if _SERVER
    public void Start()
    {
        DisplayName();
        Destroy(gameObject, destroyAfter);
    }



#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
    {
        Player player = co.GetComponentInParent<Player>();
        if (!player) return;
        if (player && player == owner && player.isAlive)
        {
            player.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
    }
#endif

#if _iMMO2D
    private void OnTriggerStay2D(Collider2D co)
#else
    private void OnTriggerStay(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if(!player) return;
        if ( player && player == owner && player.isAlive)
        {
            if (player.skills.currentSkill >= 0 &&  player.skills.skills[player.skills.currentSkill].name == praySkill.name && !pickupComplete)
            {
                detected = true;
            }
            else if(detected && !pickupComplete)
            {
                player.experience.current += (restoreExperience);
                pickupComplete= true;
#if _iMMO2D
                player.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.StartAwake;
#endif
                Destroy(gameObject, 1);
            }
        }
    }
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
    {
        Player player = co.GetComponentInParent<Player>();
        if (!player) return;
        if (player && player == owner && player.isAlive)
        {
            player.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.StartAwake;
        }
    }
#endif

#endif
}