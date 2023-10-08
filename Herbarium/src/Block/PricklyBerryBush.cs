using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common.Entities;
using herbarium.config;

namespace herbarium
{
    public class PricklyBerryBush : HerbariumBerryBush
    {
        public bool canDamage = HerbariumConfig.Current.berryBushCanDamage;
        public string[] willDamage = HerbariumConfig.Current.berryBushWillDamage;
        public float dmg = HerbariumConfig.Current.berryBushDamage;
        public float dmgTick = HerbariumConfig.Current.berryBushDamageTick;
        
        public override void OnEntityInside(IWorldAccessor world, Entity entity, BlockPos pos)
        {
            if (!canDamage || entity == null || willDamage == null)
            {
                return;
            }

            foreach(string creature in willDamage) 
            {
                if(entity.Code.ToString().Contains(creature))
                {
                    goto damagecreature;
                }
            }
            return;
            damagecreature:

            if (world.Side == EnumAppSide.Server && entity is EntityAgent)   //if the creature ins't sneaking, deal damage.
            {
                EntityAgent agent = (EntityAgent)entity;
                if (agent.ServerControls.TriesToMove && !agent.ServerControls.Sneak)
                {
                    if (world.Rand.NextDouble() > dmgTick) //while standing in the bush, how often will it hurt you
                    {
                        entity.ReceiveDamage(new DamageSource() 
                        { 
                            Source = EnumDamageSource.Block, 
                            SourceBlock = this, 
                            Type = EnumDamageType.PiercingAttack, 
                            SourcePos = pos.ToVec3d() 
                        }
                        , dmg); //Deal damage
                    }
                }
            }
            base.OnEntityInside(world, entity, pos);
        }
    }
}
