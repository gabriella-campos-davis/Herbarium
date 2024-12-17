using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace herbarium
{
    public class BEGroundBerryPlant : BEBerryPlant, IAnimalFoodSource
    {
        public string[] creatureDietFoodTags;
        public BEGroundBerryPlant() : base()
        {

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api is ICoreServerAPI)
            {
                creatureDietFoodTags = Block.Attributes["foodTags"].AsArray<string>();

                api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this);
            }
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

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            GetMainInfo(forPlayer, sb);

            GetExtraInfo(forPlayer, sb);
        }

        public virtual void GetMainInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if ((!simplifiedTooltips && (temperatureState == EnumHBBTemp.Acceptable || IsRipe())) || (!IsRipe() && simplifiedTooltips))
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