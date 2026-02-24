using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;


namespace Herbarium.src.BlockBehaviors
{
    internal class BlockBehaviorDamagingPlant : BlockBehavior
    {
        public Block block;

        public BlockBehaviorDamagingPlant(Block block)
            : base(block)
        {
            this.block = block;
        }
    }
}
