using System;
using System.IO;
using System.Text;

//pour liste
using System.Collections.Generic;
using System.Reflection;

class ProgrammeClients
{
 static string cheminFichier = "clients.dat"; 

    // les listes pour le menu dynamique
    static List<string> textesMenu = new List<string>();
    static List<string> nomsMethodes = new List<string>();

    static void InitialiserMenu()
    {
        // ordre important car fiat: index 0 = option 1, index 1 = option 2, etc..

        textesMenu.Clear();
        nomsMethodes.Clear();

        textesMenu.Add("Saisir un nouveau client");
        nomsMethodes.Add("SaisirNouveauClient");

        textesMenu.Add("Afficher un client (par nom)");
        nomsMethodes.Add("AfficherClientParNom");

        textesMenu.Add("Afficher tous les clients");
        nomsMethodes.Add("AfficherTous");

        textesMenu.Add("Afficher le nombre de clients");
        nomsMethodes.Add("AfficherNombre");

        textesMenu.Add("Modifier un client (par numero de fiche)");
        nomsMethodes.Add("ModifierClient");

        textesMenu.Add("Supprimer une fiche");
        nomsMethodes.Add("SupprimerLogiquement");

        textesMenu.Add("Restaurer une fiche supprimée");
        nomsMethodes.Add("RestaurerFiche");

        textesMenu.Add("Afficher uniquement les fiches supprimées");
        nomsMethodes.Add("AfficherSupprimees");

        textesMenu.Add("Compresser le fichier (supprimer définitivement les fiches supprimées)");
        nomsMethodes.Add("CompresserFichier");
    }

   // mes const de taille 
    const int TAILLE_NOM = 30;
    const int TAILLE_PRENOM = 30;
    const int TAILLE_TEL = 20;

    // 4 octets pour l'entier Numero + les autres 
    const int TAILLE_ENREG = 4 + TAILLE_NOM + TAILLE_PRENOM + TAILLE_TEL;

    public struct Client
    {
        public int Numero;
        public string Nom;      
        public string Prenom;    
        public string Telephone;
    }

    static void Majuscule(ref string chaine)
    {
        if (string.IsNullOrEmpty(chaine)) { chaine = ""; return; }
        chaine = chaine.Trim().ToUpper();
    }

    static void FirstMajuscule(ref string chaine)
    {
        if (string.IsNullOrEmpty(chaine)) { chaine = ""; return; }
        string m = chaine.Trim().ToLower();
        if (m.Length == 1) { chaine = m.ToUpper(); return; }
        chaine = char.ToUpper(m[0]) + m.Substring(1);
    }

    //nb fiches dans le fichier
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
                c.Numero   = br.ReadInt32();
                c.Nom      = LireChaineFixe(br, TAILLE_NOM);
                c.Prenom   = LireChaineFixe(br, TAILLE_PRENOM);
                c.Telephone= LireChaineFixe(br, TAILLE_TEL);
                return c;
            }
        }
    }

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

   //partie suppression
    static bool EstSupprime(Client c)
    {
        return c.Nom == "*";
    }

    static int ChercherEmplacementLibre()
    {
        int nb = CompterFiches();
        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);
            if (EstSupprime(c)) return i;
        }
        return -1;
    }

    // écrit la nouvelle fiche,ajout d'une case libre marquée "*" sinon ajoute à la fin
    static void AjouterClient(Client c)
    {
        int posLibre = ChercherEmplacementLibre();
        if (posLibre >= 0)
        {
            EcrireClientA(posLibre, c);
            return;
        }

        using (FileStream fs = new FileStream(cheminFichier, FileMode.Append, FileAccess.Write, FileShare.None))
        using (BinaryWriter bw = new BinaryWriter(fs, Encoding.ASCII, true))
        {
            bw.Write(c.Numero);
            EcrireChaineFixe(bw, c.Nom, TAILLE_NOM);
            EcrireChaineFixe(bw, c.Prenom, TAILLE_PRENOM);
            EcrireChaineFixe(bw, c.Telephone, TAILLE_TEL);
        }
    }


    static void SaisirNouveauClient()
    {
        Console.Clear();
        Console.WriteLine("Saisir un nouveau client");

        int nb = CompterFiches();
        int prochainNumero = nb + 1;

        Console.Write("Nom (stocké en MAJUSCULES) : ");
        string nomSaisi = Console.ReadLine();
        Majuscule(ref nomSaisi);

        Console.Write("Prénom (1ère majuscule) : ");
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

    //afficher avec nom 
    static void AfficherClientParNom()
    {
        Console.Clear();
        Console.WriteLine("Rechercher un client par NOM");

        int nb = CompterFiches();
        if (nb == 0)
        {
            Console.WriteLine("Fichier vide. Rien à afficher.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Console.Write("Saisir un nom : ");
        string saisie = Console.ReadLine();
        Majuscule(ref saisie);

        bool trouve = false;
        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);
            //eviter si supprimé
            if (EstSupprime(c)) continue;
            if (c.Nom == saisie)
            {
                Console.WriteLine("Fiche #" + i + " -> Numero: " + c.Numero + " | NOM: " + c.Nom + " | Prenom: " + c.Prenom + " | Tel: " + c.Telephone);
                trouve = true;
            }
        }
        if (!trouve) Console.WriteLine("Aucun client trouvé pour ce nom.");

        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }


    static void AfficherTous()
    {
        Console.Clear();
        Console.WriteLine("Liste des clients (non supprimés)");

        int nb = CompterFiches();
        if (nb == 0)
        {
            Console.WriteLine("Aucune fiche.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        bool auMoinsUn = false;
        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);
            if (EstSupprime(c)) continue;
            Console.WriteLine("Fiche #" + i + " -> Numero: " + c.Numero + " | NOM: " + c.Nom + " | Prenom: " + c.Prenom + " | Tel: " + c.Telephone);
            auMoinsUn = true;
        }
        if (!auMoinsUn) Console.WriteLine("Aucune fiche active.");

        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

    //nb clients 
    static void AfficherNombre()
    {
        Console.Clear();
        int nb = CompterFiches();
        int actifs = 0;
        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);
            if (!EstSupprime(c)) actifs++;
        }
        Console.WriteLine("Nombre de clients (actifs) : " + actifs);
        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

    
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
        Console.WriteLine("Modifier un client (par numéro de fiche)");

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
        if (!int.TryParse(lu, out pos) || pos < 0 || pos >= nb)
        {
            Console.WriteLine("Position invalide.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Client courant = LireClientA(pos);
        if (EstSupprime(courant))
        {
            Console.WriteLine("Fiche supprimée logiquement, restauration requise avant modification.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Actuel -> Numero: " + courant.Numero + " | NOM: " + courant.Nom + " | Prenom: " + courant.Prenom + " | Tel: " + courant.Telephone);
        Console.WriteLine("Laisser vide pour conserver la valeur actuelle.");

        InstallerNouvelleValeur("NOM", ref courant.Nom, true, false);
        InstallerNouvelleValeur("Prénom", ref courant.Prenom, false, true);

        Console.Write("Nouveau Téléphone : ");
        string nvTel = Console.ReadLine();
        if (!string.IsNullOrEmpty(nvTel)) courant.Telephone = nvTel.Trim();

        Console.Write("Confirmer l'enregistrement ? (o/n) : ");
        string rep = Console.ReadLine();
        if (rep == "o" || rep == "O")
        {
            EcrireClientA(pos, courant);
            Console.WriteLine("Fiche modifiée.");
        }
        else
        {
            Console.WriteLine("Modification annulée.");
        }

        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

    //Les suppressions
    static void SupprimerLogiquement()
    {
        Console.Clear();
        Console.WriteLine("Supprimer une fiche (suppression logique)");

        int nb = CompterFiches();
        if (nb == 0)
        {
            Console.WriteLine("Fichier vide. Impossible de supprimer.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Console.Write("Numéro de fiche (position, commence à 0) : ");
        string lu = Console.ReadLine();
        int pos;
        if (!int.TryParse(lu, out pos) || pos < 0 || pos >= nb)
        {
            Console.WriteLine("Position invalide.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Client c = LireClientA(pos);
        if (EstSupprime(c))
        {
            Console.WriteLine("Cette fiche est déjà supprimée logiquement.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Fiche #" + pos + " -> Numero: " + c.Numero + " | NOM: " + c.Nom + " | Prenom: " + c.Prenom + " | Tel: " + c.Telephone);
        Console.Write("Confirmer la suppression logique ? (o/n) : ");
        string rep = Console.ReadLine();
        if (rep == "o" || rep == "O")
        {
            c.Nom = "*";
            EcrireClientA(pos, c);
            Console.WriteLine("Fiche marquée comme supprimée (Nom=\"*\").");
        }
        else
        {
            Console.WriteLine(">> Suppression annulée.");
        }

        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }


    static void RestaurerFiche()
    {
        Console.Clear();
        Console.WriteLine("Restaurer une fiche supprimée");

        int nb = CompterFiches();
        if (nb == 0)
        {
            Console.WriteLine("Fichier vide.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        bool auMoinsUne = false;
        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);
            if (EstSupprime(c))
            {
               Console.WriteLine(
                    "Fiche #" + i +
                    " -> SUPPRIMEE (Numero: " + c.Numero +
                    ") | Prenom: " + c.Prenom +
                    " | Tel: " + c.Telephone
                );

                auMoinsUne = true;
            }
        }
        if (!auMoinsUne)
        {
            Console.WriteLine("Aucune fiche supprimée à restaurer.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Console.Write("Numéro de fiche à restaurer : ");
        string lu = Console.ReadLine();
        int pos;
        if (!int.TryParse(lu, out pos) || pos < 0 || pos >= nb)
        {
            Console.WriteLine("Position invalide.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Client cible = LireClientA(pos);
        if (!EstSupprime(cible))
        {
            Console.WriteLine("Cette fiche n'est pas marquée comme supprimée.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        Console.Write("Tapez le NOM du client (stocké en MAJUSCULES) : ");
        string nom = Console.ReadLine();
        Majuscule(ref nom);
        if (nom.Length == 0)
        {
            Console.WriteLine("Nom invalide.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        cible.Nom = nom;
        EcrireClientA(pos, cible);

        Console.WriteLine("Fiche restaurée : NOM = " + nom);
        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

    static void AfficherSupprimees()
    {
        Console.Clear();
        Console.WriteLine("Fiches supprimées (Nom = \"*\")");

        int nb = CompterFiches();
        if (nb == 0)
        {
            Console.WriteLine("Fichier vide.");
            Console.WriteLine("Entrée pour revenir au menu...");
            Console.ReadLine();
            return;
        }

        bool auMoinsUne = false;
        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);
            if (EstSupprime(c))
            {
                Console.WriteLine("Fiche #" + i + " -> Numero: " + c.Numero + " | NOM: " + c.Nom + " | Prenom: " + c.Prenom + " | Tel: " + c.Telephone);

                auMoinsUne = true;
            }
        }

        if (!auMoinsUne)
            Console.WriteLine("Aucune fiche supprimée.");

        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
    }

  
static void CompresserFichier()
{
    Console.Clear();
    Console.WriteLine("Compression du fichier (suppression physique des fiches *).");

    int nb = CompterFiches();
    if (nb == 0)
    {
        Console.WriteLine("Fichier vide, rien à compresser.");
        Console.WriteLine("Entrée pour revenir au menu...");
        Console.ReadLine();
        return;
    }

    string tempFile = "clients_tmp.dat";

    //ouvrir fichier temp
    using (FileStream fsTemp = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
    using (BinaryWriter bw = new BinaryWriter(fsTemp, Encoding.ASCII, true))
    {
        int nouveauIndex = 0;

        //parcour fichier
        for (int i = 0; i < nb; i++)
        {
            Client c = LireClientA(i);

            //si fiche est marquée supprimée (Nom = "*" on la saute
            if (EstSupprime(c)) 
                continue;

            //sinon on la recopie dans le nouveau fichier à la suite
            bw.Write(c.Numero);
            EcrireChaineFixe(bw, c.Nom, TAILLE_NOM);
            EcrireChaineFixe(bw, c.Prenom, TAILLE_PRENOM);
            EcrireChaineFixe(bw, c.Telephone, TAILLE_TEL);

            nouveauIndex++;
        }
    }

    // remplacement fichier
    File.Delete(cheminFichier);
    File.Move(tempFile, cheminFichier);

    Console.WriteLine("Compression terminée. Le fichier ne contient plus que les fiches non supprimées.");
    Console.WriteLine("Entrée pour revenir au menu...");
    Console.ReadLine();
}


    //mon menu switch qu'on remplace donc en commentaire pour le moment
    // static void AfficherMenu()
    // {
    //     Console.Clear();
    //     Console.WriteLine("GESTION DES CLIENTS");
    //     Console.WriteLine("1 - Saisir un nouveau client");
    //     Console.WriteLine("2 - Afficher un client (par nom)");
    //     Console.WriteLine("3 - Afficher tous les clients");
    //     Console.WriteLine("4 - Afficher le nombre de clients");
    //     Console.WriteLine("5 - Modifier un client (par numero de fiche)");
    //     Console.WriteLine("6 - Supprimer une fiche)");
    //     Console.WriteLine("7 - Restaurer une fiche supprimée");
    //     Console.WriteLine("8 - Afficher UNIQUEMENT les fiches supprimées");
    //     Console.WriteLine("9 - Compresser le fichier (supprimer définitivement les fiches supprimées)");

    //     Console.WriteLine("0 - Quitter");
    //     Console.Write("Votre choix : ");
    // }

    // menu avec liste dynamique
    static void AfficherMenu()
    {
        Console.Clear();
        Console.WriteLine("GESTION DES CLIENTS");

        for (int i = 0; i < textesMenu.Count; i++)
        {
            int numero = i + 1; // 1,2,3...
            Console.WriteLine(numero + " - " + textesMenu[i]);
        }

        Console.WriteLine("0 - Quitter");
        Console.Write("Votre choix : ");
    }

//ancien main avec switch
//    static void Main()
// {
//     // S'assurer que le dossier du fichier existe (utile si cheminFichier contient un répertoire)
//     string fullPath = Path.GetFullPath(cheminFichier);
//     string? dir = Path.GetDirectoryName(fullPath);
//     if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
//         Directory.CreateDirectory(dir);

//     // S'assurer que le fichier existe
//     if (!File.Exists(cheminFichier))
//         using (File.Create(cheminFichier)) { }

//     int choix = -1;
//     while (choix != 0)
//     {
//         AfficherMenu();

//         string? lu = Console.ReadLine();
//         if (!int.TryParse(lu, out choix))
//         {
//             Console.WriteLine("Choix invalide. Entrée pour continuer...");
//             Console.ReadLine();
//             continue;
//         }

//         switch (choix)
//         {
//             case 1: SaisirNouveauClient(); break;
//             case 2: AfficherClientParNom(); break;
//             case 3: AfficherTous(); break;
//             case 4: AfficherNombre(); break;
//             case 5: ModifierClient(); break;
//             case 6: SupprimerLogiquement(); break;
//             case 7: RestaurerFiche(); break;
//             case 8: AfficherSupprimees(); break;
//             case 9: CompresserFichier(); break;


//             case 0:
//                 Console.Clear();
//                 Console.WriteLine("Au revoir.");
//                 break;
//             default:
//                 Console.WriteLine("Choix invalide. Entrée pour continuer...");
//                 Console.ReadLine();
//                 break;
//         }
//     }
// }


//nouveau main avec menu dynamique
    static void Main()
    {
        // on appel la methode qui init le menu
        InitialiserMenu();

        // verifier si dossier fichier existe
        string fullPath = Path.GetFullPath(cheminFichier);
        string? dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        
        if (!File.Exists(cheminFichier))
            using (File.Create(cheminFichier)) { }

        int choix = -1;
        while (choix != 0)
        {
            AfficherMenu();

            string? lu = Console.ReadLine();
            if (!int.TryParse(lu, out choix))
            {
                Console.WriteLine("Choix invalide. Entrée pour continuer...");
                Console.ReadLine();
                continue;
            }

            if (choix == 0)
            {
                Console.Clear();
                Console.WriteLine("Au revoir.");
                break;
            }

        
            if (choix < 1 || choix > textesMenu.Count)
            {
                Console.WriteLine("Choix invalide. Entrée pour continuer...");
                Console.ReadLine();
                continue;
            }

            // permet de recuperer le nom de la méthode liée à ce numéro
            string nomMethode = nomsMethodes[choix - 1];

            //recuperer la méthode via reflection, inspire de l'exemple donné dans la fifche de mise en situation
            MethodInfo? methodInfo = typeof(ProgrammeClients)
                .GetMethod(nomMethode, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            if (methodInfo == null)
            {
                Console.WriteLine("Erreur interne : méthode \"" + nomMethode + "\" introuvable.");
                Console.ReadLine();
                continue;
            }

            try
            {
                // methodes static et sans parametres donc null,null
                methodInfo.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'exécution de l'option : " + ex.Message);
                Console.ReadLine();
            }
        }
    }
}


