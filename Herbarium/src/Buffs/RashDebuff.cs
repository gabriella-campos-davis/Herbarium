using Vintagestory.API.Common;
using ProtoBuf;
using BuffStuff;

namespace herbarium
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class RashDebuff : Buff
    {
        private static float HP_PER_TICK = 1f / 8f;
        private static int DURATION_IN_REAL_SECONDS = 45;

        public override void OnStart()
        {
            SetExpiryInRealSeconds(DURATION_IN_REAL_SECONDS);
        }

        public override void OnStack(Buff oldBuff)
        {
            SetExpiryInRealSeconds(DURATION_IN_REAL_SECONDS);
        }

        public override void OnDeath()
        {
            SetExpiryImmediately();
        }

        public override void OnExpire()
        {
            SetExpiryImmediately();
        }

        public override void OnTick()
        {
            if (TickCounter % 16 == 0)
            {
                Entity.ReceiveDamage(new DamageSource { Source = EnumDamageSource.Internal, Type = EnumDamageType.Poison }, HP_PER_TICK);
            }
        }
    }
}