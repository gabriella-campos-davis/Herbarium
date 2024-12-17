using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace herbarium.config
{
    class HerbariumConfig 
    {
        public bool? plantsCanDamage = true;
        public bool? plantsCanPoison = true;
        public float? plantsDamage = 0.5f;
        public float? plantsDamageTick = 0.7f;
        public string[] plantsWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};

        public bool? poulticeHealOverTime = true;

        public bool? berryBushCanDamage = true;
        public float? berryBushDamage = 0.5f;
        public float? berryBushDamageTick = 0.7f;
        public string[] berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
        public float? berryGrowthRateMul = 1f;
        public bool? berriesGrowByMonth = false;
        public bool? useKnifeForClipping = true;
        public bool? useShearsForClipping = true;
        public bool? simplifiedBerryTooltips = false;

        public HerbariumConfig()
        {}

        public static HerbariumConfig Current { get; set; }

        public static HerbariumConfig GetDefault()
        {
            HerbariumConfig defaultConfig = new();

            defaultConfig.plantsCanDamage = true;
            defaultConfig.plantsCanPoison = true;
            defaultConfig.plantsDamage = 0.5f;
            defaultConfig.plantsDamageTick = 0.7f;
            defaultConfig.plantsWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};

            defaultConfig.poulticeHealOverTime = true;

            defaultConfig.berryBushCanDamage = true;
            defaultConfig.berryBushDamage = 0.5f;
            defaultConfig.berryBushDamageTick = 0.7f;
            defaultConfig.berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
            defaultConfig.berryGrowthRateMul = 1f;
            defaultConfig.berriesGrowByMonth = false;
            defaultConfig.useKnifeForClipping = true;
            defaultConfig.useShearsForClipping = true;
            defaultConfig.simplifiedBerryTooltips = false;

            return defaultConfig;
        }

        internal static void createConfig(ICoreAPI api)
        {
            try
            {
                var Config = api.LoadModConfig<HerbariumConfig>("herbariumconfig.json");
                if (Config != null)
                {
                    api.Logger.Notification(Lang.Get("modconfigload"));
                    Current = Config;
                }
                else
                {
                    api.Logger.Notification(Lang.Get("nomodconfig"));
                    Current = GetDefault();
                }
            }
            catch
            {
                Current = GetDefault();
                api.Logger.Error(Lang.Get("defaultloaded"));
            }
            finally
            {
                if (Current.plantsCanDamage == null) Current.plantsCanDamage = GetDefault().plantsCanDamage;

                if (Current.plantsCanPoison == null) Current.plantsCanPoison = GetDefault().plantsCanPoison;

                if (Current.plantsDamage == null) Current.plantsDamage = GetDefault().plantsDamage;

                if (Current.plantsDamageTick == null) Current.plantsDamageTick = GetDefault().plantsDamageTick;

                if (Current.plantsWillDamage == null) Current.plantsWillDamage = GetDefault().plantsWillDamage;

                if (Current.poulticeHealOverTime == null) Current.poulticeHealOverTime = GetDefault().poulticeHealOverTime;

                if (Current.berryBushCanDamage == null) Current.berryBushCanDamage = GetDefault().berryBushCanDamage;

                if (Current.berryBushDamage == null) Current.berryBushDamage = GetDefault().berryBushDamage;

                if (Current.berryBushDamageTick == null) Current.berryBushDamageTick = GetDefault().berryBushDamageTick;

                if (Current.berryBushWillDamage == null) Current.berryBushWillDamage = GetDefault().berryBushWillDamage;

                if (Current.berryGrowthRateMul == null) Current.berryGrowthRateMul = GetDefault().berryGrowthRateMul;

                if (Current.berriesGrowByMonth == null) Current.berriesGrowByMonth = GetDefault().berriesGrowByMonth;

                if (Current.useKnifeForClipping == null) Current.useKnifeForClipping = GetDefault().useKnifeForClipping;

                if (Current.useShearsForClipping == null) Current.useShearsForClipping = GetDefault().useShearsForClipping;

                if (Current.simplifiedBerryTooltips == null) Current.simplifiedBerryTooltips = GetDefault().simplifiedBerryTooltips;

                api.StoreModConfig(Current, "herbariumconfig.json");
            }
        }
    }
}