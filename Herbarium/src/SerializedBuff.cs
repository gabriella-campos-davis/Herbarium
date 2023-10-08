using ProtoBuf;
using Vintagestory.API.Common;

namespace BuffStuff {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  internal class SerializedBuff {
    public string id;

    public double timeRemainingInDays;
    public int expireTick;
    public int tickCounter;
    public byte[] data;
  }
}
