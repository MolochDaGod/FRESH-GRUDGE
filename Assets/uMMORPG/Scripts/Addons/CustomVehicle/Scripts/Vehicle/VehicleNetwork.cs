
//(c8

using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class VehicleNetwork : NetworkBehaviour
    {
        [Header("Serverside validate if can move")]
        public bool canMove = true;
        [Header("Serverside validate max speed")]
        public float totalMaxSpeed = 100f;

        [Header("Synchronization (Best not to modify)")]
        [Tooltip("Buffer at least that many moves before applying them in FixedUpdate. A bigger min buffer offers more lag tolerance with the cost of additional latency.")]
        public int minMoveBuffer = 2;
        [Tooltip("Combine two moves as one after having more than that many pending moves in order to avoid ever growing queues.")]
        public int combineMovesAfter = 5;
        [Tooltip("Buffer at most that many moves before force reseting the client. There is no point in buffering hundreds of moves and having it always lag 3 seconds behind.")]
        public int maxMoveBuffer = 10; // 20ms fixedDelta * 10 = 200 ms. no need to buffer more than that.
        [Tooltip("Rubberband movement: player can move freely as long as the server position matches. If server and client get off further than rubberDistance then a force reset happens.")]
        public float rubberDistance = 1;
        [Tooltip("Tolerance in percent for maximum movement speed. 0% is too small because there is always some latency and imprecision. 100% is too much because it allows for double speed hacks.")]
        [Range(0, 1)] public float validSpeedTolerance = 0.2f;
        byte route = 0; // used to stamp moves sent to server, see FixedUpdate. byte is enough. it will overflow and that's fine!
        public int combinedMoves = 0; // debug information only
        public int rubbered = 0; // debug information only
        // epsilon for float/vector3 comparison (needed because of imprecision
        // when sending over the network, etc.)
        const float epsilon = 0.1f;

        // we can't point to controller.velocity because that might not be reliable
        // over the network if we apply multiple moves at once. instead save the
        // last valid move's velocity here.
        public Vector3 velocity { get; private set; }
        private Vector3 lastPosition;

        // send each FixedUpdate move to the server to apply as well
        struct Move
        {
            public byte route;
            public Vector3 position; // position is 100% accurate. moveDir alone would get out of sync after a while.
            public Vector3 rotation;
            public Move(byte route, Vector3 position, Vector3 rotation)
            {
                this.route = route;
                this.position = position;
                this.rotation = rotation;
            }
        }
        Queue<Move> pendingMoves = new Queue<Move>();
        [Command]
        void CmdFixedMove(Move move)
        {
            // check route to make sure that the move received was sent AFTER the
            // last force reset. we don't want to apply moves that were in transit
            // during the last force reset (we don't want to apply old moves).
            if (move.route != route)
                return;

            // add to pending moves until FixedUpdate happens
            // (not for localplayer, it always moves itself)
            // (not if queue is full. it will be reset in next move anyway, and
            //  there is no need to waste memory with ever growing lists in case
            //  another client or an attacker spams way too frequently)
            if (!isLocalPlayer && pendingMoves.Count < maxMoveBuffer)
                pendingMoves.Enqueue(move);

            // broadcast to all players in proximity
            // (do this for localplayer too. others need to know our moves)
            RpcFixedMove(move);
        }

        [ClientRpc]
        void RpcFixedMove(Move move)
        {
            // note: no need to ignore old routes here because TCP is ordered.
            // all RpcFixedMoves with old routes will arrive BEFORE RpcForceReset.

            // do nothing if host. CmdFixedMove already added to queue.
            if (isServer) return;

            // do nothing if localplayer. it moves itself.
            if (isLocalPlayer) return;

            // add to pending moves until FixedUpdate happens
            // (not if queue is full. it will be reset in next move anyway, and
            //  there is no need to waste memory with ever growing lists in case
            //  another client or an attacker spams way too frequently)
            if (pendingMoves.Count < maxMoveBuffer)
                pendingMoves.Enqueue(move);
        }

        // force reset needs to happen on all clients. they all need to know the
        // player's new route number to disregard old values etc.
        [ClientRpc]
        void RpcForceReset(Vector3 position, Vector3 eulerAngles, byte newRoute)
        {
            // force reset position
            transform.position = position;
            transform.eulerAngles = eulerAngles;

            // set new route
            route = newRoute;

            // clear all moves with the old route. only apply the new ones now.
            // (we do this for other players, local player has no queue)
            pendingMoves.Clear();
        }

        [Server]
        public void Warp(Vector3 position, Vector3 eulerAngles)
        {
            // set new position
            transform.position = position;
            transform.eulerAngles = eulerAngles;

            // are we a spawned object?
            // => if we are a character selection preview or similar, then we would
            //    receive a warning about Rpc being called on unspawned object
            // => and route would get out of sync if we modify it here and the
            //    client never knows because the Rpc doesn't get called.
            if (isServer)
            {
                // ignore all incoming movements with old route
                ++route;

                // clear buffer so we don't apply old moves
                pendingMoves.Clear();

                // let the clients know
                RpcForceReset(position, eulerAngles, route);
                //Debug.LogWarning("Force reset to route=" + route);
            }
        }

        // rubberbanding if position got too far off from expected
        // position. e.g. if we ran into a wall even though the
        // client walked around it.
        // -> can be TESTED easily by adding Vector3.forward to
        //    each CmdFixedMove call! it will be reset when missing
        //    walls.
        [Server]
        void RubberbandCheck(Vector3 expectedPosition)
        {
            if (Vector3.Distance(transform.position, expectedPosition) >= rubberDistance)
            {
                Debug.LogWarning("vm- rubberbandcheck warp");
                Warp(transform.position, transform.eulerAngles);
                ++rubbered; // debug information only
            }
        }

        // check if a movement vector is valid
        // ('move' is a movement vector, NOT a position!)
        bool IsValidMove(Vector3 move)
        {
            // players send Vector3.zero while idle
            // -> those should always be allowed. otherwise we spam invalid move
            //    messages to the console.
            // -> epsilon comparison for network imprecision. otherwise we still get
            //    some "(0, 0, 0)" move warnings in console
            if (move.magnitude <= epsilon)
                return true;

            // movement is allowed?
            return canMove;
        }

        bool WasValidSpeed(Vector3 moveVelocity, bool combining)
        {
            // calculate xz magnitude for max speed check.
            // we'll have to check y separately because gravity, sliding,
            // falling, jumping, etc. will cause faster speeds then max speed.
            float speed = new Vector2(moveVelocity.x, moveVelocity.z).magnitude;

            // are we going below maximum allowed speed
            float maxSpeed = totalMaxSpeed;

            // calculate max speed with a small tolerance because it's never
            // exact when going over the network
            float maxSpeedWithTolerance = maxSpeed * (1 + validSpeedTolerance);

            // we allow twice the speed when applying a combined move
            if (combining)
                maxSpeedWithTolerance *= 2;

            // we should have a small tolerance for lags, latency, etc.
            //Debug.Log(name + " speed=" + speed + " / " + maxSpeedWithTolerance);
            if (speed <= maxSpeedWithTolerance)
            {
                return true;
            }
            else Debug.Log("vm- " + name + " move rejected because too fast: combining=" + combining + " xz speed=" + speed + " / " + maxSpeedWithTolerance);
            return false;
        }

        // CharacterController movement is physics based and requires FixedUpdate.
        // (using Update causes strange movement speeds in builds otherwise)
        void FixedUpdate()
        {
            // only control movement for local player
            if (Player.localPlayer != null && isOwned)
            {
                velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;// GetComponent<Rigidbody>().velocity; // for animations and fall damage
                lastPosition = transform.position;

                // broadcast to server
                CmdFixedMove(new Move(route, transform.position, transform.rotation.eulerAngles));
            }
            // server/other clients need to do some caching and scaling too
            else
            {
                // apply next pending move if we have at least 'minMoveBuffer'
                // moves pending to make sure that we also still have one to apply
                // in the next FixedUpdate call.
                // => it's better to apply 1,1,1,1 move instead of 2,0,2,1,0 moves.
                if (pendingMoves.Count > 0 && pendingMoves.Count >= minMoveBuffer)
                {
                    // too many pending moves?
                    // (only the server has authority to reset!)
                    if (isServer && pendingMoves.Count >= maxMoveBuffer)
                    {
                        Debug.LogWarning("vm- force reset : warp");
                        // force reset
                        Warp(transform.position, transform.eulerAngles);
                    }
                    // more than combine threshold?
                    // (both server and client can combine moves if needed)
                    else if (pendingMoves.Count >= 2 && pendingMoves.Count >= combineMovesAfter)
                    {
                        Vector3 previousPosition = transform.position;
                        Vector3 previousEulerAngles = transform.eulerAngles;

                        // combine the next two moves to minimize overall delay.
                        // => if we are always behind 5 moves then we might as well
                        //    accelerate a bit to always be behind 4, 3, or 2 moves
                        //    to minimize movement delay.
                        //
                        // note: calling controller.Move() multiple times in one
                        //       FixedUpdate doesn't work well and isn't
                        //       recommended. adding the two vectors together works
                        //       better.
                        //
                        // note: we COULD warp to first.position and then move to
                        //       second.position to avoid the double-move which
                        //       always comes with the risk of getting out of sync
                        //       because A,B went behind a wall while A+B went into
                        //       the wall.
                        //
                        //       BUT if we do warp then we would ALWAYS risk wall
                        //       hack cheats since the server would sometimes set
                        //       the position without checking physics via .Move.
                        //
                        //       INSTEAD we risk move(A+B) running into a wall and
                        //       reset if rubberbanding gets too far off. at least
                        //       this method can be made cheat safe.
                        Move first = pendingMoves.Dequeue();
                        Move second = pendingMoves.Dequeue();
                        // calculate the delta before each move. using .position is 100% accurate and never gets out of sync.
                        Vector3 move = second.position - transform.position;

                        // check if allowed (only check on server)
                        if (!isServer || IsValidMove(move))
                        {
                            // multiple moves. use velocity between the two moves, not between position and second move.
                            // -> and divide by fixedDeltaTime because velocity is
                            //    direction / second

                            transform.eulerAngles = second.rotation;

                            velocity = (second.position - first.position) / Time.fixedDeltaTime;
                            transform.position += move;// second.position;

                            ++combinedMoves; // debug information only

                            // safety checks on server
                            if (isServer)
                            {
                                // check speed AFTER actually moving with the TRUE
                                // velocity. this works better than checking before
                                // moving because now we know the true velocity
                                // after collisions.
                                if (!WasValidSpeed(velocity, true))
                                {
                                    Debug.LogWarning("vm- not valid speed : warp");
                                    Warp(previousPosition, previousEulerAngles);
                                }

                                // rubberbanding check if server
                                RubberbandCheck(second.position);
                            }
                        }
                        // force reset to last allowed position if not allowed
                        else
                        {
                            Warp(transform.position, transform.eulerAngles);
                            Debug.LogWarning("vm- " + name + " combined move: " + move + " rejected and force reset to: " + transform.position + "@" + DateTime.UtcNow);
                        }
                    }
                    // less than combine threshold?
                    else
                    {
                        Vector3 previousPosition = transform.position;
                        Vector3 previousEulerAngles = transform.eulerAngles;

                        // apply one move
                        Move next = pendingMoves.Dequeue();
                        // calculate the delta before each move. using .position is 100% accurate and never gets out of sync.
                        Vector3 move = next.position - transform.position;

                        // check if allowed (only check on server)
                        if (!isServer || IsValidMove(move))
                        {
                            transform.eulerAngles = next.rotation;

                            velocity = (next.position - transform.position) / Time.fixedDeltaTime;
                            transform.position += move;// next.position;

                            // safety checks on server
                            if (isServer)
                            {
                                // check speed AFTER actually moving with the TRUE
                                // velocity. this works better than checking before
                                // moving because now we know the true velocity
                                // after collisions.
                                if (!WasValidSpeed(velocity, false))
                                {
                                    Debug.LogWarning("vm- not valid speed : warp2");
                                    Warp(previousPosition, previousEulerAngles);
                                }

                                // rubberbanding check if server
                                RubberbandCheck(next.position);
                            }
                        }
                        // force reset to last allowed position if not allowed
                        else
                        {
                            Warp(transform.position, transform.eulerAngles);
                            Debug.LogWarning("vm- " + name + " single move: " + move + " rejected and force reset to: " + transform.position + "@" + DateTime.UtcNow);
                        }
                    }
                }
                //else Debug.Log("none pending. client should send faster...");
            }
        }
    }
}
