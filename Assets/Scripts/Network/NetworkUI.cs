using UnityEngine;
using UnityEngine.UI;

namespace ZShopping.Network
{




    public class NetworkUI : MonoBehaviour
    {
        [Header("Network Manager")]        
        public NetworkManagerInitializer networkManagerInitializer;

        [Header("UI References")]
        public Button hostButton;
        public Button clientButton;
        public InputField ipInput;
        public InputField portInput;

        private void Awake()
        {
            if (hostButton != null)
                hostButton.onClick.AddListener(OnHostClicked);
            if (clientButton != null)
                clientButton.onClick.AddListener(OnClientClicked);
        }

        private void OnHostClicked()
        {
            ApplyInputSettings();
            networkManagerInitializer.StartHost();
        }

        private void OnClientClicked()
        {
            ApplyInputSettings();
            networkManagerInitializer.StartClient();
        }

        private void ApplyInputSettings()
        {
            if (networkManagerInitializer == null)
                return;

            if (ipInput != null && !string.IsNullOrEmpty(ipInput.text))
                networkManagerInitializer.serverAddress = ipInput.text;

            if (portInput != null && ushort.TryParse(portInput.text, out ushort port))
                networkManagerInitializer.serverPort = port;
        }
    }
} 
