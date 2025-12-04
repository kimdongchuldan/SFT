using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public Order Order;

    TextMeshProUGUI count;
    TextMeshProUGUI lv;
    
    void Start()
    {
        var c = transform.Find("Canvas");

        {
            var o = c.Find("Count");
            count = o.GetComponent<TextMeshProUGUI>();
        }

        {
            var o = c.Find("LV");
            lv = o.GetComponent<TextMeshProUGUI>();
        }

        
        {
            if (Order.Master.lv <= 1)
            {
                lv.gameObject.SetActive(false);
            }
            else
            {
                lv.gameObject.SetActive(true);
                lv.text = $"LV{Order.Master.lv}";
            }
        }
    }

    void Update()
    {
        {
            if (Order.OrderCount <= 1)
            {
                count.gameObject.SetActive(false);
            }
            else
            {
                count.gameObject.SetActive(true);
            }
        }

        {
            if (Order.ServedCount < Order.OrderCount)
            {
                count.text = $"x{Order.OrderCount-Order.ServedCount}";
            }
            else
            {
                gameObject.SetActive(false);
            }
        }


        
    }
}
