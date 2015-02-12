using UnityEngine;
using System.Collections;

public class QuitScript : MonoBehaviour {

    /// <summary>
    /// Quits the application.
    /// </summary>
	public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Quits the level.
    /// </summary>
    public void InGameQuit()
    {
        Destroy(GameObject.Find("NetworkManager"));
        if (Network.isServer)
        {
            foreach (NetworkPlayer player in Network.connections)
            {
                Network.RemoveRPCs(player);
                Network.DestroyPlayerObjects(player);
            }
            Network.Disconnect();
            MasterServer.UnregisterHost();
        }
        else
        {
            Network.Disconnect();
        }
        Network.SetLevelPrefix(0);
        Application.LoadLevel(0);
    }
}
