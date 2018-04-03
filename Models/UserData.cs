using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubgMod.Models
{
    public class UserData
    {
        public string Email { get; set; }
        public string Hwid { get; set; }
        public string Userkey { get; set; }
        public bool IsSubscriber { get; set; }
        public string SubEndDate { get; set; }
    }
}
