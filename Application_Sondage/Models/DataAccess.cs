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
    public class SondageNonTrouverException : Exception
    {
        public SondageNonTrouverException() { }
        public SondageNonTrouverException(string message) : base(message) { }
        public SondageNonTrouverException(string message, Exception inner) : base(message, inner) { }
        protected SondageNonTrouverException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    #endregion

    public class DataAccess
    {
        //CHEMIN BASE DE DONNEES
        const string ChaineConnexionBDD = @"Server=.\SQLEXPRESS;Database=SondOpen;Integrated Security = true";

        //REQUETE QUI RECUPERE UN SONDAGE
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
                        throw new SondageNonTrouverException();
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

        //AJOUTE UN SONDAGE A LA BASE DE DONNES
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

        //DESACTIVER UN SONDAGE
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

        //CHECK ETAT SONDAGE
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
                throw new SondageNonTrouverException();
            }
            return recupEtat;
        }
        #endregion

        //FAIRE UN(DES) VOTE(S)
        #region Faire un ou plusieurs votes
        public static void AjouteUnOuPLusieursVotesEnBDD(int IdSondage, List<Choix> ListeDeChoix)
        {

            SqlConnection connection = new SqlConnection(ChaineConnexionBDD);
            connection.Open();

            //Regarde si l'ID existe
            using (SqlCommand command = new SqlCommand("SELECT IdSondage WHERE IdSondage = @IdSondage ", connection))
            {
                var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                command.Parameters.AddWithValue("@IdSondage", IdSondage);
                SqlDataReader DataReader = command.ExecuteReader();

                if (!DataReader.Read())
                {
                    throw new SondageNonTrouverException();
                }
                else
                {
                    //Ajoute un votant au nombre total de vote sur le sondage
                    using (SqlCommand command2 = new SqlCommand("UPDATE Sondage SET NbVotes = Nbvotes+1 WHERE IdSondage = @IdSondage ", connection, transaction))
                    {
                        command2.Parameters.AddWithValue("@IdSondage", IdSondage);
                        command2.ExecuteNonQuery();
                    }

                    //Ajoute un ou plusieurs vote sur le sondage
                    foreach (var IdChoixReponse in ListeDeChoix)
                    {
                        using (SqlCommand command3 = new SqlCommand("UPDATE Choix SET NbVotes = Nbvotes+1 WHERE FK_Id_Sondage = @IdSondage AND IdChoix = @IdChoix ", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdChoix", IdChoixReponse);
                            command3.Parameters.AddWithValue("@IdSondage", IdSondage);
                            command3.ExecuteNonQuery();
                        }
                    }
                }
                transaction.Commit();
            }
        }
        #endregion
    }
}