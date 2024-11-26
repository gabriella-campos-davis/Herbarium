using System;
using System.Collections.Generic;
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
        WorldInteraction[] interactions;
        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) return;

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
            if (blockSel == null) return;

            IPlayer byPlayer = (byEntity is EntityPlayer) ? byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID) : null;

            if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFarmland)
            {
                if (!Attributes["isCrop"].AsBool()) return;

                Block cropBlock = byEntity.World.GetBlock(CodeWithPath("crop-" + itemslot.Itemstack.Collectible.LastCodePart() + "-1"));
                if (cropBlock == null) return;

                if (((BlockEntityFarmland)api.World.BlockAccessor.GetBlockEntity(blockSel.Position)).TryPlant(cropBlock))
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/block/plant"), blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    if (byPlayer?.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                    {
                        itemslot.TakeOut(1);
                        itemslot.MarkDirty();
                    }

                    handHandling = EnumHandHandling.PreventDefault;
                }
            }
            else
            {
                Block herbBlock = api.World.GetBlock(AssetLocation.Create("seedling-" + Variant["herbseedlings"] + "-planted", this.Code.Domain));

                blockSel = blockSel.Clone();
                blockSel.Position.Up();

                EnumBlockMaterial mat = api.World.BlockAccessor.GetBlock(blockSel.Position).BlockMaterial;
                if (!byEntity.Controls.Sneak || (mat != EnumBlockMaterial.Air && Attributes["waterplant"].AsBool() && mat != EnumBlockMaterial.Liquid)) return;

                string failureCode = "";
                if (!herbBlock?.TryPlaceBlock(api.World, byPlayer, itemslot.Itemstack, blockSel, ref failureCode) ?? true)
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