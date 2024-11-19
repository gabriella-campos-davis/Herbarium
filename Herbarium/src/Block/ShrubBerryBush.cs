using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using System;

namespace herbarium
{
    /*
        this class uses a specific model, ideally this should be abstracted
        so it is easier to define what parts of a model will be disappeared
        when pruning a bush,
    */
    public class ShrubBerryBush : HerbariumBerryBush
    {
        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return blockAccessor.GetBlock(pos.DownCopy()).Fertility > 0;
        }
    }
}