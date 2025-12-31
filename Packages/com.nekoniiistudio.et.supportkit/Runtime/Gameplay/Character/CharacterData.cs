using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Gameplay
{
    public class CharacterData : IWorldObject
    {
        private string id; // save id
        public string ID => id;
        public string firstName;
        public string lastName;
        public CharacterSex sex;
        public Sprite avatar;


        public void GetInitData()
        {

        }
    }
    public interface IWorldObject
    {
        public string ID { get; }
    }
    public enum CharacterSex
    {
        Male,
        Female,
    }
}
