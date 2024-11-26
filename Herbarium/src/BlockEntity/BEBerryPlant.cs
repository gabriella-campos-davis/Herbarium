using herbarium.config;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
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

    public class BEBerryPlant : BlockEntity, IAnimalFoodSource
    {
        protected double lastCheckAtTotalDays = 0;
        protected double transitionHoursLeft = -1;

        protected RoomRegistry roomreg;
        public int roomness;
        public string[] creatureDietFoodTags;

        protected float resetBelowTemp = 0;
        protected float resetAboveTemp = 0;
        protected float stopBelowTemp = 0;
        protected float stopAboveTemp = 0;
        protected float revertBelowTemp = 0;
        protected float revertAboveTemp = 0;
        EnumHBBTemp temperatureState = EnumHBBTemp.Acceptable;

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
                creatureDietFoodTags = Block.Attributes["foodTags"].AsArray<string>();

                if (transitionHoursLeft <= 0)
                {
                    transitionHoursLeft = GetHoursForNextStage();
                    lastCheckAtTotalDays = api.World.Calendar.TotalDays;
                }

                if (api.World.Config.GetBool("processCrops", true))
                {
                    RegisterGameTickListener(CheckGrow, 8000);
                }

                api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this);
                roomreg = api.ModLoader.GetModSystem<RoomRegistry>();

                ClimateCondition conds = null;
                float baseTemperature = 0;

                temperatureState = CheckTemperature(TemperatureAtDate(Api.World.Calendar.TotalDays, ref conds, ref baseTemperature));
            }
        }

        protected NatFloat nextStageDaysRnd
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

                if (temperatureState != EnumHBBTemp.Acceptable) StopGrowth(intervalHours);
                if (temperatureState == EnumHBBTemp.ResetBelow || temperatureState == EnumHBBTemp.ResetAbove) ResetGrowth();
                if (temperatureState == EnumHBBTemp.RevertBelow || temperatureState == EnumHBBTemp.RevertAbove) RevertGrowth();

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

        public override void OnExchanged(Block block)
        {
            base.OnExchanged(block);
            transitionHoursLeft = GetHoursForNextStage();
            if (Api?.Side == EnumAppSide.Server) UpdateTransitionsFromBlock();
        }

        public override void CreateBehaviors(Block block, IWorldAccessor worldForResolve)
        {
            base.CreateBehaviors(block, worldForResolve);
            if (worldForResolve.Side == EnumAppSide.Server) UpdateTransitionsFromBlock();
        }

        protected virtual void UpdateTransitionsFromBlock()
        {
            // In case we have a Block which is not a BerryBush block (why does this happen?)
            if (Block?.Attributes == null)
            {
                resetBelowTemp = stopBelowTemp = revertBelowTemp = -999;
                resetAboveTemp = stopAboveTemp = revertAboveTemp = 999;
                return;
            }
            // These Attributes lookups are costly because Newtonsoft JSON lib ~~sucks~~ uses a weird approximation to a Dictionary in JToken.TryGetValue() but it can ignore case
            resetBelowTemp = Block.Attributes["resetBelowTemperature"].AsFloat(-999);
            resetAboveTemp = Block.Attributes["resetAboveTemperature"].AsFloat(999);
            stopBelowTemp = Block.Attributes["stopBelowTemperature"].AsFloat(-999);
            stopAboveTemp = Block.Attributes["stopAboveTemperature"].AsFloat(999);
            revertBelowTemp = Block.Attributes["revertBlockBelowTemperature"].AsFloat(-999);
            revertAboveTemp = Block.Attributes["revertBlockAboveTemperature"].AsFloat(999);
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

        public virtual void StopGrowth(float intervalHours)
        {
            if (!IsRipe()) transitionHoursLeft += intervalHours;
        }

        public virtual void ResetGrowth()
        {
            if (!IsRipe()) transitionHoursLeft = GetHoursForNextStage();
        }

        public virtual void RevertGrowth()
        {
            if (Block.Variant["state"] != "empty")
            {
                Block nextBlock = Api.World.GetBlock(Block.CodeWithVariant("state", "empty"));
                Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
            }
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

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            GetMainInfo(forPlayer, sb);

            GetExtraInfo(forPlayer, sb);
        }

        public virtual void GetMainInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if ((!simplifiedTooltips && temperatureState == EnumHBBTemp.Acceptable) || (!IsRipe() && simplifiedTooltips))
            {
                double daysleft = transitionHoursLeft / Api.World.Calendar.HoursPerDay;

                string code = IsEmpty() ? "flowering" : IsFlowering() ? "ripen" : "shed";

                if (daysleft < 1)
                {
                    sb.AppendLine(Lang.Get("berrybush-" + code + "-1day"));
                }
                else
                {
                    sb.AppendLine(Lang.Get("berrybush-" + code + "-xdays", (int)daysleft));
                }
            }
        }

        public virtual void GetExtraInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (!simplifiedTooltips)
            {
                if (temperatureState < EnumHBBTemp.Acceptable)
                {
                    sb.AppendLine(Lang.Get("berrybush-too-cold"));
                }

                if (temperatureState > EnumHBBTemp.Acceptable)
                {
                    sb.AppendLine(Lang.Get("berrybush-too-hot"));
                }
            }

            if (roomness > 0)
            {
                sb.AppendLine(Lang.Get("greenhousetempbonus"));
            }
        }



        #region IAnimalFoodSource impl
        public bool IsSuitableFor(Entity entity, CreatureDiet diet)
        {
            if (diet == null) return false;
            if (!IsRipe()) return false;
            return diet.Matches(EnumFoodCategory.NoNutrition, this.creatureDietFoodTags);
        }

        public float ConsumeOnePortion(Entity entity)
        {
            AssetLocation loc = Block.CodeWithVariant("state", "empty");
            if (!loc.Valid)
            {
                Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                return 0f;
            }

            Block nextBlock = Api.World.GetBlock(loc);
            if (nextBlock?.Code == null) return 0f;

            var bbh = Block.GetBehavior<BlockBehaviorHarvestable>();
            if (bbh?.harvestedStack != null)
            {
                ItemStack dropStack = bbh.harvestedStack.GetNextItemStack();
                Api.World.PlaySoundAt(bbh.harvestingSound, Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
                Api.World.SpawnItemEntity(dropStack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            var bbhm = Block.GetCollectibleBehavior<BlockBehaviorHarvestMultiple>(true);
            if (bbhm?.harvestedStacks != null)
            {
                for (int i = 0; i < bbhm.harvestedStacks.Length; i++)
                {
                    ItemStack dropStack = bbhm.harvestedStacks[i].GetNextItemStack();
                    Api.World.SpawnItemEntity(dropStack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                Api.World.PlaySoundAt(bbhm.harvestingSound, Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
            }


            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
            MarkDirty(true);

            return 0.1f;
        }

        public Vec3d Position => Pos.ToVec3d().Add(0.5, 0.5, 0.5);
        public string Type => "food";
        #endregion


        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            if (Api.Side == EnumAppSide.Server)
            {
                Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this);
            }
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            if (Api?.Side == EnumAppSide.Server)
            {
                Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this);
            }
        }
    }
}
