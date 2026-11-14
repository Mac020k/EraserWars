using UnityEngine;

namespace Keshipin.Steam
{
    public sealed class SteamLobbyStub : MonoBehaviour
    {
        [SerializeField] string status = "Steamworks.NET not installed yet. Install in P6.";

        public string Status => status;

        public void CreateLobbyPlaceholder()
        {
            status = "Lobby create deferred until FishySteamworks is wired.";
            Debug.Log(status);
        }
    }
}
