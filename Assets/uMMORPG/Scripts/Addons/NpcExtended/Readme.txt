Open Npc.cs

Search :
    // finite state machine states /////////////////////////////////////////////
    [Server] protected override string UpdateServer() { return state; }
    [Client] protected override void UpdateClient() {}

    // attack //////////////////////////////////////////////////////////////////
    public override bool CanAttack(Entity entity) { return false; }

    // interaction /////////////////////////////////////////////////////////////
    protected override void OnInteract()
    {
        Player player = Player.localPlayer;

        // alive, close enough? then talk
        // use collider point(s) to also work with big entities
        if (health.current > 0 &&
            Utils.ClosestDistance(player, this) <= player.interactionRange)
        {
            UINpcDialogue.singleton.Show();
        }
        // otherwise walk there
        // use collider point(s) to also work with big entities
        else
        {
            Vector3 destination = Utils.ClosestPoint(this, player.transform.position);
            player.movement.Navigate(destination, player.interactionRange);
        }
    }


Replace to : 

    // finite state machine states /////////////////////////////////////////////
    //[Server] protected override string UpdateServer() { return state; }
    //[Client] protected override void UpdateClient() {}

    // attack //////////////////////////////////////////////////////////////////
    //public override bool CanAttack(Entity entity) { return false; }

    // interaction /////////////////////////////////////////////////////////////
    /*protected override void OnInteract()
    {
        Player player = Player.localPlayer;

        // alive, close enough? then talk
        // use collider point(s) to also work with big entities
        if (health.current > 0 &&
            Utils.ClosestDistance(player, this) <= player.interactionRange)
        {
            UINpcDialogue.singleton.Show();
        }
        // otherwise walk there
        // use collider point(s) to also work with big entities
        else
        {
            Vector3 destination = Utils.ClosestPoint(this, player.transform.position);
            player.movement.Navigate(destination, player.interactionRange);
        }
    }*/