using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.VisualScripting;

public class HeadsetBehaviour : NetworkBehaviour
{
    void Start()
    {
        experimentManager = FindAnyObjectByType<ExperimentManager>();
    }
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
    private ExperimentManager experimentManager;
    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn device loaded");
        if (!IsServer && !string.IsNullOrEmpty(m_SceneName) && IsOwner)
        {
            SceneManager.LoadScene(m_SceneName, LoadSceneMode.Additive);
            experimentManager?.Start();
        }
    }


    // Update is called once per frame
    private void LateUpdate()
    {
        experimentManager?.UpdateSphere();

        if (experimentManager?.GetIdle() ?? true) return;
        experimentManager?.Log();
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
    private int id = 0;
    private bool left = true;
    private bool standing = true;
    private void GetSummary()
    {
        SummaryServerRpc(id, left, standing, experimentManager?.GetDistances().ToArray());
    }
    [ClientRpc]
    public void StartExperimentClientRpc(int personId, bool leftHanded, bool standing)
    {
        id = personId;
        left = leftHanded;
        this.standing = standing;
        experimentManager?.OnServerSaidStart();
        InvokeRepeating("GetSummary", 0f, 1.0f);
    }
    [ClientRpc]
    public void StopExperimentClientRpc()
    {
        experimentManager?.OnServerSaidStop();
        CancelInvoke("GetSummary");
        Debug.Log("StopExperiment received");
    }
    [ClientRpc]
    public void RecalculatePathClientRpc()
    {
        experimentManager?.OnServerSetPath();
        Debug.Log("RecalculatePath received");
    }
}
