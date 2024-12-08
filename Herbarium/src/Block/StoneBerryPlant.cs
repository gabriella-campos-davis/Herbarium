using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace herbarium
{
    public class StoneBerryPlant : ShrubBerryBush
    {
        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            return belowBlock.BlockMaterial is EnumBlockMaterial.Stone || belowBlock.BlockMaterial is EnumBlockMaterial.Gravel;
        }
    }
}