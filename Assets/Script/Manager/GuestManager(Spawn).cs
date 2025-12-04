using System;
using System.Collections.Generic;
using System.Threading;
using Foundation;
using UnityEngine;

public partial class GuestManager
{
    public class Spawn
    {
        Guest guest;
        StageMaster.Spawn master;
        int queue = 2; // 항상 맨 뒤에서 스폰되니까
        Slot[] slots;
        GuestManager GM;

        public float RemainingPatienceTime()
        {
            if (phase.Current() == PhaseType.Order)
            {
                return guest.RemainingPatienceTime;
            }

            return 0.0f;
        }        
        public Spawn(GuestManager gm, StageMaster.Spawn m)
        {
            GM = gm;
            master = m;
        }

        public void Order()
        {
            if (phase.Current() == PhaseType.Queue)
            {
                float dur = GM.SumRemainingPatienceTime() + master.OrderPreparationTime;
                if (master.patience_time != 0)
                {
                    dur = master.patience_time;
                }

                guest.StartOrder(dur);
                phase.Transit(PhaseType.Order);
            }
        }

        public bool IsReadyToMove
        {
            get
            {
                if (phase.Current() == PhaseType.Queue && 
                    false == guest.IsMoving)
                    return true;
                return false;
            }
        }

        public bool IsDespawned
        {
            get
            {
                return guest.IsDespawned;
            }
            
        }

        public bool IsLeaving
        {
            get
            {
                return guest.IsLeaving;
            }
            
        }

        public void Move(Vector3 pos, float sc, int sortingOrder)
        {   
            guest.Move(pos, sc, sortingOrder);
        }

        enum PhaseType
        {
            Standby,
            Spawn,
            Queue,
            Order,
        }

        FSM<PhaseType> phase = new();

        DeltaElapsedTimer SharedTimer = new();

        void StandbyState()
        {
            FSM<PhaseType>.Handler b = delegate ()
            {
                //log.Log($"{GetType().Name}");
                SharedTimer.Start();
            };

            FSM<PhaseType>.Handler u = delegate ()
            {
                SharedTimer.Past(Time.deltaTime);

                if (SharedTimer.ElapsedSeconds > 0.2)
                {
                    phase.Transit(PhaseType.Spawn);
                }
            };

            FSM<PhaseType>.Handler e = delegate ()
            {
            };

            phase.Add(PhaseType.Standby, b, u, e);
        }


        void SpawnState()
        {
            FSM<PhaseType>.Handler b = delegate ()
            {
                //log.Log($"{GetType().Name}");
                guest.StartSpawn();
            };

            FSM<PhaseType>.Handler u = delegate ()
            {
                if (guest.IsQueued)
                {
                    phase.Transit(PhaseType.Queue);
                    return;
                }
            };

            FSM<PhaseType>.Handler e = delegate ()
            {
            };

            phase.Add(PhaseType.Spawn, b, u, e);
        }
        void QueueState()
        {
            FSM<PhaseType>.Handler b = delegate ()
            {
                //log.Log($"{GetType().Name}");
            };

            FSM<PhaseType>.Handler u = delegate ()
            {
            };

            FSM<PhaseType>.Handler e = delegate ()
            {
            };

            phase.Add(PhaseType.Queue, b, u, e);
        }

        void OrderState()
        {
            FSM<PhaseType>.Handler b = delegate ()
            {
                log.Log($"주문");                
            };

            FSM<PhaseType>.Handler u = delegate ()
            {
            };

            FSM<PhaseType>.Handler e = delegate ()
            {
            };

            phase.Add(PhaseType.Order, b, u, e);
        }

        public void Update()
        {
            phase.Update();            
        }

        GameObject CreatePrefab(string prefab, Vector3 pos)
        {
            var path = $"Prefab/Guest/{prefab}";
            var pf = Resources.Load<GameObject>(path);
            if (null == pf)
            {
                log.LogError("", $"프리팹 생성중 실패 {path}");
                return null;
            }
            return Instantiate(pf, pos, Quaternion.identity);
        }

        GameObject CreateGuest(Vector3 pos)
        {
            try
            {
                var shell = CreatePrefab("Guest", pos);
                if (false == string.IsNullOrEmpty(master.guest.prefab)) // 유령은 모델이 없기도 하다
                {
                    var model = shell.transform.Find("Model");
                    var body = CreatePrefab(master.guest.prefab, Vector3.zero);
                    body.transform.SetParent(model.transform, false);
                    body.transform.localScale = Vector3.one;
                }
                return shell;
            } 
            catch (Exception ex)
            {
                log.LogWarning("", $"게스트 생성중 예외 {ex.Message}\n{ex.StackTrace}");   
            }
            return null;
        }

        public void Start(Slot[] s)
        {
            slots = s;

            var e = slots[queue];
            var g = CreateGuest(e.Pos);
            guest = g.GetComponent<Guest>();
            guest.Master = master;
            
            StandbyState();
            SpawnState();
            QueueState();
            OrderState();
            phase.Transit(PhaseType.Standby);
        }

        public void Clear()
        {
            //Destroy(guest.gameObject);
        }
    }
}

