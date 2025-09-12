using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Files.Logic;

namespace UCS.Logic
{
    internal class CombatComponent : Component
    {
        public CombatComponent(ConstructionItem ci, Level level) : base(ci)
        {
            var bd = (BuildingData) ci.GetData();
            if (bd.AmmoCount != 0)
            {
                m_vAmmo = bd.AmmoCount;
            }
        }

        public override int Type => 1;

        const int m_vType = 0x01AB3F00;

        int m_vAmmo;
        int m_vAimAngle;
        int m_vAimAngleWar;
        bool m_vAttackMode;
        bool m_vAttackModeWar;
        bool m_vAttackModeDraft;

        public void FillAmmo()
        {
            var ca = GetParent().Avatar.Avatar;
            var bd = (BuildingData) GetParent().GetData();
            var rd = CSVManager.DataTables.GetResourceByName(bd.AmmoResource);

            if (ca.HasEnoughResources(rd, bd.AmmoCost))
            {
                ca.CommodityCountChangeHelper(0, rd, bd.AmmoCost);
                m_vAmmo = bd.AmmoCount;
            }
        }
        
        public void useAmmo(bool warmup = false)
        {
            if (!warmup)
            {
                var bd = (BuildingData) GetParent().GetData();
                m_vAmmo -= bd.AmmoCount / 4;
            }
        }
        
        public void toggleMode()
        {
            if (m_vAttackMode)
            {
                m_vAttackMode = false;
                m_vAttackModeDraft = false;
                m_vAttackModeWar = false;
            }
            else
            {
                m_vAttackMode = true;
                m_vAttackModeDraft = true;
                m_vAttackModeWar = true;
            }
        }

        public void rotateSweeper()
        {
            m_vAimAngle = (m_vAimAngle + 45) % 360;
        }

        public override void Load(JObject jsonObject)
        {
            if (jsonObject["ammo"] != null)
            {
                m_vAmmo = jsonObject["ammo"].ToObject<int>();
            }
            if (jsonObject["attack_mode"] != null)
            {
                m_vAttackMode = jsonObject["attack_mode"].ToObject<bool>();
            }

            if (jsonObject["attack_mode_war"] != null)
            {
                m_vAttackModeWar = jsonObject["attack_mode_war"].ToObject<bool>();
            }

            if (jsonObject["attack_mode_draft"] != null)
            {
                m_vAttackModeDraft = jsonObject["attack_mode_draft"].ToObject<bool>();
            }
            if (jsonObject["aim_angle"] != null)
                m_vAimAngle = jsonObject["aim_angle"].ToObject<int>();

            if (jsonObject["aim_angle_war"] != null)
                m_vAimAngleWar = jsonObject["aim_angle_war"].ToObject<int>();
        }

        public override JObject Save(JObject jsonObject)
        {
            jsonObject.Add("ammo", m_vAmmo);
            jsonObject.Add("attack_mode", m_vAttackMode);
            jsonObject.Add("attack_mode_war", m_vAttackModeWar);
            jsonObject.Add("attack_mode_draft", m_vAttackModeDraft);
            jsonObject.Add("aim_angle", m_vAimAngle);
            jsonObject.Add("aim_angle_war", m_vAimAngleWar);
            return jsonObject;
        }
    }
}
