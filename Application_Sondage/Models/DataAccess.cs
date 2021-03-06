﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

namespace Application_Sondage.Models
{
    #region Renvoie une Exception sondage non trouvée
    [Serializable]
    public class SondageNonTrouveException : Exception
    {
        public SondageNonTrouveException() { }
        public SondageNonTrouveException(string message) : base(message) { }
        public SondageNonTrouveException(string message, Exception inner) : base(message, inner) { }
        protected SondageNonTrouveException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    #endregion

    public class DataAccess
    {
        //CHEMIN BASE DE DONNEES
        public const string ChaineConnexionBDD = @"Server=.\SQLEXPRESS;Database=SondOpen;Integrated Security = true";

        //REQUETE QUI RECUPERE UN SONDAGE
        #region [BDD] - Recupère un sondage existant
        /// <summary>
        /// Récupère un sondage existant, 
        /// si l'ID n'existe pas en BDD retourne une Exception.
        /// Sinon ajoute dans une liste les sondages trouvés.
        /// </summary>
        public static Sondage RecupereSondageEnBDD(int Id)
        {
            List<Choix> ReponsesCourante = new List<Choix>();
            string Question = string.Empty;
            bool ChoixMultiple;
            int NbVotes;

            using (SqlConnection connection = new SqlConnection(ChaineConnexionBDD))
            {
                connection.Open();

                //Récupère la question et le type de choix
                SqlCommand command = new SqlCommand("SELECT Question, ChoixMultiple, NbVotes FROM Sondage WHERE IdSondage =@id", connection);
                command.Parameters.AddWithValue("@id", Id);
                using (SqlDataReader DataReader = command.ExecuteReader())
                {
                    if (DataReader.Read())
                    {
                        Question = (string)DataReader["Question"];
                        ChoixMultiple = (bool)DataReader["ChoixMultiple"];
                        NbVotes = (int)DataReader["NbVotes"];

                    }
                    else
                    {
                        throw new SondageNonTrouveException();
                    }
                }

                //Prend les réponses trouvées et les insère dans la liste ReponseCourante
                SqlCommand command2 = new SqlCommand("SELECT IdChoix, IntituleChoix FROM Choix WHERE FK_Id_Sondage =@id", connection);
                command2.Parameters.AddWithValue("@id", Id);
                using (SqlDataReader DataReader2 = command2.ExecuteReader())
                {
                    while (DataReader2.Read())
                    {
                        int IdChoix = (int)DataReader2["IdChoix"];
                        string valeurChoix = (string)DataReader2["IntituleChoix"];

                        Choix Reponse = new Choix(IdChoix, valeurChoix);

                        ReponsesCourante.Add(Reponse);
                    }
                }
            }

            Sondage RecupSonsage = new Sondage(Id, Question, ReponsesCourante, ChoixMultiple, NbVotes);

            return RecupSonsage;
        }
        #endregion

        //AJOUTE UN SONDAGE A LA BASE DE DONNEES
        #region [BDD] - Ajouter un sondage
        /// <summary>
        /// Ajoute un sondage à la base de données.
        /// Retourne l'id du sondage ajouté.
        /// </summary>
        public static int AjouterUnSondageEnBDD(string Question, bool ChoixMultiple, List<string> IntituleChoix, int cleUnique)
        {
            int recupId;

            using (SqlConnection connection = new SqlConnection(ChaineConnexionBDD))
            {
                connection.Open();
                //Transaction permet de lier les requêtes. Si les deux s'éxecutent rien ne se passe. Mais si l'une des deux ne s'éxecute pas, aucune ne va intéragir sur la base de données.
                var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

                //Ajoute le choix multiple et la question dans la table sondage et stocke l'id
                using (SqlCommand command = new SqlCommand("INSERT INTO Sondage (ChoixMultiple,Question,NbVotes,CleUnique) OUTPUT INSERTED.IdSondage VALUES(@Choix, @Question, 0 , @CleUnique)", connection, transaction))
                {
                    command.Parameters.AddWithValue("@Choix", ChoixMultiple);
                    command.Parameters.AddWithValue("@Question", Question);
                    command.Parameters.AddWithValue("@CleUnique", cleUnique);

                    //Récupère l'id de l'insertion
                    recupId = (int)command.ExecuteScalar();
                }

                foreach (var uneReponse in IntituleChoix)
                {
                    //Ajoute les choix du sondage et insert l'id récuperé précédemment.
                    using (SqlCommand command = new SqlCommand("INSERT INTO Choix (IntituleChoix,FK_Id_Sondage,NbVotes) VALUES(@IntituleChoix, @IdSondage,0)", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@IntituleChoix", uneReponse);
                        command.Parameters.AddWithValue("@IdSondage", recupId);
                        command.ExecuteNonQuery();
                    }
                }

                //Envoie les données de la requête
                transaction.Commit();

                //Retourne l'ID stocké
                return recupId;
            }
        }
        #endregion

        //DESACTIVER UN SONDAGE
        #region [BDD]- Désactiver un sondage
        /// <summary>
        /// Désactive un sondage en fonction d'un ID
        /// </summary>
        public static void DesactiverUnSondageEnBDD(int Id, int CleUnique)
        {
            SqlConnection connection = new SqlConnection(ChaineConnexionBDD);
            connection.Open();

            //Désactive le sondage si le clé unique et l'id correspondent.
            SqlCommand command = new SqlCommand("UPDATE Sondage SET Etat = 1  WHERE IdSondage = @IdSondage AND CleUnique = @CleUnique ", connection);
            command.Parameters.AddWithValue("@IdSondage", Id);
            command.Parameters.AddWithValue("@CleUnique", CleUnique);
            SqlDataReader DataReader = command.ExecuteReader();
        }
        #endregion

        //CHECK SI LA CLE EST BIEN LA BONNE PAR RAPPORT A L'ID SONDAGE
        #region [BDD] - Regarde si la clé correspond à l'id 
        public static bool CheckSiCleUniqueEstCorrect(int id, int cleUnique)
        {
            SqlConnection connection = new SqlConnection(ChaineConnexionBDD);
            connection.Open();

            //Désactive le sondage si le clé unique et l'id correspondent.
            SqlCommand command = new SqlCommand("SELECT IdSondage, CleUnique FROM Sondage WHERE IdSondage = @IdSondage AND CleUnique = @CleUnique ", connection);
            command.Parameters.AddWithValue("@IdSondage", id);
            command.Parameters.AddWithValue("@CleUnique", cleUnique);
            SqlDataReader DataReader = command.ExecuteReader();

            if (DataReader.Read())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        //CHECK ETAT SONDAGE
        #region [BDD] Check l'état du sondage
        /// <summary>
        /// Regarde l'état du sondage en fonction de l'ID.
        /// Si le sondage est désactivé renvoie une Exception. 
        /// Sinon stocke l'état et le renvoie.
        /// </summary>
        public static bool CheckEtatSondageEnBDD(int Id)
        {
            bool recupEtat;
            SqlConnection connection = new SqlConnection(ChaineConnexionBDD);
            connection.Open();
            SqlCommand command = new SqlCommand("Select Etat FROM Sondage WHERE IdSondage = @IdSondage ", connection);

            command.Parameters.AddWithValue("@IdSondage", Id);
            SqlDataReader DataReader = command.ExecuteReader();

            //Si l'état est trouvé on le stocke
            if (DataReader.Read())
            {
                recupEtat = DataReader.GetBoolean(0);
            }
            //Sinon on renvoie une exception
            else
            {
                throw new SondageNonTrouveException();
            }
            return recupEtat;
        }
        #endregion

        //AJOUTE UN VOTANT SUR LE SONDAGE (SONDAGE)
        #region [BDD] - Ajoute un votant dans un sondage
        /// <summary>
        /// Regarde si l'ID existe dans la classe Sondage.
        /// S'il n'existe pas renvoie une exception.
        /// Sinon incrémente de 1 le nombre de votant(s) dans la classe sondage.
        /// </summary>
        public static void AjouteUnVotantAuSondage(int IdSondage)
        {

            SqlConnection connection = new SqlConnection(ChaineConnexionBDD);
            connection.Open();
            //Check si l'ID existe dans la base de données
            using (SqlCommand command = new SqlCommand("SELECT IdSondage FROM Sondage WHERE IdSondage = @IdSondage ", connection))
            {

                command.Parameters.AddWithValue("@IdSondage", IdSondage);
                using (SqlDataReader DataReader = command.ExecuteReader())
                {
                    if (!DataReader.Read())
                    {
                        throw new SondageNonTrouveException();
                    }
                }
                //Ajoute un votant au nombre de votant(s) dans la base de données
                using (SqlCommand command2 = new SqlCommand("UPDATE Sondage SET NbVotes = Nbvotes+1 WHERE IdSondage = @IdSondage ", connection))
                {
                    command2.Parameters.AddWithValue("@IdSondage", IdSondage);
                    command2.ExecuteNonQuery();
                }
            }
        }
        #endregion

        //AJOUTE UN VOTE SUR LE SONDAGE (CHOIX)
        #region [BDD] - Ajoute un vote dans un choix
        /// <summary>
        /// Regarde si l'ID existe dans la classe Sondage.
        /// S'il n'existe pas renvoie une exception.
        /// Sinon incrémente de 1 le nombre de votes de la réponse dans la classe choix.
        /// </summary>
        public static void AjouteUnVoteEnBDD(int IdSondage, string NomChoix)
        {
            SqlConnection connection = new SqlConnection(ChaineConnexionBDD);
            connection.Open();

            //Check si l'ID existe dans la base de données
            using (SqlCommand command = new SqlCommand("SELECT IdSondage FROM Sondage WHERE IdSondage = @IdSondage ", connection))
            {

                command.Parameters.AddWithValue("@IdSondage", IdSondage);
                using (SqlDataReader DataReader = command.ExecuteReader())
                {
                    if (!DataReader.Read())
                    {
                        throw new SondageNonTrouveException();
                    }
                }
            }
            using (SqlCommand command1 = new SqlCommand("UPDATE Choix SET NbVotes = Nbvotes+1 WHERE FK_Id_Sondage = @IdSondage AND IntituleChoix = @NomChoix ", connection))
            {
                command1.Parameters.AddWithValue("@NomChoix", NomChoix);
                command1.Parameters.AddWithValue("@IdSondage", IdSondage);
                command1.ExecuteNonQuery();
            }
        }
        #endregion

        //RECHERCHE LES REPONSES ET NOMBRE DE VOTES PAR REPONSE 
        #region [BDD] - Ajoute et tri les réponse du sondage par ordre alphabétique
        /// <summary>
        /// Récupère la question, si c'est un choix multiple ou non et le nombre de votants du sondage.
        /// Puis cherche les réponses et l'id de la réponse en les triant par ordre décroissant et en les insérant dans une liste.
        /// Puis renvoie le sondage.
        /// </summary>
        public static Sondage RecupereReponseEtNombreVoteBDD(int Id)
        {
            List<Choix> ReponsesCourante = new List<Choix>();
            string Question = string.Empty;
            bool ChoixMultiple;
            int NbVotes;

            using (SqlConnection connection = new SqlConnection(ChaineConnexionBDD))
            {
                connection.Open();

                //Récupère la question et le type de choix et le nombre de votes.
                SqlCommand command = new SqlCommand("SELECT Question, ChoixMultiple, Nbvotes FROM Sondage WHERE IdSondage =@id", connection);
                command.Parameters.AddWithValue("@id", Id);
                using (SqlDataReader DataReader = command.ExecuteReader())
                {
                    if (DataReader.Read())
                    {
                        Question = (string)DataReader["Question"];
                        ChoixMultiple = (bool)DataReader["ChoixMultiple"];
                        NbVotes = (int)DataReader["NbVotes"];
                    }
                    else
                    {
                        throw new SondageNonTrouveException();
                    }
                }

                //Récupère l'ID, le nom et le nombre de votes de la réponse en les insèrant dans une liste et en les classant par ordre décroissant.
                SqlCommand command2 = new SqlCommand("SELECT IdChoix, IntituleChoix, NbVotes FROM Choix WHERE FK_Id_Sondage =@id ORDER BY NbVotes DESC", connection);
                command2.Parameters.AddWithValue("@id", Id);
                using (SqlDataReader DataReader2 = command2.ExecuteReader())
                {
                    while (DataReader2.Read())
                    {
                        int IdChoix = (int)DataReader2["IdChoix"];
                        string valeurChoix = (string)DataReader2["IntituleChoix"];
                        int NombreVotes = (int)DataReader2["NbVotes"];

                        Choix Reponse = new Choix(IdChoix, valeurChoix, NombreVotes);

                        ReponsesCourante.Add(Reponse);
                    }
                }
            }
            Sondage RecupSonsage = new Sondage(Id, Question, ReponsesCourante, ChoixMultiple,NbVotes);

            return RecupSonsage;
        }
        #endregion

        //FONCTION AUTRE
        #region Génère un nombre aléatoire
        /// <summary>
        /// Génère un nombre aléatoire entre 1 et 1 000 000 000 puis le renvoie.
        /// </summary>
        public static int GenereCleUnique()
        {
            Random nombre = new Random();
            int nombreAleatoire = nombre.Next(1000000000);

            return nombreAleatoire;
        }
        #endregion

        #region [BDD] Test de connexion
        /// <summary>
        /// Fonction servant à tester si la base de données fonctionne
        /// </summary>
        public static bool TestDeConnexionBaseDeDonnees(string CheminBDD)
        {
            bool resultat = false;
            try
            {
                using (SqlConnection db = new SqlConnection(CheminBDD))
                {
                    db.Open();
                    resultat = true;
                }
                return resultat;
            }
            catch (Exception)
            {
                return resultat;
            }
        }
        #endregion
    }
}