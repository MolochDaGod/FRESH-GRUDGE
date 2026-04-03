<?php
require_once 'Include/config.php';
require_once 'Include/Database.php';
require_once 'Include/function.php';

$query = new Database();

//$result = Database::q('select server_name as name, server_ip as ip from server where is_online = 1 and is_public = 1 order by sid asc')->fetchAll(PDO::FETCH_OBJ);
$result = Database::q('select * from server where is_online = 1 order by sid asc')->fetchAll(PDO::FETCH_OBJ);


$allowServer = [];
foreach ($result as $server) {
    /*echo 'Server id :'.$server->sid.'<br />';
    echo 'Server name :'.$server->server_name.'<br />';
    echo 'Server ip :'.$server->server_ip.'<br />';
    echo 'Server main id :'.$server->server_main_id.'<br />';
    echo 'Server port :'.$server->server_port.'<br />';
    echo 'Server online :'.$server->is_online.'<br />';
    echo 'Server public :'.$server->is_public.'<br />';
    echo 'Server authorized IP :'.$server->authorized_ip.'<br />';
    echo '-------------------------------<br />';*/
    
    if($server->is_public == false && $server->authorized_ip != ""){
        $authorizedIP = explode(',', $server->authorized_ip); // add ip
        //$authorizedIP[] = '::1'; // Add local user ipv6
        //$authorizedIP[] = '127.0.0.1'; //Add local user ipv4
        if(in_array(get_ip_address(), $authorizedIP)){
            $allowServer [] = ['name' => $server->server_name, 'ip' => $server->server_ip];
        }
    }
    else{
        $allowServer [] = ['name' => $server->server_name, 'ip' => $server->server_ip];

    }
}
//echo get_ip_address().'<br />';
/*
class Server{
    private int $id;
    private string $name;
    private string $ip;
    private int $serverMainId;
    private int $serverPort;
    private bool $online;
    private bool $public;
    private array $authorizedIp;
    
    
    public function __Construct(int $id,string $name, string $ip, int $serverMainId = null, int $serveurPort = null, bool $online = true, $public = true, array $authorizedIp = []) : void {
        
        $this->id = $id;
        $this->name = $name;
        $this->ip = $ip;
        $this->serverMainId = $serverMainId;
        $this->serverPort = $serveurPort;
        $this->online = $online;
        $this->public = $public;
        $this->authorizedIp = $authorizedIp;
        
    }
    
    
    public function listServer(){
        
    }
}*/

echo json_encode($allowServer);

