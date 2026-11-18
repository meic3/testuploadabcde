using Unity.Netcode;
using UnityEngine;

public class ObjectLauncher : NetworkBehaviour
{
    [SerializeField] private GameObject throwCubePrefab;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float throwForce = 5;
    [SerializeField] private Controls controls;

    void Update()
    {
        if(!IsOwner) return;

        if(controls.InteractTriggered())
        {
            LaunchCubeServerRPC();
        }
    }

    [Rpc(SendTo.Server)]
    void LaunchCubeServerRPC()
    {
        GameObject cube = Instantiate(throwCubePrefab, cameraHolder.position + cameraHolder.forward, cameraHolder.rotation);
        cube.GetComponent<CubeColor>().creatorID.Value = OwnerClientId;
        cube.GetComponent<NetworkObject>().Spawn();
        cube.GetComponent<Rigidbody>().AddForce(cameraHolder.forward*throwForce, ForceMode.Impulse);
    }
}
