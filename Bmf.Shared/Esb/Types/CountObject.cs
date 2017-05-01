using System;

namespace Bmf.Shared.Esb.Types
{   
    public class CountObject
    {
        public string Type { get; set; }
        public Priority Priority { get; set;}
        public DateTime Enqueued { get; set; }
        public bool IsInWork { get; set; }
        public bool IsSentToNextHost { get; set; }
        public Guid Id { get; set; }
    }
}
