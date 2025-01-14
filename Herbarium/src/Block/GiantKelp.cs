using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
namespace herbarium
{
    public class GiantKelp : BlockPlant
    {
        public ICoreAPI Api => api;
        int maxDepth;
        int minDepth = 2 ;
        string waterCode;
        int kelpMaxHeight = 20;
        int kelpMinHeight = 5;


        public override void OnLoaded(ICoreAPI api)
        {
             base.OnLoaded(api);

            maxDepth = Attributes["maxDepth"].AsInt();
            minDepth = Attributes["minDepth"].AsInt();
            waterCode = Attributes["waterCode"].AsString();

        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block aboveFluid = blockAccessor.GetBlock(pos.UpCopy(), BlockLayersAccess.Fluid);
            Block belowFluid = blockAccessor.GetBlock(pos.DownCopy(), BlockLayersAccess.Fluid);

            if(aboveFluid.LiquidCode != waterCode || aboveFluid is not GiantKelp || belowFluid.LiquidCode != waterCode || belowFluid is not GiantKelp) return false;
            return true;
        }

        /* Testing out stuff for windwave, will revisit when models are fully done
        public override void OnJsonTesselation(ref MeshData sourceMesh, ref int[] lightRgbsByCorner, BlockPos pos, Block[] chunkExtBlocks, int extIndex3d)
        {
            int windData = 0;

            Block aboveBlock = api.World.BlockAccessor.GetBlock(pos.X, pos.Y + 1, pos.Z);
            int height = 1;
            while(aboveBlock is GiantKelp)
            {
                aboveBlock = api.World.BlockAccessor.GetBlock(pos.X, pos.Y + height, pos.Z);
                windData += 1;
                height += 1;
            }

            for (int i = 0; i < sourceMesh.FlagsCount; i++)
            {
                float y = sourceMesh.xyz[i * 3 + 1];
                sourceMesh.Flags[i] = (sourceMesh.Flags[i] & VertexFlags.ClearWindDataBitsMask) | (windData + (y > 0 ? 1 : 0)) << VertexFlags.WindDataBitsPos;
            }
        }
        */

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

                        //PLACEBLOCK
                        Block baseBlock = blockAccessor.GetBlock(Code);
                        if (baseBlock == null) return false;

                        BlockPos kelpPos = pos.DownCopy(currentDepth - 1);
                        blockAccessor.SetBlock(baseBlock.BlockId, kelpPos);

                        PlaceKelp(blockAccessor, kelpPos, worldgenRandom, currentDepth);
                        return true;
                    }
                }
            }

            return false;
        }
        void PlaceKelp(IBlockAccessor blockAccessor, BlockPos pos, IRandom worldGenRand, int depth)
        {
            Block aboveBlock = blockAccessor.GetBlock(pos.UpCopy());

            Block middleBlock = blockAccessor.GetBlock(new AssetLocation(Attributes["middleBlock"].ToString()));
            Block topBlock = blockAccessor.GetBlock(new AssetLocation(Attributes["topBlock"].ToString()));

            int kelpHeight = worldGenRand.NextInt(kelpMaxHeight) + kelpMinHeight;

            for(var height = 1; height <= kelpHeight; height++)
            {
                aboveBlock = blockAccessor.GetBlock(pos.UpCopy(height));
                Block aboveAboveBlock = blockAccessor.GetBlock(pos.UpCopy(height + 1));
                if(aboveAboveBlock.LiquidCode != waterCode || height == kelpHeight - 1)
                {
                    blockAccessor.SetBlock(topBlock.BlockId, pos.UpCopy(height - 1));
                    return;
                }
                blockAccessor.SetBlock(middleBlock.BlockId, pos.UpCopy(height));
            }
        }
    }
}