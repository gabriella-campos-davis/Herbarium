using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace herbarium
{
    /// <summary>
    /// Herbarium's shield item. Behaves exactly like vanilla <see cref="ItemShieldFromAttributes"/>,
    /// but fails soft when a content pack forgets to define the "textures" attribute block.
    ///
    /// Vanilla's (private) genTextureSource dereferences Attributes["textures"] without a null check,
    /// so an itemtype that omits it throws a NullReferenceException the moment the shield is rendered
    /// (e.g. in the interaction-help HUD). Since that method is private we cannot override it; instead
    /// we inject an empty "textures" object during OnLoaded. With an empty (rather than missing) block,
    /// vanilla's texture lookup finds no variant match and falls back to the cached shape textures,
    /// rendering the base shield instead of crashing the client.
    /// </summary>
    public class ItemWildShield : ItemShieldFromAttributes
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (!Attributes["textures"].Exists)
            {
                api.Logger.Warning(
                    "Herbarium: shield item {0} is missing the 'textures' attribute block. " +
                    "Injecting an empty one so it renders with the base shape textures instead of crashing. " +
                    "Define attributes.textures (keyed by variant, e.g. \"{{metal}}-{{wood}}\") to give it proper textures.",
                    Code);

                JObject token = (Attributes?.Token as JObject)?.DeepClone() as JObject ?? new JObject();
                token["textures"] = new JObject();
                Attributes = new JsonObject(token);
            }
        }
    }
}
