using FlatyBot.Common.IO;

namespace Burning.DofusProtocol.Network.Messages
{
  public class AllianceBulletinSetRequestMessage : SocialNoticeSetRequestMessage
  {
    public new const uint Id = 6693;
    public string content;
    public bool notifyMembers;

    public override uint MessageId
    {
      get
      {
        return 6693;
      }
    }

    public AllianceBulletinSetRequestMessage()
    {
    }

    public AllianceBulletinSetRequestMessage(string content, bool notifyMembers)
    {
      this.content = content;
      this.notifyMembers = notifyMembers;
    }

    public override void Serialize(IDataWriter writer)
    {
      base.Serialize(writer);
      writer.WriteUTF(this.content);
      writer.WriteBoolean(this.notifyMembers);
    }

    public override void Deserialize(IDataReader reader)
    {
      base.Deserialize(reader);
      this.content = reader.ReadUTF();
      this.notifyMembers = reader.ReadBoolean();
    }
  }
}
