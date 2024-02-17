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

namespace Backend.StatBlock
{

    public class StatBlock
    {
        private Dictionary<StatType, float> statblock = new Dictionary<StatType, float>();
        public StatBlock() { }

        public void SetStat(StatType statType, float value)
        {
            statblock[statType] = value;
        }

        public float GetStat(StatType statType)
        {
            return statblock.ContainsKey(statType) ? statblock[statType] : 0;
        }
    }
}