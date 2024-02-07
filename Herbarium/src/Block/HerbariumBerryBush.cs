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
    public class HerbariumBerryBush : BlockBerryBush
    {
        ItemStack clipping = new();
        
        public AssetLocation harvestingSound;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            
            string code = "game:sounds/block/leafy-picking";
            if (code != null) {
                harvestingSound = AssetLocation.Create(code, this.Code.Domain);
            }
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            if ((byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Knife && HerbariumConfig.Current.useKnifeForClipping) ||
                (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Shears && HerbariumConfig.Current.useShearsForClipping))
            {

                clipping = new ItemStack(api.World.GetItem(AssetLocation.Create(this.Attributes["pruneItem"].ToString())), 1);

                if (clipping is null){
                    api.Logger.Error("Attempted to create clipping for " + this.Variant["type"] + ", came back null.");
                    return false;
                }

                if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEHerbariumBerryBush beugbush && !beugbush.Pruned)
                {
                    beugbush.Prune();
                    if (byPlayer?.InventoryManager.TryGiveItemstack(clipping) == false)
                    {
                        world.SpawnItemEntity(clipping, byPlayer.Entity.SidedPos.XYZ);
                    }

                    world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                    return true;
                }
                if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BETallBerryBush beugtbush && !beugtbush.Pruned )
                {
                    beugtbush.Prune();
                    if (byPlayer?.InventoryManager.TryGiveItemstack(clipping) == false)
                    {
                        world.SpawnItemEntity(clipping, byPlayer.Entity.SidedPos.XYZ);
                    }

                    world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                    return true;
                }
                if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEShrubBerryBush beugsbush && !beugsbush.Pruned )
                {
                    beugsbush.Prune();
                    if (byPlayer?.InventoryManager.TryGiveItemstack(clipping) == false)
                    {
                        world.SpawnItemEntity(clipping, byPlayer.Entity.SidedPos.XYZ);
                    }

                    world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                    return true;
                } 
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel); 
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            if(belowBlock is null) return false;
            if(belowBlock.Attributes is null) return false;

            if(belowBlock.Fertility > 0) return true; //we are on ground, everyone can be here
            if(belowBlock.BlockMaterial == EnumBlockMaterial.Air) return false;
            if(Attributes["stackable"].AsBool() is false) return false; //if we can't stack and aren't on ground, gtfo

            if(Attributes["stackable"].AsBool() is true)
            {
                Block belowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
                if(belowBelowBlock is null) return false;
                if(belowBelowBlock.Attributes is null) return false;
                if(belowBlock.BlockMaterial == EnumBlockMaterial.Air) return false;
                if(belowBlock.Attributes["stackable"].AsBool() is true && belowBelowBlock.Fertility > 0)
                {
                    return true;
                }
            }
            return false;
        }


        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            var drops = base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);

            foreach (var drop in drops)
            {
                if (drop.Collectible is HerbariumBerryBush || drop.Collectible is PricklyBerryBush) continue;

                float dropRate = 1;

                if (Attributes?.IsTrue("forageStatAffected") == true)
                {
                    dropRate *= byPlayer?.Entity.Stats.GetBlended("forageDropRate") ?? 1;
                }

                drop.StackSize = GameMath.RoundRandom(api.World.Rand, drop.StackSize * dropRate);
            }

            return drops;
        }
    }
}
