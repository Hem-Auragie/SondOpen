using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Application_Sondage.Models;

namespace TestUnitaireSondage
{
    [TestClass]
    public class UnitTest1
    {
        #region Test de connexion à la base de données
        //Test quand la connexion est OK
        [TestMethod]
        public void ConnexionBaseDeDonneesOK()
        {
            bool result = DataAccess.TestDeConnexionBaseDeDonnees(DataAccess.ChaineConnexionBDD);
            Assert.AreEqual(true, result);
        }

        //Test quand la connexion n'est pas bonne
        [TestMethod]
        public void ConnexionBaseDeDonneesPasOK()
        {
            bool result = DataAccess.TestDeConnexionBaseDeDonnees(" ");
            Assert.AreEqual(false, result);
        }
        #endregion
                
        #region Test regarde si clé unique correspond à l'id

        [TestMethod]
        public void RegardeSiCleUniqueEstOK()
        {
            bool result = DataAccess.CheckSiCleUniqueEstCorrect(27, 79306945);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void RegardeSiCleUniqueEstPasBon()
        {
            bool result = DataAccess.CheckSiCleUniqueEstCorrect(27, 795);
            Assert.AreEqual(false, result);
        }
        #endregion

        #region Regarde l'état du sondage
        //Test avec le sondage est activé
        [TestMethod]
        public void RegardeSiEtatEstOK()
        {
            bool result = DataAccess.CheckEtatSondageEnBDD(27);
            Assert.AreEqual(false, result);
        }

        //Test si le sondage est désactiver
        [TestMethod]
        public void RegardeSiEtatEstPasBon()
        {
            bool result = DataAccess.CheckEtatSondageEnBDD(26);
            Assert.AreEqual(true, result);
        }
        #endregion

    }
}
