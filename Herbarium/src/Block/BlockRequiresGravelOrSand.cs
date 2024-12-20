using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace herbarium
{
    public class BlockRequiresGravelOrSand : Block
    {
        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, IRandom worldgenRandom, BlockPatchAttributes attributes = null)
        {
            if (HasGravelOrSand(blockAccessor, pos)) return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldgenRandom, attributes);

            return false;
        }

        internal virtual bool HasGravelOrSand(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block block = blockAccessor.GetBlock(pos.DownCopy());

            if(block.BlockMaterial == EnumBlockMaterial.Gravel || block.BlockMaterial == EnumBlockMaterial.Sand) return true;
            else return false;
        }
    }
}