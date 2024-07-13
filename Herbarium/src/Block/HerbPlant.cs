using System.Linq;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using herbarium.config;

namespace herbarium
{
    public class HerbPlant : BlockPlant
    {
        WorldInteraction[] interactions = null;
        public static readonly string normalCodePart = "normal";
        public static readonly string harvestedCodePart = "harvested";

        public bool isPoisonous = false;

        public bool canDamage = HerbariumConfig.Current.plantsCanDamage.Value;
        public bool canPoison = HerbariumConfig.Current.plantsCanPoison.Value;
        public string[] willDamage = HerbariumConfig.Current.plantsWillDamage;

        public float dmg = HerbariumConfig.Current.berryBushDamage.Value;
        public float dmgTick = HerbariumConfig.Current.berryBushDamageTick.Value;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (Variant["state"] == "harvested")
                return;

            if(Attributes["isPoisonous"].ToString() == "true") isPoisonous = true;

            interactions = ObjectCacheUtil.GetOrCreate(api, "mushromBlockInteractions", () =>
            {
                List<ItemStack> knifeStacklist = new List<ItemStack>();

                foreach (Item item in api.World.Items)
                {
                    if (item.Code == null)
                        continue;

                    if (item.Tool == EnumTool.Knife)
                    {
                        knifeStacklist.Add(new ItemStack(item));
                    }
                }

                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-mushroom-harvest",
                        MouseButton = EnumMouseButton.Left,
                        Itemstacks = knifeStacklist.ToArray()
                    }
                };
            });
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

            if (byPlayer != null)
            {
                EnumTool? tool = byPlayer.InventoryManager.ActiveTool;
                if (IsGrown() && (tool == EnumTool.Knife || tool == EnumTool.Sickle || tool == EnumTool.Scythe))
                {
                    Block harvestedBlock = GetHarvestedBlock(world);
                    world.BlockAccessor.SetBlock(harvestedBlock.BlockId, pos);
                }
            }
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return new BlockDropItemStack[] { new BlockDropItemStack(handbookStack) };
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            if (IsGrown())
            {
                if (Attributes?.IsTrue("forageStatAffected") == true)
                {
                    dropQuantityMultiplier *= byPlayer?.Entity?.Stats.GetBlended("forageDropRate") ?? 1;
                }

                return base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
            }

            if(Attributes["hasRoot"].ToString() == "true" && !IsGrown()){
                return base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
            }
            else
            {
               return null;
            }
        }

        public bool IsGrown()
        {
            return Code.Path.Contains(normalCodePart);
        }

        public Block GetNormalBlock(IWorldAccessor world)
        {
            AssetLocation newBlockCode = Code.CopyWithPath(Code.Path.Replace(harvestedCodePart, normalCodePart));
            return world.GetBlock(newBlockCode);
        }

        public Block GetHarvestedBlock(IWorldAccessor world)
        {
            AssetLocation newBlockCode = Code.CopyWithPath(Code.Path.Replace(normalCodePart, harvestedCodePart));
            return world.GetBlock(newBlockCode);
        }

        public override void OnEntityInside(IWorldAccessor world, Entity entity, BlockPos pos)
        {
            if(!canDamage || !isPoisonous)
            {
                return;
            }

            if(entity == null)
            {
                return;
            }

            if(willDamage == null)
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

            if (world.Side == EnumAppSide.Server && entity is EntityAgent && !(entity as EntityAgent).ServerControls.Sneak) //if the creature ins't sneaking, deal damage.
            {
                if (world.Rand.NextDouble() > dmgTick)
                {
                    if(canPoison)
                    {
                        var rashDebuff = new RashDebuff();
                        rashDebuff.Apply(entity);
                    }

                    if(!canPoison && canDamage)
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

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
