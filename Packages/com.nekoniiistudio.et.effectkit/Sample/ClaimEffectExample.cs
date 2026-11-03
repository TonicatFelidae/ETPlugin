using UnityEngine;

namespace ETClaimEffect
{
    public class ClaimEffectExample : MonoBehaviour
    {
        public ETClaimEffectMain eTClaimEffectMain;
        

        public void ClaimResource0(int amount)
        {
            eTClaimEffectMain.Play(0, amount);
        }
        public void ClaimResource1(int amount)
        {
            eTClaimEffectMain.Play(1, amount);
        }
        public void ClaimResource2(int amount)
        {
            eTClaimEffectMain.Play(2, amount);
        }
        public void ClaimResource3(int amount)
        {
            eTClaimEffectMain.Play(3, amount);
        }
        public void ClaimResource4(int amount)
        {
            eTClaimEffectMain.Play(4, amount);
        }
    }
}