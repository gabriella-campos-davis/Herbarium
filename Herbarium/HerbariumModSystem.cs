using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
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
       public override void AssetsLoaded(ICoreAPI api)
		{
			base.AssetsLoaded(api);
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

            //UNUSED
            /*
            api.RegisterBlockClass("GiantKelp", typeof(GiantKelp));
            api.RegisterBlockClass("DuckWeed", typeof(DuckWeed));
            api.RegisterBlockClass("DuckWeedRoot", typeof(DuckWeedRoot));  
            api.RegisterBlockClass("BlockRequiresGravelOrSand", typeof(BlockRequiresGravelOrSand));
            api.RegisterBlockClass("BlockLeavesDropCanes", typeof(BlockLeavesDropCanes));
            api.RegisterItemClass("ItemWildTreeSeed", typeof(ItemWildTreeSeed));         
            api.RegisterBlockEntityClass("BEHerbariumSapling", typeof(BEHerbariumSapling));
            api.RegisterBlockEntityClass("BEDuckWeedRoot", typeof(BEDuckWeedRoot));
            */

            //Fruits and Berries
            api.RegisterBlockClass("GroundBerryPlant", typeof(GroundBerryPlant));
            api.RegisterBlockClass("ShrubBerryBush", typeof(ShrubBerryBush));
            api.RegisterBlockClass("PricklyBerryBush", typeof(PricklyBerryBush));
            api.RegisterBlockClass("BlockClipping", typeof(BlockClipping));
            api.RegisterBlockClass("HerbariumBerryBush", typeof(HerbariumBerryBush));
            api.RegisterBlockClass("StoneBerryPlant", typeof(StoneBerryPlant));
            api.RegisterBlockClass("StonePlant", typeof(StonePlant));
            
            api.RegisterItemClass("ItemBerrySeed", typeof(ItemBerrySeed));
            api.RegisterItemClass("ItemClipping", typeof(ItemClipping));

            api.RegisterBlockBehaviorClass("BlockBehaviorHarvestMultiple", typeof(BlockBehaviorHarvestMultiple));
            
            api.RegisterBlockEntityClass("BEClipping", typeof(BEClipping));
            api.RegisterBlockEntityClass("BETallBerryBush", typeof(BETallBerryBush));
            api.RegisterBlockEntityClass("BEShrubBerryBush", typeof(BEShrubBerryBush));
            api.RegisterBlockEntityClass("BEHerbariumBerryBush", typeof(BEHerbariumBerryBush));
            api.RegisterBlockEntityClass("BEGroundBerryPlant", typeof(BEGroundBerryPlant));

            //Herbs
            api.RegisterBlockClass("HerbPlant", typeof(HerbPlant));
            api.RegisterBlockClass("WaterHerb", typeof(WaterHerb));
            api.RegisterBlockClass("SimpleWaterPlant", typeof(SimpleWaterPlant));

            api.RegisterItemClass("HerbariumPoultice", typeof(HerbariumPoultice));
            api.RegisterItemClass("ItemHerbSeed", typeof(ItemHerbSeed));
            api.RegisterBlockEntityClass("BESeedling", typeof(BESeedling));


            //Tree
            api.RegisterItemClass("ItemWildShield", typeof(ItemWildShield));
            api.RegisterBlockClass("BlockPricklyLeaves", typeof(BlockPricklyLeaves));


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
