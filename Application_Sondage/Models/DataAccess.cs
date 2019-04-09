using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

namespace Application_Sondage.Models
{
    #region Renvoi une Exception sondage non trouver
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
        const string ChaineConnexionBDD = @"Server=.\SQLEXPRESS;Database=SondOpen;Integrated Security = true";

        //REQUETE QUI RECUPERE UN SONDAGE [OK]
        #region [BDD] - Recupère un sondage existant
        public static Sondage RecupereSondageEnBDD(int Id)
        {
            List<Choix> ReponsesCourante = new List<Choix>();
            string Question = string.Empty;
            bool ChoixMultiple;

            using (SqlConnection connection = new SqlConnection(ChaineConnexionBDD))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("SELECT Question, ChoixMultiple FROM Sondage WHERE IdSondage =@id", connection);
                command.Parameters.AddWithValue("@id", Id);
                using (SqlDataReader DataReader = command.ExecuteReader())
                {
                    if (DataReader.Read())
                    {
                        Question = (string)DataReader["Question"];
                        ChoixMultiple = (bool)DataReader["ChoixMultiple"];
                    }
                    else
                    {
                        throw new SondageNonTrouveException();
                    }
                }

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

            Sondage RecupSonsage = new Sondage(Id, Question, ReponsesCourante, ChoixMultiple);

            return RecupSonsage;
        }
        #endregion

        //AJOUTE UN SONDAGE A LA BASE DE DONNEES [OK]
        #region [BDD] - Ajouter un sondage
        /// <summary>
        /// Ajoute un sondage à la base de données.
        /// Retourne l'id du sondage ajouté.
        /// </summary>
        public static int AjouterUnSondageEnBDD(string Question, bool ChoixMultiple, List<string> IntituleChoix)
        {
            int recupId;

            using (SqlConnection connection = new SqlConnection(ChaineConnexionBDD))
            {
                connection.Open();
                var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                //Ajoute le choix multiples et la question  dans la table sondage et stock l'id
                using (SqlCommand command = new SqlCommand("INSERT INTO Sondage (ChoixMultiple,Question,NbVotes) OUTPUT INSERTED.IdSondage VALUES(@Choix, @Question,0)", connection, transaction))
                {
                    command.Parameters.AddWithValue("@Choix", ChoixMultiple);
                    command.Parameters.AddWithValue("@Question", Question);

                    //Récupère l'id de l'insertion
                    recupId = (int)command.ExecuteScalar();
                }

                foreach (var uneReponse in IntituleChoix)
                {
                    //Ajoute les choix du sondage et insert l'id récuperer précedement.
                    using (SqlCommand command = new SqlCommand("INSERT INTO Choix (IntituleChoix,FK_Id_Sondage,NbVotes) VALUES(@IntituleChoix, @IdSondage,0)", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@IntituleChoix", uneReponse);
                        command.Parameters.AddWithValue("@IdSondage", recupId);
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return recupId;
            }
        }
        #endregion

        //DESACTIVER UN SONDAGE [OK]
        #region Désactiver un sondage
        public static void DesactiverUnSondageEnBDD(int Id)
        {
            SqlConnection connection = new SqlConnection(ChaineConnexionBDD);
            connection.Open();
            SqlCommand command = new SqlCommand("UPDATE Sondage SET Etat = 1  WHERE IdSondage = @IdSondage ", connection);
            command.Parameters.AddWithValue("@IdSondage", Id);
            SqlDataReader DataReader = command.ExecuteReader();
        }
        #endregion

        //CHECK ETAT SONDAGE [OK]
        #region Check l'état du sondage
        public static bool CheckEtatSondageEnBDD(int Id)
        {
            bool recupEtat;
            SqlConnection connection = new SqlConnection(ChaineConnexionBDD);
            connection.Open();
            SqlCommand command = new SqlCommand("Select Etat FROM Sondage WHERE IdSondage = @IdSondage ", connection);

            command.Parameters.AddWithValue("@IdSondage", Id);
            SqlDataReader DataReader = command.ExecuteReader();

            if (DataReader.Read())
            {
                recupEtat = DataReader.GetBoolean(0);
            }
            else
            {
                throw new SondageNonTrouveException();
            }
            return recupEtat;
        }
        #endregion

        //AJOUTE UN VOTANT SUR LE SONDAGE (SONDAGE) [OK]
        #region Ajoute un votant dans un sondage
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
                //Ajoute un votant au nombre de votant dans la base de données
                using (SqlCommand command2 = new SqlCommand("UPDATE Sondage SET NbVotes = Nbvotes+1 WHERE IdSondage = @IdSondage ", connection))
                {
                    command2.Parameters.AddWithValue("@IdSondage", IdSondage);
                    command2.ExecuteNonQuery();
                }
            }
        }
        #endregion

        //AJOUTE UN VOTE SUR LE SONDAGE (CHOIX) [OK]
        #region Ajoute un vote dans un choix
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
    }
}