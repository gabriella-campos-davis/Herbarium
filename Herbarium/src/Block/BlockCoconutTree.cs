using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace herbarium
{
	// Token: 0x02000003 RID: 3

	public class BlockCoconutTree : Block, ITreeGenerator
	{
		// Token: 0x06000003 RID: 3 RVA: 0x00002078 File Offset: 0x00000278
		TreeGenParams treeGen = new TreeGenParams();

		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);
			ICoreServerAPI coreServerAPI = api as ICoreServerAPI;
			if (coreServerAPI != null && this.Variant["part"] == "trunk10")
			{
				coreServerAPI.RegisterTreeGenerator(new AssetLocation("coconuttree-normal-trunk10", this.Code.Domain), this);
				api.Logger.Chat("registered tree gen code");
			} else {
				api.Logger.Chat("did not register tree gen code");
			}
			if (this.trunkThickness6 == null)
			{
				IBlockAccessor blockAccessor = api.World.BlockAccessor;
				this.trunkThickness6 = blockAccessor.GetBlock(new AssetLocation("coconuttree-normal-trunk6", this.Code.Domain));
				this.trunkThickness7 = blockAccessor.GetBlock(new AssetLocation("coconuttree-normal-trunk7", this.Code.Domain));
				this.trunkThickness8 = blockAccessor.GetBlock(new AssetLocation("coconuttree-normal-trunk8", this.Code.Domain));
				this.trunkThickness9 = blockAccessor.GetBlock(new AssetLocation("coconuttree-normal-trunk9", this.Code.Domain));
				this.trunkThickness10 = blockAccessor.GetBlock(new AssetLocation("coconuttree-normal-trunk10", this.Code.Domain));
				this.trunkTopFlowers = blockAccessor.GetBlock(new AssetLocation("coconuttree-normal-trunktopflowers", this.Code.Domain));
				this.trunkTopFoliage = blockAccessor.GetBlock(new AssetLocation("coconuttree-normal-foliage", this.Code.Domain));
			}

			treeGen.hemisphere = EnumHemisphere.North;
			treeGen.mossGrowthChance = 0f;
			treeGen.otherBlockChance = 1f;
			treeGen.skipForestFloor = false;
			treeGen.size = 1f;
			treeGen.treesInChunkGenerated = 0;
			treeGen.vinesGrowthChance = 0f;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000216F File Offset: 0x0000036F
		public override void OnDecalTesselation(IWorldAccessor world, MeshData decalMesh, BlockPos pos)
		{
			base.OnDecalTesselation(world, decalMesh, pos);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000217C File Offset: 0x0000037C
		public override void OnJsonTesselation(ref MeshData sourceMesh, ref int[] lightRgbsByCorner, BlockPos pos, Block[] chunkExtBlocks, int extIndex3d)
		{
			base.OnJsonTesselation(ref sourceMesh, ref lightRgbsByCorner, pos, chunkExtBlocks, extIndex3d);
			if (this == this.trunkTopFoliage)
			{
				for (int i = 0; i < sourceMesh.FlagsCount; i++)
				{
					sourceMesh.Flags[i] = ((sourceMesh.Flags[i] & -33546241) | BlockFacing.UP.NormalPackedFlags);
				}
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000021D4 File Offset: 0x000003D4
		public string Type()
		{
			return this.Variant["type"];
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000021E8 File Offset: 0x000003E8
		public void GrowTree(IBlockAccessor blockAccessor, BlockPos pos, TreeGenParams treeGenParams)
		{
			if (treeGenParams.otherBlockChance != 0f)
			{
				BlockCoconutTree.rand.NextDouble();
			}
			else
			{
				BlockCoconutTree.rand.NextDouble();
			}
			int num = 1;
			if (num == 1)
			{
				this.GrowOneTree(blockAccessor, pos.UpCopy(1), treeGenParams.size, treeGenParams.vinesGrowthChance);
			}
			while (num-- > 0)
			{
				this.GrowOneTree(blockAccessor, pos.UpCopy(1), treeGenParams.size, treeGenParams.vinesGrowthChance);
				pos.X += BlockCoconutTree.rand.Next(8) - 4;
				pos.Z += BlockCoconutTree.rand.Next(8) - 4;
				bool flag = false;
				for (int i = 2; i >= -2; i--)
				{
					if (blockAccessor.GetBlock(pos.X, pos.Y, pos.Z, 1).Fertility > 0 && !blockAccessor.GetBlock(pos.X, pos.Y + i + 1, pos.Z, 2).IsLiquid())
					{
						pos.Y += i;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000022F0 File Offset: 0x000004F0
		private void GrowOneTree(IBlockAccessor blockAccessor, BlockPos upos, float sizeModifier, float vineGrowthChance)
		{
			Block[] array = new Block[4];
			array[0] = this.trunkThickness6;
			array[1] = this.trunkThickness7;
			array[2] = this.trunkThickness8;
			array[3] = this.trunkThickness9;
			array[4] = this.trunkThickness10;
			int num = BlockCoconutTree.rand.Next(2);
			int num2 = GameMath.Clamp((int)(sizeModifier * (float)(2 + BlockCoconutTree.rand.Next(6))), 6, 12);
			int[] array2 = new int[num2];
			for (int i = 0; i <= num2; i++)
			{
				if (num2 - i < 2 + num)
				{
					array2[i] = num2 - i;
				}
				else
				{
					array2[i] = 4 - num;
				}
			}
			this.api.World.Logger.Notification("palm trunk thickness array: {0}", new object[]
			{
				array2
			});
			for (int j = 0; j <= num2; j++)
			{
				Block block = this.trunkThickness6;
				if (!blockAccessor.GetBlock(upos.X, upos.Y + j, upos.Z, 1).IsReplacableBy(block))
				{
					return;
				}
			}
			for (int k = 0; k <= num2; k++)
			{
				Block block2 = this.trunkThickness6;
				if (k == num2)
				{
					block2 = this.trunkTopFoliage;
				}
				else if (k == num2 - 1)
				{
					block2 = this.trunkTopFlowers;
				}
				else
				{
					block2 = array[array2[k]];
				}
				this.api.World.Logger.Notification("placing palm trunk block: {0} at {1}", new object[]
				{
					block2.Code,
					upos
				});
				blockAccessor.SetBlock(block2.BlockId, upos);
				upos.Up(1);
			}
		}

		// Token: 0x04000001 RID: 1
		public Block trunkThickness6;

		// Token: 0x04000002 RID: 2
		public Block trunkThickness7;

		// Token: 0x04000003 RID: 3
		public Block trunkThickness8;

		// Token: 0x04000004 RID: 4
		public Block trunkThickness9;

		// Token: 0x04000005 RID: 5
		public Block trunkThickness10;

		// Token: 0x04000006 RID: 6
		public Block trunkTopFlowers;

		// Token: 0x04000007 RID: 7
		public Block trunkTopFoliage;

		// Token: 0x04000008 RID: 8
		private static Random rand = new Random();
	}
}
