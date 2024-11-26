using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace herbarium
{
    public class GroundBerryPlant : BlockPlant
    {
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            var drops = base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);

            foreach (var drop in drops)
            {
                if (drop.Collectible.NutritionProps == null) continue;

                float dropRate = 1;

                if (Attributes?.IsTrue("forageStatAffected") == true)
                {
                    dropRate *= byPlayer?.Entity.Stats.GetBlended("forageDropRate") ?? 1;
                }

                drop.StackSize = GameMath.RoundRandom(api.World.Rand, drop.StackSize * dropRate);
            }

            return drops;
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return blockAccessor.GetBlock(pos.DownCopy()).Fertility > 0;
        }

        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex = -1)
        {
            if (Textures == null || Textures.Count == 0) return 0;
            BakedCompositeTexture tex = Textures?.First().Value?.Baked;
            if (tex == null) return 0;

            int color = capi.BlockTextureAtlas.GetRandomColor(tex.TextureSubId, rndIndex);

            return capi.World.ApplyColorMapOnRgba("climatePlantTint", "seasonalFoliage", color, pos.X, pos.Y, pos.Z);
        }

        public override int GetColor(ICoreClientAPI capi, BlockPos pos)
        {
            int color = base.GetColorWithoutTint(capi, pos);

            return capi.World.ApplyColorMapOnRgba("climatePlantTint", "seasonalFoliage", color, pos.X, pos.Y, pos.Z);
        }
    }
}
