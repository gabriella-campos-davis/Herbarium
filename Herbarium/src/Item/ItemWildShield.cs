using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace herbarium
{
    //this class just makes it so some specific shields from wildcrafttrees can have correct names
    public class ItemWildShield : ItemShield
    {
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