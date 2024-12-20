using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace herbarium
{
    public class BlockFruitingVines : HerbariumBerryBush
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
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


        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, IRandom worldgenRandom, BlockPatchAttributes attributes = null)
        {
            if (!CanPlantStay(blockAccessor, pos))
            {
                foreach (var facing in BlockFacing.ALLFACES)
                {
                    BlockFruitingVines nBlock = (BlockFruitingVines)blockAccessor.GetBlock(CodeWithVariant("side", facing.Code));
                    if (nBlock?.CanPlantStay(blockAccessor, pos) ?? false)
                    {
                        Block block = blockAccessor.GetBlock(pos);
                        if (block.IsReplacableBy(nBlock))
                        {
                            if (block.EntityClass != null)
                            {
                                blockAccessor.RemoveBlockEntity(pos);
                            }

                            blockAccessor.SetBlock(nBlock.BlockId, pos);
                            if (nBlock.EntityClass != null)
                            {
                                blockAccessor.SpawnBlockEntity(nBlock.EntityClass, pos);
                            }

                            return true;
                        }

                        return false;
                    }
                }
            }
            return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldgenRandom, attributes);
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());
            
            if (!Attributes["isBottomBlock"].AsBool() && Variant["side"] != "down")
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
                    if (belowBlock is BlockFruitingVines || belowBlock is BlockTreeVine)
                    {
                        if (isLarge || (belowBelowBlock.Fertility > 0)) return true;
                        else if (belowBlock.Attributes?["isHuge"].AsBool() ?? false)
                        {
                            Block belowBelowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(3));

                            if ((belowBelowBelowBlock.Attributes?["isBottomBlock"].AsBool() ?? false) || belowBelowBelowBlock.Fertility > 0) return true;
                        }
                    }
                    else if (belowBlock.Fertility > 0) return true;
                }
            }
            else if (belowBlock.Fertility > 0) return true;

            return false;
        }
    }
}