using UnityEngine;
using System;
namespace EdgeMultiplay
{
  [Serializable]
  public class ClientSettings : ScriptableObject
  {
    [Header("Host Configuration", order = 10)]
    /// <summary>
    /// Set to true if you have EdgeMultiplay Server running on your machine
    /// </summary>
    [Tooltip("Set to true if you have EdgeMultiplay Server running on your machine")]
    public bool useLocalHostServer;

    /// <summary>
    /// Use 127.0.0.1 as your IP Address for testing on your computer only
    /// Or use the Host IP Address for testing between your Computer and devices connected to the same WifiNetwork
    /// For Mac : Open Terminal and type "ifconfig" and copy the "en0" address
    /// For Windows : Open CMD and type "Ipconfig /all" and copy the "IPV4" address
    /// </summary>
    [Tooltip("Use 127.0.0.1 as your IP Address for testing on you computer only,\n" +
        "Or use the Host IP Address for testing between your Computer and devices connected to the same WifiNetwork\n" +
        "For Mac : Open Terminal and type \"ifconfig\" and copy the \"en0\" address \n" +
        "For Windows : Open CMD and type \"Ipconfig /all \" and copy the \"IPV4\" address ")]
    public string hostIPAddress;

    [Header("Ports Configuration", order = 20)]
    public int WebSocketPort;
    public int StatisticsPort;
    public int UDPPort;
  }

  public class Configs
  {
    public static ClientSettings clientSettings = Resources.Load<ClientSettings>("ClientSettings");
  }
}
