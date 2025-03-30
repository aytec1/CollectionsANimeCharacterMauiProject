using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Model
{
    public class AnimeCharacter
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string SpecialAttack { get; set; } = string.Empty; 
        public string Sound { get; set; } = string.Empty;         
    }
}
