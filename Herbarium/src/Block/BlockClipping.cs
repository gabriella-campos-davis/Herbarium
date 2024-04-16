using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using System.Linq;
using System;
using Vintagestory.API.Common.Entities;
using herbarium.config;
using herbarium;

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
            Block block = blockAccessor.GetBlock(pos.DownCopy());
            BlockEntity blockEntity = blockAccessor.GetBlockEntity(pos.DownCopy());

            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());
            BlockEntity belowBlockEntity = blockAccessor.GetBlockEntity(pos.DownCopy());

            Block belowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
            BlockEntity belowBelowBlockEntity = blockAccessor.GetBlockEntity(pos.DownCopy(2));


            Block belowBelowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(3));

            BlockEntity betallbush = new BETallBerryBush();

           if (belowBlock.Fertility > 0) return true; //we're on ground, we can all be here
            if (belowBlock is HerbariumBerryBush || belowBlock is PricklyBerryBush)
            {
                if(belowBlockEntity is BETallBerryBush && blockEntity is BETallBerryBush)
                {
                    if(belowBlock.Attributes["isLarge"].AsBool())
                    {
                        if(belowBelowBelowBlock.Fertility > 0) return true;
                        return false;
                    }
                    if(belowBelowBlock.Fertility > 0) return true;
                    return false;
                }
                return false;
            }
            return false;
        }
    }
}
