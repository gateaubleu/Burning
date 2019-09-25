using FlatyBot.Common.IO;
using FlatyBot.Common.Network;
using System;

namespace Burning.DofusProtocol.Network.Messages
{
  public class GameFightTurnReadyRequestMessage : NetworkMessage
  {
    public const uint Id = 715;
    public double id;

    public override uint MessageId
    {
      get
      {
        return 715;
      }
    }

    public GameFightTurnReadyRequestMessage()
    {
    }

    public GameFightTurnReadyRequestMessage(double id)
    {
      this.id = id;
    }

    public override void Serialize(IDataWriter writer)
    {
      if (this.id < -9.00719925474099E+15 || this.id > 9.00719925474099E+15)
        throw new Exception("Forbidden value (" + (object) this.id + ") on element id.");
      writer.WriteDouble(this.id);
    }

    public override void Deserialize(IDataReader reader)
    {
      this.id = reader.ReadDouble();
      if (this.id < -9.00719925474099E+15 || this.id > 9.00719925474099E+15)
        throw new Exception("Forbidden value (" + (object) this.id + ") on element of GameFightTurnReadyRequestMessage.id.");
    }
  }
}