using System;
using System.Collections.Generic;
using System.Text;

namespace DotaBotDiscord
{
    class Heroes
    {
        public List<Hero> heroes { get; set; }
    }

    class Hero
    {
        public string name { get; set; }
        public int id { get; set; }
        public string localized_name { get; set; }
    }
}
