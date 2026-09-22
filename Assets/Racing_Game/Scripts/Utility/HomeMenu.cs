using UnityEngine;
using UnityEngine.SceneManagement;
using ALIyerEdon.RemoteInput;

namespace ALIyerEdon
{
    public class HomeMenu : MonoBehaviour
    {
        public void PlayGame()
        {
            // Default to local input; GarageRemoteController re-enables this if a phone connects.
            RemoteInputBootstrapper.DisableRemote();
            SceneManager.LoadScene("Garage");
        }

        public void UseAsController()
        {
            SceneManager.LoadScene("RemoteController");
        }
    }
}
