using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDex.Models
{
    public class AnimeCharacter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }  // Chemin vers l’image locale
        public int Strength { get; set; }
        public int Intelligence { get; set; }
    }
}
