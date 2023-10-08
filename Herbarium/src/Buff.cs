using System;
using ProtoBuf;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Common;
//using Vintagestory.API.Client;
//using Vintagestory.API.Server;

namespace BuffStuff {
  public abstract class Buff {
    // public abstract string ID { get; }
    public int TickCounter = 0;
    public double ExpireTimestampInDays = 0;
    public int ExpireTick = 0;
    internal Entity entity;
    public Entity Entity { get { return entity; } }
    /// <summary>Called when a buff is first applied. If a buff is applied while the buff is still active, OnStart is not called: OnStack is called instead.</summary>
    public virtual void OnStart() { }
    /// <summary>Called when a buff is applied while a buff with an identical ID is still active. OnStack is called on the second buff, which will replace the first buff.</summary>
    public virtual void OnStack(Buff oldBuff) { }
    /// <summary>Called when a buff expires due to time passing or SetExpiryImmediately() having been called. Note that changing the expiry of the buff will have no effect: it has already expired.</summary>
    public virtual void OnExpire() { }
    /// <summary>Called when the Entity dies.</summary>
    public virtual void OnDeath() { }
    /// <summary>Called every 250 ms (4 times per second) while the buff is active.</summary>
    public virtual void OnTick() { }
    /// <summary>Called when the player with this active buff leaves the server.</summary>
    public virtual void OnLeave() { }
    /// <summary>Called when the player with this active buff re-joins the server.</summary>
    public virtual void OnJoin() { }
    /// <summary>Sets expiry of this buff to the provided number of in-game days from now. Disables real-time (tick) based expiry.</summary>
    protected void SetExpiryInGameDays(double deltaDays) {
      ExpireTimestampInDays = BuffManager.Now + deltaDays;
      ExpireTick = Int32.MaxValue;
    }
    /// <summary>Sets expiry of this buff to the provided number of in-game hours from now. Disables real-time (tick) based expiry.</summary>
    protected void SetExpiryInGameHours(double deltaHours) {
      ExpireTimestampInDays = BuffManager.Now + deltaHours / 24.0;
      ExpireTick = Int32.MaxValue;
    }
    /// <summary>Sets expiry of this buff to the provided number of in-game minutes from now. Disables real-time (tick) based expiry.</summary>
    protected void SetExpiryInGameMinutes(double deltaMinutes) {
      ExpireTimestampInDays = BuffManager.Now + deltaMinutes / 24.0 / 60.0;
      ExpireTick = Int32.MaxValue;
    }
    /// <summary>Sets expiry of this buff to the provided number of buff ticks from now. If you supply `0`, the buff will be removed immediately after its buff tick. Disables in-game (calendar) based expiry.</summary>
    protected void SetExpiryInTicks(int deltaTicks) {
      ExpireTick = TickCounter + deltaTicks;
      ExpireTimestampInDays = double.PositiveInfinity;
    }
    /// <summary>Sets expiry of this buff to the provided number of seconds from now, using buff ticks to count time. Disables in-game (calendar) based expiry.</summary>
    protected void SetExpiryInRealSeconds(int deltaSeconds) {
      SetExpiryInTicks((int)Math.Ceiling(deltaSeconds * 1000.0 / BuffManager.TICK_MILLISECONDS));
    }
    /// <summary>Sets expiry of this buff to the provided number of minutes from now, using buff ticks to count time. Disables in-game (calendar) based expiry.</summary>
    protected void SetExpiryInRealMinutes(int deltaMinutes) {
      SetExpiryInRealSeconds(deltaMinutes * 60);
    }
    /// <summary>Disables auto-expiry of this buff.</summary>
    protected void SetExpiryNever() {
      ExpireTimestampInDays = double.PositiveInfinity;
      ExpireTick = Int32.MaxValue;
    }
    /// <summary>Forces the buff to expire immediately after OnTick has been called. If this is called from `OnTick`, the buff will expire when `OnTick` returns.</summary>
    protected void SetExpiryImmediately() {
      ExpireTimestampInDays = 0;
    }
    /// <summary>Apply this buff to an entity, which starts everything in motion. Either `OnStart` or `OnStack` will be called, depending on whether the entity already has an active buff with the same buff ID.</summary>
    public void Apply(Entity entity) {
      if (this.entity != null) { throw new Exception("BuffStuff.Buff: this buff has already been applied to an entity, use a new buff instead of re-applying an existing buff"); }
      BuffManager.ApplyBuff(entity, this);
    }
    /// <summary>Forcibly remove this buff from its entity, without calling `OnExpire` or any other methods. You may want to use the auto-expiry system instead of calling this method.</summary>
    public void Remove() {
      BuffManager.RemoveBuff(entity, this);
    }
  }
}
