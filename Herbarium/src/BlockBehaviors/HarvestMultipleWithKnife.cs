using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using System.Collections.Generic;


namespace herbarium
{
    public class BlockBehaviorHarvestMultipleWithKnife : BlockBehavior
    {
        float harvestTime;
        public BlockDropItemStack[] harvestedStacks;
        List<ItemStack> toolStacklist = new List<ItemStack>();

        public AssetLocation harvestingSound;

        AssetLocation harvestedBlockCode;
        Block harvestedBlock;
        string interactionHelpCode;

        public BlockBehaviorHarvestMultipleWithKnife(Block block) : base(block)
        {
        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            interactionHelpCode = properties["harvestTime"].AsString("blockhelp-harvetable-harvest");
            harvestTime = properties["harvestTime"].AsFloat(0);
            harvestedStacks = properties["harvestedStacks"].AsObject<BlockDropItemStack[]>(null);
            //properties[].AsObject<BlockDropItemStack>(null);

            string code = properties["harvestingSound"].AsString("game:sounds/block/leafy-picking");
            if (code != null) {
                harvestingSound = AssetLocation.Create(code, block.Code.Domain);
            }

            code = properties["harvestedBlockCode"].AsString();
            if (code != null)
            {
                harvestedBlockCode = AssetLocation.Create(code, block.Code.Domain);
            }
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if(harvestedStacks is not null )
            {
                for(int i = 0; i < harvestedStacks.Length; i++)
                {
                    harvestedStacks[i]?.Resolve(api.World, "harvestedStack of block ", block.Code);
                }
            }


            harvestedBlock = api.World.GetBlock(harvestedBlockCode);
            if (harvestedBlock == null)
            {
                api.World.Logger.Warning("Unable to resolve harvested block code '{0}' for block {1}. Will ignore.", harvestedBlockCode, block.Code);
            }
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return false;
            }
            
            handling = EnumHandling.PreventDefault;

            if (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Knife)
            {
                    return false;
            }

            for(int i = 0; i < harvestedStacks.Length; i++)
            {
                if (harvestedStacks[i] != null)
                {
                    world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                    return true;
                }
            }
            

            return false;
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handled)
        {
            if (blockSel == null) return false;

            handled = EnumHandling.PreventDefault;

            if (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Knife)
            {
                    return false;
            }

            (byPlayer as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemAttack);

            if (world.Rand.NextDouble() < 0.05)
            {
                world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
            }

            if (world.Side == EnumAppSide.Client && world.Rand.NextDouble() < 0.25)
            {
                world.SpawnCubeParticles(blockSel.Position.ToVec3d().Add(blockSel.HitPosition), harvestedStacks[0].ResolvedItemstack, 0.25f, 1, 0.5f, byPlayer, new Vec3f(0, 1, 0));
            }

            return world.Side == EnumAppSide.Client || secondsUsed < harvestTime;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handled)
        {
            handled = EnumHandling.PreventDefault;

            if (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Knife)
            {
                    return;
            }

            if (secondsUsed > harvestTime - 0.05f && harvestedStacks != null && world.Side == EnumAppSide.Server)
            {
                float dropRate = 1;

                if (block.Attributes?.IsTrue("forageStatAffected") == true)
                {
                    dropRate *= byPlayer.Entity.Stats.GetBlended("forageDropRate");
                }

                for(int i = 0; i < harvestedStacks.Length; i++)
                {
                    ItemStack stack = harvestedStacks[i].GetNextItemStack(dropRate);
                    if (stack == null) continue;
                    var origStack = stack.Clone();

                    if (byPlayer?.InventoryManager.TryGiveItemstack(stack) == false)
                    {
                        world.SpawnItemEntity(stack, blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                    }

                    TreeAttribute tree = new TreeAttribute();
                    tree["itemstack"] = new ItemstackAttribute(origStack.Clone());
                    tree["byentityid"] = new LongAttribute(byPlayer.Entity.EntityId);
                    world.Api.Event.PushEvent("onitemcollected", tree);
                }

                if (harvestedBlock != null)
                {
                    world.BlockAccessor.SetBlock(harvestedBlock.BlockId, blockSel.Position);
                }

                world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
            }
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handled)
        {
            if (harvestedStacks != null)
            {
                foreach (Item item in world.Items)
                {
                    if (item.Code == null)
                        continue;

                    if (item.Tool == EnumTool.Knife)
                    {
                        toolStacklist.Add(new ItemStack(item));
                    } 
                }
                
                bool notProtected = true;

                if (world.Claims != null && world is IClientWorldAccessor clientWorld && clientWorld.Player?.WorldData.CurrentGameMode == EnumGameMode.Survival)
                {
                    EnumWorldAccessResponse resp = world.Claims.TestAccess(clientWorld.Player, selection.Position, EnumBlockAccessFlags.Use);
                    if (resp != EnumWorldAccessResponse.Granted) notProtected = false;
                }


                if (notProtected) return new WorldInteraction[]
                {
                    
                    new WorldInteraction()
                    {
                        Itemstacks = toolStacklist.ToArray(),
                        ActionLangCode = interactionHelpCode,
                        MouseButton = EnumMouseButton.Right
                    }
                };
            }

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handled);
        }
    }
}