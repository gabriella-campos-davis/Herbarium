using herbarium;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Vintagestory.GameContent
{

    public class BEBehaviorRootSuckers : BEBehaviorBerryPlant
    {
        protected double sproutingHoursLeft = -1;
        protected Block growthBlock;
        protected JsonObject Properties;

        public BEBehaviorRootSuckers(BlockEntity blockentity) : base(blockentity)
        {

        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Properties = properties;

            growthBlock = Api.World.BlockAccessor.GetBlock(AssetLocation.Create(properties["suckerBlock"].ToString()));
            if (api is ICoreServerAPI) if (sproutingHoursLeft <= 0) sproutingHoursLeft = GetHoursSprouting();
        }

        NatFloat nextSproutDaysRnd
        {
            get
            {
                return Properties["sproutDays"].AsObject<NatFloat>() ?? NatFloat.create(EnumDistribution.UNIFORM, 25f, 5f);
            }
        }

        protected virtual bool CanSprout()
        {
            return ((growthBlock as BlockPlant)?.CanPlantStay(Api.World.BlockAccessor, Pos.UpCopy()) ?? false) &&
                   (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Air);
        }

        public override float? IntervalHours(double daysToCheck, ref EnumHandling handling)
        {
            if (CanSprout())
            {
                handling = EnumHandling.PreventSubsequent;
                return 2f;
            }

            return base.IntervalHours(daysToCheck, ref handling);
        }

        public override bool? CheckGrowExtra(ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;

            if (!CanSprout()) sproutingHoursLeft = GetHoursSprouting();
            else if (sproutingHoursLeft <= 0)
            {
                if (CanSprout()) Api.World.BlockAccessor.SetBlock(growthBlock.BlockId, Pos.UpCopy());
                sproutingHoursLeft = GetHoursSprouting();

                (Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy()) as BEClipping)?.OnGrowth((Blockentity as BEBerryPlant).lastCheckAtTotalDays);

                return true;
            }

            return false;
        }

        public virtual double GetHoursSprouting()
        {
            float hours = nextSproutDaysRnd.nextFloat(1, Api.World.Rand) * Api.World.Calendar.HoursPerDay / (Blockentity as BEBerryPlant).growthRateMul;

            return (Blockentity as BEBerryPlant).growByMonth ? hours / 9 * Api.World.Calendar.DaysPerMonth : hours;
        }

        public override void UpdateHoursLeft(float intervalHours, ref float intervalDays, ref double daysToCheck, ref EnumHandling handling)
        {
            base.UpdateHoursLeft(intervalHours, ref intervalDays, ref daysToCheck, ref handling);

            sproutingHoursLeft -= intervalHours;
        }

        public override bool? StopGrowth(float intervalHours, ref EnumHandling handling)
        {
            sproutingHoursLeft += intervalHours;

            return base.StopGrowth(intervalHours, ref handling);
        }

        public override bool? ResetGrowth(ref EnumHandling handling)
        {
            sproutingHoursLeft = GetHoursSprouting();

            return base.ResetGrowth(ref handling);
        }

        public override bool? RevertGrowth(ref EnumHandling handling)
        {
            sproutingHoursLeft = GetHoursSprouting();

            return base.ResetGrowth(ref handling);
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            sproutingHoursLeft = tree.GetDecimal("rootSuckerSproutingHoursLeft");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetDouble("rootSuckerSproutingHoursLeft", sproutingHoursLeft);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (Api is ICoreClientAPI capi)
            {

            }
        }
    }
}
