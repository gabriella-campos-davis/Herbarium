using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using System;

namespace herbarium
{
    public class DuckWeed : BlockPlant
    {
        public string orientation = "empty";
        public float needsToDieTemp = -9f;
        public float dieTemp = 4f;
        public float duckweedRootTemp = 17f;
        public float permaDuckweedTemp = 28f;
        public Random rand = new Random();

        public int growthDepth = 3;
        public int rootDepth = 2;
        bool checkForRivers = false;
        int duckweedPoints = 0;


        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            if (api.ModLoader.IsModEnabled("rivers")){
                checkForRivers = true;
            }
            if(Attributes["growthDepth"].Exists && Attributes["rootDepth"].Exists)
            {
              growthDepth = Attributes["growthDepth"].AsInt();
              rootDepth = Attributes["rootDepth"].AsInt();
            }
        }
        
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (CanPlantStay(world.BlockAccessor, blockSel.Position.UpCopy()))
            {
                blockSel = blockSel.Clone();
                blockSel.Position = blockSel.Position.Up();
                return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            }
            
            failureCode = "requirefullwater";

            return false;
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block block = blockAccessor.GetBlock(pos.DownCopy(), BlockLayersAccess.Fluid);
            Block upblock = blockAccessor.GetBlock(pos, BlockLayersAccess.Fluid);
            if (block.LiquidCode != "water") return false;
            if (blockAccessor.GetBlock(pos.DownCopy(growthDepth), BlockLayersAccess.Fluid).Id != 0) return false;
            if(block.Attributes["pushVector"].Exists)
            {
                return false;
            }
            return block.IsLiquid() && block.LiquidLevel == 7 && block.LiquidCode == "water" && upblock.Id==0;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            api.Logger.Debug("neighborBlockupdate");
            Block neighbourBlock = world.BlockAccessor.GetBlock(neibpos);
            Block[] neighbourBlocks = new Block[4];
                neighbourBlocks[0] = api.World.BlockAccessor.GetBlock(pos.NorthCopy());
                neighbourBlocks[1] = api.World.BlockAccessor.GetBlock(pos.EastCopy());
                neighbourBlocks[2] = api.World.BlockAccessor.GetBlock(pos.SouthCopy());
                neighbourBlocks[3] = api.World.BlockAccessor.GetBlock(pos.WestCopy());
            Block[] belowBlocks = new Block[4];
                neighbourBlocks[0] = api.World.BlockAccessor.GetBlock(pos.DownCopy().NorthCopy());
                neighbourBlocks[1] = api.World.BlockAccessor.GetBlock(pos.DownCopy().EastCopy());
                neighbourBlocks[2] = api.World.BlockAccessor.GetBlock(pos.DownCopy().SouthCopy());
                neighbourBlocks[3] = api.World.BlockAccessor.GetBlock(pos.DownCopy().WestCopy());

            duckweedPoints = 0;

            for(int i = 0; i < 4; i++)
            {
                if(neighbourBlocks[i].Code.ToString().Contains("duckweed"))
                {
                    api.Logger.Debug("neighbor is duckweed");
                    if(neighbourBlocks[i].Variant["stage"].ToString() == "1")
                    {
                        duckweedPoints += 1;
                        api.Logger.Debug("added 1 duckweed point");
                    }
                    if(neighbourBlock.Variant["stage"].ToString() == "2")
                    {
                        if(world.BlockAccessor.GetBlock(pos.DownCopy(2)).LiquidCode != "water" && world.BlockAccessor.GetBlock(pos.DownCopy(2)).BlockMaterial != EnumBlockMaterial.Plant)
                        {
                            duckweedPoints += 2;
                            api.Logger.Debug("added 2 duckweed point");
                        } else
                        {
                            duckweedPoints += 1;
                            api.Logger.Debug("added 1 duckweed point");
                        }
                    }   
                }

                if(neighbourBlocks[i] is null)
                {
                    if(belowBlocks[i].LiquidCode != "water")
                    {
                        duckweedPoints += 1;
                        api.Logger.Debug("added 1 duckweed point from shore");   
                    }
                }
            }

            Block placingBlock;
            if(duckweedPoints >= 4)
            {
                api.Logger.Debug("more than 4 points");   
                placingBlock = world.BlockAccessor.GetBlock(this.CodeWithPart("2", 3));
                world.BlockAccessor.SetBlock(placingBlock.BlockId, pos);
            }
            if(duckweedPoints >= 8)
            {
                api.Logger.Debug("more than 8 points");   
                placingBlock = world.BlockAccessor.GetBlock(this.CodeWithPart("3", 3));
                world.BlockAccessor.SetBlock(placingBlock.BlockId, pos);
            }

            base.OnNeighbourBlockChange(world, pos, neibpos);
        }

        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, IRandom worldgenRandom, BlockPatchAttributes attributes = null)
        {
            // Don't spawn in 4 deep water
            if (blockAccessor.GetBlock(pos.DownCopy(growthDepth), BlockLayersAccess.Fluid).Id != 0) return false;

            //if()

            return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldgenRandom, attributes);
        }

        public override bool ShouldReceiveServerGameTicks(IWorldAccessor world, BlockPos pos, Random offThreadRandom, out object extra)
        {
            extra = null;
            ClimateCondition conds = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.NowValues);
            if (conds is null) return false;
            double chance;
            if (conds.WorldGenTemperature >= permaDuckweedTemp) return false; //we don't need to tick here at all (probably)
            if (conds.Temperature <= needsToDieTemp) return true; //make sure we die when it's super cold
            if (conds.Temperature >= dieTemp && conds.Temperature <= duckweedRootTemp) return false; // don't grow here it's weird and unnecessary
            /*
                duckweed algorithm
            */
            chance = conds == null ? 0 : 0.5*Math.Sin(0.12*conds.Temperature + 10) + 0.5;


            return offThreadRandom.NextDouble() < chance;
        }

         public override void OnServerGameTick(IWorldAccessor world, BlockPos pos, object extra = null)
        {
            ClimateCondition conds = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.NowValues);
            if (conds.Temperature < dieTemp) // did we get a sever tick to die or beacuse we need to grow?
            {
                DoDeath(world, pos);
            }

            if (conds.Temperature > dieTemp)
            {
                DoGrowth(world, pos);
            }     
        }
        private void DoDeath(IWorldAccessor world, BlockPos pos)
        {
            //random chance to spawn a duckweed root when it dies.
            if (rand.Next(0, 10) > 8)
            {
                bool lakeBed = false;
                int currentDepth = 1;

                Block belowBlock;

                //find the lakebed, place the root, then die
                while(lakeBed == false)
                {
                    belowBlock = world.BlockAccessor.GetBlock(pos.DownCopy(currentDepth));
                    if (belowBlock.LiquidCode != "water" && belowBlock.BlockMaterial != EnumBlockMaterial.Plant)
                    {
                        Block placingBlock = world.BlockAccessor.GetBlock(new AssetLocation(Attributes["rootBlock"].ToString()));
                        if (placingBlock == null) return;

                        if(world.BlockAccessor.GetBlock(pos.DownCopy(currentDepth-1)).BlockMaterial ==  EnumBlockMaterial.Plant) return;
                        if(currentDepth > rootDepth) return;
                        world.BlockAccessor.SetBlock(placingBlock.BlockId, pos.DownCopy(currentDepth - 1));

                        lakeBed = true;
                    }
                    currentDepth += 1;
                }
                world.BlockAccessor.SetBlock(0, pos);
            }
            world.BlockAccessor.SetBlock(0, pos);
        }

        private void DoGrowth(IWorldAccessor world, BlockPos pos)
        {
            if(rand.Next(0,10) > 8) //Random chance to make grow less, because duckweed grows aggressivley ;(
            {
                int randomDirection = rand.Next(0,4);
                BlockPos newDuckweedPos = new BlockPos(pos.X, pos.Y, pos.Z);

                if(randomDirection == 0) newDuckweedPos.Add(1,0,0);
                else if(randomDirection == 1) newDuckweedPos.Add(-1,0,0);
                else if(randomDirection == 2) newDuckweedPos.Add(0,0,1);
                else if(randomDirection == 3) newDuckweedPos.Add(0,0,-1);

                if(world.BlockAccessor.GetBlock(newDuckweedPos).BlockMaterial != EnumBlockMaterial.Air) return; // is the block already occupied?
                if(world.BlockAccessor.GetBlock(newDuckweedPos.DownCopy(), BlockLayersAccess.Fluid).LiquidCode != "water") return; //are we placing in water?
                if(world.BlockAccessor.GetBlock(newDuckweedPos.DownCopy(growthDepth), BlockLayersAccess.Fluid).LiquidCode == "water") return; //is the water too deep?

                Block placingBlock = world.BlockAccessor.GetBlock(new AssetLocation(Attributes["duckweedBlock"].ToString()));
                world.BlockAccessor.SetBlock(placingBlock.BlockId, newDuckweedPos);
            }
        }


        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return null;
        }
    }
}