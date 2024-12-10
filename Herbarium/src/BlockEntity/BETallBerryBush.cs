using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BETallBerryBush : BEHerbariumBerryBush
    {
        protected double sproutingHoursLeft = -1;
        protected Block growthBlock;

        public BETallBerryBush() : base()
        {

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            growthBlock = Api.World.BlockAccessor.GetBlock(AssetLocation.Create(Block.Attributes["growthBlock"].ToString()));
            if (api is ICoreServerAPI) if (sproutingHoursLeft <= 0) sproutingHoursLeft = GetHoursSprouting();
        }

        NatFloat nextSproutDaysRnd
        {
            get
            {
                return Block?.Attributes?["sproutDays"].AsObject<NatFloat>() ?? NatFloat.create(EnumDistribution.UNIFORM, 25f, 5f);
            }
        }

        protected virtual bool CanSprout()
        {
            return ((growthBlock as BlockPlant)?.CanPlantStay(Api.World.BlockAccessor, Pos.UpCopy()) ?? false) &&
                   (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Air);
        }

        protected override float IntervalHours(double daysToCheck)
        {
            if (CanSprout()) return 2f;

            return base.IntervalHours(daysToCheck);
        }

        protected override bool CheckGrowExtra()
        {
            base.CheckGrowExtra();

            if (!CanSprout()) sproutingHoursLeft = GetHoursSprouting();
            else if (sproutingHoursLeft <= 0)
            {
                if (CanSprout()) Api.World.BlockAccessor.SetBlock(growthBlock.BlockId, Pos.UpCopy());
                sproutingHoursLeft = GetHoursSprouting();

                (Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy()) as BEClipping)?.OnGrowth(lastCheckAtTotalDays);

                return true;
            }

            return false;
        }

        public virtual double GetHoursSprouting()
        {
            float hours = nextSproutDaysRnd.nextFloat(1, Api.World.Rand) * Api.World.Calendar.HoursPerDay / growthRateMul;

            return growByMonth ? hours / 9 * Api.World.Calendar.DaysPerMonth : hours;
        }

        public override void UpdateHoursLeft(float intervalHours, ref float intervalDays, ref double daysToCheck)
        {
            base.UpdateHoursLeft(intervalHours, ref intervalDays, ref daysToCheck);

            sproutingHoursLeft -= intervalHours;
        }

        public override bool StopGrowth(float intervalHours)
        {
            sproutingHoursLeft += intervalHours;

            return base.StopGrowth(intervalHours);
        }

        public override bool ResetGrowth()
        {
            sproutingHoursLeft = GetHoursSprouting();

            return base.ResetGrowth();
        }

        public override bool RevertGrowth()
        {
            sproutingHoursLeft = GetHoursSprouting();

            return base.ResetGrowth();
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            sproutingHoursLeft = tree.GetDecimal("sproutingHoursLeft");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("sproutingHoursLeft", sproutingHoursLeft);
        }

        public override void GetMainInfo(IPlayer forPlayer, StringBuilder sb)
        {
            base.GetMainInfo(forPlayer, sb);
            string failureCode = "";

            if (!simplifiedTooltips)
            {
                if (CanSprout())
                {
                    if (TemperatureState == EnumHBBTemp.Acceptable)
                    {
                        double daysleft = sproutingHoursLeft / Api.World.Calendar.HoursPerDay;

                        if (daysleft < 1)
                        {
                            sb.AppendLine(Lang.Get("berrybush-sprouting-1day"));
                        }
                        else
                        {
                            sb.AppendLine(Lang.Get("berrybush-sprouting-xdays", (int)daysleft));
                        }
                    }
                }
                else if ((growthBlock as BlockClipping)?.CanPlaceClipping(Api.World.BlockAccessor, Pos.UpCopy(), ref failureCode) != null)
                {
                    if (failureCode == "fruitvineclipping-nosupport") sb.AppendLine(Lang.Get("fruitingvine-sprouting-nosupport"));
                }
            }
        }
    }
}