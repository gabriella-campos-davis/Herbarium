using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace herbarium
{
    //a class only used to get tree seeds to spin, because i couldn't figure out how to get them to spin without doing this
    public class ItemWildTreeSeed : ItemTreeSeed 
    {
        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);

            if (target == EnumItemRenderTarget.Ground)
            {
                EntityItem ei = (renderinfo.InSlot as EntityItemSlot).Ei;
                if (!ei.Collided && !ei.Swimming)
                {
                    renderinfo.Transform = renderinfo.Transform.Clone(); // dont override the original transform
                    renderinfo.Transform.Rotation.X = -90;
                    renderinfo.Transform.Rotation.Y = (float)(capi.World.ElapsedMilliseconds % 360.0) * 2f;
                    renderinfo.Transform.Rotation.Z = 0;
                }
            }
        }
    }
}