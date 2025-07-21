using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace ZShopping.Network
{




    public class NetworkManagerInitializer : MonoBehaviour
    {
        [Header("Transport Settings")]
        public string serverAddress = "127.0.0.1";
        public ushort serverPort = 7777;

        void Awake()
        {

            var existing = FindObjectOfType<NetworkManager>();
            if (existing != null && NetworkManager.Singleton != existing)
            {

                var transport = existing.GetComponent<UnityTransport>();
                if (transport == null)
                    transport = existing.gameObject.AddComponent<UnityTransport>();
                transport.ConnectionData.Address = serverAddress;
                transport.ConnectionData.Port = serverPort;


                return;
            }

            if (NetworkManager.Singleton == null)
            {
                GameObject go = new GameObject("NetworkManager");
                DontDestroyOnLoad(go);
                var transport = go.AddComponent<UnityTransport>();
                transport.ConnectionData.Address = serverAddress;
                transport.ConnectionData.Port = serverPort;
                var nm = go.AddComponent<NetworkManager>();


            }
        }




        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();

            var fieldGen = FindObjectOfType<FieldGenerator>();
            if (fieldGen != null)
            {
                Debug.Log("Regenerating field on server after host start");
                fieldGen.GenerateField();
            }
        }




        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();

            var fg = FindObjectOfType<FieldGenerator>();
            if (fg != null)
                fg.ClearField();
        }




        public void StartServer()
        {
            NetworkManager.Singleton.StartServer();
        }
    }
} 
