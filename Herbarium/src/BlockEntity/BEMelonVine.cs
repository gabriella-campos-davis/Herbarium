using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace herbarium
{
    public class BlockEntityMelonVine : BlockEntity
    {
        // Number of hours for a melon to advance to it's next stage
        public float melonHoursToGrow = 12;

        // Number of hours for a vine to advance to it's next stage
        public float vineHoursToGrowFirstStage = 12;

        // Number of hours for a vine to advance from stage 2 to stage 3
        public float vineHoursToGrowSecondStage = 6;

        // Probability that the vine will bloom once it gets to stage 3
        public float bloomProbability = 0.5f;

        // Probability that the vine will return to a normal stage 3 vine once it has bloomed
        public float debloomProbability = 0.5f;

        // Probability that a an attempt to spawn a new vine will happen at stage 2
        public float vineSpawnProbability = 0.5f;

        // Probability that a new vine will spawn in the preferred growth direction which is away from it's parent
        public float preferredGrowthDirProbability = 0.75f;

        // Maximum number of tries allowed to spawn melons
        public int maxAllowedMelonGrowthTries = 3;

        // Block code name of the type of melon
        public string melonBlockCode;
        public string domainCode;


        // Temporary data
        public long growListenerId;
        public Block stage1VineBlock;
        public Block melonBlock;


        // Permanent (stored) data

        // Total game hours when it can enter the next growth stage
        public double totalHoursForNextStage;

        // If true then the vine is allowed to bloom. The vine only gets one chance during stage 3 to bloom
        public bool canBloom;

        // Current number of times the vine has tried to grow a melon
        public int melonGrowthTries;

        // Keeps up with when each surrounding melon can advance to the next stage
        public Dictionary<BlockFacing, double> melonTotalHoursForNextStage = new Dictionary<BlockFacing, double>();

        // Position of the plant that spawned this vine.
        public BlockPos parentPlantPos;

        // Favored direction of growth for new vines
        public BlockFacing preferredGrowthDir;

        public int internalStage = 0;


        public BlockEntityMelonVine() : base()
        {
            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                melonTotalHoursForNextStage.Add(facing, 0);
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            melonHoursToGrow = Block.Attributes["melonHoursToGrow"].AsFloat(12);
            vineHoursToGrowFirstStage = Block.Attributes["vineHoursToGrowFirstStage"].AsFloat(12);
            vineHoursToGrowSecondStage = Block.Attributes["vineHoursToGrowSecondStage"].AsFloat(6);
            bloomProbability = Block.Attributes["bloomProbability"].AsFloat(0.5f);
            debloomProbability = Block.Attributes["debloomProbability"].AsFloat(0.5f);
            vineSpawnProbability = Block.Attributes["vineSpawnProbability"].AsFloat(0.5f);
            preferredGrowthDirProbability = Block.Attributes["preferredGrowthDirProbability"].AsFloat(0.75f);
            maxAllowedMelonGrowthTries = Block.Attributes["maxAllowedMelonGrowthTries"].AsInt(3);
            melonBlockCode = Block.Attributes["melonBlockCode"].AsString();
            domainCode = Block.Attributes["domainCode"].AsString("game");

            stage1VineBlock = api.World.GetBlock(new AssetLocation(domainCode + ":" + melonBlockCode + "-vine-1-normal"));
            melonBlock = api.World.GetBlock(new AssetLocation(domainCode + ":" + melonBlockCode + "-fruit-1"));

            if (api is ICoreServerAPI)
            {
                growListenerId = RegisterGameTickListener(TryGrow, 2000);
            }
        }

        
        public void CreatedFromParent(BlockPos parentPlantPos, BlockFacing preferredGrowthDir, double currentTotalHours)
        {
            totalHoursForNextStage = currentTotalHours + vineHoursToGrowFirstStage;
            this.parentPlantPos = parentPlantPos;
            this.preferredGrowthDir = preferredGrowthDir;
        }

        private void TryGrow(float dt)
        {
            if (DieIfParentDead()) return;

            while (Api.World.Calendar.TotalHours > totalHoursForNextStage)
            {
                GrowVine();
                totalHoursForNextStage += vineHoursToGrowFirstStage;
            }

            TryGrowMelons();
        }

        private void TryGrowMelons()
        {
            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                double melonTotalHours = melonTotalHoursForNextStage[facing];
                while (melonTotalHours > 0 && Api.World.Calendar.TotalHours > melonTotalHours)
                {
                    BlockPos melonPos = Pos.AddCopy(facing);
                    Block melon = Api.World.BlockAccessor.GetBlock(melonPos);

                    if (IsMelon(melon))
                    {
                        int currentStage = CurrentMelonStage(melon);
                        if (currentStage == 4)
                        {
                            //Stop growing
                            melonTotalHours = 0;
                        }
                        else
                        {
                            SetMelonStage(melon, melonPos, currentStage + 1);
                            melonTotalHours += melonHoursToGrow;
                        }
                    }
                    else
                    {
                        melonTotalHours = 0;
                    }
                    melonTotalHoursForNextStage[facing] = melonTotalHours;
                }
            }
        }

        void GrowVine()
        {
            internalStage++;

            Block block = Api.World.BlockAccessor.GetBlock(Pos);

            int currentStage = CurrentVineStage(block);

            if (internalStage > 6)
            {
                SetVineStage(block, currentStage + 1);
            }

            if (IsBlooming())
            {
                if (melonGrowthTries >= maxAllowedMelonGrowthTries || Api.World.Rand.NextDouble() < debloomProbability)
                {
                    melonGrowthTries = 0;

                    SetVineStage(block, 3);
                }
                else
                {
                    melonGrowthTries++;

                    TrySpawnMelon(totalHoursForNextStage - vineHoursToGrowFirstStage);
                }
            }

            if (currentStage == 3)
            {
                if(canBloom && Api.World.Rand.NextDouble() < bloomProbability)
                {
                    SetBloomingStage(block);
                }
                canBloom = false;
            }

            if (currentStage == 2)
            {
                if (Api.World.Rand.NextDouble() < vineSpawnProbability)
                {
                    TrySpawnNewVine();
                }

                totalHoursForNextStage += vineHoursToGrowSecondStage;
                canBloom = true;
                SetVineStage(block, currentStage + 1);
            }

            if (currentStage < 2)
            {
                SetVineStage(block, currentStage + 1);
            }
        }

        private void TrySpawnMelon(double curTotalHours)
        {
            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                BlockPos candidatePos = Pos.AddCopy(facing);
                Block block = Api.World.BlockAccessor.GetBlock(candidatePos);
                if (!CanReplace(block)) continue;
                
                if (MelonCropBehavior.CanSupportMelon(Api, candidatePos.DownCopy()))
                {
                    Api.World.BlockAccessor.SetBlock(melonBlock.BlockId, candidatePos);
                    melonTotalHoursForNextStage[facing] = curTotalHours + melonHoursToGrow;
                    return;
                }
                
            }
        }

        private bool IsMelon(Block block)
        {
            if (block != null)
            {
                string code = block.Code.GetName();
                return code.StartsWithOrdinal(melonBlockCode + "-fruit");
            }
            return false;
        }

        private bool DieIfParentDead()
        {
            if (parentPlantPos == null)//Can happen if someone places a melon mother plan on farmland in creative mode(I think...)
            {
                Die();
                return true;
            }
            else
            {
                Block parentBlock = Api.World.BlockAccessor.GetBlock(parentPlantPos);
                if (!IsValidParentBlock(parentBlock) && Api.World.BlockAccessor.GetChunkAtBlockPos(parentPlantPos) != null)
                {
                    Die();
                    return true;
                }
            }
            return false;
        }

        private void Die()
        {
            Api.Event.UnregisterGameTickListener(growListenerId);
            growListenerId = 0;
            Api.World.BlockAccessor.SetBlock(0, Pos);
        }

        private bool IsValidParentBlock(Block parentBlock)
        {
            if (parentBlock != null)
            {
                string blockCode = parentBlock.Code.GetName();
                if (blockCode.StartsWithOrdinal("crop-"+ melonBlockCode) || blockCode.StartsWithOrdinal(melonBlockCode + "-vine"))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsBlooming()
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string lastCodePart = block.LastCodePart();
            return block.LastCodePart() == "blooming";
        }

        private bool CanReplace(Block block)
        {
            return block == null || (block.Replaceable >= 6000 && !block.Code.GetName().Contains(melonBlockCode));
        }

        private void SetVineStage(Block block, int toStage)
        {
            try
            {
                ReplaceSelf(block.CodeWithParts("" + toStage, toStage == 4 ? "withered" : "normal"));
            } catch (Exception)
            {
                Api.World.BlockAccessor.SetBlock(0, Pos);
            }
            
        }

        private void SetMelonStage(Block melonBlock, BlockPos melonPos, int toStage)
        {
            Block nextBlock = Api.World.GetBlock(melonBlock.CodeWithParts("" + toStage));
            if (nextBlock == null) return;
            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, melonPos);
        }

        private void SetBloomingStage(Block block)
        {
            ReplaceSelf(block.CodeWithParts("blooming"));
        }

        private void ReplaceSelf(AssetLocation blockCode)
        {
            Block nextBlock = Api.World.GetBlock(blockCode);
            if (nextBlock == null) return;
            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
        }

        private void TrySpawnNewVine()
        {
            BlockFacing spawnDir = GetVineSpawnDirection();
            BlockPos newVinePos = Pos.AddCopy(spawnDir);
            Block blockToReplace = Api.World.BlockAccessor.GetBlock(newVinePos);

            if (!IsReplaceable(blockToReplace)) return;

            newVinePos.Y--;
            if (!CanGrowOn(Api, newVinePos)) return;
            newVinePos.Y++;

            Api.World.BlockAccessor.SetBlock(stage1VineBlock.BlockId, newVinePos);

            BlockEntityMelonVine be = Api.World.BlockAccessor.GetBlockEntity(newVinePos) as BlockEntityMelonVine;
            if (be != null)
            {
                be.CreatedFromParent(Pos, spawnDir, totalHoursForNextStage);
            }
        }

        private bool CanGrowOn(ICoreAPI api, BlockPos pos)
        {
            return api.World.BlockAccessor.GetMostSolidBlock(pos).CanAttachBlockAt(api.World.BlockAccessor, stage1VineBlock, pos, BlockFacing.UP);
        }

        private bool IsReplaceable(Block block)
        {
            return block == null || block.Replaceable >= 6000;
        }

        private BlockFacing GetVineSpawnDirection()
        {
            if(Api.World.Rand.NextDouble() < preferredGrowthDirProbability)
            {
                return preferredGrowthDir;
            }
            else
            {
                return DirectionAdjacentToPreferred();
            }
        }

        private BlockFacing DirectionAdjacentToPreferred()
        {
            if (BlockFacing.NORTH == preferredGrowthDir || BlockFacing.SOUTH == preferredGrowthDir)
            {
                return Api.World.Rand.NextDouble() < 0.5 ? BlockFacing.EAST : BlockFacing.WEST;
            }
            else
            {
                return Api.World.Rand.NextDouble() < 0.5 ? BlockFacing.NORTH : BlockFacing.SOUTH;
            }
        }

        private int CurrentVineStage(Block block)
        {
            int stage = 0;
            int.TryParse(block.LastCodePart(1), out stage);
            return stage;
        }

        private int CurrentMelonStage(Block block)
        {
            int stage = 0;
            int.TryParse(block.LastCodePart(0), out stage);
            return stage;
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            totalHoursForNextStage = tree.GetDouble("totalHoursForNextStage");
            canBloom = tree.GetInt("canBloom") > 0;

            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                melonTotalHoursForNextStage[facing] = tree.GetDouble(facing.Code);
            }
            melonGrowthTries = tree.GetInt("melonGrowthTries");

            parentPlantPos = new BlockPos(tree.GetInt("parentPlantPosX"), tree.GetInt("parentPlantPosY"), tree.GetInt("parentPlantPosZ"));
            preferredGrowthDir = BlockFacing.ALLFACES[tree.GetInt("preferredGrowthDir")];
            internalStage = tree.GetInt("internalStage");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetDouble("totalHoursForNextStage", totalHoursForNextStage);
            tree.SetInt("canBloom", canBloom ? 1 : 0);

            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                tree.SetDouble(facing.Code, melonTotalHoursForNextStage[facing]);
            }
            tree.SetInt("melonGrowthTries", melonGrowthTries);

            if (parentPlantPos != null)
            {
                tree.SetInt("parentPlantPosX", parentPlantPos.X);
                tree.SetInt("parentPlantPosY", parentPlantPos.Y);
                tree.SetInt("parentPlantPosZ", parentPlantPos.Z);
            }
            if (preferredGrowthDir != null)
            {
                tree.SetInt("preferredGrowthDir", preferredGrowthDir.Index);
            }
            tree.SetInt("internalStage", internalStage);
        }
    }
}