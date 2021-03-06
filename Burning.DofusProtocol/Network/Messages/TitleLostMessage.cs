using FlatyBot.Common.IO;
using FlatyBot.Common.Network;
using System;

namespace Burning.DofusProtocol.Network.Messages
{
  public class TitleLostMessage : NetworkMessage
  {
    public const uint Id = 6371;
    public uint titleId;

    public override uint MessageId
    {
      get
      {
        return 6371;
      }
    }

    public TitleLostMessage()
    {
    }

    public TitleLostMessage(uint titleId)
    {
      this.titleId = titleId;
    }

    public override void Serialize(IDataWriter writer)
    {
      if (this.titleId < 0U)
        throw new Exception("Forbidden value (" + (object) this.titleId + ") on element titleId.");
      writer.WriteVarShort((short) this.titleId);
    }

    public override void Deserialize(IDataReader reader)
    {
      this.titleId = (uint) reader.ReadVarUhShort();
      if (this.titleId < 0U)
        throw new Exception("Forbidden value (" + (object) this.titleId + ") on element of TitleLostMessage.titleId.");
    }
  }
}
