using System;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum SpawnLocation { Spawner, Parent, World }
    
    public GameObject prefab;
    public Transform optionalParent;
    public SpawnLocation spawnLocation = SpawnLocation.Spawner;
    
    public void SpawnObject()
    {
        
        switch (spawnLocation)
        {
            case SpawnLocation.Spawner: // spawn at the spawner origin
            {
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                this.transform.GetPositionAndRotation(out position, out rotation);
                Instantiate(prefab, position, rotation, optionalParent);
                break;
            }
            case SpawnLocation.Parent: // spawn at the parent origin
            {
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                if (optionalParent) optionalParent.GetPositionAndRotation(out position, out rotation);
                Instantiate(prefab, position, rotation, optionalParent);
                break;
            }
            case SpawnLocation.World: // spawn at prefab position
            {
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                prefab.transform.GetPositionAndRotation(out position, out rotation);
                Instantiate(prefab, position, rotation, optionalParent);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        /*
        if (optionalParent)
        {
            
        }
        else
        {
            switch (spawnLocation)
            {
                case SpawnLocation.Parent:
                    break;
                case SpawnLocation.World:
                    break;
                case SpawnLocation.Offset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return;
        
        var instance = Instantiate(prefab, transform.position, transform.rotation);
        
        if (optionalParent)
        {
            instance.transform.SetParent(optionalParent, !useLocalPosition);
        }*/
    }
}
