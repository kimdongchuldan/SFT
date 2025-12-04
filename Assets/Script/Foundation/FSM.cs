using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Foundation
{
    public class FSM<T> where T : unmanaged
    {
        private static readonly ILogger logger = Debug.unityLogger;

        public delegate void Handler();

        public class State
        {
            public T id;
            public Handler start = null;
            public Handler progress = null;
            public Handler finish = null;

            public void Start()
            {
                if (start != null)
                {
                    start();
                }
            }
            public void Progress()
            {
                if (progress != null)
                {
                    progress();
                }
            }

            public void Finish()
            {
                if (finish != null)
                {
                    finish();
                }
            }
        };

        private Dictionary<T, State> states = new();

        private State current = null;

        ~FSM()
        {
            Clear();
        }

        public bool IsEmpty
        {
            get
            {
                if (states.Count == 0)
                    return true;
                return false;
            }
        }

        public void Clear()
        {
            if (current != null)
            {
                current.Finish();
            }
            current = null;
            states.Clear();
        }
        
        public void Add(T id, Handler start, Handler progress, Handler finish)
        {
            State st = new State();
            st.id = id;
            st.start = start;
            st.progress = progress;
            st.finish = finish;
            states.Add(id, st);
        }

        virtual public void Transit(T id, bool restart = true)
        {
            State before = current;
            current = null;

            State target;
            
            if (false == states.TryGetValue(id, out target))
            {
                target = null;
                if (null != before)
                {
                    before.Finish();
                }

                logger.Log($"이동하려는 상태가 없다 {id}");
                return;
            }

            if (before == target)
            {
                if (restart)
                {
                    if (null != before)
                    {
                        before.Finish();
                    }
                    current = target;
                    target.Start();
                }
                else
                {
                    current = target;
                }
            }
            else
            {
                if (null != before)
                {
                    before.Finish();
                }
                current = target;
                target.Start();
            }
        }

        public void Update()
        {
            if (current == null)
                return;
            current.Progress();
        }

        public T Current()
        {
            if (null != current)
                return current.id;
            return default(T);
        }
    }
}
