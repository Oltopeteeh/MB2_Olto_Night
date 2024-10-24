using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Party;
using System.IO;
using System.Xml.Serialization;
using System;

namespace Olto_Night
{
    public class Olto_Night_SubModule : MBSubModuleBase
    {
        public static float Night_Disguise;
        public static float Renown_Disguise;
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            this.LoadSettings();
        }
        private void LoadSettings()
        {
            Settings settings = new XmlSerializer(typeof(Settings)).Deserialize((Stream)File.OpenRead(Path.Combine(BasePath.Name, "Modules/Olto_Night/settings.xml"))) as Settings;
            Olto_Night_SubModule.Night_Disguise = settings.Night_Disguise;
            Olto_Night_SubModule.Renown_Disguise = settings.Renown_Disguise;
        }
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            gameStarterObject.AddModel(new Olto_Night_ModelA());
            gameStarterObject.AddModel(new Olto_Night_ModelB());
        }
    }
    [Serializable]
    public class Settings
    {
        public float Night_Disguise;
        public float Renown_Disguise;
    }

    public class Olto_Night_ModelA : DefaultDisguiseDetectionModel
    {
        public override float CalculateDisguiseDetectionProbability(Settlement settlement)
        {

            float num = 0f;
            int num2 = 0;
            if (settlement.Town != null && settlement.Town.GarrisonParty != null)
            {
                foreach (TroopRosterElement item in settlement.Town.GarrisonParty.MemberRoster.GetTroopRoster())
                {
                    num2 += item.Number;
                    num += (float)(item.Number * item.Character.Level);
                }
            }

            num /= (float)MathF.Max(1, num2);
            //mod
//          float num3 = 0.3f + 0.003f * (float)Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) - 0.005f * num - MathF.Max(0.15f, 0.00015f * Clan.PlayerClan.Renown);
            float num3 = 0.3f + 0.003f * (float)Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) - 0.005f * num - MathF.Max(0.15f, Olto_Night_SubModule.Renown_Disguise * Clan.PlayerClan.Renown);
            //
            if (Hero.MainHero.CharacterObject.GetPerkValue(DefaultPerks.Roguery.TwoFaced) && num3 > 0f)
            {
                num3 += num3 * DefaultPerks.Roguery.TwoFaced.PrimaryBonus;
            }
            //mod
            num3 = Campaign.Current.IsDay ? num3 : (num3 + Olto_Night_SubModule.Night_Disguise);
            //
            return MathF.Clamp(num3, 0f, 1f);
        }
    }

    //
    public class Olto_Night_ModelB : DefaultSettlementAccessModel
    {
        public override bool CanMainHeroDoSettlementAction(Settlement settlement, SettlementAction settlementAction, out bool disableOption, out TextObject disabledText)
        {
            switch (settlementAction)
            {
                case SettlementAction.RecruitTroops:
                    return CanMainHeroRecruitTroops(settlement, out disableOption, out disabledText);
                case SettlementAction.Craft:
                    return CanMainHeroCraft(settlement, out disableOption, out disabledText);
                case SettlementAction.JoinTournament:
                    return CanMainHeroJoinTournament(settlement, out disableOption, out disabledText);
                case SettlementAction.WatchTournament:
                    return CanMainHeroWatchTournament(settlement, out disableOption, out disabledText);
                case SettlementAction.Trade:
                    return CanMainHeroTrade(settlement, out disableOption, out disabledText);
                case SettlementAction.WaitInSettlement:
                    return CanMainHeroWaitInSettlement(settlement, out disableOption, out disabledText);
                case SettlementAction.ManageTown:
                    return CanMainHeroManageTown(settlement, out disableOption, out disabledText);
                case SettlementAction.WalkAroundTheArena:
                    return CanMainHeroEnterArena(settlement, out disableOption, out disabledText);
                default:
                    Debug.FailedAssert("Invalid Settlement Action", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\GameComponents\\DefaultSettlementAccessModel.cs", "CanMainHeroDoSettlementAction", 268);
                    disableOption = false;
                    disabledText = TextObject.Empty;
                    return true;
            }
        }
        private bool CanMainHeroRecruitTroops(Settlement settlement, out bool disableOption, out TextObject disabledText)
        {
            disabledText = TextObject.Empty;
            disableOption = false;
            return true;
        }
        private bool CanMainHeroCraft(Settlement settlement, out bool disableOption, out TextObject disabledText)
        {
            disableOption = false;
            disabledText = TextObject.Empty;
            return Campaign.Current.IsCraftingEnabled;
        }
        private bool CanMainHeroJoinTournament(Settlement settlement, out bool disableOption, out TextObject disabledText)
        {
            bool num = settlement.Town.HasTournament; // && Campaign.Current.IsDay
            disableOption = false;
            disabledText = TextObject.Empty;
            if (!num)
            {
                return false;
            }

            //if (Campaign.Current.IsMainHeroDisguised)
            //{
            //    disableOption = true;
            //    disabledText = new TextObject("{=mu6Xl4RS}You cannot enter the tournament while disguised.");
            //    return false;
            //}

            if (Hero.MainHero.IsWounded)
            {
                disableOption = true;
                disabledText = new TextObject("{=68rmPu7Z}Your health is too low to fight.");
                return false;
            }

            return true;
        }
        private bool CanMainHeroWatchTournament(Settlement settlement, out bool disableOption, out TextObject disabledText)
        {
            disableOption = false;
            disabledText = TextObject.Empty;
            if (settlement.Town.HasTournament)
            {
                return Campaign.Current.IsDay;
            }

            return false;
        }
        private bool CanMainHeroTrade(Settlement settlement, out bool disableOption, out TextObject disabledText)
        {
            //if (Campaign.Current.IsMainHeroDisguised && Campaign.Current.IsDay) //mod // && Campaign.Current.IsDay
            //{
            //    disableOption = true;
            //    disabledText = new TextObject("{=shU7OlQT}You cannot trade while in disguise.");
            //    return false;
            //}

            disableOption = false;
            disabledText = TextObject.Empty;
            return true;
        }
        private bool CanMainHeroWaitInSettlement(Settlement settlement, out bool disableOption, out TextObject disabledText)
        {
            disableOption = false;
            disabledText = TextObject.Empty;
            //if (Campaign.Current.IsMainHeroDisguised)
            //{
            //    disableOption = true;
            //    disabledText = new TextObject("{=dN5Qc9vN}You cannot wait in town while disguised.");
            //    return false;
            //}

            if (settlement.IsVillage && settlement.Party.MapEvent != null)
            {
                disableOption = true;
                disabledText = new TextObject("{=dN5Qc7vN}You cannot wait in village while it is being raided.");
                return false;
            }

            if (MobileParty.MainParty.Army != null)
            {
                return MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty;
            }

            return true;
        }
        private bool CanMainHeroManageTown(Settlement settlement, out bool disableOption, out TextObject disabledText)
        {
            disabledText = TextObject.Empty;
            disableOption = false;
            if (settlement.IsTown)
            {
                return settlement.OwnerClan.Leader == Hero.MainHero;
            }

            return false;
        }
        private bool CanMainHeroEnterArena(Settlement settlement, out bool disableOption, out TextObject disabledText)
        {
            disableOption = false;
            disabledText = TextObject.Empty;
            return true;
        }

    }

}