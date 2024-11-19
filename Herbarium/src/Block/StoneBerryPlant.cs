using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace herbarium
{
    /*
        this class uses a specific model, ideally this should be abstracted
        so it is easier to define what parts of a model will be disappeared
        when pruning a bush,
    */
    public class StoneBerryPlant : ShrubBerryBush
    {
        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            return belowBlock.BlockMaterial is EnumBlockMaterial.Stone || belowBlock.BlockMaterial is EnumBlockMaterial.Gravel;
        }

    }
}