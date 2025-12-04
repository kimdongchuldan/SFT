using System.Collections.Generic;
using Unity.VisualScripting;

namespace Model
{
    public class Status
    {
        public long Lv {get; set; }
        public long Exp {get; set; }
        public long Energy {get; set; }
        public long Gold {get; set; }
        public long Gem {get; set; }
    }

    public class Item
    {
        public string ID {get; set; }
        public long Type {get; set; }
        public long Count {get; set; }
        public long Creation {get; set; }
        public long Expirtaion {get; set; }
    }
}

static partial class User
{
    static public Model.Status Status = new();

    static public Dictionary<long, Model.Item> Items = new();

    public static void Initialize()
    {
    }

    public static void Update()
    {
    }
}