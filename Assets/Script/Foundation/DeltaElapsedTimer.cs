using System.Diagnostics;
using System;

namespace Foundation
{
    public class DeltaElapsedTimer
    {
        private float _elapsedSeconds;

        public DeltaElapsedTimer()
        {
            Start();
        }

        public void Start()
        {
            _elapsedSeconds = 0.0f;
        }

        public void Past(float deltaSeconds)
        {
            _elapsedSeconds = Math.Max(0.0f, _elapsedSeconds + deltaSeconds);
        }

        public float ElapsedSeconds
        {
            get
            {
                return _elapsedSeconds;
            }
        }

        public float ElapsedMilliseconds
        {
            get
            {
                return _elapsedSeconds * 1000.0f;
            }
        }
    }
}
