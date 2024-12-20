using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BlockLeavesDropCanes : BlockLeaves
    {
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            var drops = base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);

            if(Attributes["dropsCanes"].AsBool() == true)
            {
                ItemStack caneItem = new ItemStack(api.World.GetItem(AssetLocation.Create(Attributes["willowCaneItem"].ToString(), Code.Domain)), 1);
                if(world.Rand.Next(0, 10) > 4) world.SpawnItemEntity(caneItem, pos.ToVec3d());

                //drops.Prepend(caneItem);
            }

            if(Attributes["dropsLeaves"].AsBool() == true)
            {
                ItemStack leafItem = new ItemStack(api.World.GetItem(AssetLocation.Create(Attributes["leafItem"].ToString(), Code.Domain)), 1);
                if(world.Rand.Next(0, 10) > 2) world.SpawnItemEntity(leafItem, pos.ToVec3d());
                //drops.Prepend(leafItem);
            }

            return drops;
        }
    }
}
