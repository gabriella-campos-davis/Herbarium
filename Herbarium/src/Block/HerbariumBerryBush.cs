using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using System.Linq;
using System;
using Vintagestory.API.Common.Entities;
using herbarium.config;
using herbarium;

namespace herbarium
{
    public class HerbariumBerryBush : BlockPlant
    {
        WorldInteraction[] interactions;
        ItemStack clipping = new();        
        AssetLocation harvestedSound = AssetLocation.Create("herbarium:sounds/branch_trim");
        AssetLocation harvestingSound = AssetLocation.Create("game:sounds/walk/inside/leafy/bushrustle4");
        MeshData[] prunedmeshes;
        float harvestTime = 0.6f;

        public string State => Variant["state"];
        public string Type => Variant["type"];

        public MeshData GetPrunedMesh(BlockPos pos)
        {
            if (api == null) return null;
            if (prunedmeshes == null) genPrunedMeshes();

            int rnd = RandomizeAxes == EnumRandomizeAxes.XYZ ? GameMath.MurmurHash3Mod(pos.X, pos.Y, pos.Z, prunedmeshes.Length) : GameMath.MurmurHash3Mod(pos.X, 0, pos.Z, prunedmeshes.Length);

            return prunedmeshes[rnd];
        }

        private void genPrunedMeshes()
        {
            var capi = api as ICoreClientAPI;

            prunedmeshes = new MeshData[Shape.BakedAlternates.Length];

            var selems = new string[] { "Berries", "branchesN", "branchesS", "Leaves" };
            if (State == "empty") selems = selems.Remove("Berries");

            for (int i = 0; i < Shape.BakedAlternates.Length; i++)
            {
                var cshape = Shape.BakedAlternates[i];
                var shape = capi.TesselatorManager.GetCachedShape(cshape.Base);
                capi.Tesselator.TesselateShape(this, shape, out prunedmeshes[i], this.Shape.RotateXYZCopy, null, selems);
            }
        }

        
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "berrybushclippingInteractions", () =>
            {
                List<ItemStack> toolStacklist = new List<ItemStack>();
                foreach (Item item in api.World.Items)
                {
                    if (item.Code == null)
                        continue;

                    if(HerbariumConfig.Current.useKnifeForClipping && !HerbariumConfig.Current.useShearsForClipping && item.Tool == EnumTool.Knife)
                    {
                        toolStacklist.Add(new ItemStack(item));
                    }
                    else if(!HerbariumConfig.Current.useKnifeForClipping && HerbariumConfig.Current.useShearsForClipping && item.Tool == EnumTool.Shears)
                    {
                        toolStacklist.Add(new ItemStack(item));
                    }

                    else if(HerbariumConfig.Current.useKnifeForClipping && HerbariumConfig.Current.useShearsForClipping)
                    {
                        if (item.Tool == EnumTool.Knife || item.Tool == EnumTool.Shears)
                        {
                            toolStacklist.Add(new ItemStack(item));
                        } 
                    }
                }

                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        Itemstacks = toolStacklist.ToArray(),
                        MouseButton = EnumMouseButton.Right,
                        ActionLangCode = "herbarium:blockhelp-berrybush-clip",
                    }
                };
            });
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {         
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return false;
            }

            if ((byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Knife && HerbariumConfig.Current.useKnifeForClipping) ||
                (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Shears && HerbariumConfig.Current.useShearsForClipping))
            {
                if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEHerbariumBerryBush beugbush && !beugbush.Pruned)
                {
                    world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);                
                    return true;
                }
                else if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BETallBerryBush beugtbush && !beugtbush.Pruned )
                {
                    world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                    return true;
                }
                else if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEShrubBerryBush beugsbush && !beugsbush.Pruned )
                {
                    world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                    return true;
                }
                return false;       
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if ((byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Knife && HerbariumConfig.Current.useKnifeForClipping) ||
                (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Shears && HerbariumConfig.Current.useShearsForClipping))
            {
                if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEHerbariumBerryBush beugbush && beugbush.Pruned)
                {                
                    return false;
                }
                else if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BETallBerryBush beugtbush && beugtbush.Pruned )
                {
                    return false;
                }
                else if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEShrubBerryBush beugsbush && beugsbush.Pruned )
                {
                    return false;
                }

                if (blockSel == null) return false;

                (byPlayer as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemAttack);

                if (world.Rand.NextDouble() < 0.05)
                {
                    world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                }

                clipping = new ItemStack(api.World.GetItem(AssetLocation.Create(this.Attributes["pruneItem"].ToString())), 1);
                if (clipping is null){
                    api.Logger.Error("Attempted to create clipping for " + this.Variant["type"] + ", came back null.");
                    return false;
                }      

                if (world.Rand.NextDouble() < 0.25)
                {
                    world.SpawnCubeParticles(blockSel.Position.ToVec3d().Add(blockSel.HitPosition), clipping, 0.25f, 1, 0.5f, byPlayer, new Vec3f(0, 1, 0));
                    
                }

                return world.Side == EnumAppSide.Client || secondsUsed < harvestTime;
            }
            return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel);
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Knife &&
                byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Shears)
                {
                    base.OnBlockInteractStop(secondsUsed, world, byPlayer, blockSel);
                    return;
                }
            world.PlaySoundAt(harvestedSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
            if (secondsUsed > harvestTime - 0.05f && clipping != null && world.Side == EnumAppSide.Server)
            {
                if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEHerbariumBerryBush beugbush && !beugbush.Pruned)
                {
                    beugbush.Prune();
                    GiveClipping(world, byPlayer, blockSel);
                    world.PlaySoundAt(harvestedSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                    return;
                }
                else if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BETallBerryBush beugtbush && !beugtbush.Pruned )
                {
                    beugtbush.Prune();
                    GiveClipping(world, byPlayer, blockSel);
                    world.PlaySoundAt(harvestedSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                    return;
                }
                else if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEShrubBerryBush beugsbush && !beugsbush.Pruned )
                {
                    beugsbush.Prune();
                    GiveClipping(world, byPlayer, blockSel);
                    world.PlaySoundAt(harvestedSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                    return;
                }
                return;
            }
            return;
        }

        void GiveClipping(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (byPlayer?.InventoryManager.TryGiveItemstack(clipping) == false)
            {
                world.SpawnItemEntity(clipping, byPlayer.Entity.SidedPos.XYZ);
            }
                    
            byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.DamageItem(world, byPlayer as Entity, byPlayer.InventoryManager.ActiveHotbarSlot, 1);
            world.PlaySoundAt(harvestedSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);   
        }


        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());

            if(belowBlock is null) return false;
            if(belowBlock.Attributes is null) return false;

            if(belowBlock.Fertility > 0) return true; //we are on ground, everyone can be here
            if(belowBlock.BlockMaterial == EnumBlockMaterial.Air) return false;
            if(Attributes["stackable"].AsBool() is false) return false; //if we can't stack and aren't on ground, gtfo

            if(Attributes["stackable"].AsBool() is true)
            {
                Block belowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
                if(belowBelowBlock is null) return false;
                if(belowBelowBlock.Attributes is null) return false;
                if(belowBlock.BlockMaterial == EnumBlockMaterial.Air) return false;
                if(belowBlock.Attributes["stackable"].AsBool() is true && belowBelowBlock.Fertility > 0)
                {
                    return true;
                }
            }
            return false;
        }


        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            var drops = base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);

            foreach (var drop in drops)
            {
                if (drop.Collectible is HerbariumBerryBush || drop.Collectible is PricklyBerryBush) continue;

                float dropRate = 1;

                if (Attributes?.IsTrue("forageStatAffected") == true)
                {
                    dropRate *= byPlayer?.Entity.Stats.GetBlended("forageDropRate") ?? 1;
                }

                drop.StackSize = GameMath.RoundRandom(api.World.Rand, drop.StackSize * dropRate);
            }

            return drops;
        }

        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex = -1)
        {
            if (Textures == null || Textures.Count == 0) return 0;
            BakedCompositeTexture tex = Textures?.First().Value?.Baked;
            if (tex == null) return 0;

            int color = capi.BlockTextureAtlas.GetRandomColor(tex.TextureSubId, rndIndex);
            color = capi.World.ApplyColorMapOnRgba("climatePlantTint", "seasonalFoliage", color, pos.X, pos.Y, pos.Z);
            return color;
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer).Append(interactions);
        }
    }
}
