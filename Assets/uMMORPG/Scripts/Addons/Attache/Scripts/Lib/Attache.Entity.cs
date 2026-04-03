using UnityEngine;

// ENTITY
public partial class Entity
{
    public enum ParentThreshold { None, Below, Above }

    public enum ChildAction { None, Spawn, Destroy }

    protected bool attacheSpawned;
}