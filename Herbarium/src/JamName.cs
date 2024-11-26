using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace herbarium
{
    public class JamRecipeName : ICookingRecipeNamingHelper
    {
        public string GetNameForIngredients(IWorldAccessor worldForResolve, string recipeCode, ItemStack[] stacks)
        {
            OrderedDictionary<ItemStack, int> quantitiesByStack = new OrderedDictionary<ItemStack, int>();
            quantitiesByStack = mergeStacks(worldForResolve, stacks);

            CookingRecipe recipe = worldForResolve.Api.GetCookingRecipe(recipeCode);

            if (recipeCode == null || recipe == null || quantitiesByStack.Count == 0) return Lang.Get("unknown");

            ItemStack[] fruits = new ItemStack[2];
            int i = 0;
            foreach (var val in quantitiesByStack)
            {
                if (val.Key.Collectible.NutritionProps?.FoodCategory == EnumFoodCategory.Fruit || val.Key.Collectible.NutritionProps?.FoodCategory == EnumFoodCategory.Vegetable)
                {
                    fruits[i++] = val.Key;
                    if (i == 2) break;
                }
            }

            if (fruits[1] != null)
            {
                string jamName = fruits[0].Collectible.LastCodePart() + "-" + fruits[1].Collectible.LastCodePart() + "-jam";
                if (Lang.HasTranslation(jamName)) return Lang.Get(jamName);

                string firstFruitInJam = (fruits[0].Collectible.Code.Domain == "game" ? "" : fruits[0].Collectible.Code.Domain + ":") + fruits[0].Collectible.LastCodePart() + "-in-jam-name";
                string secondFruitInJam = (fruits[1].Collectible.Code.Domain == "game" ? "" : fruits[1].Collectible.Code.Domain + ":") + fruits[1].Collectible.LastCodePart() + "-in-jam-name";
                return Lang.Get("mealname-mixedjam", Lang.HasTranslation(firstFruitInJam) ? Lang.Get(firstFruitInJam) : fruits[0].GetName(), Lang.HasTranslation(secondFruitInJam) ? Lang.Get(secondFruitInJam) : fruits[1].GetName());
            }
            else if (fruits[0] != null)
            {
                string jamName = fruits[0].Collectible.LastCodePart() + "-jam";
                if (Lang.HasTranslation(jamName)) return Lang.Get(jamName);

                string fruitInJam = (fruits[0].Collectible.Code.Domain == "game" ? "" : fruits[0].Collectible.Code.Domain + ":") + fruits[0].Collectible.Code.Domain + ":" + fruits[0].Collectible.LastCodePart() + "-in-jam-name";
                return Lang.Get("mealname-singlejam", Lang.HasTranslation(fruitInJam) ? Lang.Get(fruitInJam) : fruits[0].GetName());
            }
            else return Lang.Get("unknown");
        }

        private OrderedDictionary<ItemStack, int> mergeStacks(IWorldAccessor worldForResolve, ItemStack[] stacks)
        {
            OrderedDictionary<ItemStack, int> dict = new OrderedDictionary<ItemStack, int>();

            List<ItemStack> stackslist = new List<ItemStack>(stacks);
            while (stackslist.Count > 0)
            {
                ItemStack stack = stackslist[0];
                stackslist.RemoveAt(0);
                if (stack == null) continue;

                int cnt = 1;

                while (true)
                {
                    ItemStack foundstack = stackslist.FirstOrDefault((otherstack) => otherstack != null && otherstack.Equals(worldForResolve, stack, GlobalConstants.IgnoredStackAttributes));

                    if (foundstack != null)
                    {
                        stackslist.Remove(foundstack);
                        cnt++;
                        continue;
                    }

                    break;
                }

                dict[stack] = cnt;
            }

            return dict;
        }
    }
}