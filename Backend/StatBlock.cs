using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public enum StatType
{
    HEALTH,
    PHYSICAL_DAMAGE,
    ABILITY_POINTS,
    ARMOR,
    MAGIC_RESIST,
    MOVE_SPEED,
    MANA,
    ATTACK_SPEED,
    CASTING_SPEED,

}



namespace MMOTest.Backend
{

    public class StatBlock
    {

        //private Dictionary<StatType, float> statblock = new Dictionary<StatType, float>();
        private JObject statblock = new JObject();
        public StatBlock() { }

        public void SetStat(StatType statType, float value)
        {
            //statblock[statType] = value;
            statblock.Add(Enum.GetName(typeof(StatType), statType), value);
        }

        public float GetStat(StatType statType)
        {
            return statblock.ContainsKey(Enum.GetName(typeof(StatType), statType)) ? (float)statblock.Property(Enum.GetName(typeof(StatType), statType)) : 0;
        }

        public void SetStatBlock(string JString)
        {
            statblock = new JObject(JString);
        }

        public void SetStatBlock(JObject Job)
        {
            statblock = Job;
        }

        public string GetStatBlockString()
        {
            return statblock.ToString();
        }
    }
}