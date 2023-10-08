using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace herbarium 
{
    public class BEShrubBerryBush : BEHerbariumBerryBush
    {

        public BEShrubBerryBush() : base()
        {

        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Pruned)
            {
                mesher.AddMeshData((Block as ShrubBerryBush).GetPrunedMesh(Pos));
                return true;
            }

            return base.OnTesselation(mesher, tessThreadTesselator);
        }
    }
}