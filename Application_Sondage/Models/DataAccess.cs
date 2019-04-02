using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

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
        //CHEMIN BASE DE DONNES
        const string ChaineConnexionBDD = @"Server=.\SQLEXPRESS;Database=SondOpen;Integrated Security = true";

        //REQUETE QUI RECUPERE LA QUESTION CORRESPONDANT A L'ID DONNER
        #region [BDD] - Recupère un sondage existant
        public static Sondage RecupereSondageEnBDD(int Id)
        {
            List<string> ReponseCourante = new List<string>();
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
                        
                SqlCommand command2 = new SqlCommand("SELECT IntituleChoix FROM Choix WHERE FK_Id_Sondage =@id", connection);
                command2.Parameters.AddWithValue("@id", Id);
                using (SqlDataReader DataReader2 = command2.ExecuteReader())
                {
                    while (DataReader2.Read())
                    {
                        string Reponse = (string)DataReader2["IntituleChoix"];

                        ReponseCourante.Add(Reponse);
                    }
                }                    
            }

            Sondage RecupSonsage = new Sondage(Id,Question, ReponseCourante, ChoixMultiple);

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
                var transaction = connection.BeginTransaction();
                //Ajoute le choix multiples et la question  dans la table sondage et stock l'id
                using (SqlCommand command = new SqlCommand("INSERT INTO Sondage (ChoixMultiple,Question,NbVotes) OUTPUT INSERTED.IdSondage VALUES(@Choix, @Question,0)", connection, transaction))
                {
                    command.Parameters.AddWithValue("@Choix", ChoixMultiple);
                    command.Parameters.AddWithValue("@Question", Question);

                    //Récupère l'id de l'insertion
                    recupId = (int)command.ExecuteScalar();
                }

                foreach(var uneReponse in IntituleChoix)
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
    }
}