using UnityEngine;

namespace ETClaimEffect
{
    [CreateAssetMenu(fileName = "ClaimResourceData", menuName = "Data/ClaimEffectResourceData")]
    public class ClaimResourceData : ScriptableObject
    {
        public int resourceID;
        public Material material;//particle standard unlit
        public int valuePerResourceParticle;
    }
}
