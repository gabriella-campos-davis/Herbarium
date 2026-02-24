using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace herbarium
{
    //this class just makes it so some specific shields from wildcrafttrees can have correct names
    public class ItemWildShield : ItemShieldFromAttributes
    {
        float offY;
        float curOffY = 0;
        ICoreClientAPI capi;


        public string Construction => Variant["construction"];

        Dictionary<string, Dictionary<string, int>> durabilityGains;

        private static readonly MethodInfo addAllTypesMethod = typeof(ItemShieldFromAttributes).GetMethod("AddAllTypesToCreativeInventory", BindingFlags.Instance | BindingFlags.NonPublic);


        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            curOffY = offY = FpHandTransform.Translation.Y;
            capi = api as ICoreClientAPI;

            durabilityGains = Attributes["durabilityGains"].AsObject<Dictionary<string, Dictionary<string, int>>>();

            addAllTypesMethod?.Invoke(this, null);
        }

         
        public override string GetHeldItemName(ItemStack itemStack)
        {
            bool ornate = itemStack.Attributes.GetString("deco") == "ornate";
            string wood = itemStack.Attributes.GetString("wood");

            switch (Construction)
            {
                case "woodmetal":
                    if(wood == "chlorociboria")
                    {
                        return ornate ? Lang.Get("Ornate Chlorociboria-dyed wooden shield") : Lang.Get("Chlorociboria-dyed wooden shield");
                    }
                    if(wood == "petrified")
                    {
                        return ornate ? Lang.Get("Petrified ornate shield"): Lang.Get("Petrified wooden shield");
                    }
                    return base.GetHeldItemName(itemStack);
            }

            return base.GetHeldItemName(itemStack);
        }




    }
}