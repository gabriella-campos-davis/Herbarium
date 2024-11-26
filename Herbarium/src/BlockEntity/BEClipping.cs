using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace herbarium
{
    public class BEClipping : BlockEntity
    {
        double totalHoursTillGrowth;
        long growListenerId;
        public float dieBelowTemp;

        
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            dieBelowTemp = Block.Attributes?["dieBelowTemp"].AsInt(-2) ?? -2;

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

        public override void OnBlockPlaced(ItemStack byItemStack)
        {
            totalHoursTillGrowth = GetHoursForNextStage();
        }

        private void CheckGrow(float dt)
        {
            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);

            if (conds?.Temperature < dieBelowTemp) DoDie();
            if (conds?.Temperature < 0) totalHoursTillGrowth = Api.World.Calendar.TotalHours + nextStageDaysRnd.nextFloat(1, Api.World.Rand) * Api.World.Calendar.HoursPerDay * GrowthRateMod;

            if (conds?.Temperature >= 5 && Api.World.Calendar.TotalHours > totalHoursTillGrowth) DoGrow();
        }

        private void DoGrow()
        {
            if ((Api.World.BlockAccessor.GetBlock(Pos.DownCopy()).Attributes?["isLarge"].AsBool() ?? false) &&
                 Api.World.BlockAccessor.GetBlock(Pos.DownCopy(2)).BlockMaterial is not EnumBlockMaterial.Plant &&
                 (Block.Attributes?["isGrowth"].AsBool() ?? false))
            {
                Block newBottomBlock = Api.World.GetBlock(AssetLocation.Create(Api.World.BlockAccessor.GetBlock(Pos.DownCopy()).Attributes?["bottomBlock"].ToString()));

                if (newBottomBlock is null) return;

                Api.World.BlockAccessor.SetBlock(newBottomBlock.BlockId, Pos.DownCopy());
            }
            Block newBushBlock = Api.World.GetBlock(AssetLocation.Create(Block.Attributes?["bushCode"].ToString()));

            if (newBushBlock is null) return;

            Api.World.BlockAccessor.SetBlock(newBushBlock.BlockId, Pos);
        }

        private void DoDie()
        {
            string bushType = Block.Variant["type"].ToString();

            if (bushType != null)
            {
                Block deadClippingBlock = Api.World.GetBlock(Block.CodeWithVariant("state", "dead"));
                if (deadClippingBlock is null) return;

                Api.World.BlockAccessor.SetBlock(deadClippingBlock.BlockId, Pos);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("totalHoursTillGrowth", totalHoursTillGrowth);
            tree.SetFloat("dieBelowTemp", dieBelowTemp);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            totalHoursTillGrowth = tree.GetDouble("totalHoursTillGrowth", 0);
            dieBelowTemp = tree.GetFloat("dieBelowTemp", -2);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            if(Block.Variant["state"].ToString() == "dead")
            {
                string type = Block.Variant["type"].ToString();
                dsc.AppendLine(Lang.Get("Dead {0} clipping", type));
            }
            else 
            {
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
}
