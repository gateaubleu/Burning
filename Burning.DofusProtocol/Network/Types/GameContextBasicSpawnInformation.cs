using FlatyBot.Common.IO;
using System;

namespace Burning.DofusProtocol.Network.Types
{
    public class GameContextBasicSpawnInformation
    {
        public const uint Id = 568;
        public uint teamId;
        public bool alive;
        public GameContextActorPositionInformations informations;

        public virtual uint TypeId
        {
            get
            {
                return 568;
            }
        }

        public GameContextBasicSpawnInformation()
        {
        }

        public GameContextBasicSpawnInformation(
          uint teamId,
          bool alive,
          GameContextActorPositionInformations informations)
        {
            this.teamId = teamId;
            this.alive = alive;
            this.informations = informations;
        }

        public virtual void Serialize(IDataWriter writer)
        {
            writer.WriteByte((byte)this.teamId);
            writer.WriteBoolean(this.alive);
            writer.WriteShort((short)this.informations.TypeId);
            this.informations.Serialize(writer);
        }

        public virtual void Deserialize(IDataReader reader)
        {
            this.teamId = (uint)reader.ReadByte();
            if (this.teamId < 0U)
                throw new Exception("Forbidden value (" + (object)this.teamId + ") on element of GameContextBasicSpawnInformation.teamId.");
            this.alive = reader.ReadBoolean();
            this.informations = ProtocolTypeManager.GetInstance<GameContextActorPositionInformations>((uint)reader.ReadUShort());
            this.informations.Deserialize(reader);
        }
    }
}
