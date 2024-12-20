using Vintagestory.API.Client;
using Vintagestory.API.Common;
using System.Collections.Generic;

namespace herbarium
{
    public class BlockBehaviorHarvestMultipleWithKnife : BlockBehaviorHarvestMultiple
    {
        public BlockBehaviorHarvestMultipleWithKnife(Block block) : base(block)
        {
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Knife) return false;

            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handled)
        {
            if (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Knife) return false;

            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handled);
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handled)
        {
            if (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Knife) return;

            base.OnBlockInteractStart(world, byPlayer, blockSel, ref handled);
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handled)
        {
            if (harvestedStacks != null)
            {
                List<ItemStack> toolStacklist = new List<ItemStack>();

                foreach (Item item in world.Items)
                {
                    if (item.Code == null) continue;
                    if (item.Tool == EnumTool.Knife) toolStacklist.Add(new ItemStack(item));
                }
                
                bool notProtected = true;

                if (world.Claims != null && world is IClientWorldAccessor clientWorld && clientWorld.Player?.WorldData.CurrentGameMode == EnumGameMode.Survival)
                {
                    EnumWorldAccessResponse resp = world.Claims.TestAccess(clientWorld.Player, selection.Position, EnumBlockAccessFlags.Use);
                    if (resp != EnumWorldAccessResponse.Granted) notProtected = false;
                }

                if (notProtected) return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        Itemstacks = toolStacklist.ToArray(),
                        ActionLangCode = interactionHelpCode,
                        MouseButton = EnumMouseButton.Right
                    }
                };
            }

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handled);
        }
    }
}