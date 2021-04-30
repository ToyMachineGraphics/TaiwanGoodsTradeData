using System;
using System.Diagnostics;

namespace TaiwanGoodsTradeData
{
    [DebuggerDisplay("{Model}, {Price}")]
    class Car
    {
        public String Model { get; set; }
        public String Price { get; set; }
        public String Link { get; set; }
        public String ImageUrl { get; set; }
    }
}
