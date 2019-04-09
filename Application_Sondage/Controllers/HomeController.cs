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
            return View(DataAccess.RecupereReponseEtNombreVoteBDD(id));
        }

        public ActionResult Desactiver(int id)
        {
            return View(id);
        }

        public ActionResult ConfirmationDesactivation(int id)
        {
            return View(id);
        }

        public ActionResult DesactivationSondage(int id)
        {
            DataAccess.DesactiverUnSondageEnBDD(id);
            return RedirectToAction(nameof(ConfirmationDesactivation), new { ID = id });
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
                return RedirectToAction(nameof(ErreurQuestionSondage));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Erreur veuillez entrez une question.");
            }

            if (ReponseNonNul.Count < 2)
            {
                return RedirectToAction(nameof(ErreurReponseSondage));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Erreur veuillez entrez au moins 2 réponses.");
            }

            int id = DataAccess.AjouterUnSondageEnBDD(question, choixMultiple.GetValueOrDefault(false), ReponseNonNul);
            return RedirectToAction(nameof(Liens), new { ID = id });
        }

        //[AFF] - Page création de sondage
        public ActionResult New()
        {
            return View();
        }

        //[AFF] - Page d'erreur sondage Question
        public ActionResult ErreurQuestionSondage()
        {
            return View();
        }

        //[AFF] - Page d'erreur sondage Reponse
        public ActionResult ErreurReponseSondage()
        {
            return View();
        }

        //[AFF] - Page de retour d'erreur de vote
        public ActionResult ErreurDesactiver(int id)
        {
            return View(id);
        }

        //[AFF] - Page de vote
        public ActionResult Vote(int id)
        {
            //Si le sondage est désactiver 
            if (DataAccess.CheckEtatSondageEnBDD(id) == true)
            {
                //Renvoie la page ErreurDesactivé
                return RedirectToAction(nameof(ErreurDesactiver), new { ID = id });
            }
            //Sinon
            else
            {
                //Recupère le sondage en fonction de l'id
                return View(DataAccess.RecupereSondageEnBDD(id));
            }
        }

        //[BDD] - Ajoute des votes
        public ActionResult ConfirmeVote(int id, List<string> multiplechoise)
        {
            //Ajoute un votant dans la base de données dans la table sondage
            DataAccess.AjouteUnVotantAuSondage(id);

            //Pour chaque réponse cocher, incrémente de 1 le nombre de vote dans la table Choix
            foreach(var NomChoix in multiplechoise)
            {
                DataAccess.AjouteUnVoteEnBDD(id, NomChoix);
            }
  
            //Retourne automatiquement vers la page de Confirmation de vote.
            return RedirectToAction(nameof(ConfirmationVote), new { ID = id });
        }

        //[AFF] - Page de confirmation de vote
        public ActionResult ConfirmationVote(int id)
        {
            return View(id);
        }

        #region Pages Optionnel
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