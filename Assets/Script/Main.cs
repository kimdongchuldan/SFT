using UnityEngine;

public class Main : MonoBehaviour
{
    // 아이디어 
    // 젤다에서 한번 칼을 휘두르고 쉬는 시간 때문에 위험할수 있다.
    // 즉 어떤 결정을 했을때, 리스크와 기회가 공존해야 한다

    //public MasterManager MasterManager { get; private set; }

    void Start()
    {
        InputManager.Initialize();

        MasterManager.Initialize();

        /*MasterManager = new MasterManager(new MasterBase[]
        {
            new GuestMaster(),
            new KitchenItemMaster(),
            new StageMaster(),
            new TruckMaster(),
        });
        MasterManager.InitializeAll();*/

        /*{
            GuestMaster.Load();
            KitchenItemMaster.Load();
            StageMaster.Load();


            // ------------------------

            GuestMaster.PostProcess();
            KitchenItemMaster.PostProcess();
            StageMaster.PostProcess();
        }*/
        
        User.Initialize();

        
    }

    void Update()
    {
        InputManager.Update();
        User.Update();
    }
}
