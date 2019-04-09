using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using Application_Sondage.Models;
using System.Net;

namespace Application_Sondage.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Resultat(int id)
        {
            return View(id);
        }

        public ActionResult Desactiver(int id)
        {
            DataAccess.DesactiverUnSondageEnBDD(id);
            return View();
        }

        //Mets aux liens l'id demander
        public ActionResult Liens(int id)
        {
            return View(id);
        }

        //Crée et stock le sondage en BDD
        public ActionResult PosteNew(string question, string reponseUn, string reponseDeux, string reponseTrois, string reponseQuatre, bool? choixMultiple)
        {
            List<string> ListeDeReponse = new List<string>();
            ListeDeReponse.Add(reponseUn);
            ListeDeReponse.Add(reponseDeux);
            ListeDeReponse.Add(reponseTrois);
            ListeDeReponse.Add(reponseQuatre);

            List<string> ReponseNonNul = new List<string>();
            foreach (var reponse in ListeDeReponse)
            {
                bool laReponseEstVide = string.IsNullOrWhiteSpace(reponse);
                if (!laReponseEstVide)
                {
                    ReponseNonNul.Add(reponse);
                }
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Erreur veuillez entrez une question.");
            }

            if (ReponseNonNul.Count < 2)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Erreur veuillez entrez au moins 2 réponses.");
            }

            int id = DataAccess.AjouterUnSondageEnBDD(question, choixMultiple.GetValueOrDefault(false), ReponseNonNul);
            return RedirectToAction(nameof(Liens), new { ID = id });
        }

        //Page d'accueil de création de sondage
        public ActionResult New()
        {
            return View();
        }

        //Page de vote
        public ActionResult Vote(int id)
        {
            if (DataAccess.CheckEtatSondageEnBDD(id) == true)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Le sondage est désactiver, vous pouvez uniquement accéder aux résultats.");
            }
            else
            {
                //Recupère le sondage en fonction de l'id
                return View(DataAccess.RecupereSondageEnBDD(id));
            }
        }

        public ActionResult ConfirmeVote(int id)
        {


           // DataAccess.AjouteUnOuPLusieursVotesEnBDD(id, IdChoix);
            return RedirectToAction(nameof(Vote), new { ID = id });
        }

        //FONCTIONNE
        #region Page Optionnel
        public ActionResult Manuel()
        {
            return View();
        }

        public ActionResult Confidentialite()
        {
            return View();
        }

        public ActionResult APropos()
        {
            //Affiche la page Propos
            return View();
        }
        #endregion
    }
}