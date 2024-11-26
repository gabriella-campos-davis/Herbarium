using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BlockClipping : BlockPlant
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (CanPlaceClipping(world.BlockAccessor, blockSel.Position, ref failureCode))
            {
                if (CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
                {
                    return DoPlaceBlock(world, byPlayer, blockSel, itemstack);
                }
            }

            return false;
        }

        public virtual bool CanPlaceClipping(IBlockAccessor blockAccessor, BlockPos pos, ref string failureCode)
        {
            string failureCodeOld = failureCode;
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            if (belowBlock is HerbariumBerryBush)
            {
                Block belowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
                bool isLarge = (belowBlock.Attributes?["isLarge"].AsBool() ?? false) && (belowBelowBlock.Attributes?["isBottomBlock"].AsBool() ?? false);

                if (blockAccessor.GetBlockEntity(pos.DownCopy()) is not BETallBerryBush) failureCode = "berrybushclipping-nottallbush";
                else if (belowBelowBlock.Fertility <= 0 && !isLarge) failureCode = "berrybushclipping-tootall";
            }
            else if (belowBlock.Fertility <= 0) failureCode = "berrybushclipping";

            return failureCode == failureCodeOld;
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            string failureCode = "";

            return CanPlaceClipping(blockAccessor, pos, ref failureCode);
        }
    }
}
