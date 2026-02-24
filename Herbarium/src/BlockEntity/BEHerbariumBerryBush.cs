using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace herbarium 
{
    public class BEHerbariumBerryBush : BEGroundBerryPlant
    {
        public bool Pruned;
        double LastPrunedTotalDays = -1;
        protected double prunedHoursLeft = -1;

        public BEHerbariumBerryBush() : base()
        {
            
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api is ICoreServerAPI)
            {
                if (LastPrunedTotalDays != -1) prunedHoursLeft = LastPrunedTotalDays * Api.World.Calendar.HoursPerDay + GetPrunedHours() - api.World.Calendar.TotalDays;
                if (prunedHoursLeft <= 0) prunedHoursLeft = GetPrunedHours();
            }
        }

        protected NatFloat nextClippedDaysRnd
        {
            get
            {
                return Block?.Attributes?["clippedDays"].AsObject<NatFloat>() ?? NatFloat.create(EnumDistribution.UNIFORM, 25f, 5f);
            }
        }

        public virtual void Prune()
        {
            Pruned = true;
            prunedHoursLeft = GetPrunedHours();
            MarkDirty(true);
        }

        protected override float IntervalHours(double daysToCheck)
        {
            if (Pruned) return 2f;

            return base.IntervalHours(daysToCheck);
        }

        protected override bool CheckGrowExtra()
        {
            base.CheckGrowExtra();

            if (Pruned && prunedHoursLeft <= 0)
            {
                Pruned = false;
                MarkDirty(false);
            }

            return false;
        }

        public virtual double GetPrunedHours()
        {
            float hours = nextClippedDaysRnd.nextFloat(1, Api.World.Rand) * Api.World.Calendar.HoursPerDay / growthRateMul;

            return growByMonth ? hours / 9 * Api.World.Calendar.DaysPerMonth : hours;
        }

        public override void UpdateHoursLeft(float intervalHours, ref float intervalDays, ref double daysToCheck)
        {
            base.UpdateHoursLeft(intervalHours, ref intervalDays, ref daysToCheck);
            if (Pruned) prunedHoursLeft -= intervalHours;
        }

        public override bool StopGrowth(float intervalHours)
        {
            if (Pruned) prunedHoursLeft += intervalHours;

            return base.StopGrowth(intervalHours);
        }

        public override bool ResetGrowth()
        {
            if (Pruned) prunedHoursLeft = GetPrunedHours();

            return base.ResetGrowth();
        }

        public override bool RevertGrowth()
        {
            if (Pruned) prunedHoursLeft = GetPrunedHours();

            return base.RevertGrowth();
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            Pruned = tree.GetBool("pruned");
            LastPrunedTotalDays = tree.GetDecimal("lastPrunedTotalDays", -1);
            prunedHoursLeft = tree.GetDecimal("prunedHoursLeft");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetBool("pruned", Pruned);
            tree.SetDouble("prunedHoursLeft", prunedHoursLeft);
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

        public override void GetExtraInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (Pruned && !simplifiedTooltips && TemperatureState == EnumHBBTemp.Acceptable)
            {
                double daysleft = prunedHoursLeft / Api.World.Calendar.HoursPerDay;

                if (daysleft < 1)
                {
                    sb.AppendLine(Lang.Get("berrybush-clipping-growth-1day"));
                }
                else
                {
                    sb.AppendLine(Lang.Get("berrybush-clipping-growth-xdays", (int)daysleft));
                }
            }

            base.GetExtraInfo(forPlayer, sb);
        }
    }
}