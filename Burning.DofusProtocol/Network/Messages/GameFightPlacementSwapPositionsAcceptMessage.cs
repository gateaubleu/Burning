using FlatyBot.Common.IO;
using FlatyBot.Common.Network;
using System;

namespace Burning.DofusProtocol.Network.Messages
{
  public class GameFightPlacementSwapPositionsAcceptMessage : NetworkMessage
  {
    public const uint Id = 6547;
    public uint requestId;

    public override uint MessageId
    {
      get
      {
        return 6547;
      }
    }

    public GameFightPlacementSwapPositionsAcceptMessage()
    {
    }

    public GameFightPlacementSwapPositionsAcceptMessage(uint requestId)
    {
      this.requestId = requestId;
    }

    public override void Serialize(IDataWriter writer)
    {
      if (this.requestId < 0U)
        throw new Exception("Forbidden value (" + (object) this.requestId + ") on element requestId.");
      writer.WriteInt((int) this.requestId);
    }

    public override void Deserialize(IDataReader reader)
    {
      this.requestId = (uint) reader.ReadInt();
      if (this.requestId < 0U)
        throw new Exception("Forbidden value (" + (object) this.requestId + ") on element of GameFightPlacementSwapPositionsAcceptMessage.requestId.");
    }
  }
}