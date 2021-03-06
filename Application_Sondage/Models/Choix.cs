﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Application_Sondage.Models
{
    public class Choix
    {
        public int IdChoix { get; private set; }
        public string ValeurChoix { get; private set; }
        public int NombreVotes { get; private set; }

        public Choix(int idChoix, string valeurChoix)
        {
            IdChoix = idChoix;
            ValeurChoix = valeurChoix;
        }

        public Choix(int idChoix, string valeurChoix, int nombreVotes) : this(idChoix, valeurChoix)
        {
            NombreVotes = nombreVotes;
        }
    }
}