using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;

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

    public class StatProperty
    {

        public StatType StatType { get; set; }
        public float Value { get; set; }
        public string StatName { get; set; }

        public StatProperty()
        {

        }

        public StatProperty(StatType statType, float statValue)
        {
            this.StatType = statType;
            this.Value = statValue;
            this.StatName = statType.ToString();
        }

        public StatProperty(string StatName, float statValue)
        {
            this.StatName = StatName;
            this.Value = statValue;
            object stype;
            if (Enum.TryParse(typeof(StatType), StatName, true, out stype))
            {
                this.StatType = (StatType)stype;
            }
        } 
    }


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

        public void SetStatFromChangeList(string jstatchange)
        {
            JObject jchange = new JObject(jstatchange);
            foreach (JProperty ch in jchange.Properties())
            {
                if (statblock.ContainsKey(ch.Name)) 
                {
                    statblock[ch] = ch.Value;
                }
                else
                {
                    statblock.Add(ch.Name, ch.Value);
                }
            }
        }

        public string GetStatBlockString()
        {
            return statblock.ToString();
        }
    }
}