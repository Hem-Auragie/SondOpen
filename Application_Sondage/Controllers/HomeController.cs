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
        //[AFF] - Page d'accueil
        public ActionResult Index()
        {
            return View();
        }

        //[AFF] - Affiche la page résultat
        public ActionResult Resultat(int id)
        {
            return View(DataAccess.RecupereReponseEtNombreVoteBDD(id));
        }

        //ActionResult lié au désactivation d'un sondage
        #region Desactivation sondage

        //[AFF] - Page principal désactivation sondage
        public ActionResult Desactiver(int id, int cle)
        {
            //Regarde si le clé et l'id sont sur la même ligne dans la BDD 
            if (DataAccess.CheckSiCleUniqueEstCorrect(id, cle) == true)
            {
                Sondage fusion = new Sondage(id, cle);
                return View(fusion);
            }
            else
            {
                //Si pas cohérent retourne la page Erreur404
                return RedirectToAction(nameof(Erreur404), new { ID = id });
            }
        }

        //[BDD] - Désactive le sondage à l'aide de l'id et la clé du sondage
        public ActionResult DesactivationSondage(int id, int cle)
        {
            DataAccess.DesactiverUnSondageEnBDD(id, cle);
            return RedirectToAction(nameof(ConfirmationDesactivation), new { id = id });
        }

        //[AFF] - Page de confirmation de désactivation
        public ActionResult ConfirmationDesactivation(int id)
        {
            return View(id);
        }
        #endregion

        //[AFF] - Affiche la page de lien avec l'id et la clé correspondante
        public ActionResult Liens(int id, int cle)
        {
            Sondage fusion = new Sondage(id, cle);
            return View(fusion);
        }

        #region Création de sondage
        //[BDD] - Ajoute un sondage 
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
            }

            List<string> ListeSansDoublons = ReponseNonNul.Distinct().ToList();

            if (ListeSansDoublons.Count < 2)
            {
                return RedirectToAction(nameof(ErreurReponseSondage));
            }

            int cleUnique = DataAccess.GenereCleUnique();
            int id = DataAccess.AjouterUnSondageEnBDD(question, choixMultiple.GetValueOrDefault(false), ListeSansDoublons, cleUnique);
            return RedirectToAction(nameof(Liens), new { id = id, cle = cleUnique });
        }

        //[AFF] - Page création de sondage
        public ActionResult New()
        {
            return View();
        }
        #endregion

        #region Gestion des erreurs
        //[AFF] - Page d'erreeur Erreur404
        public ActionResult Erreur404(int id)
        {
            return View(id);
        }

        public ActionResult ErreurDejaVote(int id)
        {
            return View(id);
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
        #endregion

        #region Gestion des votes
        //[AFF] - Page de vote
        public ActionResult Vote(int id)
        {
            //Si le sondage est désactiver 
            if (DataAccess.CheckEtatSondageEnBDD(id) == true)
            {
                //Renvoie la page ErreurDesactivé
                return RedirectToAction(nameof(ErreurDesactiver), new { id = id });
            }
            //Vérifie si la personne a déjà voter
            if (VerifieSiDejaVoterCookie(Request.Cookies, id))
            {
                return RedirectToAction("ErreurDejaVote", new { id = id });
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
            EnregistreUnVotantCookie(id);
            //Ajoute un votant dans la base de données dans la table sondage
            DataAccess.AjouteUnVotantAuSondage(id);

            //Pour chaque réponse cocher, incrémente de 1 le nombre de vote dans la table Choix
            foreach (var NomChoix in multiplechoise)
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
        #endregion

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

        #region Enregistre un coockie avec l'id du sondage
        public void EnregistreUnVotantCookie(int id)
        {
            string Votant = Request.UserHostAddress;
            HttpCookie cookie = new HttpCookie("CookieUser" + id);
            cookie.Value = "";
            cookie.Expires = DateTime.MaxValue;
            this.Response.Cookies.Add(cookie);
        }
        #endregion

        #region Regarde si la personne a déjà voter
        public static bool VerifieSiDejaVoterCookie(HttpCookieCollection cookies, int id)
        {
            return cookies["cookieUser" + id] != null;
        }
        #endregion
    }
}