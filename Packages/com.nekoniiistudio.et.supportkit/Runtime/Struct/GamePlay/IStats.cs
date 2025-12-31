namespace ET.Gameplay
{
    /// <summary>
    /// Mere blueprint 
    /// </summary>
    public interface IHitpoint
    {
        public float HP { get; set; }
        public float MaxHP { get; }
        public void IncreaseHP(float value);
        public void DecreaseHP(float value);
        public void ResetHP();
    }
    public interface IEnergy
    {
        public float EN { get; set; }
        public void IncreaseEN(float value);
        public void DecreaseEN(float value);
        public void ResetEN();
    }
    public interface IManapoint
    {
        public float MP { get; set; }
        public void IncreaseMP(float value);
        public void DecreaseMP(float value);
        public void ResetMP();
    }
    public interface IHunger
    {
        public float HG { get; set; }
        public void IncreaseHG(float value);
        public void DecreaseHG(float value);
        public void ResetHG();
    }
    public interface IThirst
    {
        public float TH { get; set; }
        public void IncreaseTH(float value);
        public void DecreaseTH(float value);
        public void ResetTH();
    }
    public interface IStamina
    {
        public float ST { get; set; }
        public void IncreaseST(float value);
        public void DecreaseST(float value);
        public void ResetST();
    }
}


