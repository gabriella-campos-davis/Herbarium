using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace herbarium 
{
    public class BEHerbariumBerryBush : BEBerryPlant
    {
        public bool Pruned;
        public double LastPrunedTotalDays;

        public BEHerbariumBerryBush() : base()
        {

        }
        public virtual void Prune()
        {
            Pruned = true;
            LastPrunedTotalDays = Api.World.Calendar.TotalDays;
            MarkDirty(true);
        }

        protected override void CheckGrow(float dt)
        {
            base.CheckGrow(dt);

            if (Api.World.Calendar.TotalDays - LastPrunedTotalDays > 3 * Api.World.Calendar.DaysPerMonth / (float)Api.World.Config.GetDecimal("cropGrowthRateMul", 1.0))
            {
                Pruned = false;
                MarkDirty(false);
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            Pruned = tree.GetBool("pruned");
            LastPrunedTotalDays = tree.GetDecimal("lastPrunedTotalDays");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetBool("pruned", Pruned);
            tree.SetDouble("lastPrunedTotalDays", LastPrunedTotalDays);
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