using Mirror;
using UnityEngine;

public class PlayerStamina : NetworkBehaviour
{
#if _iMMOSTAMINA
    public Player player;

    [Header("[-=-=-[ Configuration Stamina ]-=-=-]")]
    public Tmpl_StaminaConfiguration configuration;

    private float speed = 0f;
    private float time = 0.0f;

    // Update is called once per frame
    private void Start()
    {
        speed = player.speed * (1 + configuration.moveSprintSpeed);
    }
    void Update()
    {
        time += Time.deltaTime;

        if (player.state == "MOVING")
        {
            if (Input.GetKey(configuration.keyCode) && player.stamina.current > configuration.costSprint)
            {
                Cmd_Sprint();
                player.movement.SetSpeed(speed);

            }
        }
        /*if (player.state == "CASTING")
        {
            //Skill skill = player.skills.currentSkill
            player.stamina.current -= configuration.costSkillDefault; // + skill.skillStaminaCost
        }
        if (player.state == "MOUNTED")
        {
            if (Input.GetKey(configuration.keyCode) && player.stamina.current > configuration.moveSprintSpeed)
            {
                Cmd_Sprint();
                speed = player.speed * (1 + configuration.moveSprintSpeed);
                player.mountControl.activeMount.movement.SetSpeed(speed);
                //player.stamina.current -= configuration.costMovingMounted; // + skill.skillStaminaCost
            }
        }*/

    }

    [Command]
    private void Cmd_Sprint()
    {
        speed = player.speed * (1 + configuration.moveSprintSpeed);
        player.movement.SetSpeed(speed);
        if (time >= configuration.costPerSeconds)
        {
            player.stamina.current -= configuration.costSprint;
            //player.stamina.current -= (player.state == "MOVING") ? configuration.costSprint : configuration.costMovingMounted;
            time = 0.0f;
        }
    }

    [ClientRpc]
    private void SetSpeedPlayer(float speed)
    {
        player.movement.SetSpeed(speed);
        //
        //player.movement.SetSpeed(speed);
        player.Tools_setSpeedModifier(speed);
        player.Tools_OverwriteSpeed();
    }
#else
     public string staminadisabled = "Addon stamina is disabled";
#endif
}
