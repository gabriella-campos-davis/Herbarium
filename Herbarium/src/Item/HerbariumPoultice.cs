using System;
using System.Text;
using herbarium.config;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace herbarium
{
    public class HerbariumPoultice : ItemPoultice
    {
        bool isHealingOverTimeEnabled = HerbariumConfig.Current.poulticeHealOverTime.Value;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            isHealingOverTimeEnabled = api.World.Config.GetBool("poulticeHealOverTime", isHealingOverTimeEnabled);
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            byEntity.World.RegisterCallback((dt) =>
            {
                if (byEntity.Controls.HandUse == EnumHandInteract.HeldItemInteract)
                {
                    IPlayer player = null;
                    if (byEntity is EntityPlayer) player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                    byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/player/poultice"), byEntity, player);
                }
            }, 200);


            JsonObject attr = slot.Itemstack.Collectible.Attributes;
            if (attr != null && attr["primaryHealth"].Exists)
            {
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (secondsUsed > 0.7f && byEntity.World.Side == EnumAppSide.Server)
            {
                JsonObject attr = slot.Itemstack.Collectible.Attributes;
                float materialHealth = attr["materialHealth"].AsFloat();
                float primaryHealth = attr["primaryHealth"].AsFloat();
                float secondaryHealth = attr["secondaryHealth"].AsFloat();
                bool curesRash = attr["curesRash"].AsBool();

                float health = materialHealth + primaryHealth + secondaryHealth;

                double time = 8 - secondaryHealth;

                /*
                following line for vanilla poultice health attributes. 
                (i could make a patch to give them proper health properties,
                but I tried and it wasn't working and it's late while i'm coding
                it and this is easier right now.)
                */
                if(attr["health"].Exists) health = attr["health"].AsFloat(); 

                if(!isHealingOverTimeEnabled){
                    byEntity.ReceiveDamage(new DamageSource()
                        {
                        Source = EnumDamageSource.Internal,
                        Type = health > 0 ? EnumDamageType.Heal : EnumDamageType.Poison
                        }, Math.Abs(health));

                    slot.TakeOut(1);
                    slot.MarkDirty();
                } 
                else 
                {
                    var isPoulticeActive = BuffStuff.BuffManager.IsBuffActive(byEntity, "PoulticeBuff");
                    var isRashDebuff = BuffStuff.BuffManager.IsBuffActive(byEntity, "RashDebuff");
                    // only consume item and apply buff if there isn't a buff already active
                    if (!isPoulticeActive) {
                        var buff = new PoulticeBuff();
                        buff.SetHealthAndTime(health, time);
                        buff.Apply(entitySel?.Entity != null ? entitySel.Entity : byEntity);

                        if(isRashDebuff && curesRash){
                            BuffStuff.BuffManager.GetActiveBuff(byEntity, "RashDebuff").Remove();
                        }
                    } 
                    
                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
            }
        }

         public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            JsonObject attr = inSlot.Itemstack.Collectible.Attributes;
            if (attr != null && attr["primaryHealth"].Exists)
            {
                float materialHealth = attr["materialHealth"].AsFloat();
                float primaryHealth = attr["primaryHealth"].AsFloat();
                float secondaryHealth = attr["secondaryHealth"].AsFloat();

                float health = materialHealth + primaryHealth + secondaryHealth;
                dsc.AppendLine(Lang.Get("When used: +{0} hp", health));
            }
        }
    }
}