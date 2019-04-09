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
        public bool EtatSondage { get; private set; }
        public  int NombreVoteTotal { get; private set; }

        public Sondage(int id, string question, List<Choix> listeDeChoix, bool choixMultiple)
        {
            Id = id;
            Question = question;
            ListeDeChoix = listeDeChoix;
            ChoixMultiple = choixMultiple;
        }

        public Sondage(int id, string question, List<Choix> listeDeChoix, bool choixMultiple, int nombreVoteTotal) : this(id, question, listeDeChoix, choixMultiple)
        {
            NombreVoteTotal = nombreVoteTotal;
        }

        public int PourcentageDeVote(int NombreDeVoteChoix)
        {
            int PourcentageVote;
            if (NombreVoteTotal > 0)
            {
                return PourcentageVote = NombreDeVoteChoix * 100 / NombreVoteTotal;
            }
            else
            {
                return PourcentageVote = 0;
            }
        }


    }
}