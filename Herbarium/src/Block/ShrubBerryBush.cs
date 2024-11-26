using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace herbarium
{
    public class ShrubBerryBush : HerbariumBerryBush
    {
        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return blockAccessor.GetBlock(pos.DownCopy()).Fertility > 0;
        }
    }
}