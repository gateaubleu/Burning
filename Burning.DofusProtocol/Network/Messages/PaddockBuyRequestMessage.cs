using FlatyBot.Common.IO;
using FlatyBot.Common.Network;
using System;

namespace Burning.DofusProtocol.Network.Messages
{
  public class PaddockBuyRequestMessage : NetworkMessage
  {
    public const uint Id = 5951;
    public double proposedPrice;

    public override uint MessageId
    {
      get
      {
        return 5951;
      }
    }

    public PaddockBuyRequestMessage()
    {
    }

    public PaddockBuyRequestMessage(double proposedPrice)
    {
      this.proposedPrice = proposedPrice;
    }

    public override void Serialize(IDataWriter writer)
    {
      if (this.proposedPrice < 0.0 || this.proposedPrice > 9.00719925474099E+15)
        throw new Exception("Forbidden value (" + (object) this.proposedPrice + ") on element proposedPrice.");
      writer.WriteVarLong((long) this.proposedPrice);
    }

    public override void Deserialize(IDataReader reader)
    {
      this.proposedPrice = (double) reader.ReadVarUhLong();
      if (this.proposedPrice < 0.0 || this.proposedPrice > 9.00719925474099E+15)
        throw new Exception("Forbidden value (" + (object) this.proposedPrice + ") on element of PaddockBuyRequestMessage.proposedPrice.");
    }
  }
}