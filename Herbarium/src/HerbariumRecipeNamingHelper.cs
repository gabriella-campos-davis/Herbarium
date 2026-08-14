using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace herbarium
{
    public class HerbariumRecipeNames : VanillaCookingRecipeNames
    {
        protected override string GetNameForMergedIngredients(IWorldAccessor worldForResolve, CookingRecipe recipe, Vintagestory.API.Datastructures.OrderedDictionary<ItemStack, int> quantitiesByStack)
        {
            string recipeCode = recipe.Code!;

            switch (recipeCode)
            {
                case "gruel":
                    {
                        List<string> MainIngredientNames = [];
                        string MainIngredientFormat = "{0}";
                        List<string> MashedIngredientNames = [];
                        List<string> FreshIngredientNames = [];
                        string? StockName = null;
                        CookingRecipeIngredient? ingred = null;
                        int typesOfGrain = quantitiesByStack.Count(val => recipe.GetIngrendientFor(val.Key)?.Code == "grain-base");
                        int max = 0;

                        foreach ((ItemStack stack, int quantity) in quantitiesByStack)
                        {
                            string itemName;
                            if (stack.Collectible.Code == "waterportion") continue;

                            ingred = recipe.GetIngrendientFor(stack);
                            if (ingred?.Code == "stock")
                            {
                                StockName = ingredientName(stack, EnumIngredientNameType.InsturmentalCase);
                                continue;
                            }

                            if (ingred?.Code == "grain-base")
                            {
                                if (typesOfGrain < 3)
                                {
                                    if (MainIngredientNames.Count < 2)
                                    {
                                        itemName = getMainIngredientName(stack, recipeCode, MainIngredientNames.Count > 0);
                                        if (!MainIngredientNames.Contains(itemName)) MainIngredientNames.Add(itemName);
                                    }
                                }
                                else
                                {
                                    itemName = ingredientName(stack);
                                    if (!MainIngredientNames.Contains(itemName)) MainIngredientNames.Add(itemName);
                                }

                                max += quantity;
                                continue;
                            }

                            itemName = ingredientName(stack, EnumIngredientNameType.InsturmentalCase);

                            if (getFoodCat(worldForResolve, stack, ingred) == EnumFoodCategory.Vegetable)
                            {
                                if (!MashedIngredientNames.Contains(itemName)) MashedIngredientNames.Add(itemName);
                            }
                            else if (recipe.GetIngrendientFor(stack)?.Code.StartsWith("grain") == false)
                            {
                                if (!FreshIngredientNames.Contains(itemName)) FreshIngredientNames.Add(itemName);
                            }
                        }

                        string ExtraIngredientsFormat = Lang.HasTranslation("meal-adds-gruel-mashed")
                            ? "meal-adds-gruel-mashed"
                            : "meal-adds-porridge-mashed";

                        if (FreshIngredientNames.Count > 0)
                        {
                            if (MashedIngredientNames.Count > 0)
                            {
                                ExtraIngredientsFormat = Lang.HasTranslation("meal-adds-gruel-mashed-and-fresh")
                                    ? "meal-adds-gruel-mashed-and-fresh"
                                    : "meal-adds-porridge-mashed-and-fresh";
                            }
                            else
                            {
                                ExtraIngredientsFormat = Lang.HasTranslation("meal-adds-gruel-fresh")
                                    ? "meal-adds-gruel-fresh"
                                    : "meal-adds-porridge-fresh";
                            }
                        }

                        if (MainIngredientNames.Count == 2) MainIngredientFormat = "multi-main-ingredients-format";

                        string MealFormat = getMaxMealFormat("meal", recipeCode, max);

                        if (StockName != null) MealFormat += "-on-stock";
                        MealFormat = Lang.Get(MealFormat, getMainIngredientsString(MainIngredientNames, MainIngredientFormat), getMealAddsString(ExtraIngredientsFormat, MashedIngredientNames, FreshIngredientNames), StockName);

                        return MealFormat.Trim().UcFirst();
                    }
                case "compote":
                    {
                        string MainIngredientName = string.Empty;
                        List<string> BoiledIngredientNames = [];
                        string itemName = string.Empty;

                        foreach ((ItemStack stack, int quantity) in quantitiesByStack)
                        {
                            CookingRecipeIngredient? ingred = recipe.GetIngrendientFor(stack);

                            if (stack.Collectible.Code.Path is "waterportion" or "sweetwaterportion")
                            {
                                continue;
                            }

                            if (ingred?.Code == "fruit-base" && MainIngredientName == string.Empty)
                            {
                                MainIngredientName = getMainIngredientName(stack, recipeCode);
                                continue;
                            }

                            itemName = ingredientName(stack, EnumIngredientNameType.InsturmentalCase);

                            if (!BoiledIngredientNames.Contains(itemName)) BoiledIngredientNames.Add(itemName);
                        }

                        string BoiledIngredientsFormat = Lang.HasTranslation("meal-adds-compote-boiled")
                            ? "meal-adds-compote-boiled"
                            : "meal-adds-soup-boiled";

                        string MealFormat = getMaxMealFormat("meal", recipeCode, 1);
                        MealFormat = Lang.Get(MealFormat, getMainIngredientsString([MainIngredientName], "{0}"), getMealAddsString(BoiledIngredientsFormat, BoiledIngredientNames));
                        return MealFormat.Trim().UcFirst();
                    }
            }

            return Lang.Get("unknown");
        }
    }
}
