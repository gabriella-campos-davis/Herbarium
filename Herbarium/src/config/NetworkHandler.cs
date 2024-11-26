using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace herbarium.config
{
    public class NetworkHandler
    {
        internal void RegisterMessages(ICoreAPI api)
        {
            api.Network
               .RegisterChannel("herbariumchannel")
               .RegisterMessageType(typeof(HerbariumConfigFromServerMessage))
               .RegisterMessageType(typeof(OnPlayerLoginMessage));
        }

        #region Client
        IClientNetworkChannel clientChannel;
        ICoreClientAPI clientApi;
        public void InitializeClientSideNetworkHandler(ICoreClientAPI capi)
        {
            clientApi = capi;

            clientChannel = capi.Network.GetChannel("herbariumchannel")
                                        .SetMessageHandler<HerbariumConfigFromServerMessage>(RecieveHerbariumConfigAction);
        }

        //SetToolConfigValues received from Server
        private void RecieveHerbariumConfigAction(HerbariumConfigFromServerMessage herbariumConfig)
        {
            HerbariumConfig.Current.plantsCanDamage = herbariumConfig.plantsCanDamage;
            HerbariumConfig.Current.plantsCanPoison = herbariumConfig.plantsCanPoison;
            HerbariumConfig.Current.plantsDamage = herbariumConfig.plantsDamage;
            HerbariumConfig.Current.plantsDamageTick = herbariumConfig.plantsDamageTick;
            HerbariumConfig.Current.plantsWillDamage = herbariumConfig.plantsWillDamage;

            HerbariumConfig.Current.poulticeHealOverTime = herbariumConfig.poulticeHealOverTime;

            HerbariumConfig.Current.berryBushCanDamage = herbariumConfig.berryBushCanDamage;
            HerbariumConfig.Current.berryBushDamage = herbariumConfig.berryBushDamage;
            HerbariumConfig.Current.berryBushDamageTick = herbariumConfig.berryBushDamageTick;
            HerbariumConfig.Current.berryBushWillDamage = herbariumConfig.berryBushWillDamage;
            HerbariumConfig.Current.useKnifeForClipping = herbariumConfig.useKnifeForClipping;
            HerbariumConfig.Current.useShearsForClipping = herbariumConfig.useShearsForClipping;
        }

        #endregion

        #region server
        IServerNetworkChannel serverChannel;
        ICoreServerAPI serverApi;
        public void InitializeServerSideNetworkHandler(ICoreServerAPI api)
        {
            serverApi = api;

            //Listen for player join events
            api.Event.PlayerJoin += OnPlayerJoin;

            serverChannel = api.Network.GetChannel("herbariumchannel")
                                       .SetMessageHandler<OnPlayerLoginMessage>(OnPlayerJoin);
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            OnPlayerJoin(player, new OnPlayerLoginMessage());
        }

        //Send a packet on the client channel containing a new instance of HerbariumConfigFromServerMessage
        //Which is pre-loaded on creation with all the values for Tool Config.
        private void OnPlayerJoin(IServerPlayer fromPlayer, OnPlayerLoginMessage packet)
        {
            serverChannel.SendPacket(new HerbariumConfigFromServerMessage(), fromPlayer);
        }


        #endregion

        
        [ProtoContract]
        class HerbariumConfigFromServerMessage
        {
            public bool? plantsCanDamage = HerbariumConfig.Current.plantsCanDamage;
            public bool? plantsCanPoison = HerbariumConfig.Current.plantsCanPoison;
            public float? plantsDamage = HerbariumConfig.Current.plantsDamage;
            public float? plantsDamageTick = HerbariumConfig.Current.plantsDamageTick;
            public string[] plantsWillDamage = HerbariumConfig.Current.plantsWillDamage;

            public bool? poulticeHealOverTime = HerbariumConfig.Current.poulticeHealOverTime;
            public bool? berryBushCanDamage = HerbariumConfig.Current.berryBushCanDamage;
            public float? berryBushDamage = HerbariumConfig.Current.berryBushDamage;
            public float? berryBushDamageTick = HerbariumConfig.Current.berryBushDamageTick;
            public string[] berryBushWillDamage = HerbariumConfig.Current.berryBushWillDamage;
            public bool? useKnifeForClipping = HerbariumConfig.Current.useKnifeForClipping;
            public bool? useShearsForClipping = HerbariumConfig.Current.useShearsForClipping;
        }

        [ProtoContract]
        class OnPlayerLoginMessage
        {
            [ProtoMember(1)]
            IPlayer[] fromPlayer;
        }
    }
}