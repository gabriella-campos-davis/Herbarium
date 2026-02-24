using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BlockTreeVine : BlockPlant
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockEntity blockEntity = api.World.BlockAccessor.GetBlockEntity(blockSel.Position);
            BlockTreeVine orientedBlock = world.BlockAccessor.GetBlock(CodeWithVariant("side", SuggestedHVOrientation(byPlayer, blockSel)[0].Code)) as BlockTreeVine;

            if (orientedBlock.CanPlantStay(world.BlockAccessor, blockSel.Position))
            {
                if (orientedBlock.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
                {
                    return orientedBlock.DoPlaceBlock(world, byPlayer, blockSel, itemstack);
                }
            }

            failureCode = "requirefertileground";
            return false;
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return blockAccessor.GetBlock(pos.DownCopy()).Fertility > 0;
        }
    }
}