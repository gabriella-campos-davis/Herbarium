using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace herbarium
{
    public class WaterHerb : HerbPlant
    {
        public ICoreAPI Api => api;
        int maxDepth;
        int minDepth = 2 ;
        string waterCode;


        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (Variant["state"] == "harvested") return;

            maxDepth = Attributes["maxDepth"].AsInt();
            minDepth = Attributes["minDepth"].AsInt();
            waterCode = Attributes["waterCode"].AsString();

        }

        // Worldgen placement, tests to see how many blocks below water the plant is being placed, and if that's allowed for the plant
        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, IRandom worldgenRandom, BlockPatchAttributes attributes = null)
        {
            Block block = blockAccessor.GetBlock(pos);

            if (!block.IsReplacableBy(this))
            {
                return false;
            }

            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            if (belowBlock.Fertility > 0 && minDepth == 0)
            {
                Block placingBlock = blockAccessor.GetBlock(Code);
                if (placingBlock == null) return false;

                blockAccessor.SetBlock(placingBlock.BlockId, pos);
                return true;
            }

            if (belowBlock.LiquidCode == waterCode)
            {
                for(var currentDepth = 1; currentDepth <= maxDepth + 1; currentDepth ++)
                {
                    belowBlock = blockAccessor.GetBlock(pos.DownCopy(currentDepth));
                    if (belowBlock.Fertility > 0)
                    {
                        Block aboveBlock = blockAccessor.GetBlock(pos.DownCopy(currentDepth - 1));
                        if(aboveBlock.LiquidCode != waterCode) return false;
                        if(currentDepth < minDepth + 1) return false;

                        Block placingBlock = blockAccessor.GetBlock(Code);
                        if (placingBlock == null) return false;

                        blockAccessor.SetBlock(placingBlock.BlockId, pos.DownCopy(currentDepth - 1));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
 
 
 
 