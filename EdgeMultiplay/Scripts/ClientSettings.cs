using UnityEngine;
using System;
namespace EdgeMultiplay
{
  [Serializable]
  public class ClientSettings : ScriptableObject
  {
    public int WebSocketPort;
    public int StatisticsPort;
    public int UDPPort;
  }

  public class Configs
  {
    public static ClientSettings clientSettings = Resources.Load<ClientSettings>("ClientSettings");
  }
}
