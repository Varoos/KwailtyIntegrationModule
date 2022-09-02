using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KwailtyIntegrationModule.Models
{
    public class PostingData
    {
        public PostingData()
        {
            data = new List<Hashtable>();
        }
        public List<Hashtable> data { get; set; }
    }
}