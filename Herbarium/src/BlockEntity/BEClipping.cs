using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace herbarium
{
    public class BEClipping : BEBerryPlant
    {
        double totalHoursTillGrowth = -1;

        public override void Initialize(ICoreAPI api)
        {
            if (api is ICoreServerAPI)
            {
                if (totalHoursTillGrowth != -1)
                {
                    transitionHoursLeft = totalHoursTillGrowth - api.World.Calendar.TotalDays;
                    lastCheckAtTotalDays = api.World.Calendar.TotalDays;
                }

                if (Block?.Attributes != null)
                {
                    resetBelowTemp = Block.Attributes["resetBelowTemp"].AsFloat(0);
                    resetAboveTemp = Block.Attributes["resetAboveTemp"].AsFloat(999);
                    stopBelowTemp = Block.Attributes["stopBelowTemp"].AsFloat(5);
                    stopAboveTemp = Block.Attributes["stopAboveTemp"].AsFloat(999);
                    revertBelowTemp = Block.Attributes["dieBelowTemp"].AsFloat(-2);
                    revertAboveTemp = Block.Attributes["dieAboveTemp"].AsFloat(999);
                }
            }

            base.Initialize(api);
        }

        protected override NatFloat nextStageDaysRnd
        {
            get
            {
                return Block?.Attributes?["matureDays"].AsObject<NatFloat>() ?? NatFloat.create(EnumDistribution.UNIFORM, 7f, 2f);
            }
        }

        protected override float IntervalHours(double daysToCheck)
        {
            return 2f;
        }

        protected override bool DoGrow()
        {
            Block belowBlock = Api.World.BlockAccessor.GetBlock(Pos.DownCopy());

            if (((belowBlock.Attributes?["isLarge"].AsBool() ?? false) || (belowBlock.Attributes?["isHuge"].AsBool() ?? false)) &&
                (!Api.World.BlockAccessor.GetBlock(Pos.DownCopy(2)).Attributes?["isBottomBlock"].AsBool() ?? false) &&
                (!Api.World.BlockAccessor.GetBlock(Pos.DownCopy(3)).Attributes?["isBottomBlock"].AsBool() ?? false) &&
                (Block.Attributes?["isGrowth"].AsBool() ?? false))
            {
                Block newBottomBlock = Api.World.GetBlock(AssetLocation.Create(belowBlock.Attributes?["bottomBlock"].ToString()));

                if (newBottomBlock is null) return true;

                Api.World.BlockAccessor.ExchangeBlock(newBottomBlock.BlockId, Pos.DownCopy());
            }
            string blockCode = Block.Attributes?["bushCode"].ToString();
            if (blockCode == null) blockCode = Block.Attributes?["plantCode"].ToString();
            Block newBushBlock = Api.World.GetBlock(AssetLocation.Create(blockCode));

            if (Block is BlockVineClipping)
            {
                BlockFacing facing = BlockFacing.FromCode(Block.Code.EndVariant());

                BlockPos attachingBlockPos = Pos.AddCopy(facing);
                Block attachingBlock = Api.World.BlockAccessor.GetBlock(attachingBlockPos);

                if (!attachingBlock.CanAttachBlockAt(Api.World.BlockAccessor, newBushBlock, attachingBlockPos, facing.Opposite, null))
                {
                    newBushBlock = Api.World.BlockAccessor.GetBlock(newBushBlock.CodeWithVariant("side", "down"));
                }
            }

            if (newBushBlock is null) return true;

            Api.World.BlockAccessor.SetBlock(newBushBlock.BlockId, Pos);

            (Api.World.BlockAccessor.GetBlockEntity(Pos) as BEBerryPlant)?.OnGrowth(lastCheckAtTotalDays);

            return false;
        }

        public override bool RevertGrowth()
        {
            string bushType = Block.Variant["type"].ToString();

            if (bushType != null)
            {
                Block deadClippingBlock = Api.World.GetBlock(Block.CodeWithVariant("state", "dead"));
                if (deadClippingBlock is null) return false;

                Api.World.BlockAccessor.SetBlock(deadClippingBlock.BlockId, Pos);
            }

            return true;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            totalHoursTillGrowth = tree.GetDouble("totalHoursTillGrowth", -1);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            if ((!simplifiedTooltips && temperatureState == EnumHBBTemp.Acceptable) || simplifiedTooltips)
            {
                double daysleft = transitionHoursLeft / Api.World.Calendar.HoursPerDay;

                if (daysleft <= 1)
                {
                    dsc.AppendLine(Lang.Get("Will grow in less than a day"));
                }
                else
                {
                    dsc.AppendLine(Lang.Get("Will grow in about {0} days", (int)daysleft));
                }
            }

            if (!simplifiedTooltips)
            {
                if (temperatureState < EnumHBBTemp.Acceptable)
                {
                    dsc.AppendLine(Lang.Get("clipping-too-cold"));
                }

                if (temperatureState > EnumHBBTemp.Acceptable)
                {
                    dsc.AppendLine(Lang.Get("clipping-too-hot"));
                }
            }

            if (roomness > 0)
            {
                dsc.AppendLine(Lang.Get("greenhousetempbonus"));
            }
        }
    }
}
