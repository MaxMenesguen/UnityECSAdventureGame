using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 offset = new Vector3(0, 50, -40);

    void LateUpdate()
    {
        // Dynamically check if the player transform is assigned
        if (playerTransform == null)
        {
            ECSPlayerBridge bridge = FindObjectOfType<ECSPlayerBridge>();
            if (bridge != null)
            {
                playerTransform = bridge.transform;
                Debug.Log($"CameraBehavior: Dynamically assigned playerTransform to {bridge.name}");
            }
            else
            {
                Debug.LogError("CameraBehavior: playerTransform is null, and ECSPlayerBridge could not be found.");
                return;
            }
        }

        // Update camera position and look at the player
        Vector3 newPos = playerTransform.position + offset;
        transform.position = newPos;
        transform.LookAt(playerTransform.position);
    }
}
