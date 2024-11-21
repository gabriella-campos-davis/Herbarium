using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class StonePlant : BlockPlant
    {
       public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            return belowBlock.BlockMaterial is EnumBlockMaterial.Stone || belowBlock.BlockMaterial is EnumBlockMaterial.Gravel;
        }

    }
}