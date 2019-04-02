namespace Application_Sondage.Models
{
    public class Choix_vote
    {
        public string Nom { get; }
        public int Id { get; }
        public Choix_vote(string nom, int id)
        {
            Nom = nom;
            Id = id;
        }
    }
}
