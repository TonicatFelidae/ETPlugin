using UnityEngine;
using System;
namespace ET.Modules
{ // Define an interface for lock events
    public interface ILockable
    {
        void OnLockStateChanged(bool isLocked);
    }

    public class Ragnarok
    {
        private static Ragnarok instance;
        private bool isLocked = false;

        // Event to notify other scripts about the lock state change
        public event Action<bool> LockStateChanged;

        private Ragnarok() { }

        // Static property to access the lock state
        public static bool IsLocked
        {
            get { return Instance.isLocked; }
        }

        // Static property to access the instance
        public static Ragnarok Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Ragnarok();
                    instance.Initialize();
                }
                return instance;
            }
        }

        // Initialize the instance (called once)
        private void Initialize()
        {
            Doom("01/06/24"); // Set your initial lock time
        }

        private void Doom(string lockTime)
        {
            // Parse the lock time string to a DateTime object
            DateTime lockDateTime = DateTime.ParseExact(lockTime, "dd/MM/yy", null);

            // Get the current date and time
            DateTime currentDate = DateTime.Now;

            // Calculate the time difference
            TimeSpan remainingTime = lockDateTime - currentDate;

            // Output the remaining time in the "dd/MM/yy" format
            Debug.Log("Remaining Time: " + remainingTime.ToString("dd'/'MM'/'yy"));

            // Check if remaining time is zero or negative
            if (remainingTime <= TimeSpan.Zero)
            {
                isLocked = true;
                NotifyLockStateChanged();
            }
        }

        // Notify other scripts about the lock state change
        private void NotifyLockStateChanged()
        {
            LockStateChanged?.Invoke(isLocked);
        }
    }
}


