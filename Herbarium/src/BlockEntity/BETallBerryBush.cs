using Vintagestory.API.Common;

namespace herbarium 
{
    public class BETallBerryBush : BEHerbariumBerryBush
        {

        public BETallBerryBush() : base()
        {

        }

        protected override bool DoGrow()
        {
            try
            {
                if (!base.DoGrow()) return false;

                if (Api.World.BlockAccessor.GetBlock(Pos).LastCodePart() == "ripe" && Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Air)
                {
                    if (Api.World.BlockAccessor.GetBlock(Pos).Attributes["isLarge"].AsBool() && Api.World.BlockAccessor.GetBlock(Pos.DownCopy()).Attributes["isBottomBlock"].AsBool())
                    {
                        if (Api.World.BlockAccessor.GetBlock(Pos.DownCopy(2)).BlockMaterial is not EnumBlockMaterial.Plant)
                        {
                            Block growthBlock = Api.World.BlockAccessor.GetBlock(AssetLocation.Create(Block.Attributes["growthBlock"].ToString()));
                            if (growthBlock is not null) Api.World.BlockAccessor.SetBlock(growthBlock.BlockId, Pos.UpCopy());
                        }
                    }
                    if (Api.World.BlockAccessor.GetBlock(Pos.DownCopy()).BlockMaterial is not EnumBlockMaterial.Plant)
                    {
                        Block growthBlock = Api.World.BlockAccessor.GetBlock(AssetLocation.Create(Block.Attributes["growthBlock"].ToString()));
                        if (growthBlock is not null) Api.World.BlockAccessor.SetBlock(growthBlock.BlockId, Pos.UpCopy());
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
