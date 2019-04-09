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
        public List<Choix> ListeDeChoix { get; private set; }
        public bool ChoixMultiple { get; private set; }

        public Sondage(int id, string question, List<Choix> listeDeChoix, bool choixMultiple)
        {
            Id = id;
            Question = question;
            ListeDeChoix = listeDeChoix;
            ChoixMultiple = choixMultiple;
        }
    }
}