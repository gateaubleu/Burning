using FlatyBot.Common.IO;
using FlatyBot.Common.Network;

namespace Burning.DofusProtocol.Network.Messages
{
  public class BasicNoOperationMessage : NetworkMessage
  {
    public const uint Id = 176;

    public override uint MessageId
    {
      get
      {
        return 176;
      }
    }

    public override void Serialize(IDataWriter writer)
    {
    }

    public override void Deserialize(IDataReader reader)
    {
    }
  }
}
