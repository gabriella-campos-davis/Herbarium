using Vintagestory.API.Client;
using Vintagestory.API.Common;
using System.Collections.Generic;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BlockBehaviorHarvestableWithTool : BlockBehaviorHarvestable
    {
        public BlockBehaviorHarvestableWithTool(Block block) : base(block)
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
            if (harvestedStacks == null) return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handled);

            List<ItemStack> toolStacklist = new List<ItemStack>();

            foreach (Item item in world.Items)
            {
                if (item.Code == null) continue;
                if (item.Tool == EnumTool.Knife) toolStacklist.Add(new ItemStack(item));
            }

            WorldInteraction[] interaction = base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handled);
            interaction[0].Itemstacks = toolStacklist.ToArray();

            return interaction;
        }
    }
}