using System;
using System.Collections.Generic;

namespace KevDataAccess.Models
{
    public class Career
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public IList<CareerItem> Items { get; set; }

        public Career()
        {
            Items = new List<CareerItem>();
        }
    }
}
