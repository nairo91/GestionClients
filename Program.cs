using System;
using System.IO;
using System.Text;

class ProgrammeClients
{

    static string cheminFichier = @"c:\users\orian\clients.dat";
    
    //je met des const comme demandé par le prof je choisi de faire pour taille nom prenom tel
    const int TAILLE_NOM = 30;          
    const int TAILLE_PRENOM = 30;
    const int TAILLE_TEL = 20;

    //ici on fait la taille totale de l'enregistrement de l'info 
    const int TAILLE_ENREG = 4 + TAILLE_NOM + TAILLE_PRENOM + TAILLE_TEL;

    //ma classe client pour le binaire
    public struct Client
    {
        public int Numero;       
        public string Nom;       
        public string Prenom;   
        public string Telephone;
    }

    //mes conversions
    static void Majuscule(ref string chaine)
    {
        if (string.IsNullOrEmpty(chaine)) { chaine = ""; return; }
        chaine = chaine.Trim().ToUpper();
    }
 
    static void FirstMajuscule(ref string chaine)
    {
        if (string.IsNullOrEmpty(chaine)) 
        { chaine = ""; return; }
        string m = chaine.Trim().ToLower();
        if (m.Length == 1) 
        { chaine = m.ToUpper(); return; }
        chaine = char.ToUpper(m[0]) + m.Substring(1);
    }

 
    static int CompterFiches()
    {
        if (!File.Exists(cheminFichier)) return 0;
        long octets = new FileInfo(cheminFichier).Length;
        return (int)(octets / TAILLE_ENREG);
    }

    static long OffsetDe(int index)
    {
        return index * (long)TAILLE_ENREG;
    }

    static void EcrireChaineFixe(BinaryWriter bw, string valeur, int taille)
    {
        if (valeur == null) valeur = "";
        if (valeur.Length > taille) valeur = valeur.Substring(0, taille);
        if (valeur.Length < taille) valeur = valeur.PadRight(taille, '\0');
        byte[] bytes = Encoding.ASCII.GetBytes(valeur);
        bw.Write(bytes, 0, taille);
    }

    static string LireChaineFixe(BinaryReader br, int taille)
    {
        byte[] bytes = br.ReadBytes(taille);
        string s = Encoding.ASCII.GetString(bytes);
        int zero = s.IndexOf('\0');
        if (zero >= 0) s = s.Substring(0, zero);
        return s;
    }

    static Client LireClientA(int index)
    {
        using (FileStream fs = new FileStream(cheminFichier, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            fs.Seek(OffsetDe(index), SeekOrigin.Begin);
            using (BinaryReader br = new BinaryReader(fs, Encoding.ASCII, true))
            {
                Client c = new Client();
                c.Numero = br.ReadInt32();
                c.Nom = LireChaineFixe(br, TAILLE_NOM);
                c.Prenom = LireChaineFixe(br, TAILLE_PRENOM);
                c.Telephone = LireChaineFixe(br, TAILLE_TEL);
                return c;
            }
        }
    }

    //reecrit une fiche déjà présente à une position précise
    static void EcrireClientA(int index, Client c)
    {
        using (FileStream fs = new FileStream(cheminFichier, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
        {
            fs.Seek(OffsetDe(index), SeekOrigin.Begin);
            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.ASCII, true))
            {
                bw.Write(c.Numero);
                EcrireChaineFixe(bw, c.Nom, TAILLE_NOM);
                EcrireChaineFixe(bw, c.Prenom, TAILLE_PRENOM);
                EcrireChaineFixe(bw, c.Telephone, TAILLE_TEL);
            }
        }
    }

    //écrit la nouvelle fiche à la suite des autres
    static void AjouterClient(Client c)
    {
        using (FileStream fs = new FileStream(cheminFichier, FileMode.Append, FileAccess.Write, FileShare.None))
        using (BinaryWriter bw = new BinaryWriter(fs, Encoding.ASCII, true))
        {
            bw.Write(c.Numero);
            EcrireChaineFixe(bw, c.Nom, TAILLE_NOM);
            EcrireChaineFixe(bw, c.Prenom, TAILLE_PRENOM);
            EcrireChaineFixe(bw, c.Telephone, TAILLE_TEL);
        }
    }

   
   //demande les infos au clavier, formate, crée la fiche
    static void SaisirNouveauClient()
    {
        Console.Clear();
        Console.WriteLine("Saisir un nouveau client");
        int nb = CompterFiches();
        int prochainNumero = nb + 1;

        Console.Write("Nom (sera stocké en MAJUSCULES) : ");
        string nomSaisi = Console.ReadLine();
        Majuscule(ref nomSaisi);

        Console.Write("Prénom (1ère majuscule, reste minuscule) : ");
        string prenomSaisi = Console.ReadLine();
        FirstMajuscule(ref prenomSaisi);

        Console.Write("Téléphone : ");
        string telSaisi = Console.ReadLine();
        if (telSaisi == null) telSaisi = "";
        telSaisi = telSaisi.Trim();

        Client c = new Client();
        c.Numero = prochainNumero;
        c.Nom = nomSaisi;
        c.Prenom = prenomSaisi;
        c.Telephone = telSaisi;

        AjouterClient(c);

        Console.WriteLine(">> Client ajouté avec numéro " + c.Numero);
        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

    //afficher un client (par NOM avec stockage en MAJ)
    static void AfficherClientParNom()
    {
        Console.Clear();
        Console.WriteLine("Rechercher un client par nom :");

        int nb = CompterFiches();
        if (nb == 0)
        {
            Console.WriteLine("Fichier vide. Rien à afficher.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Console.Write("Saisir un nom (majuscule ou minuscule) : ");
        string saisie = Console.ReadLine();
        Majuscule(ref saisie); // on convertit en MAJ pour comparer au stockage

        bool trouve = false;
        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);
            if (c.Nom == saisie)
            {
                Console.WriteLine("Fiche #" + i + " -> Numero: " + c.Numero + " | NOM: " + c.Nom + " | Prenom: " + c.Prenom + " | Tel: " + c.Telephone);
                trouve = true;
            }
        }

        if (!trouve)
        {
            Console.WriteLine("Aucun client trouvé pour ce nom.");
        }

        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

    static void AfficherTous()
    {
        Console.Clear();
        Console.WriteLine("Tous les clients :");
        int nb = CompterFiches();
        if (nb == 0)
        {
            Console.WriteLine("Aucune fiche.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);
            Console.WriteLine("Fiche #" + i + " -> Numero: " + c.Numero + " | NOM: " + c.Nom + " | Prenom: " + c.Prenom + " | Tel: " + c.Telephone);
        }

        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

    static void AfficherNombre()
    {
        Console.Clear();
        int nb = CompterFiches();
        Console.WriteLine("Nombre de clients : " + nb);
        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

    // modifier un client avec numéro de fiche = position, evite la repetitionde de code pouir les modif
    static void InstallerNouvelleValeur(string etiquette, ref string champ, bool appliquerMaj, bool appliquerFirst)
    {
        Console.Write("Nouveau " + etiquette + " : ");
        string saisie = Console.ReadLine();
        if (!string.IsNullOrEmpty(saisie))
        {
            if (appliquerMaj) Majuscule(ref saisie);
            if (appliquerFirst) FirstMajuscule(ref saisie);
            champ = saisie.Trim();
        }
    }

    static void ModifierClient()
    {
        Console.Clear();
        Console.WriteLine("Modifier un client par numéro de fiche");

        int nb = CompterFiches();
        if (nb == 0)
        {
            Console.WriteLine("Fichier vide. Impossible de modifier.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Console.Write("Numéro de fiche (position, commence à 0) : ");
        string lu = Console.ReadLine();
        int pos;
        if (!int.TryParse(lu, out pos))
        {
            Console.WriteLine("Numéro invalide.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }
        if (pos < 0 || pos >= nb)
        {
            Console.WriteLine("Cette fiche n'existe pas.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Client courant = LireClientA(pos);
        Console.WriteLine("Actuel -> Numero: " + courant.Numero + " | NOM: " + courant.Nom + " | Prenom: " + courant.Prenom + " | Tel: " + courant.Telephone);
        Console.WriteLine("Laisser vide pour conserver la valeur actuelle.");

        //MaJ conditionnelle, entrée = conserver , a tester ?
        InstallerNouvelleValeur("NOM", ref courant.Nom, true, false);
        InstallerNouvelleValeur("Prénom", ref courant.Prenom, false, true);

        Console.Write("Nouveau Téléphone : ");
        string nvTel = Console.ReadLine();
        if (!string.IsNullOrEmpty(nvTel)) courant.Telephone = nvTel.Trim();

        Console.Write("Confirmer l'enregistrement ? (o/n) : ");
        string rep = Console.ReadLine();
        if (rep == "o" || rep == "O")
        {
            EcrireClientA(pos, courant); // réécriture au même endroit avec la position qu'on a stock
        
            Console.WriteLine("Fiche modifiée.");
        }
        else
        {
            Console.WriteLine("Modification annulée.");
        }

        Console.WriteLine("Entrée pour revenir au menu");
        Console.ReadLine();
    }

// le switch boucle pourfaire menu 
    static void AfficherMenu()
    {
        Console.Clear();
        Console.WriteLine("=== GESTION DES CLIENTS (binaire) ===");
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
        //créer dossier si chemin contient répertoire, si existe pas cree le doss
        string dossier = Path.GetDirectoryName(cheminFichier);
        if (!string.IsNullOrEmpty(dossier)) Directory.CreateDirectory(dossier);

        if (!File.Exists(cheminFichier))
        {
            using (File.Create(cheminFichier)) { } // crée un binaire vide
        }

        int choix = -1;
        while (choix != 0)
        {
            AfficherMenu();
            string lu = Console.ReadLine();
            int.TryParse(lu, out choix);

            switch (choix)
            {
                case 1: SaisirNouveauClient(); break;
                case 2: AfficherClientParNom(); break;
                case 3: AfficherTous(); break;
                case 4: AfficherNombre(); break;
                case 5: ModifierClient(); break;
                case 0:
                    Console.Clear();
                    Console.WriteLine("Au revoir.");
                    break;
                default:
                    Console.WriteLine("Choix invalide. Entrée pour continuer...");
                    Console.ReadLine();
                    break;
            }
        }
    }
    
}
