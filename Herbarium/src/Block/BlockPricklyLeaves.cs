using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using System.Linq;
using System;
using Vintagestory.API.Common.Entities;
using herbarium.config;
using herbarium;


namespace herbarium
{
    public class BlockPricklyLeaves : BlockLeavesDropCanes
    {
        public bool canDamage = HerbariumConfig.Current.berryBushCanDamage.Value;
        public string[] willDamage = HerbariumConfig.Current.berryBushWillDamage;
        public float dmg = HerbariumConfig.Current.berryBushDamage.Value;
        public float dmgTick = HerbariumConfig.Current.berryBushDamageTick.Value;
        
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
