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
            string metal = itemStack.Attributes.GetString("metal");
            string wood = itemStack.Attributes.GetString("wood");
            string color = itemStack.Attributes.GetString("color");

            switch (Construction)
            {
                case "crude":
                    return Lang.Get("Crude shield");
                case "woodmetal":
                    if(wood == "chlorociboria")
                    {
                        return ornate ? Lang.Get("Ornate Chlorociboria-dyed wooden shield") : Lang.Get("Chlorociboria-dyed wooden shield");
                    }
                    if(wood == "petrified")
                    {
                        return ornate ? Lang.Get("Petrified ornate  shield"): Lang.Get("Petrified wooden shield");
                    }
                    if (wood == "generic")
                    {
                        return ornate ? Lang.Get("Ornate wooden shield") : Lang.Get("Wooden shield");
                    }
                    if (wood == "aged")
                    {
                        return ornate ? Lang.Get("Aged ornate shield") : Lang.Get("Aged wooden shield");
                    }
                    return ornate ? Lang.Get("Ornate {0} shield", Lang.Get("material-" + wood)) : Lang.Get("{0} shield", Lang.Get("material-" + wood));
                case "woodmetalleather":
                    return ornate ? Lang.Get("Ornate leather reinforced wooden shield") : Lang.Get("Leather reinforced wooden shield");
                case "metal":
                    return ornate ? Lang.Get("shield-ornatemetal", Lang.Get("color-" + color), Lang.Get("material-" + metal)) : Lang.Get("shield-withmaterial", Lang.Get("material-" + metal));
                case "blackguard":
                    return Lang.Get("Blackguard shield");
            }

            return base.GetHeldItemName(itemStack);
        }




    }
}