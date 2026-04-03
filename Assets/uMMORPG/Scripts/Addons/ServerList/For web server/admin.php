<?php
require_once 'Include/config.php';
require_once 'Include/Database.php';
require_once 'Include/function.php';

/**
 *	function list
 *
 */
     /*echo 'Server id :'.$server->sid.'<br />';
    echo 'Server name :'.$server->server_name.'<br />';
    echo 'Server ip :'.$server->server_ip.'<br />';
    echo 'Server main id :'.$server->server_main_id.'<br />';
    echo 'Server port :'.$server->server_port.'<br />';
    echo 'Server online :'.$server->is_online.'<br />';
    echo 'Server public :'.$server->is_public.'<br />';
    echo 'Server authorized IP :'.$server->authorized_ip.'<br />';
    echo '-------------------------------<br />';*/
 function createServer(string $server_name, string $server_ip, string $server_main_id, int $server_port, bool $id_online, bool $is_public, string $authorized_ip){

    
 }

 function updateServer(int $server_id, string $server_name, string $server_ip, string $server_main_id, int $server_port, bool $is_online, bool $is_public, string $authorized_ip){
     $query = 'UPDATE set sid = :server_id, server_name = :server_id'
     $q = Database::q($query, 
        [
            ':server_id' => $server_id,
            ':server_name' => $server_name,
            ':server_ip' => $server_ip,
            ':server_main_id' => $server_main_id,
            ':server_port' => $server_port,
            ':is_online' => $is_online,
            ':is_public' => $is_public,
            ':authorized' => $authorized_ip
        ]
     );

 }

 function removeServer(int $server_id){
     return = Database::q('delete from server where where sid = '.$server_id);
 }

 // retrun objrct server
 function listServers(int $limit = 15, $page = 0){ 
     return = Database::q('select * from server where 1 order by sid asc')->fetchAll(PDO::FETCH_OBJ)
 }