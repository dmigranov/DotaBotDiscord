using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotaBotDiscord
{
    public class Heroes
    {
        private List<Hero> _heroes;
        private Dictionary<int, Hero> heroesMap = null;
        public List<Hero> heroes { 
            get { return _heroes; }
            set { 
                _heroes = value;
            }
        }

        public Hero GetHero(int id)
        {
            if(heroesMap == null)
                heroesMap = _heroes.ToDictionary(x => x.id, x => x);
            return heroesMap[id];
        }
    }

    public class Hero
    {
        public string name { get; set; }
        public int id { get; set; }
        public string localized_name { get; set; }
    }
}
