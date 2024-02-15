using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
    [SerializeField]
    private ExperimentManager experimentManager;
    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn device loaded");
        if (!IsServer && !string.IsNullOrEmpty(m_SceneName) && IsOwner)
        {
            SceneManager.LoadScene(m_SceneName, LoadSceneMode.Additive);
            experimentManager.Start();
        }
    }


    private int limit = 100; // Get summary every limit measurements
    private int counter = 0;

    // Update is called once per frame
    private void LateUpdate()
    {
        experimentManager.UpdateSphere();

        // if (_state != State.Running) return;
        experimentManager.Log();
        counter++;
        if (counter > limit)
        {
            const int id = 0;
            const bool left = true;
            const bool standing = true;
            SummaryServerRpc(id, left, standing, experimentManager.GetDistances().ToArray());
            Debug.Log(experimentManager.GetSummary().ToString());
            counter = 0;
        }
    }

    // Here comes the code for the communication between them

    [ServerRpc]
    public void SummaryServerRpc(int personId, bool leftHanded, bool standing, float[] distances)
    {
        Debug.Log("Summary received");
        Debug.Log("Person ID: " + personId);
        Debug.Log("Left handed: " + leftHanded);
        Debug.Log("Standing: " + standing);
        Debug.Log("Distances: " + distances);
    }
    [ClientRpc]
    public void StartExperimentClientRpc(int personId, bool leftHanded, bool standing)
    {
        experimentManager.OnServerSaidStart();
        Debug.Log("StartExperiment received");
        Debug.Log("Person ID: " + personId);
        Debug.Log("Left handed: " + leftHanded);
        Debug.Log("Standing: " + standing);
    }
    [ClientRpc]
    public void StopExperimentClientRpc()
    {
        experimentManager.OnServerSaidStop();
        Debug.Log("StopExperiment received");
    }
    [ClientRpc]
    public void RecalculatePathClientRpc()
    {
        experimentManager.OnServerSetPath();
        Debug.Log("RecalculatePath received");
    }
}
