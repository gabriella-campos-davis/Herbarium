using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace herbarium
{
    //a class only used to get tree seeds to spin, because i couldn't figure out how to get them to spin without doing this
    public class ItemWildTreeSeed : ItemTreeSeed 
    {
        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "treeSeedInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();

                foreach (Block block in api.World.Blocks)
                {
                    if (block.Code == null || block.EntityClass == null) continue;
                    if (block.Fertility > 0)
                    {
                        stacks.Add(new ItemStack(block));
                    }
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "heldhelp-plant",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "shift",
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }

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