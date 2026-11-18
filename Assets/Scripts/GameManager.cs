using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    public GameObject[] assignedPlayerNum; 

    public override void OnNetworkSpawn()
    {
        // if(!IsServer)
        // {
        // assignedPlayerNum = new PlayerManager[4];
        // }
        
    }
}
