using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace herbarium
{
    public class ItemBerrySeed : Item
    {
        Block berryBlock;

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
            if (blockSel == null || !byEntity.Controls.Sneak)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }

            if (byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFarmland) return;

            berryBlock = byEntity.Api.World.GetBlock(AssetLocation.Create("groundberryseedling-" + Variant["type"].ToString() + "-planted", Code.Domain));
            IPlayer byPlayer = (byEntity is EntityPlayer) ? byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID) : null;

            blockSel = blockSel.Clone();
            blockSel.Position.Up();

            string failureCode = "";
            if (!berryBlock?.TryPlaceBlock(api.World, byPlayer, itemslot.Itemstack, blockSel, ref failureCode) ?? true)
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


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }
    }
}