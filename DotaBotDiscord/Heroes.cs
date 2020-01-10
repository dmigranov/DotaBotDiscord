using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotaBotDiscord
{
    class Heroes
    {
        private List<Hero> _heroes;
        private Dictionary<int, Hero> heroesMap;
        public List<Hero> heroes { 
            get { return _heroes; }
            set { 
                _heroes = value;
                heroesMap = _heroes.ToDictionary(x => x.id, x => x);
            }
        }

        public Hero GetHero(int id)
        {
            return heroesMap[id];
        }
    }

    class Hero
    {
        public string name { get; set; }
        public int id { get; set; }
        public string localized_name { get; set; }
    }
}
