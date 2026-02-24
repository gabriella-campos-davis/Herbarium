using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace herbarium
{
    public class ItemHerbSeed : Item
    {        
        WorldInteraction[] interactions = null!;
        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) return;

            interactions = ObjectCacheUtil.GetOrCreate(api, "herbseedInteractions", () =>
            {
                ItemStack[] stacks = [.. api.World.Blocks.Where(block => block.Code != null && block.EntityClass != null && block.Fertility > 0)
                                             .Select(block => new ItemStack(block))];

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "heldhelp-plant",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "sneak",
                        Itemstacks = stacks
                    }
                };
            });
        }

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel == null) return;

            IPlayer? byPlayer = byEntity.World.PlayerByUid((byEntity as EntityPlayer)?.PlayerUID);

            if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFarmland)
            {
                if (!Attributes["isCrop"].AsBool()) return;

                Block cropBlock = byEntity.World.GetBlock(CodeWithPath("crop-" + itemslot.Itemstack.Collectible.LastCodePart() + "-1"));
                if (cropBlock == null) return;

                if (((BlockEntityFarmland)api.World.BlockAccessor.GetBlockEntity(blockSel.Position)).TryPlant(cropBlock, itemslot, byEntity, blockSel))
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/block/plant"), blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    if (byPlayer?.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                    {
                        itemslot.TakeOut(1);
                        itemslot.MarkDirty();
                    }

                api.Logger.Error("herb block not in water");
                if(api.World.BlockAccessor.GetBlock(blockSel.Position.UpCopy()).BlockMaterial != EnumBlockMaterial.Air)
                {
                    if(api.World.BlockAccessor.GetBlock(blockSel.Position.UpCopy()).BlockMaterial == EnumBlockMaterial.Water && waterPlant)
                    {
                        goto plantHerb;
                    }
                    return;
                }
                plantHerb:
                if(herbBlock is not null)
                {
                    IPlayer byPlayer = null;
                    if (byEntity is EntityPlayer)
                        byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                    api.World.BlockAccessor.SetBlock(herbBlock.Id, blockSel.Position.UpCopy());

                blockSel = blockSel.Clone();
                blockSel.Position.Up();

                EnumBlockMaterial mat = api.World.BlockAccessor.GetBlock(blockSel.Position).BlockMaterial;
                if (!byEntity.Controls.Sneak || (mat != EnumBlockMaterial.Air && Attributes["waterplant"].AsBool() && mat != EnumBlockMaterial.Liquid)) return;

                }
            }
            handHandling = EnumHandHandling.PreventDefault;
            return;
        }
        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if(blockSel is null) return;
            BlockPos pos = blockSel.Position.Copy();
            string lastCodePart = itemslot.Itemstack.Collectible.LastCodePart();
            Block belowBlock = api.World.BlockAccessor.GetBlock(pos);
            BlockEntity be = api.World.BlockAccessor.GetBlockEntity(pos);
            if(belowBlock.BlockMaterial != EnumBlockMaterial.Soil) return;
            if (be is not BlockEntityFarmland) placeHerb(itemslot, byEntity, blockSel, entitySel, true, ref handHandling);
            if (be is BlockEntityFarmland && Attributes["isCrop"].AsBool())
            {
               placeCrop(itemslot, byEntity, blockSel, entitySel, true, ref handHandling);
            }
            return;
        }

        private void placeCrop(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
            api.Logger.Error("crop start");
            string lastCodePart = itemslot.Itemstack.Collectible.LastCodePart();
            BlockPos pos = blockSel.Position;
            BlockEntity be = api.World.BlockAccessor.GetBlockEntity(pos);

            Block cropBlock = byEntity.World.GetBlock(CodeWithPath("crop-" + lastCodePart + "-1"));
            if (cropBlock == null) return;

            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            bool planted = ((BlockEntityFarmland)be).TryPlant(cropBlock,itemslot,byEntity,blockSel);
            if (planted)
            {
                byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/block/plant"), pos.X, pos.Y, pos.Z, byPlayer);

                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    if (byPlayer?.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                    {
                        itemslot.TakeOut(1);
                        itemslot.MarkDirty();
                    }
                }

                handHandling = EnumHandHandling.PreventDefault;
            }
        }
        

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            if(!Attributes["isCrop"].AsBool())
            {
                dsc.AppendLine(Lang.Get("plantable-on-normal-soil"));
                if (Attributes["waterplant"].AsBool()) dsc.AppendLine(Lang.Get("plantable-in-water-or-land"));
                return;
            }
            else if(Attributes["isCrop"].AsBool())
            {
                Block cropBlock = world.GetBlock(CodeWithPath("crop-" + inSlot.Itemstack.Collectible.LastCodePart() + "-1"));
                if (cropBlock == null || cropBlock.CropProps == null) return;

                dsc.AppendLine(Lang.Get("soil-nutrition-requirement") + cropBlock.CropProps.RequiredNutrient);
                dsc.AppendLine(Lang.Get("soil-nutrition-consumption") + cropBlock.CropProps.NutrientConsumption);

                double totalDays = cropBlock.CropProps.TotalGrowthDays;
                if (totalDays > 0)
                {
                    var defaultTimeInMonths = totalDays / 12;
                    totalDays = defaultTimeInMonths * world.Calendar.DaysPerMonth;
                } else
                {
                    totalDays = cropBlock.CropProps.TotalGrowthMonths * world.Calendar.DaysPerMonth;
                }

                totalDays /= api.World.Config.GetDecimal("cropGrowthRateMul", 1);

                dsc.AppendLine(Lang.Get("soil-growth-time") + " " + Lang.Get("count-days", Math.Round(totalDays, 1)));
                dsc.AppendLine(Lang.Get("crop-coldresistance", Math.Round(cropBlock.CropProps.ColdDamageBelow, 1)));
                dsc.AppendLine(Lang.Get("crop-heatresistance", Math.Round(cropBlock.CropProps.HeatDamageAbove, 1)));
                dsc.AppendLine(Lang.Get("plantable-on-farmland-or-soil"));  
                if (Attributes["waterplant"].AsBool()) dsc.AppendLine(Lang.Get("plantable-in-water-or-land"));
            }
        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }
    }
}