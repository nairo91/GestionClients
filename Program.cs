using System;
using System.Collections.Generic;
using System.IO;

class ProgrammeClients
{
   
    class Client
    {
        public string Nom;    
        public string Prenom; 

        public override string ToString()
        {
            return Nom + ";" + Prenom;
        }

        public static Client DepuisLigne(string ligne)
        {
            //Pour avoir NOM;Prénom
            string[] morceaux = ligne.Split(';');
            Client c = new Client();
            if (morceaux.Length >= 2)
            {
                c.Nom = morceaux[0];
                c.Prenom = morceaux[1];
            }
            else
            {
                // Si pb
                c.Nom = "INCONNU";
                c.Prenom = "Inconnu";
            }
            return c;
        }
    }

    
    static string cheminFichier = "clients.txt";
    static List<Client> listeClients = new List<Client>();

    // Conversion
    static string Majuscule(string chaine)
    {
        if (string.IsNullOrEmpty(chaine)) return string.Empty;
        return chaine.ToUpper();
    }

    static string FirstMajuscule(string chaine)
    {
        if (string.IsNullOrEmpty(chaine)) return string.Empty;
        string minuscule = chaine.ToLower();
        if (minuscule.Length == 1) return minuscule.ToUpper();
        return char.ToUpper(minuscule[0]) + minuscule.Substring(1);
    }

    // Tous Read et Write de fichier 
    static void ChargerDepuisFichier()
    {
        listeClients.Clear();
        if (!File.Exists(cheminFichier))
        {
            File.WriteAllText(cheminFichier, "");
            return;
        }

        string[] lignes = File.ReadAllLines(cheminFichier);
        for (int i = 0; i < lignes.Length; i++)
        {
            string li = lignes[i].Trim();
            if (li.Length == 0) continue;
            listeClients.Add(Client.DepuisLigne(li));
        }
    }

    static void SauvegarderDansFichier()
    {
        List<string> lignes = new List<string>();
        foreach (Client c in listeClients)
        {
            lignes.Add(c.ToString());
        }
        File.WriteAllLines(cheminFichier, lignes);
    }


    static void SaisirNouveauClient()
    {
        Console.Write("Nom du client : ");
        string nomSaisi = Console.ReadLine();
        Console.Write("Prénom du client : ");
        string prenomSaisi = Console.ReadLine();

        Client c = new Client();
        c.Nom = Majuscule(nomSaisi);
        c.Prenom = FirstMajuscule(prenomSaisi);

        listeClients.Add(c);
        SauvegarderDansFichier();
        Console.WriteLine(">> Nouveau client enregistré.");
    }


    static void AfficherClientParNom()
    {
        Console.Write("Entrez le NOM à rechercher (majuscule ou minuscule accepté) : ");
        string nomRecherche = Console.ReadLine();
        string cle = Majuscule(nomRecherche);

        bool auMoinsUn = false;
        for (int i = 0; i < listeClients.Count; i++)
        {
            if (listeClients[i].Nom == cle)
            {
                Console.WriteLine("Fiche #" + i + " -> NOM: " + listeClients[i].Nom + " | Prénom: " + listeClients[i].Prenom);
                auMoinsUn = true;
            }
        }
        if (!auMoinsUn)
        {
            Console.WriteLine("Aucun client trouvé pour ce nom.");
        }
    }

    
    static void AfficherTous()
    {
        if (listeClients.Count == 0)
        {
            Console.WriteLine("Aucune fiche pour le moment.");
            return;
        }

        for (int i = 0; i < listeClients.Count; i++)
        {
            Console.WriteLine("Fiche #" + i + " -> NOM: " + listeClients[i].Nom + " | Prénom: " + listeClients[i].Prenom);
        }
    }

    //4
    static void AfficherNombre()
    {
        Console.WriteLine("Nombre de clients : " + listeClients.Count);
    }

    // 5
    static void ModifierClient()
    {
        Console.Write("Entrez le numéro de la fiche à modifier (position, commence à 0) : ");
        string saisie = Console.ReadLine();
        int num;
        if (!int.TryParse(saisie, out num))
        {
            Console.WriteLine("Numéro incorrect.");
            return;
        }
        if (num < 0 || num >= listeClients.Count)
        {
            Console.WriteLine("Cette fiche n'existe pas.");
            return;
        }

        Client courant = listeClients[num];
        Console.WriteLine("Fiche #" + num + " actuelle : NOM: " + courant.Nom + " | Prénom: " + courant.Prenom);

        Console.Write("Nouveau NOM (Entrée = conserver) : ");
        string nouvNom = Console.ReadLine();
        if (!string.IsNullOrEmpty(nouvNom))
        {
            courant.Nom = Majuscule(nouvNom);
        }

        Console.Write("Nouveau Prénom (Entrée = conserver) : ");
        string nouvPrenom = Console.ReadLine();
        if (!string.IsNullOrEmpty(nouvPrenom))
        {
            courant.Prenom = FirstMajuscule(nouvPrenom);
        }

        Console.Write("Confirmer l'enregistrement ? (o/n) : ");
        string rep = Console.ReadLine();
        if (rep == "o" || rep == "O")
        {
            listeClients[num] = courant;
            SauvegarderDansFichier();
            Console.WriteLine(">> Fiche modifiée.");
        }
        else
        {
            Console.WriteLine(">> Modification annulée.");
        }
    }

    
    static void AfficherMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== GESTION DES CLIENTS ===");
        Console.WriteLine("1 - Saisir un nouveau client");
        Console.WriteLine("2 - Afficher un client (par NOM)");
        Console.WriteLine("3 - Afficher tous les clients");
        Console.WriteLine("4 - Afficher le nombre de clients");
        Console.WriteLine("5 - Modifier un client (par numéro de fiche)");
        Console.WriteLine("0 - Quitter");
        Console.Write("Votre choix : ");
    }

    static void Main()
    {
        ChargerDepuisFichier();

        int choix = -1;
        while (choix != 0)
        {
            AfficherMenu();
            string lu = Console.ReadLine();
            int.TryParse(lu, out choix);

            switch (choix)
            {
                case 1:
                    SaisirNouveauClient();
                    break;
                case 2:
                    AfficherClientParNom();
                    break;
                case 3:
                    AfficherTous();
                    break;
                case 4:
                    AfficherNombre();
                    break;
                case 5:
                    ModifierClient();
                    break;
                case 0:
                    Console.WriteLine("Au revoir.");
                    break;
                default:
                    Console.WriteLine("Choix invalide.");
                    break;
            }
        }
    }
}
