using Unity.Netcode;
using UnityEngine;

public class StartServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        Debug.Log("Running in Unity Editor");
        NetworkManager.Singleton.StartServer();
#elif UNITY_ANDROID
        string model = SystemInfo.deviceModel;
        if (model.Contains("Quest"))
        {
            Debug.Log("Running on Oculus Quest");
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            Debug.Log("Running on another Android device");
        }
#else
        Debug.Log("Running on an unsupported platform");
#endif
    }
}
