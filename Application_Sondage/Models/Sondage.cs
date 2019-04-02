using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Application_Sondage.Models
{
    public class Sondage
    {
        public int Id { get; private set; }
        public string Question { get; private set; }
        public List<string> IntituleChoix { get; private set; }
        public bool ChoixMultiple { get; private set; }

        public Sondage(int id, string question, List<string> intituleChoix, bool choixMultiple)
        {
            Id = id;
            Question = question;
            IntituleChoix = intituleChoix;
            ChoixMultiple = choixMultiple;
        }
    }
}