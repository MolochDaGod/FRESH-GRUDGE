
//(c8

#if _SQLITE

using BurnyDev.UMMORPG.AddonCustomVehicle;
using SQLite;
using System.Collections.Generic;
using UnityEngine;

public partial class Database
{
    class customvehicles
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string name { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float xrotation { get; set; }
        public float yrotation { get; set; }
        public float zrotation { get; set; }
        public float fuel { get; set; }
    }

    public void OnConnected_CustomVehicle()
    {
        connection.CreateTable<customvehicles>();
    }

    void SaveCustomVehicle(EnterVehicle enterVehicle, bool useTransaction = true)
    {
        // only use a transaction if not called within SaveMany transaction
        if (useTransaction) connection.BeginTransaction();

        // get position and rotation (so we don't have to access .transform 6x)
        Vector3 position = enterVehicle.transform.position;
        Vector3 rotation = enterVehicle.transform.rotation.eulerAngles;

        connection.Insert(new customvehicles
        {
            name = enterVehicle.transform.name,
            x = position.x,
            y = position.y,
            z = position.z,
            xrotation = rotation.x,
            yrotation = rotation.y,
            zrotation = rotation.z,
            fuel = enterVehicle.vehicleController.vehicleFuel
        });

        if (useTransaction) connection.Commit();
    }

    public void SaveCustomVehicles(IEnumerable<EnterVehicle> vehicles)
    {
        connection.BeginTransaction(); // transaction for performance

        // remove all old entries first, then add all the new ones
        // (we could use UPDATE where ... but deleting everything makes sure
        //  that there are never any ghosts)
        connection.DeleteAll<customvehicles>();
        foreach (EnterVehicle vehicle in vehicles)
            SaveCustomVehicle(vehicle, false);

        connection.Commit(); // end transaction
    }

    public void LoadCustomVehicle(EnterVehicle vehicle)
    {
        customvehicles row = connection.FindWithQuery<customvehicles>("SELECT * FROM customvehicles WHERE name=?", vehicle.name);
        if (row != null)
        {
            vehicle.transform.position = new Vector3(row.x, row.y, row.z);
            vehicle.transform.rotation = Quaternion.Euler(row.xrotation, row.yrotation, row.zrotation);
            vehicle.vehicleController.vehicleFuel = row.fuel;
        }
    }
}

#endif
