using  System;
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
    }
}