using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using herbarium.config;
using BuffStuff;
using Vintagestory.GameContent;


[assembly: ModInfo( "Herbarium Plant Library",
	Description = "Adds implements various useful classes related to plants for other mods to use",
	Website     = "",
	Authors     = new []{ "gabb", "CATASTEROID" } )]

namespace herbarium
{
    public class Herbarium : ModSystem
    {
        NetworkHandler networkHandler;
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        #region Client
        public override void StartClientSide(ICoreClientAPI capi)
        {
            networkHandler.InitializeClientSideNetworkHandler(capi);
            base.StartClientSide(capi);
        }
        #endregion

        #region server
        public override void StartServerSide(ICoreServerAPI api)
        {
            networkHandler.InitializeServerSideNetworkHandler(api);
            BuffManager.Initialize(api, this);
            BuffManager.RegisterBuffType("RashDebuff", typeof(RashDebuff));
            BuffManager.RegisterBuffType("PoulticeBuff", typeof(PoulticeBuff));
        }
        #endregion
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            networkHandler = new NetworkHandler();

            api.RegisterBlockClass("HerbariumBerryBush", typeof(HerbariumBerryBush));
            api.RegisterBlockClass("PricklyBerryBush", typeof(PricklyBerryBush));
            api.RegisterBlockClass("ShrubBerryBush", typeof(HerbariumBerryBush));
            api.RegisterBlockClass("GroundBerryPlant", typeof(GroundBerryPlant));
            api.RegisterBlockClass("BlockClipping", typeof(BlockClipping));

            api.RegisterBlockClass("BlockVineClipping", typeof(BlockVineClipping));
            api.RegisterBlockClass("BlockFruitingVines", typeof(BlockFruitingVines));
            api.RegisterBlockClass("BlockTreeVine", typeof(BlockTreeVine));

            api.RegisterBlockClass("StoneBerryPlant", typeof(StoneBerryPlant));
            api.RegisterBlockClass("StonePlant", typeof(StonePlant));

            api.RegisterBlockClass("HerbPlant", typeof(HerbPlant));
            api.RegisterBlockClass("WaterHerb", typeof(WaterHerb));
            api.RegisterBlockClass("SimpleWaterPlant", typeof(SimpleWaterPlant));

            api.RegisterBlockClass("DuckWeed", typeof(DuckWeed));
            api.RegisterBlockClass("DuckWeedRoot", typeof(DuckWeedRoot));

            api.RegisterBlockClass("BlockRequiresGravelOrSand", typeof(BlockRequiresGravelOrSand));

            api.RegisterBlockClass("BlockPricklyLeaves", typeof(BlockPricklyLeaves));
            api.RegisterBlockClass("BlockLeavesDropCanes", typeof(BlockLeavesDropCanes));

            api.RegisterBlockClass("GiantKelp", typeof(GiantKelp));

            api.RegisterBlockBehaviorClass("BlockBehaviorHarvestMultiple", typeof(BlockBehaviorHarvestMultiple));
           
            api.RegisterBlockEntityClass("BEHerbariumBerryBush", typeof(BEHerbariumBerryBush));
            api.RegisterBlockEntityClass("BEShrubBerryBush", typeof(BEHerbariumBerryBush));
            api.RegisterBlockEntityClass("BETallBerryBush", typeof(BETallBerryBush));
            api.RegisterBlockEntityClass("BEClipping", typeof(BEClipping));
            api.RegisterBlockEntityClass("BEGroundBerryPlant", typeof(BEGroundBerryPlant));
            api.RegisterBlockEntityClass("BESeedling", typeof(BEClipping));

            api.RegisterBlockEntityClass("BEHerbariumSapling", typeof(BlockEntitySapling));

            api.RegisterBlockEntityClass("BEDuckWeedRoot", typeof(BEDuckWeedRoot));

            api.RegisterItemClass("ItemClipping", typeof(ItemClipping));
            api.RegisterItemClass("ItemBerrySeed", typeof(ItemBerrySeed));
            api.RegisterItemClass("ItemHerbSeed", typeof(ItemHerbSeed));
            api.RegisterItemClass("HerbariumPoultice", typeof(HerbariumPoultice));

            api.RegisterItemClass("ItemWildTreeSeed", typeof(ItemWildTreeSeed));
            api.RegisterItemClass("ItemWildShield", typeof(ItemWildShield));
            
            networkHandler.RegisterMessages(api);
            HerbariumConfig.createConfig(api);
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);
            api.GetCookingRecipes().ForEach(recipe =>
            {
                if (CookingRecipe.NamingRegistry.ContainsKey("jam"))
                {
                    CookingRecipe.NamingRegistry["jam"] = new JamRecipeName();
                }
            });
        }
    }
}
