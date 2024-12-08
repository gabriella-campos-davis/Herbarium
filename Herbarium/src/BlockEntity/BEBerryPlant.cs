using herbarium.config;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace herbarium
{
    public enum EnumHBBTemp
    {
        RevertBelow = -3,
        ResetBelow,
        StopBelow,
        Acceptable,
        StopAbove,
        ResetAbove,
        RevertAbove
    }

    public class BEBerryPlant : BlockEntity
    {
        protected double lastCheckAtTotalDays = 0;
        protected double transitionHoursLeft = -1;

        protected RoomRegistry roomreg;
        public int roomness;

        protected float resetBelowTemp = 0;
        protected float resetAboveTemp = 0;
        protected float stopBelowTemp = 0;
        protected float stopAboveTemp = 0;
        protected float revertBelowTemp = 0;
        protected float revertAboveTemp = 0;
        protected EnumHBBTemp temperatureState = EnumHBBTemp.Acceptable;

        public EnumHBBTemp TemperatureState { get { return temperatureState; } }

        protected float growthRateMul = HerbariumConfig.Current.berryGrowthRateMul.Value;
        protected bool growByMonth = HerbariumConfig.Current.berriesGrowByMonth.Value;
        protected bool simplifiedTooltips = HerbariumConfig.Current.simplifiedBerryTooltips.Value;

        public BEBerryPlant() : base()
        {

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            growthRateMul = api.World.Config.GetFloat("berryGrowthRateMul", growthRateMul);
            growByMonth = api.World.Config.GetBool("berriesGrowByMonth", growByMonth);
            simplifiedTooltips = api.World.Config.GetBool("simplifiedBerryTooltips", simplifiedTooltips);

            if (api is ICoreServerAPI)
            {
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

                ClimateCondition conds = null;
                float baseTemperature = 0;

                temperatureState = CheckTemperature(TemperatureAtDate(Api.World.Calendar.TotalDays, ref conds, ref baseTemperature));
            }
        }

        protected virtual NatFloat nextStageDaysRnd
        {
            get
            {
                NatFloat days = NatFloat.create(EnumDistribution.UNIFORM, 8.8f, 0.8f);

                if (IsRipe()) days = Block?.Attributes?["sheddingDays"].AsObject<NatFloat>() ?? NatFloat.create(EnumDistribution.UNIFORM, 35.2f, 3.2f);
                else if (IsFlowering()) days = Block?.Attributes?["ripeningDays"].AsObject<NatFloat>() ?? days;
                else days = Block?.Attributes?["floweringDays"].AsObject<NatFloat>() ?? days;

                return days;
            }
        }

        protected virtual float IntervalHours(double daysToCheck)
        {
            return  Math.Clamp((float)(daysToCheck / Api.World.Calendar.DaysPerMonth) * 2f, 2f, Api.World.Calendar.DaysPerMonth * Api.World.Calendar.HoursPerDay);
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

            if (Block.EntityClass == null)
            {
                Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                return;
            }

            // In case this block was imported from another older world. In that case lastCheckAtTotalDays would be a future date.
            lastCheckAtTotalDays = Math.Min(lastCheckAtTotalDays, Api.World.Calendar.TotalDays);

            double daysToCheck = Api.World.Calendar.TotalDays - lastCheckAtTotalDays;

            float intervalDays = IntervalHours(daysToCheck) / Api.World.Calendar.HoursPerDay;
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
                float intervalHours = IntervalHours(daysToCheck);
                UpdateHoursLeft(intervalHours, ref intervalDays, ref daysToCheck);

                temperatureState = CheckTemperature(TemperatureAtDate(lastCheckAtTotalDays, ref conds, ref baseTemperature));

                if (temperatureState != EnumHBBTemp.Acceptable) if (StopGrowth(intervalHours)) return;
                if (temperatureState == EnumHBBTemp.ResetBelow || temperatureState == EnumHBBTemp.ResetAbove) if (ResetGrowth()) return;
                if (temperatureState == EnumHBBTemp.RevertBelow || temperatureState == EnumHBBTemp.RevertAbove) if (RevertGrowth()) return;

                if (CheckGrowExtra()) continue;

                if (transitionHoursLeft <= 0)
                {
                    if (!DoGrow()) return;
                }
            }

            MarkDirty(false);
        }

        protected virtual bool CheckGrowExtra()
        {
            return false;
        }

        public virtual double GetHoursForNextStage()
        {
            float hours = nextStageDaysRnd.nextFloat(1, Api.World.Rand) * Api.World.Calendar.HoursPerDay / growthRateMul;
            if (IsRipe()) hours = nextStageDaysRnd.nextFloat(1, Api.World.Rand) * Api.World.Calendar.HoursPerDay;

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
            if (temp < revertBelowTemp) return EnumHBBTemp.RevertBelow;
            if (temp < resetBelowTemp) return EnumHBBTemp.ResetBelow;
            if (temp < stopBelowTemp) return EnumHBBTemp.StopBelow;
            if (temp > stopAboveTemp) return EnumHBBTemp.StopAbove;
            if (temp > resetAboveTemp) return EnumHBBTemp.ResetAbove;
            if (temp > revertAboveTemp) return EnumHBBTemp.RevertAbove;
            return EnumHBBTemp.Acceptable;
        }

        public virtual void UpdateHoursLeft(float intervalHours, ref float intervalDays, ref double daysToCheck)
        {
            intervalDays = intervalHours / Api.World.Calendar.HoursPerDay;
            daysToCheck -= intervalDays;
            lastCheckAtTotalDays += intervalDays;
            transitionHoursLeft -= intervalHours;
        }

        public virtual bool StopGrowth(float intervalHours)
        {
            if (!IsRipe()) transitionHoursLeft += intervalHours;

            return false;
        }

        public virtual bool ResetGrowth()
        {
            if (!IsRipe()) transitionHoursLeft = GetHoursForNextStage();

            return false;
        }

        public virtual bool RevertGrowth()
        {
            if (Block.Variant["state"] != "empty")
            {
                Block nextBlock = Api.World.GetBlock(Block.CodeWithVariant("state", "empty"));
                Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
            }

            return false;
        }

        public virtual bool IsEmpty()
        {
            return Block.Variant["state"] == "empty";
        }

        public virtual bool IsFlowering()
        {
            return Block.Variant["state"] == "flowering";
        }

        public virtual bool IsRipe()
        {
            return Block.Variant["state"] == "ripe";
        }

        protected virtual bool DoGrow()
        {
            string nextCodePart = IsEmpty() ? "flowering" : (IsFlowering() ? "ripe" : "empty");


            AssetLocation loc = Block.CodeWithVariant("state", nextCodePart);
            if (!loc.Valid)
            {
                Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                return false;
            }

            Block nextBlock = Api.World.GetBlock(loc);
            if (nextBlock?.Code == null) return false;

            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);

            MarkDirty(true);
            return true;
        }




        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

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
    }
}
