using herbarium.config;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BEClipping : BlockEntity
    {
        protected float intervalHours = 2f;
        protected double lastCheckAtTotalDays = 0;
        protected double transitionHoursLeft = -1;
        double totalHoursTillGrowth = -1;

        protected RoomRegistry roomreg;
        public int roomness;

        protected float resetBelowTemp = 0;
        protected float resetAboveTemp = 999;
        protected float stopBelowTemp = 5;
        protected float stopAboveTemp = 999;
        protected float dieBelowTemp = -2;
        protected float dieAboveTemp = 999;
        EnumHBBTemp temperatureState = EnumHBBTemp.Acceptable;

        public EnumHBBTemp TemperatureState { get { return temperatureState; } }

        protected float growthRateMul = HerbariumConfig.Current.berryGrowthRateMul.Value;
        protected bool growByMonth = HerbariumConfig.Current.berriesGrowByMonth.Value;
        protected bool simplifiedTooltips = HerbariumConfig.Current.simplifiedBerryTooltips.Value;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            growthRateMul = api.World.Config.GetFloat("berryGrowthRateMul", growthRateMul);
            growByMonth = api.World.Config.GetBool("berriesGrowByMonth", growByMonth);
            simplifiedTooltips = api.World.Config.GetBool("simplifiedBerryTooltips", simplifiedTooltips);

            if (api is ICoreServerAPI)
            {
                if (totalHoursTillGrowth != -1)
                {
                    transitionHoursLeft = totalHoursTillGrowth - api.World.Calendar.TotalDays;
                    lastCheckAtTotalDays = api.World.Calendar.TotalDays;
                }

                if (transitionHoursLeft <= 0)
                {
                    transitionHoursLeft = GetHoursForNextStage();
                    lastCheckAtTotalDays = api.World.Calendar.TotalDays;
                }

                if (api.World.Config.GetBool("processCrops", true))
                {
                    RegisterGameTickListener(CheckGrow, 8000);
                }

                roomreg = api.ModLoader.GetModSystem<RoomRegistry>();

                if (Block?.Attributes != null)
                {
                    resetBelowTemp = Block.Attributes["resetBelowTemp"].AsFloat(0);
                    resetAboveTemp = Block.Attributes["resetAboveTemp"].AsFloat(999);
                    stopBelowTemp = Block.Attributes["stopBelowTemp"].AsFloat(5);
                    stopAboveTemp = Block.Attributes["stopAboveTemp"].AsFloat(999);
                    dieBelowTemp = Block.Attributes["dieBelowTemp"].AsFloat(-2);
                    dieAboveTemp = Block.Attributes["dieAboveTemp"].AsFloat(999);
                }

                ClimateCondition conds = null;
                float baseTemperature = 0;

                temperatureState = CheckTemperature(TemperatureAtDate(Api.World.Calendar.TotalDays, ref conds, ref baseTemperature));
            }
        }

        NatFloat nextMatureDaysRnd
        {
            get
            {
                return Block?.Attributes?["matureDays"].AsObject<NatFloat>() ?? NatFloat.create(EnumDistribution.UNIFORM, 7f, 2f);
            }
        }

        public virtual void OnGrowth(double lastCheck)
        {
            lastCheckAtTotalDays = lastCheck;
            transitionHoursLeft = GetHoursForNextStage();
            CheckGrow(0);
        }

        protected virtual void CheckGrow(float dt)
        {
            if (!(Api as ICoreServerAPI).World.IsFullyLoadedChunk(Pos)) return;

            if (Block.Attributes == null) return;

            // In case this block was imported from another older world. In that case lastCheckAtTotalDays would be a future date.
            lastCheckAtTotalDays = Math.Min(lastCheckAtTotalDays, Api.World.Calendar.TotalDays);

            double daysToCheck = Api.World.Calendar.TotalDays - lastCheckAtTotalDays;

            float intervalDays = intervalHours / Api.World.Calendar.HoursPerDay;
            if (daysToCheck <= intervalDays) return;

            if (Api.World.BlockAccessor.GetRainMapHeightAt(Pos) > Pos.Y) // Fast pre-check
            {
                Room room = roomreg?.GetRoomForPosition(Pos);
                roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;
            }
            else
            {
                roomness = 0;
            }

            ClimateCondition conds = null;
            float baseTemperature = 0;
            while (daysToCheck > intervalDays)
            {
                intervalDays = intervalHours / Api.World.Calendar.HoursPerDay;
                daysToCheck -= intervalDays;
                lastCheckAtTotalDays += intervalDays;
                transitionHoursLeft -= intervalHours;

                temperatureState = CheckTemperature(TemperatureAtDate(lastCheckAtTotalDays, ref conds, ref baseTemperature));

                if (temperatureState != EnumHBBTemp.Acceptable) transitionHoursLeft += intervalHours;
                if (temperatureState == EnumHBBTemp.ResetBelow || temperatureState == EnumHBBTemp.ResetAbove) transitionHoursLeft = GetHoursForNextStage();
                if (temperatureState == EnumHBBTemp.RevertBelow || temperatureState == EnumHBBTemp.RevertAbove)
                {
                    DoDie();
                    return;
                }

                if (transitionHoursLeft <= 0)
                {
                    DoGrow();
                    return;
                }
            }

            MarkDirty(false);
        }

        public virtual double GetHoursForNextStage()
        {
            float hours = nextMatureDaysRnd.nextFloat(1, Api.World.Rand) * Api.World.Calendar.HoursPerDay / growthRateMul;

            return growByMonth ? hours / 9 * Api.World.Calendar.DaysPerMonth : hours;
        }

        public virtual float TemperatureAtDate(double date, ref ClimateCondition conds, ref float baseTemperature)
        {
            if (conds == null)
            {
                conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, date);
                if (conds == null) return 0;
                baseTemperature = conds.WorldGenTemperature;
            }
            else
            {
                conds.Temperature = baseTemperature;  // Keep resetting the field we are interested in, because it can be modified by the OnGetClimate event
                Api.World.BlockAccessor.GetClimateAt(Pos, conds, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, date);
            }

            float temperature = conds.Temperature;

            if (roomness > 0)
            {
                temperature += 5;
            }

            return temperature;
        }

        public virtual EnumHBBTemp CheckTemperature(float temp)
        {
            if (temp < dieBelowTemp) return EnumHBBTemp.RevertBelow;
            if (temp < resetBelowTemp) return EnumHBBTemp.ResetBelow;
            if (temp < stopBelowTemp) return EnumHBBTemp.StopBelow;
            if (temp > stopAboveTemp) return EnumHBBTemp.StopAbove;
            if (temp > resetAboveTemp) return EnumHBBTemp.ResetAbove;
            if (temp > dieAboveTemp) return EnumHBBTemp.RevertAbove;
            return EnumHBBTemp.Acceptable;
        }

        private void DoGrow()
        {
            if ((Api.World.BlockAccessor.GetBlock(Pos.DownCopy()).Attributes?["isLarge"].AsBool() ?? false) &&
                (!Api.World.BlockAccessor.GetBlock(Pos.DownCopy(2)).Attributes?["isBottomBlock"].AsBool() ?? false) &&
                (Block.Attributes?["isGrowth"].AsBool() ?? false))
            {
                Block newBottomBlock = Api.World.GetBlock(AssetLocation.Create(Api.World.BlockAccessor.GetBlock(Pos.DownCopy()).Attributes?["bottomBlock"].ToString()));

                if (newBottomBlock is null) return;


                Api.World.BlockAccessor.ExchangeBlock(newBottomBlock.BlockId, Pos.DownCopy());
            }
            string blockCode = Block.Attributes?["bushCode"].ToString();
            if (blockCode == null) blockCode = Block.Attributes?["plantCode"].ToString();
            Block? newBushBlock = Api.World.GetBlock(AssetLocation.Create(blockCode));

            if (newBushBlock is null) return;

            Api.World.BlockAccessor.SetBlock(newBushBlock.BlockId, Pos);

            (Api.World.BlockAccessor.GetBlockEntity(Pos) as BEBerryPlant)?.OnGrowth(lastCheckAtTotalDays);
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

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            totalHoursTillGrowth = tree.GetDouble("totalHoursTillGrowth", -1);
            transitionHoursLeft = tree.GetDouble("transitionHoursLeft");
            lastCheckAtTotalDays = tree.GetDouble("lastCheckAtTotalDays");

            roomness = tree.GetInt("roomness");
            temperatureState = (EnumHBBTemp)tree.GetInt("temperatureState");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("transitionHoursLeft", transitionHoursLeft);
            tree.SetDouble("lastCheckAtTotalDays", lastCheckAtTotalDays);

            tree.SetInt("roomness", roomness);
            tree.SetInt("temperatureState", (int)temperatureState);
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
