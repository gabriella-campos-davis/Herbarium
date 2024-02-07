using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace herbarium
{
    public class BEDuckWeedRoot : BlockEntity
    {
        double totalHoursTillGrowth;
        long growListenerId;
        
        float swampyPoint = 8;
        Block block;

        
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            
            block = api.World.BlockAccessor.GetBlock(Pos);

            if (api is ICoreServerAPI)
            {
                growListenerId = RegisterGameTickListener(CheckGrow, 2000);
            }
        }

        public override void OnBlockPlaced(ItemStack byItemStack)
        {
            ICoreServerAPI sapi = Api as ICoreServerAPI;
        }


        private void CheckGrow(float dt)
        {
            if (Api.World.Calendar.TotalHours < totalHoursTillGrowth)
                return;

            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
            if (conds == null)
            {
                return;
            }

            BlockPos nPos = Pos.UpCopy(1);
            if (conds.Temperature > swampyPoint)
            {
                DoGrow();
                return;
            }


        }
        private void DoGrow()
        {
            Random rand = new Random();
            if (rand.Next(0, 10) > 8)
            {
                bool lakeTop = false;
                int currentDepth = 1;

                Block aboveBlock;

                BlockPos topBlockPos = new BlockPos(Pos.X, Api.World.BlockAccessor.GetRainMapHeightAt(Pos.Copy()), Pos.Z, 0);
                Block topBlock = Api.World.BlockAccessor.GetBlock(topBlockPos);
                /*
                if(!topBlock.IsLiquid()) 
                {
                    if(topBlock.BlockMaterial == EnumBlockMaterial.Plant) Api.World.BlockAccessor.SetBlock(0, Pos);
                    return;
                }
   

                Block placingBlock = this.Api.World.BlockAccessor.GetBlock(new AssetLocation(Block.Attributes["duckweedBlock"].ToString()));
                Api.World.BlockAccessor.SetBlock(placingBlock.BlockId, topBlockPos.UpCopy());

                Api.World.BlockAccessor.SetBlock(0, Pos);
                */
                while(lakeTop == false)
                {
                    aboveBlock = this.Api.World.BlockAccessor.GetBlock(Pos.UpCopy(currentDepth));

                    if (aboveBlock.LiquidCode != "water")
                    {
                        //if the block ontop of the lake is already duckweed we dont need to be here
                        if(aboveBlock.FirstCodePart() == "duckweed") this.Api.World.BlockAccessor.SetBlock(0, Pos);

                        Block placingBlock = this.Api.World.BlockAccessor.GetBlock(new AssetLocation(Block.Attributes["duckweedBlock"].ToString()));
                        if (placingBlock == null)
                        {
                            this.Api.World.Logger.Chat("duckwwed root tried place block and it's null. returns");
                            return;
                        } 

                        this.Api.World.BlockAccessor.SetBlock(placingBlock.BlockId, Pos.UpCopy(currentDepth));
                        lakeTop = true;
                    }

                    currentDepth += 1;
                }
                this.Api.World.BlockAccessor.SetBlock(0, Pos);
                
            }
        }
    }
}
