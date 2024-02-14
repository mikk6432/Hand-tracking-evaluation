using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeadsetBehaviour : NetworkBehaviour
{
    // Here comes the code for distinguishing between the server and the client
    // and loading the scene on the client
#if UNITY_EDITOR
    public UnityEditor.SceneAsset SceneAsset;
    private void OnValidate()
    {
        if (SceneAsset != null)
        {
            m_SceneName = SceneAsset.name;
        }
    }
#endif
    [SerializeField]
    private string m_SceneName;

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn device loaded");
        if (!IsServer && !string.IsNullOrEmpty(m_SceneName) && IsOwner)
        {
            SceneManager.LoadScene(m_SceneName, LoadSceneMode.Additive);
        }
    }

    // Here comes the code for the communication between them

    [ServerRpc]
    public void Summary(int personId, bool leftHanded, bool standing, float[] distances)
    {
        Debug.Log("Summary received");
        Debug.Log("Person ID: " + personId);
        Debug.Log("Left handed: " + leftHanded);
        Debug.Log("Standing: " + standing);
        Debug.Log("Distances: " + distances);
    }
    [ClientRpc]
    public void StartExperiment(int personId, bool leftHanded, bool standing)
    {
        Debug.Log("StartExperiment received");
        Debug.Log("Person ID: " + personId);
        Debug.Log("Left handed: " + leftHanded);
        Debug.Log("Standing: " + standing);
    }
    [ClientRpc]
    public void StopExperiment()
    {
        Debug.Log("StopExperiment received");
    }
    [ClientRpc]
    public void RecalculatePath()
    {
        Debug.Log("RecalculatePath received");
    }
}
