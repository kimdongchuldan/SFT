using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Foundation;
using Unity.VisualScripting;
using UnityEngine;

public partial class GuestManager
{
    public class Line
    {
        Queue<Spawn> waitings = new();
        Slot[] slots = new Slot[3];
        GuestManager GM;

        public float RemainingPatienceTime()
        {
            return slots[0].RemainingPatienceTime();
        }

        public Line(GuestManager gm, StageMaster.Line m, List<UnityEngine.Vector3> path)
        {
            GM = gm;

            int i=0;
            foreach (var p in path)
            {
                slots[i++] = new Slot()
                {
                    GM = gm,
                    Index = i,
                    Pos = p
                };
            }

            {
                slots[2].Scale = 0.8f;
                slots[1].Scale = 0.9f;
                slots[0].Scale = 1.0f;
            }

            {
                slots[2].SortingLayer = "IG-Line3";
                slots[1].SortingLayer = "IG-Line2";
                slots[0].SortingLayer = "IG-Line1";
            }

            foreach (var s in m.spawns)
            {
                waitings.Enqueue(new Spawn(GM, s));
            }

            WorkingState();
            step.Add(StepType.Ready, null, null, null);
            step.Transit(StepType.Ready);
        }

        public void Update()
        {
            step.Update();
        }

        enum StepType
        {
            Ready,
            Working,
        }

        FSM<StepType> step = new();

        void WorkingState()
        {
            FSM<StepType>.Handler b = delegate ()
            {
            };

            FSM<StepType>.Handler u = delegate ()
            {
                foreach (var s in slots)
                {
                    s.Update();
                }

                // 조건이 되면 새로운 스폰을 시작
                {
                    if (waitings.Count > 0)
                    {
                        var slot = slots[2];
                        if (null == slot.Spawn)
                        {
                            var s = waitings.Dequeue();
                            s.Start(slots);
                            slot.Spawn = s;
                        }
                    }
                }

                {
                    {
                        var c = slots[2];
                        var n = slots[1];

                        if (n.Spawn == null)
                        {
                            if (c.IsReadyToMove)
                            {
                                n.Spawn = c.Spawn;
                                c.Spawn = null;
                                n.Spawn.Move(n.Pos, n.Scale, 1);
                            }
                        }
                    }

                    {
                        var c = slots[1];
                        var n = slots[0];

                        if (n.Spawn == null)
                        {
                            if (c.IsReadyToMove)
                            {
                                n.Spawn = c.Spawn;
                                c.Spawn = null;
                                n.Spawn.Move(n.Pos, n.Scale, 2);
                            }
                        }
                    }

                    {
                        var c = slots[0];

                        if (null != c.Spawn)
                        {
                            if (c.IsReadyToMove)
                            {
                                c.Order();
                            }

                            if (c.IsLeaving || c.IsDespawned)
                            {   
                                c.Clear();
                            }
                        }
                    }
                }
            };

            step.Add(StepType.Working, b, u, null);
        }

        public void Start()
        {
            step.Transit(StepType.Working);
        }
    }
}

