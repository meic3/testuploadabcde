using Unity.Netcode;
using UnityEngine;

public class CubeColor : NetworkBehaviour
{
    [SerializeField] Material redMaterial;
    [SerializeField] Material blueMaterial;


    public NetworkVariable<ulong> creatorID = new NetworkVariable<ulong>();

    public override void OnNetworkSpawn()
    {
        Renderer meshRenderer = GetComponent<Renderer>();
        if(!IsOwner) return;
        
        if(creatorID.Value == 0)
        {
            meshRenderer.material = redMaterial;
        }
        if(creatorID.Value == 1)
        {
            meshRenderer.material = blueMaterial;
        }
    }
}
