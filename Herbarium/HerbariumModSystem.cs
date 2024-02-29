using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using herbarium.config;
using BuffStuff;


[assembly: ModInfo( "Herbarium Plant Library",
	Description = "Adds implements various useful classes related to plants for other mods to use",
	Website     = "",
	Authors     = new []{ "gabb", "CATASTEROID" } )]

namespace herbarium
{
    public class Herbarium : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }
       public override void AssetsLoaded(ICoreAPI api)
		{
			base.AssetsLoaded(api);
			api.RegisterBlockClass("BlockCoconutTree", typeof(BlockCoconutTree));
		}
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            //Api = api;
            api.RegisterBlockClass("HerbariumBerryBush", typeof(HerbariumBerryBush));
            api.RegisterBlockClass("PricklyBerryBush", typeof(PricklyBerryBush));
            api.RegisterBlockClass("ShrubBerryBush", typeof(ShrubBerryBush));
            api.RegisterBlockClass("GroundBerryPlant", typeof(GroundBerryPlant));
            api.RegisterBlockClass("BlockClipping", typeof(BlockClipping));

            api.RegisterBlockClass("StoneBerryPlant", typeof(StoneBerryPlant));
            api.RegisterBlockClass("StonePlant", typeof(StonePlant));

            api.RegisterBlockClass("HerbPlant", typeof(HerbPlant));
            api.RegisterBlockClass("WaterHerb", typeof(WaterHerb));
            api.RegisterBlockClass("SimpleWaterPlant", typeof(SimpleWaterPlant));

            api.RegisterBlockClass("DuckWeed", typeof(DuckWeed));
            api.RegisterBlockClass("DuckWeedRoot", typeof(DuckWeedRoot));

            api.RegisterBlockClass("BlockRequiresGravelOrSand", typeof(BlockRequiresGravelOrSand));

            api.RegisterBlockClass("GiantKelp", typeof(GiantKelp));
           
            api.RegisterBlockEntityClass("BEHerbariumBerryBush", typeof(BEHerbariumBerryBush));
            api.RegisterBlockEntityClass("BEShrubBerryBush", typeof(BEShrubBerryBush));
            api.RegisterBlockEntityClass("BETallBerryBush", typeof(BETallBerryBush));
            api.RegisterBlockEntityClass("BEClipping", typeof(BEClipping));
            api.RegisterBlockEntityClass("BEGroundBerryPlant", typeof(BEGroundBerryPlant));
            api.RegisterBlockEntityClass("BESeedling", typeof(BESeedling));

            api.RegisterBlockEntityClass("BEHerbariumSapling", typeof(BEHerbariumSapling));

            api.RegisterBlockEntityClass("BEDuckWeedRoot", typeof(BEDuckWeedRoot));

            api.RegisterItemClass("ItemClipping", typeof(ItemClipping));
            api.RegisterItemClass("ItemBerrySeed", typeof(ItemBerrySeed));
            api.RegisterItemClass("ItemHerbSeed", typeof(ItemHerbSeed));
            api.RegisterItemClass("HerbariumPoultice", typeof(HerbariumPoultice));

            api.RegisterItemClass("ItemWildTreeSeed", typeof(ItemWildTreeSeed));
            api.RegisterItemClass("ItemWildShield", typeof(ItemWildShield));


            try
            {
                var Config = api.LoadModConfig<HerbariumConfig>("herbariumconfig.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    HerbariumConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    HerbariumConfig.Current = HerbariumConfig.GetDefault();
                }
            }
            catch
            {
                HerbariumConfig.Current = HerbariumConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
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

                api.StoreModConfig(HerbariumConfig.Current, "HerbariumConfig.json");
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            BuffManager.Initialize(api, this);
            BuffManager.RegisterBuffType("RashDebuff", typeof(RashDebuff));
            BuffManager.RegisterBuffType("PoulticeBuff", typeof(PoulticeBuff));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

        }
    }
}
