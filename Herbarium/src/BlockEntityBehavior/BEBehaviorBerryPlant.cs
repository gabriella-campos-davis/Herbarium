using herbarium;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Vintagestory.GameContent
{

    public class BEBehaviorBerryPlant : BlockEntityBehavior
    {
        public BEBehaviorBerryPlant(BlockEntity blockentity) : base(blockentity)
        {

        }

        public virtual float? IntervalHours(double daysToCheck, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;

            return null;
        }

        public virtual void OnGrowth(double lastCheck, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;
        }

        public virtual bool? CheckGrowExtra(ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;

            return null;
        }

        public virtual void UpdateHoursLeft(float intervalHours, ref float intervalDays, ref double daysToCheck, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;
        }

        public virtual bool? StopGrowth(float intervalHours, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;

            return null;
        }

        public virtual bool? ResetGrowth(ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;

            return null;
        }

        public virtual bool? RevertGrowth(ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;

            return null;
        }

        public virtual void GetMainInfo(IPlayer forPlayer, StringBuilder sb, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;
        }

        public virtual void GetExtraInfo(IPlayer forPlayer, StringBuilder sb, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;
        }
    }
}
