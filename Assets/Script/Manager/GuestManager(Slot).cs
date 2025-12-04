using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Foundation;
using UnityEngine;

public partial class GuestManager
{
    public class Slot
    {
        public int Index;
        public Spawn Spawn;
        public UnityEngine.Vector3 Pos;
        public float Scale;
        public string SortingLayer;

        public GuestManager GM;
        
        public void Update()
        {
            if (null == Spawn)
                return;

            Spawn.Update();
        }

        public bool IsReadyToMove
        {
            get
            {
                if (null == Spawn)
                    return false;
                return Spawn.IsReadyToMove;
            }
        }

        public void Order()
        {
            Spawn.Order();
        }

        public bool IsDespawned
        {
            get
            {
                if (null == Spawn)
                    return true;
                return Spawn.IsDespawned;
            }
        }

        public bool IsLeaving
        {
            get
            {
                if (null == Spawn)
                    return true;
                return Spawn.IsLeaving;
            }
        }

        public void Clear()
        {
            if (null != Spawn)
            {
                Spawn.Clear();
                Spawn = null;
            }
        }

        public float RemainingPatienceTime()
        {
            if (null != Spawn)
            {
                return Spawn.RemainingPatienceTime();
            }

            return 0.0f;
        }
    }
}

