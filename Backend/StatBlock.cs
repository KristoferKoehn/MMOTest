using Godot;
using Newtonsoft.Json;
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

        public StatProperty()
        {

        }

        public StatProperty(StatType statType, float statValue)
        {
            this.StatType = statType;
            this.Value = statValue;
        }
    }


    public class StatBlock
    {

        private Dictionary<StatType, float> statblock = new Dictionary<StatType, float>();
        //private JObject statblock = new JObject();
        public StatBlock() { }

        public void SetStat(StatType statType, float value)
        {
            statblock[statType] = value;
        }

        public float GetStat(StatType statType)
        {
            return statblock.ContainsKey(statType) ? statblock[statType] : 0f;
        }

        public void SetStatBlock(string JString)
        {
            statblock = JsonConvert.DeserializeObject<Dictionary<StatType,float>>(JString);
        }

        public void SetStatBlock(Dictionary<StatType, float> sb)
        {
            statblock = sb;
        }

        public string SerializeStatBlock()
        {
            return JsonConvert.SerializeObject(statblock);
        }

        public void ApplyChange(StatType statType, float value)
        {
            SetStat(statType, GetStat(statType) + value);
        }

        public void ApplyAllChanges(Dictionary<StatType, float> sb)
        {
            foreach (StatType statType in sb.Keys)
            {
                GD.Print(statType.ToString());
                statblock[statType] = GetStat(statType) + sb[statType];
            }
        }
    }
}