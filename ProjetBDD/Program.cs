using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;
using System.Xml;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;



namespace ProjetBDD
{
    class Program
    {
        //Modif : l'utilisateur et le mot de passe n'était pas bon
        #region Manipulation Données SQL
        public static List<string> RecupererDonnees(MySqlConnection maConnexion, string requete)
        {
            List<string> donnees = new List<string>();
            MySqlCommand command = maConnexion.CreateCommand();
            command.CommandText = requete;
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string currentRowAsString = "";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string valueAsString = reader.GetValue(i).ToString();
                    currentRowAsString += valueAsString;
                }
                donnees.Add(currentRowAsString);
            }
            command.Dispose();
            reader.Close();
            return donnees;
        }

        public static List<string> RecupererNomsColonnes(string nomTable, MySqlConnection maConnexion) //Récupérer les noms de colonnes en fonction de la table
        {
            string nomsColonnes = "SELECT column_name FROM information_schema.columns WHERE table_name = '" + nomTable + "';";
            List<string> donnees = RecupererDonnees(maConnexion, nomsColonnes); //Tous les noms de colonnes sont stockés dedans

            return donnees;
        }

        public static List<string> RecupererTypeColonnes(string nomTable, MySqlConnection maConnexion, List<string> nomsColonnes)
        {
            List<string> typesVariables = new List<string>(); //Tous les types de variables des colonnes sont stockés dedans
            for (int i = 0; i < nomsColonnes.Count; i++)
            {
                string requete = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + nomTable + "' AND COLUMN_NAME = '" + nomsColonnes[i] + "'";
                MySqlCommand command = maConnexion.CreateCommand();
                command.CommandText = requete;
                MySqlDataReader reader = command.ExecuteReader();
                reader.Read();
                typesVariables.Add(reader.GetValue(0).ToString());
                command.Dispose();
                reader.Close();
            }

            return typesVariables;
        }

        public static void Creation(string nomTable, MySqlConnection maConnexion) //Ajouter élément dans la BDD
        {

            List<string> donnees = RecupererNomsColonnes(nomTable, maConnexion); //Tous les noms de colonnes sont stockés dedans
            List<string> typesVariables = RecupererTypeColonnes(nomTable, maConnexion, donnees);


            string nomsColonnesRequete = "";

            for (int i = 0; i < donnees.Count - 1; i++)
            {
                nomsColonnesRequete += donnees[i] + ", ";
            }
            nomsColonnesRequete += donnees[donnees.Count - 1];

            string reponsesUtilisateur = "";
            for (int i = 0; i < donnees.Count; i++)
            {
                Console.WriteLine(donnees[i] + "(" + typesVariables[i] + ") :");
                if (typesVariables[i] == "varchar" || typesVariables[i] == "date")
                {
                    string a = Console.ReadLine();
                    if (a != "null")
                    {
                        reponsesUtilisateur += "\"" + a + "\", ";
                    }
                    else
                    {
                        reponsesUtilisateur += a + ", ";
                    }
                }
                else
                {
                    reponsesUtilisateur += Console.ReadLine() + ", ";
                }
            }
            reponsesUtilisateur = reponsesUtilisateur.Substring(0, reponsesUtilisateur.Length - 2);

            string requete = "insert into " + nomTable + "(" + nomsColonnesRequete + ")" + " values(" + reponsesUtilisateur + ");";
            Console.WriteLine(requete);
            MySqlCommand command2 = maConnexion.CreateCommand();
            command2.CommandText = requete;
            MySqlDataReader reader2 = command2.ExecuteReader();
            Console.WriteLine("Votre requête a bien été ajouté à la BDD");
        }

        public static void Modifier(string nomTable, MySqlConnection maConnexion)
        {
            List<string> donnees = RecupererNomsColonnes(nomTable, maConnexion); //Tous les noms de colonnes sont stockés dedans
            List<string> typesVariables = RecupererTypeColonnes(nomTable, maConnexion, donnees); //Tous les types de variables des colonnes sont stockés dedans

            Console.WriteLine("Sur quel " + donnees[0] + " voulez-vous modifier une donnée ? ");
            string id = Convert.ToString(Console.ReadLine());
            Console.WriteLine("A partir de quel attribut voulez-vous modifier une donnée ?");
            for (int i = 0; i < donnees.Count; i++)
            {
                Console.WriteLine((i + 1) + ") " + donnees[i]);
            }
            int choix = Convert.ToInt32(Console.ReadLine());
            Console.Clear();
            string requete = "SELECT " + donnees[choix - 1] + " FROM " + nomTable + " WHERE " + donnees[0] + "= \"" + id + "\";";
            List<string> valeurPrecedente = RecupererDonnees(maConnexion, requete);
            Console.WriteLine(donnees[choix - 1] + " est actuellement égal à " + valeurPrecedente[0] + " pour " + donnees[0] + " = \"" + id+"\"");
            Console.WriteLine("Quelle sera alors sa nouvelle valeur ?");
            string nouvelleValeur = "";
            if (typesVariables[choix - 1] == "int")
            {
                nouvelleValeur = Console.ReadLine();
            }
            else
            {
                nouvelleValeur = "\"" + Console.ReadLine() + "\"";
            }
            Console.Clear();
            string requete2 = "UPDATE " + nomTable + " SET " + donnees[choix - 1] + "=" + nouvelleValeur + " WHERE " + donnees[0] + "= \"" + id + "\";";
            MySqlCommand command = maConnexion.CreateCommand();
            command.CommandText = requete2;
            MySqlDataReader reader = command.ExecuteReader();
            Console.WriteLine("Votre requête a bien modifié la donnée dans la BDD");
        }

        public static void Supprimer(string nomTable, MySqlConnection maConnexion)
        {

            string nomsColonnes = "SELECT column_name FROM information_schema.columns WHERE table_name = '" + nomTable + "';"; //requete pour avoir le nom des colonnes
            List<string> donnees = RecupererDonnees(maConnexion, nomsColonnes); //stocke le nom des colonnes

            Console.WriteLine("Quel " + donnees[0] + " voulez-vous supprimer ?");
            string reponse = Console.ReadLine();
            string requete = "DELETE FROM " + nomTable + " WHERE " + donnees[0] + "= \"" + reponse + "\";";
            Console.WriteLine(requete);
            MySqlCommand command2 = maConnexion.CreateCommand();
            command2.CommandText = requete;
            MySqlDataReader reader2 = command2.ExecuteReader();
            Console.WriteLine(reponse + " a bien été supprimé de la BDD");
        }
        static void Selection(string req, MySqlConnection maConnexion)
        {
            MySqlCommand maRequete = maConnexion.CreateCommand();   // Selection <==> écriture de la requête
            maRequete.CommandText = req;
            MySqlDataReader exeRequete = maRequete.ExecuteReader();
            
            string tableau = ""; //Afficher résultat
            string mem = ""; // sera utile pour l'affichage
            while (exeRequete.Read())
            {
                string ligne = "";
                for (int i = 0; i < exeRequete.FieldCount; i++)
                {
                    ligne += " ".PadLeft(30 - exeRequete.GetValue(i).ToString().Length) + exeRequete.GetValue(i).ToString()+ "|"; 
                }
                tableau += "\n " + ligne;
                mem = ligne;
            }
            tableau += "\n";
            for (int i = 0; i <= mem.Length; i++)
                tableau += "-";
            Console.WriteLine(tableau);
            maRequete.Dispose();
            exeRequete.Close();
        }

        static void SelectionAvecSomme(string req1, string req2, MySqlConnection maConnexion, int modeAffichage)
        {
            MySqlCommand maRequete = maConnexion.CreateCommand();
            maRequete.CommandText = req1;
            MySqlDataReader exeRequete = maRequete.ExecuteReader();
            List<int> myList = new List<int>();
            while (exeRequete.Read())
            {
                myList.Add(Convert.ToInt32(exeRequete.GetValue(exeRequete.FieldCount - 1)));
            }

            maRequete.Dispose();
            exeRequete.Close();
            MySqlCommand maRequete2 = maConnexion.CreateCommand();
            maRequete2.CommandText = req2;
            MySqlDataReader exeRequete2 = maRequete2.ExecuteReader();
            List<int> myList2 = new List<int>();
            List<string> ListCient = new List<string>();
            while (exeRequete2.Read())
            {
                if (Convert.ToString(exeRequete2.GetValue(0)) != "")
                    ListCient.Add(Convert.ToString(exeRequete2.GetValue(0)));
                if (Convert.ToString(exeRequete2.GetValue(1)) != "")
                    ListCient.Add(Convert.ToString(exeRequete2.GetValue(1)));
                myList2.Add(Convert.ToInt32(exeRequete2.GetValue(exeRequete2.FieldCount - 1)));
            }
            int count = 0;
            foreach (int elem in myList)
            {
                myList2[count] += elem;
                count++;
            }

            if (modeAffichage == 1)
                Console.WriteLine("L'individu qui nous a le plus enrichi a dépensé " + myList2.Max());
            else
            {
                for (int index = 0; index < myList2.Count; index++)
                {
                    Console.WriteLine("Le client " + ListCient[index] + " a dépensé " + myList2[index]);
                }
            }
            maRequete2.Dispose();
            exeRequete2.Close();

        }
        #endregion

        #region Exportation XML & Jason
        static List<Fournisseur> FournisseurVersXML(string req, MySqlConnection maConnexion)
        {
            MySqlCommand maRequete = maConnexion.CreateCommand();   // Selection <==> écriture de la requête
            maRequete.CommandText = req;
            MySqlDataReader exeRequete = maRequete.ExecuteReader();

            List<Fournisseur> liste = new List<Fournisseur>();
            while (exeRequete.Read())
            {
                Fournisseur a = new Fournisseur { Siret = Convert.ToString(exeRequete.GetValue(0)), nomEntreprise=Convert.ToString(exeRequete.GetValue(1)), contact = Convert.ToString(exeRequete.GetValue(2)), adresse = Convert.ToString(exeRequete.GetValue(3)), libelle = Convert.ToInt32(exeRequete.GetValue(4)) };
                liste.Add(a);
            }
            maRequete.Dispose();
            exeRequete.Close();
            return liste;
        }
        static List<StockFaible> StockFaibleVersXML(string req, MySqlConnection maConnexion)
        {
            MySqlCommand maRequete = maConnexion.CreateCommand();   // Selection <==> écriture de la requête
            maRequete.CommandText = req;
            MySqlDataReader exeRequete = maRequete.ExecuteReader();

            List<StockFaible> liste = new List<StockFaible>();
            while (exeRequete.Read())
            {
                StockFaible a = new StockFaible { Siret = Convert.ToString(exeRequete.GetValue(0)), numProduit = Convert.ToString(exeRequete.GetValue(1)), quantite = Convert.ToInt32(exeRequete.GetValue(2)) };
                liste.Add(a);
            }
            maRequete.Dispose();
            exeRequete.Close();
            return liste;
        }
        public static void VersJson(MySqlConnection maConnexion,string req,string filename)
        {
            DataTable table = new DataTable("tbl");

            MySqlCommand command = maConnexion.CreateCommand();
            command.CommandText = req;
            MySqlDataReader reader = command.ExecuteReader();
            table.Load(reader);
            reader.Close();
            string jsonexport = JsonConvert.SerializeObject(table);
            File.WriteAllText(filename, jsonexport);
            
        }
        #endregion
        
        #region Menus
        static void MenuProjet()
        {
            #region 1.Ouverture de la connexion avec MySQL
            MySqlConnection maConnexion = null;

            try
            {
                string connexionInfo = "SERVER=localhost;PORT=3306;DATABASE=veloMax;UID=bozo;PASSWORD=bozo";
                maConnexion = new MySqlConnection(connexionInfo);
                maConnexion.Open();

            }
            catch (MySqlException e)
            {
                Console.WriteLine("Erreur de connexion : " + e.ToString());
                return;
            }
            #endregion

            Console.WriteLine("Que souhaitez-vous faire : \n\n" +
                              "1) Gestion des Pièces de rechanges\n" +
                              "2) Gestion des Individus\n" +
                              "3) Gestion des Boutiques\n" +
                              "4) Gestion des Fournisseurs\n" +
                              "5) Gestion des Commandes\n" +
                              "6) Gestion du Stock\n" +
                              "7) Module Statistiques\n" +
                              "8) Mode Démo\n" +
                              "9) Requête de notre création\n"+
                              "10) Export des stocks faibles avec fournisseurs pour command en XML \n" +
                              "11) Export des clients dont le programme de fidélité arrive à expiration dans moins de 2 mois avec historique des abonnements afin de les relancer en JSON");

            string choix;
            bool statut = false;

            do
            {
                Console.Write("\nVeillez saisir votre choix : ");
                choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        statut = true;
                        Console.Clear();
                        Menu("Piece", maConnexion);
                        break;
                    case "2":
                        statut = true;
                        Console.Clear();
                        Menu("Individu", maConnexion);
                        break;
                    case "3":
                        statut = true;
                        Console.Clear();
                        Menu("Boutique", maConnexion);
                        break;
                    case "4":
                        statut = true;
                        Console.Clear();
                        Menu("Fournisseur", maConnexion);
                        break;
                    case "5":
                        statut = true;
                        Console.Clear();
                        Menu("Commande", maConnexion);
                        break;
                    case "6":
                        statut = true;
                        Console.Clear();
                        MenuStock(maConnexion);
                        break;

                    case "7":
                        statut = true;
                        Console.Clear();
                        MenuStat(maConnexion);
                        break;
                    case "8":
                        statut = true;
                        Console.Clear();
                        Demo(maConnexion);
                        break;
                    case "9":
                        statut = true;
                        Console.Clear();
                        RequeteDeMaCreation(maConnexion);
                        break;
                    case "10":
                        statut = true;
                        Console.Clear();
                        Console.WriteLine("Exportation des stocks faibles de pièces avec fournisseurs en fichier XML...");
                        List<StockFaible> myList = StockFaibleVersXML("SELECT Siret, numProduit, quantite from fournisseur natural join fournit natural join piece where quantite <5 group by numProduit;", maConnexion);
                        string file = @"StockFaible.xml";
                        new XmlSerialize().ConvertListOfObjectsToXml(myList, file, true);
                        Console.WriteLine("Exportation terminée");
                        break;
                    case "11":
                        statut = true;
                        Console.Clear();
                        Console.WriteLine("Export vers JSON en cours...");
                        VersJson(maConnexion, "SELECT * ,DATEDIFF(NOW(), dateAdhesion) FROM Individu natural join adhere natural join Fidelio WHERE(DATEDIFF(NOW(), dateAdhesion) - 305 >= 0     AND DATEDIFF(NOW(), dateAdhesion) - 300 <= 65  AND Rabais = 5) OR(DATEDIFF(NOW(), dateAdhesion) - (305 + 365) >= 0  AND DATEDIFF(NOW(), dateAdhesion) - (300 + 365) <= 65 AND(Rabais = 8 or Rabais = 10)) OR(DATEDIFF(NOW(), dateAdhesion) - (305 + 365 * 3) >= 0    AND DATEDIFF(NOW(), dateAdhesion) - (300 + 365 * 3) <= 65 AND Rabais = 12); ","FidelioExpirationFuture.json");
                        Console.WriteLine("Exportation terminée");
                        break;
                }

            } while (statut == false);
        }
        static void Menu(string nomTable, MySqlConnection maConnexion)
        {


            Console.WriteLine("Choisir une action : ");
            Console.WriteLine("1) Ajouter " + nomTable);
            Console.WriteLine("2) Modifier " + nomTable);
            Console.WriteLine("3) Supprimer " + nomTable);



            string choix;
            bool statut = false;

            do
            {
                Console.Write("\nVeillez saisir votre choix : ");
                choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        statut = true;
                        Console.Clear();
                        Creation(nomTable, maConnexion);
                        break;

                    case "2":
                        statut = true;
                        Console.Clear();
                        Modifier(nomTable, maConnexion);
                        break;
                    case "3":
                        statut = true;
                        Console.Clear();
                        Supprimer(nomTable, maConnexion);
                        break;

                }
            } while (statut == false);
        }

        static void MenuStock(MySqlConnection maConnexion)
        {
            Console.WriteLine("Module Stock");
            Console.WriteLine("1) Afficher le stock par pièce");
            Console.WriteLine("2) Afficher le stock par fournisseur");
            Console.WriteLine("3) Afficher le stock par catégorie");
          

            string choix;
            bool statut = false;

            do
            {
                Console.Write("\nVeillez saisir votre choix : ");
                choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        {
                            statut = true;
                            Console.Clear();
                            string display = " ".PadLeft(21) + "numProduit|" + " ".PadLeft(22) + "quantite|";
                            Console.WriteLine(display);
                            for (int i = 0; i < display.Length; i++)
                                Console.Write("-");
                            Selection("SELECT numProduit, sum(quantite) FROM Fournisseur natural join fournit group by numProduit;", maConnexion);
                        }
                        break;
                    case "2":
                        {
                            statut = true;
                            Console.Clear();
                            string display = " ".PadLeft(26) + "Siret|" + " ".PadLeft(20) + "numProduit|" + " ".PadLeft(22) + "quantite|";
                            Console.WriteLine(display);
                            for (int i = 0; i < display.Length; i++)
                                Console.Write("-");
                            Selection("SELECT Siret, numProduit, quantite FROM Fournisseur natural join Fournit;", maConnexion);
                        }
                        break;
                    case "3":
                        statut = true;
                        Console.Clear();
                        // non réalisé car pour moi il n'y a pas directement des vélos dans la bd, seulement les pièces les constituant
                        break;


                }
            }
            while (statut == false);
        }

        static void MenuStat(MySqlConnection maConnexion)
        {
            Console.WriteLine("Module Statistique");
            Console.WriteLine("1) Les quantités vendues de chaque item");
            Console.WriteLine("2) Liste des membres pour chaque programme d’adhésion");
            Console.WriteLine("3) Date d’expiration des adhésions qui n'ont pas encore expiré");
            Console.WriteLine("4) Meilleur client en fonction du nombre de pièces vendues");
            Console.WriteLine("5) meilleur client en fonction du nb de pièces achetées en euros, nb de modèles achetés en euros, et valeur du montant le plus élevé dépensé par un client");
            Console.WriteLine("6) moyenne des montants des commandes, moyenne du nombre de pièces et moyenne du nb de vélos par commande.");

            string choix;
            bool statut = false;

            do
            {
                Console.Write("\nVeillez saisir votre choix : ");
                choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        {
                            statut = true;
                            Console.Clear();
                            string display = " ".PadLeft(22) + "numModele|" + " ".PadLeft(9) + "quantite modele vendu|";
                            string display2 = " ".PadLeft(23) + "numPiece|" + " ".PadLeft(10) + "quantite piece vendu|";
                            Console.WriteLine(display);
                            for (int i = 0; i < display.Length; i++)
                                Console.Write("-");
                            Selection("SELECT numModele, sum(quantite_commande_modele) FROM EstConstitueModele natural join Commande WHERE dateLivraison <= \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" GROUP BY numModele;", maConnexion);
                            Console.WriteLine(display2);
                            for (int i = 0; i < display2.Length; i++)
                                Console.Write("-");
                            Selection("SELECT numProduit, sum(quantite_commande_piece) FROM EstConstituePiece natural join Commande WHERE dateLivraison <= \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" GROUP BY numProduit;", maConnexion);
                        }
                        break;
                    case "2":
                        {
                            statut = true;
                            Console.Clear();
                            string display = " ".PadLeft(21) + "idIndividu|" + " ".PadLeft(18) + "numProgramme|";
                            Console.WriteLine(display);
                            for (int i = 0; i < display.Length; i++)
                                Console.Write("-");
                            Selection("SELECT idIndividu, numProgramme FROM Adhere NATURAL JOIN fidelio WHERE  NOW() < DATE_ADD(dateAdhesion, INTERVAL duree YEAR);", maConnexion);
                            
                        }
                        break;
                    case "3":
                        {
                            statut = true;
                            Console.Clear();
                            string display = " ".PadLeft(21) + "idIndividu|" + " ".PadLeft(16) + "DateExpiration|";
                            Console.WriteLine(display);
                            for (int i = 0; i < display.Length; i++)
                                Console.Write("-");
                            Selection("SELECT idIndividu, DATE_ADD(dateAdhesion, INTERVAL duree YEAR) as DateExpiration FROM Adhere NATURAL JOIN Fidelio Where NOW() < DATE_ADD(dateAdhesion, INTERVAL duree YEAR);", maConnexion);
                            
                        }
                        break;
                    case "4":
                        {
                            statut = true;
                            Console.Clear();
                            string display = " ".PadLeft(21) + "idBoutique|" + " ".PadLeft(20) + "idIndividu|" + " ".PadLeft(19) + "Total Pieces|";
                            Console.WriteLine(display);
                            for (int i = 0; i < display.Length; i++)
                                Console.Write("-");
                            Selection("SELECT idBoutique,idIndividu, sum(quantite_commande_piece) as somme FROM Commande natural join estConstituePiece GROUP BY idBoutique, idIndividu ORDER BY somme DESC LIMIT 1; ",maConnexion);
                        }
                        break;
                    case "5":
                        {
                            statut = true;
                            Console.Clear();
                            string display = " ".PadLeft(21) + "idBoutique|" + " ".PadLeft(20) + "idIndividu|" + " ".PadLeft(12) + "Total Dépense Modèle|";
                            Console.WriteLine(display);
                            for (int i = 0; i < display.Length; i++)
                                Console.Write("-");
                            Selection("select idBoutique, idIndividu, sum(quantite_commande_piece* prixUnitaire) as prixTotal from commande natural join estConstituePiece natural join Piece group by idIndividu, idBoutique order by idIndividu; ", maConnexion);
                            string display2 = " ".PadLeft(21) + "idBoutique|" + " ".PadLeft(20) + "idIndividu|" + " ".PadLeft(11) + "Total Dépense Pièce|";
                            Console.WriteLine(display2);
                            for (int i = 0; i < display2.Length; i++)
                                Console.Write("-");
                            Selection("select idBoutique, idIndividu, quantite_commande_modele* prixUnitaire as prixTotal from commande natural join estConstitueModele natural join Modele group by idIndividu, idBoutique ORDER By idIndividu; ", maConnexion);

                            SelectionAvecSomme("select idBoutique, idIndividu, sum(quantite_commande_piece* prixUnitaire) as prixTotal from commande natural join estConstituePiece natural join Piece group by idIndividu, idBoutique order by idIndividu; ", "select idBoutique, idIndividu, quantite_commande_modele* prixUnitaire as prixTotal from commande natural join estConstitueModele natural join Modele group by idIndividu, idBoutique ORDER By idIndividu; ", maConnexion,1);
                        }
                        break;
                    case "6":
                        {
                            statut = true;
                            Console.Clear();
                            string display = (" ".PadLeft(3) + "MoyenneNbPiece par commandes|");
                            Console.WriteLine(display);
                            for (int i = 0; i < display.Length; i++)
                                Console.Write("-");
                            Selection("select sum(quantite_commande_piece)/(select count(numCommande) from commande) as MoyenneNbPiece from commande natural join estConstituePiece; ", maConnexion);
                            string display2 = (" ".PadLeft(2) + "MoyenneNbModele par commandes|");
                            Console.WriteLine(display2);
                            for (int i = 0; i < display2.Length; i++)
                                Console.Write("-");
                            Selection("select sum(quantite_commande_modele)/(select count(numCommande) from commande) as MoyenneNbModele from commande natural join estConstitueModele; ", maConnexion);
                            string display3 = (" ".PadLeft(2) + "Moyenne montant des commandes|");
                            Console.WriteLine(display3);
                            for (int i = 0; i < display3.Length; i++)
                                Console.Write("-");
                            Selection("select sum(quantite_commande_piece*prixUnitaire)/(select count(numCommande) from commande) + (select sum(quantite_commande_modele*prixUnitaire)/(select count(numCommande) from commande) as MoyenneNbModele from commande natural join estConstitueModele natural join Modele) as PrixMoyenCommande from commande natural join estConstituePiece natural join Piece; ", maConnexion);

                        }
                        break;


                }
            }
            while (statut == false);
        }

        #endregion

        static void Demo(MySqlConnection maConnexion)
        {
            Console.Clear();
            Console.WriteLine("Voici le nombre de clients chez Vélomax :\n");
            string display = " ".PadLeft(17) + "nombre Clients|";
            for (int i = 0; i < display.Length; i++)
                Console.Write("-");
            Console.WriteLine("\n" +display);
            for (int i = 0; i < display.Length; i++)
                Console.Write("-");
            Selection("Select count(idBoutique) + (Select count(idIndividu) from individu) from boutique;",maConnexion);
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Noms des clients avec le cumul de toutes ses commandes en euros : \n");
            SelectionAvecSomme("select idBoutique, idIndividu, sum(quantite_commande_piece* prixUnitaire) as prixTotal from commande natural join estConstituePiece natural join Piece group by idIndividu, idBoutique order by idIndividu; ", "select idBoutique, idIndividu, sum(quantite_commande_modele* prixUnitaire) as prixTotal from commande natural join estConstitueModele natural join Modele group by idIndividu, idBoutique ORDER By idIndividu; ", maConnexion, 0);
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Liste des produits ayant une quantité en stock <= 2");
            string display2 = " ".PadLeft(17) + "numéro produit|" +" ".PadLeft(13) + "quantité actuelle|";
            Console.WriteLine(display2);
            for (int i = 0; i < display2.Length; i++)
                Console.Write("-");
            Selection("SELECT numProduit, SUM(quantite) FROM Piece natural join fournit GROUP BY numProduit HAVING SUM(quantite) <= 2 ;", maConnexion);

            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Nombres de pièces et/ou vélos fournis par fournisseur : ");
            string display3 = " ".PadLeft(17) + "nomFournisseur|" + " ".PadLeft(14) + "quantité fournie|";
            Console.WriteLine(display3);
            for (int i = 0; i < display3.Length; i++)
                Console.Write("-");
            Selection("SELECT nomFournisseur, SUM(quantite) FROM Piece natural join fournit GROUP BY nomFournisseur", maConnexion);

            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Exportation de la table fournisseur en fichier XML");
            List<Fournisseur> myList = FournisseurVersXML("Select * from Fournisseur;", maConnexion);
            string file = @"Fournisseur.xml";
            new XmlSerialize().ConvertListOfObjectsToXml(myList, file, true);
            Console.WriteLine("Exportation terminée");
            Console.WriteLine("Exportation de la table fournisseur en fichier JSON");
            VersJson(maConnexion, "Select * from Fournisseur","Fournisseur.json");
            Console.WriteLine("Exportation terminée");
        }
    
        static void RequeteDeMaCreation(MySqlConnection maConnexion)
        {
            //1 Requête synchronisée
            Console.WriteLine("lsite de");
            Selection("select p.description, f.nomEntreprise from Piece p natural join Fournit natural join Fournisseur f where nomEntreprise in(SELECT nomEntreprise from fournisseur ff where ff.nomEntreprise LIKE \" % O\");", maConnexion);
            //1 Requête avec auto - jointure
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Avec auto-jointure : Liste des individus qui ont passés moins de 5 commandes chez VELOMAX");
            Selection("SELECT nom,prenom, count(numCommande) FROM commande natural join individu GROUP BY idIndividu having count(numCommande)<5;", maConnexion);
            //1 Requête avec une union
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Avec une union : Noms des clients avec le cumul de toutes ses commandes en euros"); //NB : cette requête avait déjà été faite dans le mode démo mais pas en utilisant une union
            Selection("SELECT idBoutique, idIndividu, SUM(prixTotal) FROM (select idBoutique, idIndividu, sum(estConstituePiece.quantite_commande_piece * prixUnitaire) as prixTotal from commande, estConstituePiece, Piece WHERE Commande.numCommande = estConstituePiece.numCommande AND estConstituePiece.numProduit = Piece.numProduit group by idIndividu, idBoutique UNION ALL select idBoutique, idIndividu, sum(quantite_commande_modele* prixUnitaire) as prixTotal from commande natural join estConstitueModele natural join Modele group by idIndividu, idBoutique ORDER By idIndividu) as t group by idIndividu, idBoutique;", maConnexion);
                    }

        static void Main(string[] args)
        {
            MenuProjet();
            Console.ReadKey();
        }

    }
   
}
