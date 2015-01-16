using UnityEngine;
using System.Collections;

public class PlayerMenu : MonoBehaviour {

    /// <summary>
    /// Literally just quits. That's it.
    /// This should be merged with another script.
    /// </summary>
	public void Quit()
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
