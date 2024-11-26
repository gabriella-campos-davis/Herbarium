using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BlockClipping : BlockPlant
    {
        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            if (belowBlock is HerbariumBerryBush || belowBlock is PricklyBerryBush)
            {
                if(blockAccessor.GetBlockEntity(pos.DownCopy()) is BETallBerryBush)
                {
                    if((belowBlock.Attributes?["isLarge"].AsBool() ?? false) &&
                       (blockAccessor.GetBlock(pos.DownCopy(2)).Attributes?["isBottomBlock"].AsBool() ?? false)) return true;

                    if(blockAccessor.GetBlock(pos.DownCopy(2)).Fertility > 0) return true;
                }
            }
            return belowBlock.Fertility > 0;
        }
    }
}
