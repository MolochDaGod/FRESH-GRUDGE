if you have an older version of leaderboard already installed, you must delete the "leaderboard" table
for the script to recreate the table.

then you add:
on the Prefab Player you add the "PlayerLeaderboard" component
    - you must assign all fields of the component
    - you have to go to the "Player" component and link the "PlayerLeaderboard" component

on Monster prefabs you add the "MonsterLeaderboard" component
     - you must assign all fields of the component

if you have not updated UI_Leaderboard you must also do it