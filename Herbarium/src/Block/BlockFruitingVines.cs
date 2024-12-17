using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BlockFruitingVines : HerbariumBerryBush
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockEntity blockEntity = api.World.BlockAccessor.GetBlockEntity(blockSel.Position);
            Block belowBlock = api.World.BlockAccessor.GetBlock(blockSel.Position.DownCopy());

            string facing = SuggestedHVOrientation(byPlayer, blockSel)[0].Code;
            if (belowBlock is BlockFruitingVines || belowBlock is BlockTreeVine) facing = belowBlock.Code.EndVariant();
            BlockFruitingVines orientedBlock = world.BlockAccessor.GetBlock(CodeWithVariant("side", facing)) as BlockFruitingVines;

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
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            if ((belowBlock is BlockFruitingVines || belowBlock is BlockTreeVine) && !Attributes["isBottomBlock"].AsBool())
            {
                BlockFacing facing = BlockFacing.FromCode(Code.EndVariant());

                BlockPos attachingBlockPos = pos.AddCopy(facing);
                BlockPos attachingBelowBlockPos = attachingBlockPos.DownCopy();

                bool canAttach = blockAccessor.GetBlock(attachingBlockPos).CanAttachBlockAt(blockAccessor, this, attachingBlockPos, facing.Opposite, null);
                bool canAttachBelow = blockAccessor.GetBlock(attachingBelowBlockPos).CanAttachBlockAt(blockAccessor, this, attachingBelowBlockPos, facing.Opposite, null);

                Block belowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
                bool isLarge = (belowBlock.Attributes?["isLarge"].AsBool() ?? false) && (belowBelowBlock.Attributes?["isBottomBlock"].AsBool() ?? false);

                if (canAttach && canAttachBelow)
                {
                    if (isLarge || (belowBelowBlock.Fertility > 0)) return true;
                    else if (belowBlock.Attributes?["isHuge"].AsBool() ?? false)
                    {
                        Block belowBelowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(3));

                        if ((belowBelowBelowBlock.Attributes?["isBottomBlock"].AsBool() ?? false) || belowBelowBelowBlock.Fertility > 0) return true;
                    }
                }
            }
            else if (belowBlock.Fertility > 0) return true;

            return false;
        }
    }
}