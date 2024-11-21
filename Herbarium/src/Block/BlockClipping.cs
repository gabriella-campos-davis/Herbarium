using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BlockClipping : BlockPlant
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());
            BlockEntity belowBlockEntity = blockAccessor.GetBlockEntity(pos.DownCopy());

            Block belowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));

            Block belowBelowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(3));

            if (belowBlock is HerbariumBerryBush || belowBlock is PricklyBerryBush)
            {
                if(belowBlockEntity is BETallBerryBush)
                {
                    if(belowBlock.Attributes["isLarge"].AsBool())
                    {
                        if(belowBelowBelowBlock.Fertility > 0) return true;
                    }
                    if(belowBelowBlock.Fertility > 0) return true;
                }
            }
            return belowBlock.Fertility > 0;
        }
    }
}
