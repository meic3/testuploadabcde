using Unity.Netcode;
using UnityEngine;


public class PlayerManager : NetworkBehaviour
{
    PlayerManager playerManager;
    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;
        playerManager = GetComponent<PlayerManager>();
        AssignANumberRPC();
    }

    [Rpc(SendTo.Server)]
    public void AssignANumberRPC()
    {
        Debug.Log("Test1");
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        GameObject[] temp = gameManager.assignedPlayerNum;

        for(int i=0; i<temp.Length ; i++)
        {
            Debug.Log("Test2");
            if(temp[i] == null)
            {
                Debug.Log("Test4");
                temp[i] = playerManager.gameObject;
                Debug.Log("new player is assigned!");
                return;
            }
            
        }
        Debug.Log("Test3");
        gameManager.assignedPlayerNum = temp;
    }
}
