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
                if(!Attributes["willowCaneItem"].Exists)
                {
                    api.Logger.Warning("Herbarium: block {0} has dropsCanes=true but no 'willowCaneItem' attribute; skipping cane drop.", Code);
                }
                else
                {
                    Item caneItemType = api.World.GetItem(AssetLocation.Create(Attributes["willowCaneItem"].ToString(), Code.Domain));
                    if(caneItemType == null)
                    {
                        api.Logger.Warning("Herbarium: block {0} willowCaneItem '{1}' does not resolve to a known item; skipping cane drop.", Code, Attributes["willowCaneItem"].ToString());
                    }
                    else
                    {
                        ItemStack caneItem = new ItemStack(caneItemType, 1);
                        if(world.Rand.Next(0, 10) > 4) world.SpawnItemEntity(caneItem, pos.ToVec3d());
                    }
                }
            }

            if(Attributes["dropsLeaves"].AsBool() == true)
            {
                if(!Attributes["leafItem"].Exists)
                {
                    api.Logger.Warning("Herbarium: block {0} has dropsLeaves=true but no 'leafItem' attribute; skipping leaf drop.", Code);
                }
                else
                {
                    Item leafItemType = api.World.GetItem(AssetLocation.Create(Attributes["leafItem"].ToString(), Code.Domain));
                    if(leafItemType == null)
                    {
                        api.Logger.Warning("Herbarium: block {0} leafItem '{1}' does not resolve to a known item; skipping leaf drop.", Code, Attributes["leafItem"].ToString());
                    }
                    else
                    {
                        ItemStack leafItem = new ItemStack(leafItemType, 1);
                        if(world.Rand.Next(0, 10) > 2) world.SpawnItemEntity(leafItem, pos.ToVec3d());
                    }
                }
            }

            return drops;
        }
    }
}
