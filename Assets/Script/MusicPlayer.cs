using System.Collections.Generic;
using Foundation;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public class Music
    {
        public AudioSource Source { get; set; }

        enum StateType
        {
            Standby,
            FadeIn,
            FadeOut,
            Play
        }

        FSM<StateType> state = new();

        float target;
        float duration;

        DeltaElapsedTimer timer = new();

        void FadeInState()
        {
            FSM<StateType>.Handler b = delegate ()
            {
                timer.Start();
                target = Source.volume;
                Source.volume = 0;
                Source.Play();
            };

            FSM<StateType>.Handler u = delegate ()
            {
                timer.Past(Time.deltaTime);
                var t = Mathf.Clamp01(timer.ElapsedSeconds / duration);
                Source.volume = Mathf.Lerp(0, target, t);

                if (target <= Source.volume)
                {
                    state.Transit(StateType.Play);
                }
            }; 
            
            state.Add(StateType.FadeIn, b, u, null);
        }        

        void FadeOutState()
        {
            FSM<StateType>.Handler b = delegate ()
            {
                target = Source.volume;
            };

            FSM<StateType>.Handler u = delegate ()
            {
                timer.Past(Time.deltaTime);
                var t = Mathf.Clamp01(timer.ElapsedSeconds / duration);
                Source.volume = Mathf.Lerp(target, 0, t);
                if (Source.volume <= 0.0f)
                {
                    Source.Stop();
                    state.Transit(StateType.Standby);
                }
            }; 
            
            state.Add(StateType.FadeOut, b, u, null);
        }

        public void Start()
        {
            state.Add(StateType.Standby, null, null, null);
            state.Add(StateType.Play, null, null, null);
            FadeInState();
            FadeOutState();
            state.Transit(StateType.Standby);
        }
        
        public void Update()
        {
            state.Update();
        }

        public bool IsStandby()
        {
            if (state.Current() == StateType.Standby)
                return true;
            return false;
        }

        public void StartFadeOut(float dur = 1.0f)
        {
            if (dur <= 0)
                dur = 0.1f;
            duration = dur;
            state.Transit(StateType.FadeOut, false);
        }

        public void StartFadeIn(float dur = 1.0f)
        {
            if (dur <= 0)
                dur = 0.1f;
            duration = dur;
            state.Transit(StateType.FadeIn, false);
        }
    }

    enum TransitType
    {
        Standby,
        Transit,
    }

    FSM<TransitType> transit = new();


    List<Music> Musics = new();
    Music NewMusic;

    void TransitState()
    {
        FSM<TransitType>.Handler b = delegate ()
        {
        };

        FSM<TransitType>.Handler u = delegate ()
        {
            // 전부다 FadeOut되었는지 확인
            foreach (var ms in Musics)
            {
                // 하나라도 아니면 리턴
                if (false == ms.IsStandby())
                    return;
            }

            Musics.Clear();
            Musics.Add(NewMusic);
            NewMusic.StartFadeIn();
        }; 
        
        transit.Add(TransitType.Transit, b, u, null);
    }

    // 음악 변경이 마구 들어와도 문제 없도록
    public void Play(AudioSource src)
    {
        // 기존 재생중인 음악 페이드 아웃
        foreach (var ms in Musics)
        {
            ms.StartFadeOut();
        }
        
        if (null != src)
        {
            // 새 음악 일단 등록
            var m = new Music
            {
                Source = src
            };
            Musics.Add(m);
            NewMusic = m;
            NewMusic.Start();
        }

        transit.Transit(TransitType.Transit);
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        transit.Add(TransitType.Standby, null, null, null);
        TransitState();
        transit.Transit(TransitType.Standby);
    }

    void Update()
    {
        transit.Update();

        foreach (var ms in Musics)
        {
            ms.Update();
        }
    }
}
