using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using System.Linq;
using Vintagestory.API.Common.Entities;
using herbarium.config;
using System;

namespace herbarium
{
    /*
        this class uses a specific model, ideally this should be abstracted
        so it is easier to define what parts of a model will be disappeared
        when pruning a bush,
    */
    public class ShrubBerryBush : HerbariumBerryBush
    {
        MeshData[] prunedmeshes;
        string[] prunedMeshFaces;
        string[] fruitingFaces;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            prunedMeshFaces = Attributes["prunedMeshFaces"].AsObject<String[]>(null);
            fruitingFaces = Attributes["fruitingFaces"].AsObject<String[]>(null);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;
        }

        new public MeshData GetPrunedMesh(BlockPos pos)
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

            var selems = prunedMeshFaces;
            //if (fruitingFaces is null) return;
            if (State == "empty")
            {
                for(int j = 0; j < fruitingFaces.Length; j++)
                {
                    selems = selems.Remove(fruitingFaces[j]);
                }
            } 

            for (int i = 0; i < Shape.BakedAlternates.Length; i++)
            {
                var cshape = Shape.BakedAlternates[i];
                var shape = capi.TesselatorManager.GetCachedShape(cshape.Base);
                capi.Tesselator.TesselateShape(this, shape, out prunedmeshes[i], this.Shape.RotateXYZCopy, null, selems);
            }
        }
        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block block = blockAccessor.GetBlock(pos.DownCopy());
            BlockEntity blockEntity = blockAccessor.GetBlockEntity(pos.DownCopy());

            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());
            BlockEntity belowBlockEntity = blockAccessor.GetBlockEntity(pos.DownCopy());

            Block belowBelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
            if (belowBlock.Fertility > 0) return true;
            else return false;
        }
    }
}