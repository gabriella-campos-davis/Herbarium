using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace herbarium 
{
    public class BEHerbariumBerryBush : BlockEntityBerryBush, IAnimalFoodSource
    {
        public BEHerbariumBerryBush() : base()
        {

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api is ICoreServerAPI)
            {

                if (Api.World.Config.GetBool("processCrops", true))
                {
                    RegisterGameTickListener(CheckPruneRegrowth, 8000);
                }

            }
        }

        public void Prune()
        {
            Pruned = true;
            LastPrunedTotalDays = Api.World.Calendar.TotalDays;
            MarkDirty(true);
        }

        private void CheckPruneRegrowth(float dt)
        {
            if (Api.World.Calendar.TotalDays - LastPrunedTotalDays > 3 * Api.World.Calendar.DaysPerMonth / (float)Api.World.Config.GetDecimal("cropGrowthRateMul", 1.0))
            {
                Pruned = false;
            }
        }

        public float ConsumeOnePortion(Entity entity)
        {
            AssetLocation loc = Block.CodeWithParts("empty");
            if (!loc.Valid)
            {
                Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                return 0f;
            }

            Block nextBlock = Api.World.GetBlock(loc);
            if (nextBlock?.Code == null) return 0f;

            var bbh = Block.GetBehavior<BlockBehaviorHarvestable>();
            if (bbh?.harvestedStack != null)
            {
                ItemStack dropStack = bbh.harvestedStack.GetNextItemStack();
                Api.World.PlaySoundAt(bbh.harvestingSound, Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
                Api.World.SpawnItemEntity(dropStack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            var bbhm = Block.GetBehavior<BlockBehaviorHarvestMultiple>();
            if (bbhm?.harvestedStacks != null)
            {
                for(int i = 0; i < bbhm.harvestedStacks.Length; i++)
                {
                    ItemStack dropStack = bbhm.harvestedStacks[i].GetNextItemStack();
                    Api.World.SpawnItemEntity(dropStack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                Api.World.PlaySoundAt(bbhm.harvestingSound, Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
            }

            var bbhmk = Block.GetBehavior<BlockBehaviorHarvestMultipleWithKnife>();
            if (bbhmk?.harvestedStacks != null)
            {
                for(int i = 0; i < bbhmk.harvestedStacks.Length; i++)
                {
                    ItemStack dropStack = bbhmk.harvestedStacks[i].GetNextItemStack();
                    Api.World.SpawnItemEntity(dropStack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                Api.World.PlaySoundAt(bbhmk.harvestingSound, Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
            }


            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
            MarkDirty(true);

            return 0.1f;
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Pruned)
            {
                mesher.AddMeshData((Block as HerbariumBerryBush).GetPrunedMesh(Pos));
                return true;
            }

            return base.OnTesselation(mesher, tessThreadTesselator);
        }
    }
}