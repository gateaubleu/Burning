using FlatyBot.Common.IO;
using FlatyBot.Common.Network;
using System.Collections.Generic;

namespace Burning.DofusProtocol.Network.Messages
{
  public class ExchangeMountsPaddockRemoveMessage : NetworkMessage
  {
    public List<int> mountsId = new List<int>();
    public const uint Id = 6559;

    public override uint MessageId
    {
      get
      {
        return 6559;
      }
    }

    public ExchangeMountsPaddockRemoveMessage()
    {
    }

    public ExchangeMountsPaddockRemoveMessage(List<int> mountsId)
    {
      this.mountsId = mountsId;
    }

    public override void Serialize(IDataWriter writer)
    {
      writer.WriteShort((short) this.mountsId.Count);
      for (int index = 0; index < this.mountsId.Count; ++index)
        writer.WriteVarInt(this.mountsId[index]);
    }

    public override void Deserialize(IDataReader reader)
    {
      uint num = (uint) reader.ReadUShort();
      for (int index = 0; (long) index < (long) num; ++index)
        this.mountsId.Add(reader.ReadVarInt());
    }
  }
}
