using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common.Entities;
using herbarium.config;


namespace herbarium
{
    public class BlockPricklyLeaves : BlockLeavesDropCanes
    {
        public bool canDamage = HerbariumConfig.Current.plantsCanDamage.Value;
        public string[] willDamage = HerbariumConfig.Current.plantsWillDamage;
        public float dmg = HerbariumConfig.Current.plantsDamage.Value;
        public float dmgTick = HerbariumConfig.Current.plantsDamageTick.Value;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            canDamage = api.World.Config.GetBool("plantsCanDamage", canDamage);
            dmg = api.World.Config.GetFloat("plantsDamage", dmg);
            dmgTick = api.World.Config.GetFloat("plantsDamageTick", dmgTick);
        }

        public override void OnEntityInside(IWorldAccessor world, Entity entity, BlockPos pos)
        {
            if (world.Side == EnumAppSide.Server && entity is EntityAgent && canDamage && willDamage != null)
            {
                foreach (string creature in willDamage)
                {
                    if (entity.Code.ToString().Contains(creature))
                    {
                        EntityAgent agent = (EntityAgent)entity;
                        if (agent.ServerControls.TriesToMove && !agent.ServerControls.Sneak)   //if the creature ins't sneaking, deal damage.
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
    }
}
