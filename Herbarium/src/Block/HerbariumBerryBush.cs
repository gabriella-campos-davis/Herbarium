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

            clipping = new ItemStack(api.World.GetItem(AssetLocation.Create(this.Attributes["pruneItem"].ToString())), 1);

            if ((byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Knife && HerbariumConfig.Current.useKnifeForClipping) ||
                (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Shears && HerbariumConfig.Current.useShearsForClipping))
            {

                //clipping = new ItemStack(api.World.GetItem(AssetLocation.Create("clipping-" + this.Variant["type"] + "-green", this.Code.Domain)), 1);

                if (clipping is null){
                    throw new ArgumentNullException(nameof(clipping), "UndergrowthBerryBush clipping is Null. Exiting.");  
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
            if (belowBlock.Fertility > 0) return true;
            if (!(belowBlock is HerbariumBerryBush || belowBlock is PricklyBerryBush)) return false;

            Block belowbelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
            return belowbelowBlock.Fertility > 0 && this.Attributes?.IsTrue("stackable") == true && belowBlock.Attributes?.IsTrue("stackable") == true;
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
