using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.API;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class ItemHerbSeed : Item
    {
        Block herbBlock;
        
        WorldInteraction[] interactions;
        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client)
                return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "herbseedInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();

                foreach (Block block in api.World.Blocks)
                {
                    if (block.Code == null || block.EntityClass == null)
                        continue;
                    if (block.Fertility > 0)
                    {
                        stacks.Add(new ItemStack(block));
                    }
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "heldhelp-plant",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "sneak",
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }
        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if(blockSel is null) return;
            BlockPos pos = blockSel.Position;
            string lastCodePart = itemslot.Itemstack.Collectible.LastCodePart();
            BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(pos);
            if (be is BlockEntityFarmland && Attributes["isCrop"].AsBool())
            {
               placeCrop(itemslot, byEntity, blockSel, entitySel, true, ref handHandling);
            }
            if(be is not BlockEntityFarmland)
            {
                placeHerb(itemslot, byEntity, blockSel, entitySel, true, ref handHandling);
            }
        }

        private void placeCrop(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
            string lastCodePart = itemslot.Itemstack.Collectible.LastCodePart();
            BlockPos pos = blockSel.Position;
            BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(pos);

            Block cropBlock = byEntity.World.GetBlock(CodeWithPath("crop-" + lastCodePart + "-1"));
            if (cropBlock == null) return;

            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            bool planted = ((BlockEntityFarmland)be).TryPlant(cropBlock);
            if (planted)
            {
                byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), pos.X, pos.Y, pos.Z, byPlayer);

                ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                if (byPlayer?.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                {
                    itemslot.TakeOut(1);
                    itemslot.MarkDirty();
                }
            }

            if (planted) handHandling = EnumHandHandling.PreventDefault;
        }

        private void placeHerb(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
            if (blockSel == null || !byEntity.Controls.Sneak)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }

            string herbtype = this.LastCodePart();
            herbBlock = byEntity.Api.World.GetBlock(AssetLocation.Create("seedling-" + herbtype + "-planted", Code.Domain));

            if (herbBlock != null)
            {
                IPlayer byPlayer = null;
                if (byEntity is EntityPlayer)
                    byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                blockSel = blockSel.Clone();
                blockSel.Position.Up();
                if(byEntity.Api.World.GetBlockAccessor(true, false, true).GetBlock(blockSel.Position).IsLiquid() == true){
                    if (Attributes["waterplant"].AsBool())
                    {
                        goto growPlant;
                    }
                    else 
                    {
                        return;
                    }
                }
                growPlant:

                string failureCode = "";
                if (!herbBlock.TryPlaceBlock(api.World, byPlayer, itemslot.Itemstack, blockSel, ref failureCode))
                {
                    if (api is ICoreClientAPI capi && failureCode != null && failureCode != "__ignore__")
                    {
                        capi.TriggerIngameError(this, failureCode, Lang.Get("placefailure-" + failureCode));
                    }
                }
                else
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), blockSel.Position.X + 0.5f, blockSel.Position.Y, blockSel.Position.Z + 0.5f, byPlayer);

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

                dsc.AppendLine(Lang.Get("soil-growth-time") + Math.Round(totalDays, 1) + " days");
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