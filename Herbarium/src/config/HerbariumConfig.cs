using  System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace herbarium.config
{
    class HerbariumConfig 
    {
        public Nullable<bool> plantsCanDamage = true;
        public Nullable<bool> plantsCanPoison = true;
        public string[] plantsWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
        public Nullable<bool> poulticeHealOverTime = true;
        public Nullable<bool> berryBushCanDamage = true;
        public Nullable<float> berryBushDamage = 0.5f;
        public Nullable<float> berryBushDamageTick = 0.7f;
        public string[] berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
        public Nullable<bool> useKnifeForClipping = true;
        public Nullable<bool> useShearsForClipping = true;

        public HerbariumConfig()
        {}

        public static HerbariumConfig Current { get; set; }

        public static HerbariumConfig GetDefault()
        {
            HerbariumConfig defaultConfig = new();

            defaultConfig.plantsCanDamage = true;
            defaultConfig.plantsCanPoison = true;
            defaultConfig.plantsWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};

            defaultConfig.poulticeHealOverTime = true;

            defaultConfig.berryBushCanDamage = true;
            defaultConfig.berryBushDamage = 0.5f;
            defaultConfig.berryBushDamageTick = 0.7f;
            defaultConfig.berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
            defaultConfig.useKnifeForClipping = true;
            defaultConfig.useShearsForClipping = true;
            

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
                    HerbariumConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification(Lang.Get("nomodconfig"));
                    HerbariumConfig.Current = HerbariumConfig.GetDefault();
                }
            }
            catch
            {
                HerbariumConfig.Current = HerbariumConfig.GetDefault();
                api.Logger.Error(Lang.Get("defaultloaded"));
            }
            finally
            {
                if (HerbariumConfig.Current.plantsCanDamage == null)
                    HerbariumConfig.Current.plantsCanDamage = HerbariumConfig.GetDefault().plantsCanDamage;

                if (HerbariumConfig.Current.plantsCanPoison == null)
                    HerbariumConfig.Current.plantsCanPoison = HerbariumConfig.GetDefault().plantsCanPoison;

                if (HerbariumConfig.Current.plantsWillDamage == null)
                    HerbariumConfig.Current.plantsWillDamage = HerbariumConfig.GetDefault().plantsWillDamage;

                if (HerbariumConfig.Current.poulticeHealOverTime == null)
                    HerbariumConfig.Current.poulticeHealOverTime = HerbariumConfig.GetDefault().poulticeHealOverTime;

                if (HerbariumConfig.Current.berryBushCanDamage == null)
                    HerbariumConfig.Current.berryBushCanDamage = HerbariumConfig.GetDefault().berryBushCanDamage;

                if (HerbariumConfig.Current.berryBushDamage == null)
                    HerbariumConfig.Current.berryBushDamage = HerbariumConfig.GetDefault().berryBushDamage;

                if (HerbariumConfig.Current.berryBushDamageTick == null)
                    HerbariumConfig.Current.berryBushDamageTick = HerbariumConfig.GetDefault().berryBushDamageTick;

                if (HerbariumConfig.Current.berryBushWillDamage == null)
                    HerbariumConfig.Current.berryBushWillDamage = HerbariumConfig.GetDefault().berryBushWillDamage;

                if (HerbariumConfig.Current.useKnifeForClipping == null)
                    HerbariumConfig.Current.useKnifeForClipping = HerbariumConfig.GetDefault().useKnifeForClipping;

                if (HerbariumConfig.Current.useShearsForClipping == null)
                    HerbariumConfig.Current.useShearsForClipping = HerbariumConfig.GetDefault().useShearsForClipping;

                api.StoreModConfig(HerbariumConfig.Current, "herbariumconfig.json");
            }
        }
    }
}