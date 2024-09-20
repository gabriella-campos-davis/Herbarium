using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace herbarium 
{
    public class BEHerbariumBerryBush : BlockEntityBerryBush
    {
        public BEHerbariumBerryBush() : base()
        {

        }

        public void Prune()
        {
            Pruned = true;
            LastPrunedTotalDays = Api.World.Calendar.TotalDays;
            MarkDirty(true);
        }

        public override void CheckGrow(float dt)
        {
            base.CheckGrow(dt);
            
            if (Api.World.Calendar.TotalDays - LastPrunedTotalDays > 9)
            {
                Pruned = false;
            }
        }

        public override void OnExchanged(Block block)
        {
            base.OnExchanged(block);
            transitionHoursLeft = GetHoursForNextStage();
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