using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace herbarium
{
    public class BESeedling : BlockEntity
    {
        double totalHoursTillGrowth;
        long growListenerId;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api is ICoreServerAPI)
            {
                growListenerId = RegisterGameTickListener(CheckGrow, 2000);
            }
        }

        NatFloat nextStageDaysRnd
        {
            get
            {
                return Block?.Attributes?["matureDays"].AsObject<NatFloat>() ?? NatFloat.create(EnumDistribution.UNIFORM, 7f, 2f);
            }
        }

        float GrowthRateMod => Api.World.Config.GetString("saplingGrowthRate").ToFloat(1);

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            totalHoursTillGrowth = Api.World.Calendar.TotalHours + nextStageDaysRnd.nextFloat(1, Api.World.Rand) * 24 * GrowthRateMod;
        }


        private void CheckGrow(float dt)
        {
            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);

            if (conds?.Temperature < 0) totalHoursTillGrowth = Api.World.Calendar.TotalHours + nextStageDaysRnd.nextFloat(1, Api.World.Rand) * Api.World.Calendar.HoursPerDay * GrowthRateMod;

            Block berryBlock = Api.World.GetBlock(AssetLocation.Create(Block.Attributes?["plantCode"].ToString()));
            if (conds?.Temperature >= 5 && Api.World.Calendar.TotalHours > totalHoursTillGrowth && berryBlock != null) Api.World.BlockAccessor.SetBlock(berryBlock.BlockId, Pos);
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("totalHoursTillGrowth", totalHoursTillGrowth);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            totalHoursTillGrowth = tree.GetDouble("totalHoursTillGrowth", 0);
        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            double hoursleft = totalHoursTillGrowth - Api.World.Calendar.TotalHours;
            double daysleft = hoursleft / Api.World.Calendar.HoursPerDay;

            if (daysleft <= 1)
            {
                dsc.AppendLine(Lang.Get("Will grow in less than a day"));
            }
            else
            {
                dsc.AppendLine(Lang.Get("Will grow in about {0} days", (int)daysleft));
            }
        }
    }
}
