using System.Diagnostics;
using System;

namespace Foundation
{
    public class ElapsedTimer
    {
        Stopwatch Watch = new Stopwatch();

        public ElapsedTimer()
        {
            Watch.Start();
            Start();
        }

        public void Start()
        {
            Watch.Restart();
        }

        public float ElapsedSeconds
        {
            get
            {
                return (float)Watch.ElapsedMilliseconds / 1000.0f;
            }
        }

        public float ElapsedMilliseconds
        {
            get
            {
                return Watch.ElapsedMilliseconds;
            }
        }
    }
}
