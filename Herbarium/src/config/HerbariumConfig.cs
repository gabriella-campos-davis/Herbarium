namespace herbarium.config
{
    class HerbariumConfig 
    {
        public bool plantsCanDamage = true;
        public bool plantsCanPoison = true;
        public string[] plantsWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};

        public bool poulticeHealOverTime = true;

        public bool berryBushCanDamage = true;
        public float berryBushDamage = 0.5f;
        public float berryBushDamageTick = 0.7f;
        public string[] berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
        public bool useKnifeForClipping = true;
        public bool useShearsForClipping = true;


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