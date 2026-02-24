using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace herbarium
{
    public class BlockVineClipping : BlockClipping
    {
        public override bool CanPlaceClipping(IBlockAccessor blockAccessor, BlockPos pos, ref string failureCode)
        {
            string failureCodeOld = failureCode;
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            if (belowBlock is BlockFruitingVines || belowBlock is BlockTreeVine)
            {
                BlockFacing facing = BlockFacing.FromCode(Code.EndVariant());

                BlockPos attachingBlockPos = pos.AddCopy(facing);
                BlockPos attachingBelowBlockPos = attachingBlockPos.DownCopy();

                bool canAttach = blockAccessor.GetBlock(attachingBlockPos).CanAttachBlockAt(blockAccessor, this, attachingBlockPos, facing.Opposite, null);
                bool canAttachBelow = blockAccessor.GetBlock(attachingBelowBlockPos).CanAttachBlockAt(blockAccessor, this, attachingBelowBlockPos, facing.Opposite, null);

                Block belowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
                bool isLarge = (belowBlock.Attributes?["isLarge"].AsBool() ?? false) && (belowBelowBlock.Attributes?["isBottomBlock"].AsBool() ?? false);

                if (belowBlock.Attributes?["isHuge"].AsBool() ?? false)
                {
                    Block belowBelowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(3));

                    if ((!belowBelowBelowBlock.Attributes?["isBottomBlock"].AsBool() ?? false) && belowBelowBelowBlock.Fertility <= 0) failureCode = "fruitvineclipping-tootall";
                    else if (!canAttach || !canAttachBelow) failureCode = "fruitvineclipping-nosupport";
                }
                else if (belowBelowBlock.Fertility <= 0 && !isLarge) failureCode = "fruitvineclipping-tootall";
                else if (!canAttach || !canAttachBelow) failureCode = "fruitvineclipping-nosupport";
            }
            else if (belowBlock is HerbariumBerryBush) failureCode = "fruitvineclipping-bushmix";
            else if (belowBlock.Fertility <= 0) failureCode = "fruitvineclipping";

            return failureCode == failureCodeOld;
        }
    }
}
