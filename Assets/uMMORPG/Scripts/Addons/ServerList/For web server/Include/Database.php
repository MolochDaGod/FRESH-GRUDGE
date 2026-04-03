<?php

class Database{

	protected static $db = null;
	protected static $stmt;

	public static $nbrQuery = 0;
	public static $listQuery = [];

	public static function getInstance(){
		if(null === static::$db) {
			static::$db = new \PDO('mysql:dbname='.DATABASE['DBNAME'].';host='.DATABASE['HOST'].';port='.DATABASE['HOST'], DATABASE['USER'], DATABASE['PASSWORD']);
			static::$db->setAttribute(\PDO::ATTR_ERRMODE, \PDO::ERRMODE_EXCEPTION);
			static::$db->query('SET NAMES utf8;');
		}
		return static::$db;
	}

        /**
         * Execute une requetes avec la methode exec
         * @param type $query
         * @return type
         */
        public static function qexec($query){
		static::getInstance();
		$stmt = static::$db->exec($query);
		self::_showQuery($query);
		return $stmt;
	}

        /**
         * execute une requetes préparé 
         * @param type $query
         * @param array $values
         * @return type
         */
	public static function q($query, array $values = array()){
		static::getInstance();
		$stmt = static::$db->prepare($query);
		static::qBindValues($stmt, $values);
		self::_showQuery($query, $values);
		return $stmt;
	}

        /**
         * Vérifie si une table existe
         * @param type $table
         * @return type bool
         */
	public static function checkTable($table){
		$query = 'SHOW TABLES LIKE :table';
		$requete = Database::q($query, array(':table' => $table));
		return (($requete->rowCount() ==1) ? true : false);
	}

        private static function _showQuery($req, $values){
		if(is_array($values)){
			foreach ($values as $key => $value){
				$req = str_replace($key, ((is_numeric($value)) ? $value : '"'.addslashes($value).'"'), $req);
			}
		}
		self::$listQuery[] = $req;
	}

        /**
         * Retourne la liste de toutes les requêtes de la pages
         * @return type STRING
         */
	public static function showQuery(){
		return self::$listQuery;
	}

        /**
         * Retourne le dernier id inséré dans la base de donnée durant le dernier insert
         * @return type int
         */
        public static function lastInsertId(){
            return static::$db->lastInsertId();
        }

	protected static function qBindValues(\PDOStatement $stmt, array $values){
		foreach($values as $placeholder => $val) {
			if(is_array($val)) {
				static::qBindValues($stmt, $val);
			}
			else {
				$stmt->bindValue($placeholder, $val, ((is_numeric($val)) ? \PDO::PARAM_INT : \PDO::PARAM_STR));
			}
		}
		self::$nbrQuery++;
		$stmt->execute();
	}

        /**
         * Permet d'affiché simplement le nombre de requetes par page sans ajout dans les model
         */
	public static function showNbrQuery(){
		return self::$nbrQuery;
	}

        /** gestion des tasaction **/
        // Start trasaction
        public static function start_transaction(){
            static::$db->query("SET AUTOCOMMIT=0");
            static::$db->query("BEGIN");
        }
        // end transction
        public static function end_transaction(){
            static::$db->query("COMMIT");
            static::$db->query("SET AUTOCOMMIT=1");
        }
        // rollback transaction
        public static function end_transaction_bad(){
            static::$db->query("ROLLBACK");
            static::$db->query("SET AUTOCOMMIT=1");
        }
}