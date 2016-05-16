//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Samatar Hassan.
//
//===================================================================

using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;
using SafeNetWS.utils;
using SafeNetWS.messages;
using SafeNetWS.database.result;
using SafeNetWS.database.row.value;
using SafeNetWS.login;
using SafeNetWS.utils.crypting;
using SafeNetWS.creditcard;
using SafeNetWS.utils.cache;
using SafeNetWS.database.row;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.creditcard.creditcardvalidator;

namespace SafeNetWS.database
{

    /// <summary>
    /// Cette classe définie une connection SQL à la base de données
    /// contenant les cartes bancaires (Navision)
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>

    public class NavisionDbConnection
    {
        // Informations sur le compte utilisateur
        private UserInfo user;
        // Connexion ouverte
        private SqlConnection myConn;
        // point of sale
        // We need here the point of sale because the connection settings
        // depends from the POS
        //
        private string pos;

        /// <summary>
        /// Définition d'une nouvelle connexion vers Navision
        /// </summary>
        /// <param name="useri">Compte utilisateur</param>
        public NavisionDbConnection(UserInfo useri, string pos)
        {
            // Set user information
            SetUser(useri);

            // Set pos and correct it if needed
            SetPos(Util.CorrectPos(GetUser(), pos));

            // Get Navision connection string from the POS
            SetConnection(new SqlConnection(GetConnString()));
        }

        /// <summary>
        /// Returns Navision connection string
        /// from POS
        /// </summary>
        /// <returns>SQL Navision connection string</returns>
        private string GetConnString()
        {
            // We need to return here the connection string
            // First let's check in the cache
            // that will avoid us to rebuild it
            string connString = Global.GetConnStringNav(GetPos());

            if (connString != null)
            {
                // We have something in the cache
                // great, no need to continue
                // we will thie string and that's it.
                return connString;
            }

            // Connect to Navision settings database 
            // and return connection string for the pos
            connString = GetNavisionConnectionString();

            if (String.IsNullOrEmpty(connString))
            {
                // something went wrong, we don't Navision connection for that pos
                // it's probably unknown POS
                throw new Exception(GetMessages().GetString("PosUnknown", GetPos(), true));
            }

            // We have a connection string to Navision
            return connString;
        }

        /// <summary>
        /// Returns point of sale
        /// </summary>
        /// <returns>point of sale</returns>
        private string GetPos()
        {
            return this.pos;
        }

        /// <summary>
        /// Set point of sale
        /// </summary>
        /// <param name="value">point of sale</param>
        private void SetPos(string value)
        {
            this.pos = value;
        }

        /// <summary>
        /// Returns connection string for a specific point of sale
        /// </summary>
        /// <returns>Navision connection sale</returns>
        private string GetNavisionConnectionString()
        {
            NavisionSettingsConnection conn = null;

            try
            {
                // define a new Navision connection
                conn = new NavisionSettingsConnection(GetUser());

                // open connection
                conn.Open();

                // Return Navision connection string from pos
                return conn.GetNavisionConnectionString(GetPos());
            }
            finally
            {
                // Close the connection
                if (conn != null) conn.Close();
            }
        }

        /// <summary>
        /// Ouverture d'une connection vers la
        /// base de données contenant les données Navision
        /// </summary>
        public void Open()
        {
            try
            {
                this.myConn.Open();
            }
            catch (Exception e)
            {
                // On met à jour le statut
                throw new Exception(GetMessages().GetString("NavisionDbConnection.ErrorConnecting", GetConnection().Database, e.Message, true));
            }
        }


        /// <summary>
        /// Fermeture de la connection vers la
        /// base de données contenant les données Navision
        /// Si un pool de connexion est utilisée, la connexion
        /// sera retournée au pool
        /// </summary>
        public void Close()
        {
            if (GetConnection() != null)
            {
                try
                {
                    // et libération des ressources
                    this.myConn.Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(GetMessages().GetString("NavisionDbConnection.ErrorClosingConnection", e.Message, true));
                }
            }
        }


        /// <summary>
        /// Récupération des informations sur la carte depuis Navision
        /// ExpirationDate
        /// ExtendedNo
        /// CVC
        /// truncatedPAN
        /// CardType
        /// </summary>
        /// <param name="posi">Pos Navision</param>
        /// <param name="token">Token de la carte</param>
        /// <returns>PanInfoResult (informations carte)</returns>
        public PanInfoResult GetPanInfos(long token)
        {
            string request = String.Format(
            "select "
            + "TOP 1 "
            + " [Expiration Date] 'ExpirationDate'"
            + ", [Extended No_] 'ExtendedNo'"
            + ", CVC"
            + ", [Card No_ 1] + [Card No_ 2] + [Card No_ 3] + [Card No_ 4] + [Card No_ 5] 'truncatedPAN'"
            + ", case [Card Type] "
            + "  when 0 then 'Visa' "
            + "  when 1 then 'Eurocard/Mastercard' "
            + "  when 2 then 'CB' "
            + "  when 3 then 'Amex' "
            + "  when 4 then 'Diners club' "
            + "  when 5 then 'Discover' "
            + "  when 6 then 'JCB1' "
            + "  when 7 then 'JCB2' "
            + "  when 8 then 'Accord Finance' "
            + "  when 9 then 'Cofinoga' "
            + "  when 10 then 'Aurore/Cetelem' "
            + "  when 11 then 'Kyriel' "
            + "  when 12 then 'Airplus' "
            + "  when 13 then 'Switch/Maestro' "
            + "  when 14 then 'ChinaUnionPay' "
            + "  else '' "
            + "end 'CardType' "
            + ", case [Card Type] "
            + "  when 0 then 'VI' "
            + "  when 1 then 'CA' "
            + "  when 2 then 'CB' "
            + "  when 3 then 'AX' "
            + "  when 4 then 'DC' "
            + "  when 5 then 'DI' "
            + "  when 6 then 'JC' "
            + "  when 7 then 'JC' "
            + "  when 8 then 'AF' "
            + "  when 9 then 'CO' "
            + "  when 10 then 'AC' "
            + "  when 11 then 'KY' "
            + "  when 12 then 'TP' "
            + "  when 13 then 'SW' "
            + "  when 14 then 'CU' "
            + "  else '' "
            + "end 'ShortCardType' "
            + ", [card] 'FinancialFlow' "
            + ", [Enhanced Flow] 'EnhancedFlow' "
            + "from [egencia {0}$payment card] (NOLOCK) "
            + "where Token=@token "
            + "and Status IN (0, 5)", GetPos()); // Return only valide credit card

            // objet command     
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;
            PanInfoResult PanInfo = new PanInfoResult();
            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@token", SqlDbType.VarChar, 30);
                command.Parameters["@token"].Value = token.ToString();

                dr = command.ExecuteReader();

                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    PanInfo.SetValues(
                        Util.GetSQLDataTime(dr, "ExpirationDate"),
                        dr["ExtendedNo"].ToString(),
                        dr["CVC"].ToString(),
                        dr["truncatedPAN"].ToString(),
                        dr["CardType"].ToString(),
                        dr["ShortCardType"].ToString(),
                        dr["FinancialFlow"].ToString(),
                        dr["EnhancedFlow"].ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("NavisionDbConnection.GetPanInfos.Error", token, e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return PanInfo;

        }

         
        /// <summary>
        /// Vérification si la carte est utilisée par d'autres
        /// clients au sein d'un même pos
        /// </summary>
        /// <param name="customer">numéro du client (comcode)</param>
        /// <param name="token">token de la carte </param>
        /// <returns>Chaine information carte</returns>
        public string CheckCardUsedForOtherCustomers(string customer, long token)
        {
            string retval = null;

            // On construit la chaîne
            string request = String.Format("select TOP 1 [customer No_] 'customer',[card Reference] 'refcard' from [egencia {0}$payment card] (NOLOCK) where [customer No_]<>@customer and [token]=@token", GetPos());

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;
            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@customer", SqlDbType.VarChar, 20);
                command.Parameters["@customer"].Value = customer;
                command.Parameters.Add("@token", SqlDbType.VarChar, 30);
                command.Parameters["@token"].Value = token.ToString();

                dr = command.ExecuteReader();
                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    // Cette carte est visiblement utilisée par un autre client!
                    // Cela n'est pas autorisé!
                    // On lève une exception
                    retval = GetMessages().GetString("NavisionDbConnection.CheckCardUsedForOtherCustomer.CardUsed", token,
                        dr["customer"].ToString(), dr["refcard"].ToString(), GetPos(), false);
                }
            }
            catch (Exception p)
            {
                throw new Exception(p.Message);
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;
        }

        /// <summary>
        /// Vérification si le traveller
        /// appartient bien à la companie spécifiée
        /// </summary>
        /// <param name="customer">numéro du client (comcode)</param>
        /// <param name="traveller">numéro du travelleur (percode</param>
        public void CheckTravellerForCustomer(string customer, string traveller)
        {
            if (String.IsNullOrEmpty(traveller))
            {
                // Le code du voyageur n'a pas été spécifié
                // On quitte cette fonction
                return;
            }
            // On construit la chaîne
            string request = "select [customer No_] 'comcode' "
            + "from [egencia " + GetPos() + "$traveller] (NOLOCK) where [No_]=@traveller";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;
            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@traveller", SqlDbType.VarChar, 20);
                command.Parameters["@traveller"].Value = traveller;

                string comcode = null;
                dr = command.ExecuteReader();
                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    // On récupère la companie du traveller
                    comcode = dr["comcode"].ToString();
                }
                if (comcode == null)
                {
                    // Ce traveller est visiblement introuvable!
                    throw new Exception(GetMessages().GetString("CheckTravellerForCustomer.CanNotFindTraveller", traveller, GetPos(), true));
                }
                if (!comcode.Equals(customer))
                {
                    // Ce traveller n'appartient à la companie!
                    throw new Exception(GetMessages().GetString("CheckTravellerForCustomer.TravellerDoNotBelongToCustomer", traveller, customer, comcode, true));
                }
            }
            catch (Exception p)
            {
                throw new Exception(GetMessages().GetString("CheckTravellerForCustomer.Error", traveller, p.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
        }


        /// <summary>
        /// Vérification si le centre de cout 1
        /// appartient bien à la companie spécifiée
        /// </summary>
        /// <param name="user">Identifiant client</param>
        /// <param name="customer">numéro du client (comcode)</param>
        /// <param name="traveller">Nom de centre de coût 1</param>
        public void CheckCC1ForCustomer(string customer, string cc1)
        {
            if (String.IsNullOrEmpty(cc1))
            {
                // Le code du cc1 n'a pas été spécifié
                // On quitte cette fonction
                return;
            }
            // On construit la chaîne
            string request = String.Format("select [Analytical code] "
            + "from [egencia {0}$analytical code] (NOLOCK) "
            + "where [Customer No_]=@customer and [Analytical code]=@cc1 "
            + "and [Analytical range]=0", GetPos());

            // objet command
            SqlCommand command = null;
            DataTable DT = new DataTable();
            SqlDataAdapter adapter = null;
            try
            {
                command = new SqlCommand(request, GetConnection());
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@customer", SqlDbType.VarChar, 20);
                command.Parameters.Add("@cc1", SqlDbType.VarChar, 30);

                command.Parameters["@customer"].Value = customer;
                command.Parameters["@cc1"].Value = cc1;

                adapter = new SqlDataAdapter(command);
                adapter.Fill(DT);

                if (DT.Rows.Count == 0)
                {
                    // Ce centre de cout est visiblement introuvable!
                    throw new Exception(GetMessages().GetString("CheckCC1ForCustomer.CanNotCC1", cc1, customer, true));
                }
            }
            catch (Exception p)
            {
                throw new Exception(GetMessages().GetString("CheckCC1ForCustomer.Error", cc1, p.Message, true));
            }
            finally
            {
                DisposeSqlDataAdapter(adapter);
                CloseDataTable(DT, command);
            }
        }

        //>>EGE-66905
        /// <summary>
        /// Return Cost center 1 value from id
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="customer">Customer</param>
        /// <param name="cc1Id">Cost center 1 Id</param>
        /// <returns>Cost center 1 value</returns>
        public string GetCostCenter1ValueFromId(string customer, string cc1Id)
        {

            // On construit la chaîne
            string request = String.Format("select [Analytical code] AS CC1"
            + " from [egencia {0}$analytical code] (NOLOCK)"
            + " where [Customer No_]=@customer and [Analytical range]=@ccRange and [Id]=@cc1Id", GetPos());


            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;
            try
            {
                command = new SqlCommand(request, GetConnection());
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@customer", SqlDbType.VarChar, 20);
                command.Parameters.Add("@ccRange", SqlDbType.Int);
                command.Parameters.Add("@cc1Id", SqlDbType.VarChar, 30);

                command.Parameters["@customer"].Value = customer;
                command.Parameters["@ccRange"].Value = 0;
                command.Parameters["@cc1Id"].Value = cc1Id;

                dr = command.ExecuteReader();

                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    // On a trouvé la corporation
                    return dr["CC1"].ToString();
                }

                // Ce centre de cout est visiblement introuvable!
                throw new Exception(GetMessages().GetString("CheckCC1ForCustomer.CanNotCC1", cc1Id, customer, true));
            }
            catch (Exception p)
            {
                throw new Exception(GetMessages().GetString("CheckCC1ForCustomer.Error", cc1Id, p.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
        }
        //<<EGE-66905

        /// <summary>
        /// Vérification si VPayment est activé
        /// pour une companie
        /// </summary>
        /// <param name="corporation">Corporation code</param>
        public void CheckVPaymentForCorporation(string corporation)
        {

            // On construit la chaîne
            string request = String.Format("SELECT "
                + "[Vpayment Gestion] AS VPaymentActivated "
                + "FROM [EGENCIA {0}$Corporation] (NOLOCK) "
                + "WHERE [Corporation Code]=@corporation "
                + "AND [Service Group] IN (1, 6)", GetPos());

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;
            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@corporation", SqlDbType.VarChar, 10);
                command.Parameters["@corporation"].Value = corporation;

                dr = command.ExecuteReader();

                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    // On a trouvé la corporation
                    if (Util.ConvertStringToBool(dr["VPaymentActivated"].ToString()))
                    {
                        // VPayment est activée pour cette corporation
                        return;
                    }
                    throw new Exception(GetMessages().GetString("CheckVPaymentForCorporation.VPaymentNotAllowedForCorporation", true));
                }
                // Cette companie est visiblement introuvable!
                throw new Exception(GetMessages().GetString("CheckVPaymentForCorporation.CanNotFindCorporation", corporation, GetPos(), true));
            }
            catch (Exception p)
            {
                throw new Exception(GetMessages().GetString("CheckVPaymentForCorporation.Error", corporation, p.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
        }

        /// <summary>
        /// Vérification si le client
        /// appartient bien au Pos spécifié
        /// </summary>
        /// <param name="customer">numéro du client (comcode)</param>
        public void CheckCustomerForPos(string customer)
        {

            // On construit la chaîne
            string request = String.Format("select [No_] from [egencia {0}$customer] (NOLOCK) where [No_]=@customer", GetPos());

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;
            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@customer", SqlDbType.VarChar, 20);
                command.Parameters["@customer"].Value = customer;

                dr = command.ExecuteReader();

                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    // On récupère la companie du traveller
                    return;
                }
                // Ce client est visiblement introuvable!
                throw new Exception(GetMessages().GetString("CheckCustomerForPos.CanNotFindCustomer", customer, GetPos(), true));
            }
            catch (Exception p)
            {
                throw new Exception(GetMessages().GetString("CheckCustomerForPos.Error", customer, p.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
        }

      



        /// <summary>
        /// Récupération de la prochaine valeur de l'id de la carte
        /// utilisée dans dans la table des cartes history
        /// </summary>
        /// <param name="myTrans">Transaction SQL</param>
        /// <returns>Sequence suivante</returns>
        private int GetNextCardHistoryId(SqlTransaction myTrans)
        {
            int NextId = 0;

            // On construit la chaîne
            string request = "SELECT ISNULL((SELECT MAX(id) FROM [EGENCIA "
                   + GetPos() + "$Payment Card History](NOLOCK)),0)+1 'NextId'";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection(), myTrans);
            SqlDataReader dr = null;
            try
            {
                dr = command.ExecuteReader();

                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    NextId = int.Parse(dr["NextId"].ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("NavisionDbConnection.GetNextCardHistoryId.Error", GetPos(), e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return NextId;
        }


    

        /// <summary>
        /// Récupération des informations de la carte
        /// Le numéro de carte est représenté par le token
        /// </summary>
        /// <param name="customer">Numéro du client</param>
        /// <param name="cc1">Numéro du centre de coût 1</param>
        /// <param name="traveller">Numéro du voyageur</param>
        /// <param name="service">service group (air, rail, ...)</param>
        /// <returns>UserBookingPaymentRSResult de la carte depuis Navision</returns>
        public UserBookingPaymentRSResult GetUserBookingPayment(string customer, string cc1, string traveller, string service)
        {
            UserBookingPaymentRSResult UserBooking = new UserBookingPaymentRSResult();

            SqlCommand dCmd = new SqlCommand("getCardNumberFromPercode", GetConnection());
            dCmd.CommandType = CommandType.StoredProcedure;

            SqlDataReader myReader = null;
            try
            {
                // On passe les paramètres
                dCmd.Parameters.Add("@percode", SqlDbType.NVarChar, 20);
                dCmd.Parameters["@percode"].Value = traveller;
                dCmd.Parameters.Add("@comcode", SqlDbType.NVarChar, 20);
                dCmd.Parameters["@comcode"].Value = customer;
                dCmd.Parameters.Add("@service", SqlDbType.NVarChar, 30);
                dCmd.Parameters["@service"].Value = service;
                dCmd.Parameters.Add("@cc1", SqlDbType.NVarChar, 30);
                dCmd.Parameters["@cc1"].Value = cc1;

                myReader = dCmd.ExecuteReader();
                // On exécute et on récupère les informations
                if (myReader.Read())
                {
                    UserBooking.SetValues(
                        myReader["token"].ToString(),
                        myReader["truncatedPAN"].ToString(),
                        Util.GetSQLDataTime(myReader, "expiration_date"),
                        Util.GetSQLDataTime(myReader, "creation_date"),
                        myReader["card_type"].ToString(),
                        myReader["card_type2"].ToString(),
                        myReader["origin"].ToString(),
                        myReader["cvv2"].ToString(),
                        myReader["service"].ToString(),
                        String.IsNullOrEmpty(myReader["card_type"].ToString())?0: Util.GetSQLInt(myReader, "lodgedCard"),
                        myReader["financialFlow"].ToString(),
                        myReader["error_code"].ToString(),
                        myReader["error_msg"].ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("NavisionDbConnection.GetUserBookingPayment.Error",
                    customer, cc1, traveller, service, e.Message, true));
            }
            finally
            {
                CloseDataReader(myReader, dCmd);
            }
            return UserBooking;

        }


        /// <summary>
        /// Récupération des informations sur le mode de paiement
        /// </summary>
        /// <param name="customer">Numéro du client</param>
        /// <param name="traveller">Numéro du voyageur</param>
        /// <param name="service">service group (air, rail, ...)</param>
        /// <returns>UserPaymentTypeResult (infos mode de paiement)</returns>
        public UserPaymentTypeResult GetUserPaymentType(string customer, string cc1, string traveller, string service)
        {
            UserPaymentTypeResult PaymentType = new UserPaymentTypeResult();

            SqlCommand dCmd = new SqlCommand("getPaymentTypePercode", GetConnection());
            dCmd.CommandType = CommandType.StoredProcedure;

            SqlDataReader myReader = null;
            try
            {
                // On passe les paramètres
                dCmd.Parameters.Add("@comcode", SqlDbType.NVarChar, 20);
                dCmd.Parameters["@comcode"].Value = customer;
                dCmd.Parameters.Add("@percode", SqlDbType.NVarChar, 20);
                dCmd.Parameters["@percode"].Value = traveller;
                dCmd.Parameters.Add("@service", SqlDbType.NVarChar, 30);
                dCmd.Parameters["@service"].Value = service;
                dCmd.Parameters.Add("@costCenter", SqlDbType.NVarChar, 30);
                dCmd.Parameters["@costCenter"].Value = Util.Nvl(cc1, string.Empty);

                myReader = dCmd.ExecuteReader();
                // On exécute et on récupère les informations
                while (myReader.Read())
                {
                    // payment_type, origin, service, error_code, error_msg, cc1
                    PaymentType.SetValues(
                     myReader["payment_type"].ToString(),
                     myReader["origin"].ToString(),
                     myReader["service"].ToString(),
                     int.Parse(Util.Nvl(myReader["error_code"].ToString(), "0")),
                     myReader["error_msg"].ToString(),
                     myReader["cc1"].ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("NavisionDbConnection.GetUserPaymentType.Error", customer, traveller, service, e.Message, true));
            }
            finally
            {
                CloseDataReader(myReader, dCmd);
            }
            return PaymentType;

        }




        /// <summary>
        /// Suppression d'une carte dans Navision
        /// La carte est supprimée de la table de cartes
        /// et cette suppression est historisée
        /// De plus, ce changement dans la stucture des cartes
        /// demande le recalcul des moyens de paiement
        /// et demande une mise à jour de la carte sur Amadeus (via le MID)
        /// </summary>
        /// <param name="customer">Code du client</param>
        /// <param name="traveler">Code du voyageur</param>
        /// <param name="cardReference">Référénce de la carte</param>
        public void DeleteProfilCard(string customer, string cc1, string traveler, int serviceGroup)
        {
            // On construit la chaîne
            string request = string.Format("DELETE FROM [EGENCIA {0}$Payment Card] where [Traveller No_]=@traveller and "
                + "[Customer No_]=@customer and [Service Group]=@serviceGroup and [Analytical Code 1]=@cc1 and Status=@status", GetPos());


            // On va démarrer une transaction
            // afin de gérer les éventuelles pb lors du traitement
            // et être en mesure d'effectuer une annulation sur plusieurs tables
            // payment card, payment card history, ...
            SqlTransaction MyTrans = GetNewTransaction();


            try
            {
                // On va vérifier si la carte existe
                NavisionCardResult cardInNavision = GetProfilCard(user, customer, cc1, traveler, serviceGroup, MyTrans);
               
                if (cardInNavision == null)
                {
                    // La carte n'existe pas
                    // On ne peut pas aller plus loin
                    throw new Exception(GetMessages().GetString("NavisionDbConnection.DeleteCard.CardNotFound.Error", customer, Util.Nvl(cc1, string.Empty)
                        , Util.Nvl(traveler, string.Empty), Util.GetService(serviceGroup), true));
                }

                #region Suppression de la carte

                // objet command
                SqlCommand command = new SqlCommand(request, GetConnection(), MyTrans);

                try
                {
                    // La carte a été trouvée, on peut procéder à sa suppression
                    // Paramètres (bind variable)
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@traveller", SqlDbType.VarChar, 20);
                    command.Parameters.Add("@customer", SqlDbType.VarChar, 20);
                    command.Parameters.Add("@serviceGroup", SqlDbType.Int);
                    command.Parameters.Add("@cc1", SqlDbType.VarChar, 30);
                    command.Parameters.Add("@status", SqlDbType.Int);

                    command.Parameters["@traveller"].Value = Util.Nvl(traveler, string.Empty);
                    command.Parameters["@customer"].Value = customer;
                    command.Parameters["@serviceGroup"].Value = serviceGroup;
                    command.Parameters["@cc1"].Value = Util.Nvl(cc1, string.Empty); ;
                    command.Parameters["@status"].Value = CardInfos.NavisionCardStatusValid;

                    // On exécute la requête
                    command.ExecuteNonQuery();
                }
                finally
                {
                    // objet command
                    DisposeCommand(command);
                }

                #endregion Suppression de la carte

                #region Historisation des changements sur cette carte

                // Date de traitement (insertion, mise à jour)
                // sans les heures
                DateTime NowDateOnly = Util.getDateOnly(DateTime.Now);

                // Historisation des changements sur cette carte
                // On va signaler une mise à jour
                // Pour effectuer cette opération, nous avons besoin de retourner
                // le dernier id actuellement dans la base de données
                int id = GetNextCardHistoryId(MyTrans);

                // On effectue tout d'abord une insertion des anciennes valeurs
                // On récupère un nouvel id si on a des soucis de doublons du à l'appel
                // en multi threads du Bulk load
                id = InsertHistoryForCard(cardInNavision.GetCustomerNo(), cardInNavision.GetAnalyticalCode1(),
                    cardInNavision.GetTravellerNo(), cardInNavision.GetCardReference(), cardInNavision.GetToken(),
                    cardInNavision.GetDescription(), cardInNavision.GetServiceGroup(),
                    //--> EGE-62034 : Revamp - CCE - Change Financial flow update
                    //cardInNavision.GetCard(),
                    cardInNavision.GetFinancialflow(),
                    cardInNavision.GetEnhancedFlow(),
                    //--> EGE-62034 : Revamp - CCE - Change Financial flow update
                    cardInNavision.GetSharingType(), cardInNavision.GetExpirationDate(), cardInNavision.GetExtendedNo(),
                    cardInNavision.GetCardType(), cardInNavision.GetStatus(), cardInNavision.GetPaymentBTA(),
                    cardInNavision.GetPaymentAIRPLUS(), cardInNavision.GetLodgedCard(),
                    cardInNavision.GetFictiveBta(), cardInNavision.GetCVC(), cardInNavision.GetCardNo1(),
                    cardInNavision.GetCardNo2(), cardInNavision.GetCardNo3(), cardInNavision.GetCardNo4(), cardInNavision.GetCardNo5(),
                    NowDateOnly, MyTrans, id,
                    cardInNavision.GetUserCode(), cardInNavision.GetFirstTransactionCode(), cardInNavision.GetTransactionsEntries(), cardInNavision.GetTransactionsAmount(),
                    cardInNavision.GetDateSentCVC(),
                    cardInNavision.GetCreationDate(), cardInNavision.GetModificationDate(),
                    //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
                    //Const.HistCardOperationDelete, Const.HistCardCategoryBefore, cardInNavision.GetModificationOrder(), cardInNavision.GetBlocked());
                    Const.HistCardOperationDelete, Const.HistCardCategoryBefore, cardInNavision.GetModificationOrder(), cardInNavision.GetBlocked(), cardInNavision.GetFirstCardReference());
                    //<< EGE-79826 - [BO] Lodge Card - First card Enhancement
                    
                #endregion Historisation des changements sur cette carte


                // Les moyens de paiement doivent être recalculés
                // pour cette companie
                // Uniquement pour les cartes non transactionnelles
                RequestPaymentMeansReset(customer, cardInNavision.IsTransactional(), MyTrans);

                // Il faut egalement informer Amadeus
                RequestAmadeusAlignment(customer, cc1, traveler, cardInNavision.IsTransactional(), cardInNavision.GetSharingType(), MyTrans);

                // On peut valider la connexion
                MyTrans.Commit();
            }
            catch (Exception e)
            {
                // On va annuler la transaction
                MyTrans.Rollback();
                throw new Exception(GetMessages().GetString("NavisionDbConnection.DeleteCard.Error", customer, Util.Nvl(cc1, string.Empty)
                    , Util.Nvl(traveler, string.Empty), Util.GetService(serviceGroup), e.Message, true));
            }
            finally
            {
                MyTrans.Dispose();
            }
        }


 
        /// <summary>
        /// Returns a profil card
        /// If there is no card with the specified paramaters
        /// </summary>
        /// <param name="user">Application account</param>
        /// <param name="customer">customer code</param>
        /// <param name="cc1">Cost center 1</param>
        /// <param name="traveller">percode</param>
        /// <param name="serviceGroup">service group</param>
        /// <param name="myTrans">SQL transaction</param>
        /// <returns>NavisionCardResult</returns>
        private NavisionCardResult GetProfilCard(UserInfo user, string customer, string cc1, string traveler, int serviceGroup, SqlTransaction myTrans)
        {
            NavisionCardResult cardFound = null;

            // On construit la chaîne
            string request = "select [Customer No_],[Traveller No_] , [Card Reference], [Sharing Type], "
            + "[Card No_ 1], [Card No_ 2], [Card No_ 3], [Card No_ 4], [Card No_ 5], "
            + "[Expiration Date], [Extended No_], [Card Type], [Status], [Transactions Entries], "
            //--> EGE-62034 : Revamp - CCE - Change Financial flow update
            //+ "[Transactions Amount], [Bank], [Description], [Card], [Payment BTA], [LodgedCard], "
            + "[Transactions Amount], [Bank], [Description], [Card], [Enhanced Flow], [Payment BTA], [LodgedCard], "
            //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
            + "[PaymentAIRPLUS], [Analytical Code 1], [Service Group], [CreationDate], "
            + "[ModificationDate], [FictiveBta], [CVC], [first transaction code], [date sent CVC], "
            + "[user code], [Token], [FirstCardReference] "
            + "from [egencia " + GetPos() + "$Payment Card] (NOLOCK) "
            + "where [Traveller No_]=@traveller AND [Customer No_]=@customer AND [Service Group]=@serviceGroup AND [Analytical Code 1]=@cc1 AND [Status]=@status";


            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection(), myTrans);
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@traveller", SqlDbType.VarChar, 20);
                command.Parameters.Add("@customer", SqlDbType.VarChar, 20);
                command.Parameters.Add("@serviceGroup", SqlDbType.Int);
                command.Parameters.Add("@cc1", SqlDbType.VarChar, 30);
                command.Parameters.Add("@status", SqlDbType.Int);

                command.Parameters["@traveller"].Value = Util.Nvl(traveler, string.Empty);
                command.Parameters["@customer"].Value = customer;
                command.Parameters["@serviceGroup"].Value = serviceGroup;
                command.Parameters["@cc1"].Value = Util.Nvl(cc1, string.Empty);
                command.Parameters["@status"].Value = CardInfos.NavisionCardStatusValid;

                dr = command.ExecuteReader();

                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    cardFound = new NavisionCardResult();
                    cardFound.SetValues(dr);
                }
            }
            catch (Exception p)
            {
                throw new Exception(p.Message);
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return cardFound;
        }


   

        /// <summary>
        /// Update Expiration date for all credit card with the same token
        /// </summary>
        /// <param name="newToken">Token</param>
        /// <param name="cardInfos">Credit card infos</param>
        /// <param name="myTrans">Current SQL transaction</param>
        /// <param name="NowDateOnly">Date time</param>
        private void UpdateCardsWithToken(long newToken, CardInfos cardInfos, SqlTransaction myTrans, DateTime NowDateOnly)
        {

            // On construit la chaîne
            string request = "UPDATE [EGENCIA " + GetPos() + "$Payment Card] "
                    + "SET [ModificationDate]=@modificationDate "
                    + ", [user code]=@usercode "
                    + ", [Expiration date]=@expirationDate "
                    + ", [Description]=@description "
                    + "where [Token]=@token and Status=@status";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection(), myTrans);

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                // ------------------ Paramètres
                // Valeurs à mettre à jour                
                command.Parameters.Add("@modificationDate", SqlDbType.DateTime);
                command.Parameters.Add("@usercode", SqlDbType.VarChar, 20);
                command.Parameters.Add("@expirationDate", SqlDbType.DateTime);
                command.Parameters.Add("@description", SqlDbType.VarChar, 80);
                command.Parameters.Add("@status", SqlDbType.Int);
                // Clés
                command.Parameters.Add("@token", SqlDbType.VarChar, 30);

                //-------------------- Valeurs
                // Valeurs mettre à jour
                command.Parameters["@modificationDate"].Value = NowDateOnly;
                command.Parameters["@usercode"].Value = GetLogin();
                command.Parameters["@expirationDate"].Value = cardInfos.GetExpirationDate();              
                command.Parameters["@description"].Value = cardInfos.GetDescription();
                command.Parameters["@status"].Value = 0;
                // Clés
                command.Parameters["@token"].Value = newToken;

                // On exécute la requête
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Demande de recalcul des moyens de paiement
        /// pour le client
        /// </summary>
        /// <param name="customer">Code client</param>
        /// <param name="isTransactionalCard">Carte transactionnellee sur la carte</param>
        /// <param name="myTrans">Transaction SQL</param>
        private void RequestPaymentMeansReset(string customer,
            bool isTransactionalCard, SqlTransaction myTrans)
        {
            if (!isTransactionalCard)
            {
                // Le recalcul des moyens de paiement est uniquement demandé
                // pour les cartes non transactionnelles
                string objectValue = customer;
                string nature = "paymentmean";
               
                RequestFlagObject(objectValue, nature, myTrans);
            }
        }

        /// <summary>
        /// Demande de mise à jour des profiles dans Amadeus
        /// 
        /// </summary>
        /// <param name="customer">Code client</param>
        /// <param name="cc1">Centre de cout 1</param>
        /// <param name="traveler">code voyageur</param>
        /// <param name="isTransactionalCard">Carte transactionnelle</param>
        /// <param name="cardSharingType">Type de cartye (private, corporate)</param>
        /// <param name="myTrans">Transaction SQL</param>
        private void RequestAmadeusAlignment(string customer, string cc1
            , string traveler, bool isTransactionalCard, int cardSharingType, SqlTransaction myTrans)
        {
            if (isTransactionalCard)
            {
                // On ne va pas demander une mise à jour d'amadeus
                // pour les cartes transactionnelles
                return;
            }

            // Parametres
            string objectValue = string.Empty;
            string nature = "midPaymeansSynchro";
            string flowStatus = string.Empty;

            if (cardSharingType == CardInfos.NavisionSharingTypePrivate)
            {
                // Carte privée, on considère le voyageur
                objectValue = string.Format("per{0}", traveler);
            }
            else
            {
                // Carte corporate
                if (String.IsNullOrEmpty(cc1))
                {
                    // pas de CC1, on utilise le code de la compagnie
                    objectValue = string.Format("com{0}", customer);
                }
                else
                {
                    // on un CC1
                    objectValue = string.Format("com{0}¤cc1{1}", customer, cc1);
                }
            }

            RequestFlagObject(objectValue, nature, myTrans);
        }


        /// <summary>
        /// Demande de mise à jour de process
        /// indiqué par la nature
        /// </summary>
        /// <param name="objectValue">Nom de l'objet</param>
        /// <param name="nature">Nature de l'objet</param>
        /// <param name="myTrans">Transaction SQL</param>
        private void RequestFlagObject(string objectValue, string nature, SqlTransaction myTrans)
        {

            SqlCommand command = new SqlCommand("Flag_Workflow_Objects", GetConnection(), myTrans);
            command.CommandType = CommandType.StoredProcedure;
            try
            {
                // On passe les paramètres
                command.Parameters.Add("@Object", SqlDbType.NVarChar, 50);
                command.Parameters["@Object"].Value = objectValue;
                command.Parameters.Add("@Nature", SqlDbType.NVarChar, 50);
                command.Parameters["@Nature"].Value = nature;
                command.Parameters.Add("@POSID", SqlDbType.NVarChar, 30);
                command.Parameters["@POSID"].Value = GetPos();
                command.Parameters.Add("@FlagYN", SqlDbType.Int);
                command.Parameters["@FlagYN"].Value = 0;
                command.Parameters.Add("@Flowstatus", SqlDbType.NVarChar, 1);
                command.Parameters["@Flowstatus"].Value = string.Empty;

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                DisposeCommand(command);
            }
        }        



        /// <summary>
        /// Historisation des changements pour une carte
        /// Cette fonction permet d'insérer une ligne dans
        /// la table 
        /// </summary>
        /// <param name="customer">Code client</param>
        /// <param name="cc1">Centre de coût 1</param>
        /// <param name="traveller">Code voyageur</param>
        /// <param name="refCard">Référence de la carte</param>
        /// <param name="token">Token de la carte</param>
        /// <param name="description">Description</param>
        /// <param name="servicegroup">Service group</param>
        /// <param name="card">Card</param>
        /// <param name="sharingType">Carte private ou corporate</param>
        /// <param name="expirationDate">Date d'expiration</param>
        /// <param name="extendedNo">No étendu</param>
        /// <param name="cardType">Type de carte</param>
        /// <param name="status">Statut</param>
        /// <param name="paymentBTA">Payment BTA</param>
        /// <param name="paymentAIRPLUS">Payment AIRPLUS</param>
        /// <param name="lodgedCard">LodgedCard</param>
        /// <param name="fictiveBTA">Fictive BTA</param>
        /// <param name="cvc">CVC</param>
        /// <param name="card1">4 premiers caractères du numéro masqué</param>
        /// <param name="card2">4 caractères suivants du numéro masqué</param>
        /// <param name="card3">4 caractères suivants du numéro masqué</param>
        /// <param name="card4">4 (ou 3) derniers caractères du numéro masqué</param>
        /// <param name="card5">4 (ou 3) derniers caractères du numéro masqué</param>
        /// <param name="NowDateOnly">Date sans heures</param>
        /// <param name="myTrans">Transaction SQL</param>
        /// <param name="id">Identifiant pour l'historisation</param>
        /// <param name="userCode">Compte utilisater</param>
        /// <param name="firstTransactionCode">Code première transaction</param>
        /// <param name="transactionsEntries">Nombre de transactions</param>
        /// <param name="transactionsAmount">Montant de transactions</param>
        ///  <param name="dateSendCVC">dateSendCVC</param>
        /// <param name="creationDate">Date de création</param>
        /// <param name="modificationDate">Date de dernière modification</param>
        /// <param name="operation">Opération (INSERT, DELETE, UPDATE)</param>
        /// <param name="category">Catégorie (BEFORE, AFTER)</param>
        /// <param name="modificationOrder">Modifcation order<param>
        /// <param name="blocked">blocked<param>
        /// <param name="firstCardReference">firstCardReference<param>
        /// <returns>Id de l'insertion</returns>
        private int InsertHistoryForCard(string customer, string cc1, string traveller,
            string refCard, long token, string description, int servicegroup,
            //--> EGE-62034 : Revamp - CCE - Change Financial flow update
            // string card
            string financialFlow, string enhancedFlow,
            //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
            int sharingType, DateTime expirationDate, string extendedNo, int cardType, int status,
            int paymentBTA, int paymentAIRPLUS, int lodgedCard, int fictiveBTA, string cvc,
            string card1, string card2, string card3, string card4, string card5,
            DateTime NowDateOnly, SqlTransaction myTrans, int inputId,
            string userCode, string firstTransactionCode, int transactionsEntries, decimal transactionsAmount,
            DateTime dateSendCVC,
            DateTime creationDate, DateTime modificationDate,
            //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
            //string operation, string category, string modificationOrder, int blocked)
            string operation, string category, string modificationOrder, int blocked, string firstCardReference)
           //<< EGE-79826 - [BO] Lodge Card - First card Enhancement
        
        {

            // On prépare l'insertion dans la table
            string requestHist = "INSERT INTO [EGENCIA " + GetPos() + "$Payment Card History]"
            + "([Customer No_],[Traveller No_],[Card Reference],[Sharing Type], [Analytical Code 1],"
            + "[Expiration Date], [Extended No_], [Card Type], [Status], [Transactions Entries], [Transactions Amount],"
            + "[Bank],[Description], [Card], [Enhanced Flow], [Payment BTA], [LodgedCard], [PaymentAIRPLUS], [Service Group],"
            + "[CreationDate], [ModificationDate], [FictiveBta], [CVC], [first transaction code], [date sent CVC],"
            + "[user code], [Token],"
            + "[Card No_ 1], [Card No_ 2], [Card No_ 3], [Card No_ 4],[Card No_ 5],"
            //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
            //+ "id, category, operation, [Modification Order], [Blocked]) VALUES ("
            + "id, category, operation, [Modification Order], [Blocked], [FirstCardReference]) VALUES ("            
            //<< EGE-79826 - [BO] Lodge Card - First card Enhancement
            + "@customer, @traveller, @refCard, @sharingType, @cc1,"
            + "@expirationDate, @ExtendedNo, @cardType, @status, @transactionsEntries, @transactionsAmount,"
            + "@bank, @description, @card, @enhancedFlow, @paymentBTA, @lodgedCard, @paymentAirplus, @serviceGroup,"
            + "@creationDate, @modificationDate,@fictiveBTA, @cvc,@firstTransactionCode, @dateSendCVC,"
            + "@usercode, @token, "
            + "@card1, @card2, @card3, @card4, @card5, "
            //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
            //+ "@id, @category, @operation, @modificationOrder, @blocked)";
            + "@id, @category, @operation, @modificationOrder, @blocked, @firstCardReference)";            
            //<< EGE-79826 - [BO] Lodge Card - First card Enhancement

            // Initilisation de l'indicateur d'insertion
            int InsertValueStatus = EncryptedDataConnection.ValueNotYetInserted;

            // On prend la valeur envoyée lors de l'appel de cette fonction
            int id = inputId;

            // Nombre de tentatives d'insertion
            int nrTry = 0;

            while (InsertValueStatus == EncryptedDataConnection.ValueNotYetInserted)
            {
                // On va essayer d'insérer à chaque fois que l'on a des erreurs de doublons
                // et à chaque tentative on va récupérer la dernière valeur de l'id dans la table

                // objet command
                SqlCommand comdHist = new SqlCommand(requestHist, GetConnection(), myTrans);

                if (nrTry > 0 || (nrTry == 0 && id == -1))
                {
                    // On va récupérer le dernier id
                    id = GetNextCardHistoryId(myTrans);
                }

                try
                {
                    comdHist.Parameters.Add("@customer", SqlDbType.VarChar, 20);
                    comdHist.Parameters.Add("@traveller", SqlDbType.VarChar, 20);
                    comdHist.Parameters.Add("@refCard", SqlDbType.VarChar, 20);
                    comdHist.Parameters.Add("@sharingType", SqlDbType.Int);
                    comdHist.Parameters.Add("@cc1", SqlDbType.VarChar, 30);
                    comdHist.Parameters.Add("@expirationDate", SqlDbType.DateTime);
                    comdHist.Parameters.Add("@ExtendedNo", SqlDbType.VarChar, 10);
                    comdHist.Parameters.Add("@cardType", SqlDbType.Int);
                    comdHist.Parameters.Add("@status", SqlDbType.Int);
                    comdHist.Parameters.Add("@transactionsEntries", SqlDbType.Int);
                    comdHist.Parameters.Add("@transactionsAmount", SqlDbType.Decimal);
                    comdHist.Parameters.Add("@bank", SqlDbType.VarChar, 10);
                    comdHist.Parameters.Add("@description", SqlDbType.VarChar, 80);
                    comdHist.Parameters.Add("@card", SqlDbType.VarChar, 30);
                    //--> EGE-62034 : Revamp - CCE - Change Financial flow update
                    comdHist.Parameters.Add("@enhancedFlow", SqlDbType.VarChar, 30);
                    //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
                    comdHist.Parameters.Add("@paymentBTA", SqlDbType.Int);
                    comdHist.Parameters.Add("@lodgedCard", SqlDbType.Int);
                    comdHist.Parameters.Add("@paymentAirplus", SqlDbType.Int);
                    comdHist.Parameters.Add("@serviceGroup", SqlDbType.Int);
                    comdHist.Parameters.Add("@creationDate", SqlDbType.DateTime);
                    comdHist.Parameters.Add("@modificationDate", SqlDbType.DateTime);
                    comdHist.Parameters.Add("@fictiveBTA", SqlDbType.Int);
                    comdHist.Parameters.Add("@cvc", SqlDbType.VarChar, 10);
                    comdHist.Parameters.Add("@firstTransactionCode", SqlDbType.VarChar, 30);
                    comdHist.Parameters.Add("@dateSendCVC", SqlDbType.DateTime);
                    comdHist.Parameters.Add("@usercode", SqlDbType.VarChar, 20);
                    comdHist.Parameters.Add("@token", SqlDbType.VarChar, 30);
                    comdHist.Parameters.Add("@card1", SqlDbType.VarChar, 4);
                    comdHist.Parameters.Add("@card2", SqlDbType.VarChar, 4);
                    comdHist.Parameters.Add("@card3", SqlDbType.VarChar, 4);
                    comdHist.Parameters.Add("@card4", SqlDbType.VarChar, 4);
                    comdHist.Parameters.Add("@card5", SqlDbType.VarChar, 4);
                    comdHist.Parameters.Add("@id", SqlDbType.Int);
                    comdHist.Parameters.Add("@category", SqlDbType.VarChar, 10);
                    comdHist.Parameters.Add("@operation", SqlDbType.VarChar, 10);
                    comdHist.Parameters.Add("@modificationOrder", SqlDbType.VarChar, 30);
                    comdHist.Parameters.Add("@blocked", SqlDbType.Int);

                    //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
                    comdHist.Parameters.Add("@firstCardReference", SqlDbType.VarChar, 20);
                    //<< EGE-79826 - [BO] Lodge Card - First card Enhancement


                    // Affectation valeurs
                    comdHist.Parameters["@customer"].Value = customer;
                    comdHist.Parameters["@traveller"].Value = traveller;
                    comdHist.Parameters["@refCard"].Value = refCard;
                    comdHist.Parameters["@sharingType"].Value = sharingType;
                    comdHist.Parameters["@cc1"].Value = cc1;
                    comdHist.Parameters["@expirationDate"].Value = expirationDate;
                    comdHist.Parameters["@ExtendedNo"].Value = extendedNo;
                    comdHist.Parameters["@cardType"].Value = cardType;
                    comdHist.Parameters["@status"].Value = status;
                    comdHist.Parameters["@transactionsEntries"].Value = transactionsEntries;
                    comdHist.Parameters["@transactionsAmount"].Value = transactionsAmount;
                    comdHist.Parameters["@bank"].Value = "";
                    comdHist.Parameters["@description"].Value = Util.Nvl(description, string.Empty);
                    comdHist.Parameters["@card"].Value = financialFlow;
                    //--> EGE-62034 : Revamp - CCE - Change Financial flow update
                    comdHist.Parameters["@enhancedFlow"].Value = enhancedFlow;
                    //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
                    comdHist.Parameters["@paymentBTA"].Value = paymentBTA;
                    comdHist.Parameters["@lodgedCard"].Value = lodgedCard;
                    comdHist.Parameters["@paymentAirplus"].Value = paymentAIRPLUS;
                    comdHist.Parameters["@serviceGroup"].Value = servicegroup;
                    comdHist.Parameters["@creationDate"].Value = creationDate;
                    comdHist.Parameters["@modificationDate"].Value = modificationDate;
                    comdHist.Parameters["@fictiveBTA"].Value = fictiveBTA;
                    comdHist.Parameters["@cvc"].Value = cvc;
                    comdHist.Parameters["@firstTransactionCode"].Value = firstTransactionCode;
                    comdHist.Parameters["@dateSendCVC"].Value = dateSendCVC;
                    comdHist.Parameters["@usercode"].Value = userCode;
                    comdHist.Parameters["@token"].Value = Util.ConvertTokenToString(token);
                    comdHist.Parameters["@card1"].Value = card1;
                    comdHist.Parameters["@card2"].Value = card2;
                    comdHist.Parameters["@card3"].Value = card3;
                    comdHist.Parameters["@card4"].Value = card4;
                    comdHist.Parameters["@card5"].Value = card5;
                    comdHist.Parameters["@id"].Value = id;
                    comdHist.Parameters["@category"].Value = category;
                    comdHist.Parameters["@operation"].Value = operation;
                    comdHist.Parameters["@modificationOrder"].Value = modificationOrder;
                    comdHist.Parameters["@blocked"].Value = blocked;
                    //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
                    comdHist.Parameters["@firstCardReference"].Value = String.IsNullOrEmpty(firstCardReference) ? string.Empty : (firstCardReference.Length<8?firstCardReference:firstCardReference.ToUpper().Substring(0, 8));
                    //>> EGE-79826 - [BO] Lodge Card - First card Enhancement


                    // On exécute la requête
                    comdHist.ExecuteNonQuery();

                    // Les changements de la carte ont été enregistrées
                    InsertValueStatus = EncryptedDataConnection.ValueInserted;
                }
                catch (SqlException s)
                {
                    if (s.Number == EncryptedDataConnection.KeyViolationErrorNumber)
                    {
                        // Erreur de violation de clé lors de l'insertion
                        // Cela veut dire que l'id généré existe déjà dans la base
                        // 
                        // Dans ce cas, on va réessayer avec un nouvel id jusqu'à insertion
                    }
                    else
                    {
                        // Une exception a été levée lors de l'insertion
                        InsertValueStatus = EncryptedDataConnection.ValueError;
                        // On retourne l'erreur
                        throw new Exception(GetMessages().GetString("NavisionDbConnection.InsertHistoryForCard.Error", s.Message, true));
                    }

                }
                catch (Exception c)
                {
                    InsertValueStatus = EncryptedDataConnection.ValueError;
                    throw new Exception(c.Message);
                }
                finally
                {
                    nrTry++;
                    DisposeCommand(comdHist);
                }
            }
            return id;
        }



        /// <summary>
        /// Insertion d'un ID VPayment dans la base Navision
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="id">ID VPayment</param>
        /// <param name="args">Liste des arguments</param>
        public void InsertVPaymentIDForHotel(UserInfo user, string id, ArgsForVPaymentIDHotel args)
        {
            string request = "INSERT INTO VPaymentIDs ("
                + "id, pos, travelerCode, travelerName, cc1, cc2, bookingType, insertUser, insertDate, bookingDate, "
                + "hotelType_H, hotelName_H, city_H, zipCode_H, arrivalDate_H, departureDate_H, country_H, "
                + "tripType_LC, departureFrom_LC, goingTo_LC, departureDate_LC, returnDate_LC, company_LC) "
                + "VALUES (@id, @pos, @travelerCode, @travelerName, @cc1, @cc2, @bookingType, @insertUser, @insertDate, @bookingDate, "
                + "@hotelType, @hotelName, @city, @zipCode, @arrivalDate, @departureDate, @country, "
                + "@empty, @empty, @empty, @emptyDate, @emptyDate, @empty)";


            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // On prépare l'instruction en passant les variables
                // Champs communs
                command.Parameters.Add("@id", SqlDbType.VarChar, 15);
                command.Parameters.Add("@pos", SqlDbType.VarChar, 50);
                command.Parameters.Add("@travelerCode", SqlDbType.VarChar, 20);
                command.Parameters.Add("@travelerName", SqlDbType.VarChar, 50);
                command.Parameters.Add("@cc1", SqlDbType.VarChar, 30);
                command.Parameters.Add("@cc2", SqlDbType.VarChar, 30);
                command.Parameters.Add("@bookingType", SqlDbType.VarChar, 30);
                command.Parameters.Add("@insertUser", SqlDbType.VarChar, 50);
                command.Parameters.Add("@insertDate", SqlDbType.DateTime);
                command.Parameters.Add("@bookingDate", SqlDbType.DateTime);
                // Champs hotel
                command.Parameters.Add("@hotelType", SqlDbType.VarChar, 30);
                command.Parameters.Add("@hotelName", SqlDbType.VarChar, 100);
                command.Parameters.Add("@city", SqlDbType.VarChar, 100);
                command.Parameters.Add("@zipCode", SqlDbType.VarChar, 30);
                command.Parameters.Add("@arrivalDate", SqlDbType.DateTime);
                command.Parameters.Add("@departureDate", SqlDbType.DateTime);
                command.Parameters.Add("@country", SqlDbType.VarChar, 30);
                // Champs LC à mettre en defaut
                command.Parameters.Add("@empty", SqlDbType.VarChar, 30);
                command.Parameters.Add("@emptyDate", SqlDbType.DateTime);


                // Valeurs communes
                command.Parameters["@id"].Value = id;
                command.Parameters["@pos"].Value = args.GetPOS().ToUpper();
                command.Parameters["@travelerCode"].Value = args.GetTravelerCode();
                command.Parameters["@travelerName"].Value = args.GetTravelerName();
                command.Parameters["@cc1"].Value = Util.Nvl(args.GetCC1(), string.Empty);
                command.Parameters["@cc2"].Value = Util.Nvl(args.GetCC2(), string.Empty);
                command.Parameters["@bookingType"].Value = args.GetBookingType();
                command.Parameters["@insertUser"].Value = user.GetLogin();
                command.Parameters["@insertDate"].Value = DateTime.Now;
                command.Parameters["@bookingDate"].Value = args.GetBookingDate();
                // Hotel values
                command.Parameters["@hotelType"].Value = args.GetHotelType();
                command.Parameters["@hotelName"].Value = args.GetHotelName();
                command.Parameters["@city"].Value = args.GetCity();
                command.Parameters["@zipCode"].Value = args.GetZipCode();
                command.Parameters["@arrivalDate"].Value = args.GetArrivalDate();
                command.Parameters["@departureDate"].Value = args.GetDepartureDate();
                command.Parameters["@country"].Value = args.GetCountry();
                // others default values
                command.Parameters["@empty"].Value = string.Empty;
                command.Parameters["@emptyDate"].Value =  Const.NavisionEmptyDate;


                // Exécution de la requête
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("NavisionDbConnection.InsertVPaymentID.Error", e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Insertion d'un ID VPayment dans la base Navision
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="id">ID VPayment</param>
        /// <param name="args">Liste des arguments</param>
        public void InsertVPaymentIDForLC(UserInfo user, string id, ArgsForVPaymentIDLC args)
        {
            string request = "INSERT INTO VPaymentIDs "
             + "(id, pos, travelerCode, travelerName, cc1, cc2, bookingType, insertUser, insertDate, bookingDate, "
             + "hotelType_H, hotelName_H, city_H, zipCode_H, arrivalDate_H, departureDate_H, country_H, "
             + "tripType_LC, departureFrom_LC, goingTo_LC, departureDate_LC, returnDate_LC, company_LC) "
             + "VALUES (@id, @pos, @travelerCode, @travelerName, @cc1, @cc2, @bookingType, @insertUser, @insertDate, @bookingDate, "
             + "@empty, @empty, @empty, @empty, @emptyDate, @emptyDate, @empty, "
             + "@tripType, @departureFrom, @goingTo, @departureDate, @returnDate, @company)";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                command.Parameters.Add("@id", SqlDbType.VarChar, 15);
                command.Parameters.Add("@pos", SqlDbType.VarChar, 50);
                command.Parameters.Add("@travelerCode", SqlDbType.VarChar, 20);
                command.Parameters.Add("@travelerName", SqlDbType.VarChar, 50);
                command.Parameters.Add("@cc1", SqlDbType.VarChar, 30);
                command.Parameters.Add("@cc2", SqlDbType.VarChar, 30);
                command.Parameters.Add("@bookingType", SqlDbType.VarChar, 30);
                command.Parameters.Add("@insertUser", SqlDbType.VarChar, 50);
                command.Parameters.Add("@insertDate", SqlDbType.DateTime);
                command.Parameters.Add("@bookingDate", SqlDbType.DateTime);
                // Champs LC
                command.Parameters.Add("@tripType", SqlDbType.VarChar, 30);
                command.Parameters.Add("@departureFrom", SqlDbType.VarChar, 30);
                command.Parameters.Add("@goingTo", SqlDbType.VarChar, 30);
                command.Parameters.Add("@departureDate", SqlDbType.DateTime);
                command.Parameters.Add("@returnDate", SqlDbType.DateTime);
                command.Parameters.Add("@company", SqlDbType.VarChar, 30);
                // Champs Hotel à mettre en defaut
                command.Parameters.Add("@empty", SqlDbType.VarChar, 30);
                command.Parameters.Add("@emptyDate", SqlDbType.DateTime);


                // Valeurs communes
                command.Parameters["@id"].Value = id;
                command.Parameters["@pos"].Value = args.GetPOS();
                command.Parameters["@travelerCode"].Value = args.GetTravelerCode();
                command.Parameters["@travelerName"].Value = args.GetTravelerName();
                command.Parameters["@cc1"].Value = Util.Nvl(args.GetCC1(), string.Empty);
                command.Parameters["@cc2"].Value = Util.Nvl(args.GetCC2(), string.Empty);
                command.Parameters["@bookingType"].Value = args.GetBookingType();
                command.Parameters["@insertUser"].Value = user.GetLogin();
                command.Parameters["@insertDate"].Value = DateTime.Now;
                command.Parameters["@bookingDate"].Value = args.GetBookingDate();
                // LC values
                command.Parameters["@tripType"].Value = args.GetTripType();
                command.Parameters["@departureFrom"].Value = args.GetDepartureFrom();
                command.Parameters["@goingTo"].Value = args.GetGoingTo();
                command.Parameters["@departureDate"].Value = args.GetDepartureDate();
                command.Parameters["@returnDate"].Value = args.GetReturnDate();
                command.Parameters["@company"].Value = args.GetCompany();

                // others default values
                command.Parameters["@empty"].Value = string.Empty;
                command.Parameters["@emptyDate"].Value = Const.NavisionEmptyDate;

                // Exécution de la requête
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("NavisionDbConnection.InsertVPaymentID.Error", e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Récupération des informations sur
        /// ID Vpayment depuis Navision
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="id">ID VPayment</param>
        /// <returns>Informations sur l'id</returns>
        public VPaymentIDData GetVPaymentInfos(UserInfo user, string id)
        {
            // On construit la chaîne
            string request = "select id, travelerCode, travelerName, insertUser, insertDate from VPaymentIDs (NOLOCK) where [id]=@id";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            VPaymentIDData retval = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@id", SqlDbType.VarChar, 15);

                command.Parameters["@id"].Value = id;

                dr = command.ExecuteReader();

                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    retval= new VPaymentIDData();
                    retval.SetID(dr["id"].ToString());
                    retval.SetTravelerCode(dr["travelerCode"].ToString());
                    retval.SetTravelerName(dr["travelerName"].ToString());
                    retval.SetUser(dr["insertUser"].ToString());
                    retval.SetInsertDate(Util.GetSQLDataTime(dr, "insertDate"));
                }
            }
            catch (Exception p)
            {
                throw new Exception(p.Message);
            }
            finally
            {
                CloseDataReader(dr, command);
            }

            return retval;
        }


        /// <summary>
        /// Retourne le type de paiement accepté par une companie aérienne
        /// </summary>
        /// <param name="corporation">Corporation code</param>
        /// <returns>Le type de paiement</returns>
        public string GetGDSPaymentTypeForCorporation(string corporation)
        {

            // On construit la chaîne
            string request = String.Format("SELECT "
                + "PaymentType AS PaymentType "
                + "FROM [EGENCIA {0}$Corporation] (NOLOCK) "
                + "WHERE [Corporation Code]=@corporation "
                + "AND [Service Group] = @serviceGroup", GetPos());

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;
            // Uniquement pour l'AIR
            int serviceGroup = Const.ServiceGroupAIR;
            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@corporation", SqlDbType.VarChar, 10);
                command.Parameters["@corporation"].Value = corporation;

                command.Parameters.Add("@serviceGroup", SqlDbType.Int);
                command.Parameters["@serviceGroup"].Value = serviceGroup;

                dr = command.ExecuteReader();
                
                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    // On a trouvé la corporation
                    // On récupère le mode de paiement autorisé
                    int pType = Util.ConvertStringToInt(dr["PaymentType"].ToString());

                    if (pType > (int)Const.CorporationPaymentType.CREDIT_CARD)
                    {
                        // Pour les cas non gérés, on retour le mode de paiement par défaut
                        return Const.CorporationPaymentTypeString[(int)Const.CorporationPaymentType.ALL];
                    }

                    return Const.CorporationPaymentTypeString[pType];
                }
                // Cette companie est visiblement introuvable!
                //>> EGE-103660
                //throw new Exception(GetMessages().GetString("CheckGDSPaymentTypeForCorporation.CanNotFindCorporation", corporation, GetPos(), true));
                return Const.PaymentTypeStringALL;
                //<< EGE-103660
            }
            catch (Exception p)
            {
                throw new Exception(GetMessages().GetString("CheckGDSPaymentTypeForCorporation.Error", corporation, p.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
        }



        /// <summary>
        /// Retourne le type de paiement accepté par le GDS (Amadeus)
        /// pour un client donné
        /// </summary>
        /// <param name="customerCode">Code client</param>
        /// <returns>Le type de paiement (CASH, ALL)</returns>
        public string GetGDSPaymentTypeForCustomer(string customerCode)
        {

            // On construit la chaîne
            string request = String.Format("SELECT noRTU FROM [EGENCIA {0}$Customer Information] (NOLOCK) WHERE [No_]=@customerCode", GetPos());

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@customerCode", SqlDbType.VarChar, 20);
                command.Parameters["@customerCode"].Value = customerCode;

                dr = command.ExecuteReader();

                // Normalement une ligne est renvoyée
                if (dr.Read())
                {
                    // On a trouvé le client
                    // On récupère le mode de paiement autorisé
                    int Type = Util.ConvertStringToInt(dr["noRTU"].ToString());

                    if (Type == (int) Const.CorporationPaymentType.CASH)
                    {
                        // Le mode de paiement est mis en cash
                        // dans amadeus
                        return Const.PaymentTypeStringCASH;
                    }
                    
                    return Const.PaymentTypeStringALL;
                }
                // Ce client est visiblement introuvable!
                throw new Exception(GetMessages().GetString("CheckCustomerForPos.CanNotFindCustomer", customerCode, GetPos(), true));
            }
            catch (Exception p)
            {
                throw new Exception(GetMessages().GetString("GDSPaymentTypeForCustomer.Error", customerCode, p.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
        }

        /// Test function Connection
        /// This is a dummy function to check that we can connect to database
        /// </summary>
        /// <exception cref="Exception"/>
        public void Test()
        {
            string request = "SELECT 1";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Run the statement
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                // Faced exception
                throw new Exception(GetMessages().GetString("CreditCardLogConnection.Test.Error", e.Message, false));
            }
            finally
            {
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Returns lodged card references
        /// </summary>
        /// <param name="requestorDetail">Request arguments</param>
        /// <returns>Lodged card references</returns>
        public LodgedCardReferencesData GetLodgedCardReferences(ArgsLodgedCardReferences requestorDetail)
        {
            // Prepare lodged card references container
            LodgedCardReferencesData retval = new LodgedCardReferencesData();

            string listLabels="'PR', 'PK', 'DS', 'KS', 'AU', 'AE', 'RZ', 'AK', 'IK', 'BD'" ;

            // Build SQL statetement
            string request = String.Format("SELECT "
                + "FIELD as 'KEY' "
                + ", CASE [MOVE] "
                + "WHEN 'MD' THEN  (CASE OPTION2 "
                + "		WHEN 'BDC_DATA' THEN 'FREETEXT1' "
                + "		WHEN 'BDC2_DATA' THEN 'FREETEXT2' "
                + "		WHEN 'BDCLIST_DATA' THEN 'FREETEXT3' "
                + "		ELSE '' "
                + "	END)  ELSE [MOVE] "
                + " END as 'LABEL' "
                + ", CASE [MOVE] "
                + "	WHEN 'CC4' THEN t.[Analytical Code 4] "
                + "	WHEN 'CC5' THEN t.[Analytical Code 5] "
                + "	WHEN 'FIXEDVALUE' THEN OPTION1 "
                + "	WHEN 'OFFICEID' THEN (SELECT IATANumber FROM  [EGENCIA {0}$General Parameters Egencia] p (nolock)) "
                + " END 'VALUE' "
                + "from [egencia {0}$customer_ref_translate] r (nolock) "
                + "left outer join [Egencia {0}$traveller] t (nolock) "
                + "on (t.[no_]=@percode and t.[Customer No_]=@customer) "
                + "where r.[no_]=@customer "
                + "and r.FIELD IN  ("
                + "{1}"
                + ")"
                , GetPos(), listLabels);

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            SqlDataReader dr = null;
            try
            {
                // Send parameters
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@customer", SqlDbType.VarChar, 20);
                command.Parameters["@customer"].Value = requestorDetail.GetCustomerCode();
                command.Parameters.Add("@percode", SqlDbType.VarChar, 20);
                command.Parameters["@percode"].Value = requestorDetail.GetTravelerCode();

                // Execute the statetement
                dr = command.ExecuteReader();
  
                // Get all references   
                // let's read sql data reader until the end...
                while (dr.Read())
                {
                    // Add this reference
                    retval.AddReference(dr);
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("NavisionDbConnection.GetCardProvider.Error"
                    , requestorDetail.GetPOS(), requestorDetail.GetCustomerCode(), e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;
        }


        /// <summary>
        /// Retourne la connexion SQL
        /// courante
        /// </summary>
        /// <returns>Connexion SQL</returns>
        private SqlConnection GetConnection()
        {
            return this.myConn;
        }

 
        /// <summary>
        /// Démarre une nouvelle transaction
        /// sur la connexion courante
        /// et la retourne
        /// </summary>
        /// <returns>Nouvelle transaction</returns>
        private SqlTransaction GetNewTransaction()
        {
            return GetConnection().BeginTransaction();
        }

        /// <summary>
        /// Renvoi du message
        /// correspondant à la langue
        /// de l'utilisateur
        /// </summary>
        /// <returns>Message (langue)</returns>
        private Messages GetMessages()
        {
            return GetUser().GetMessages();
        }

        /// <summary>
        /// Retourne le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
        private string GetLogin()
        {
            return GetUser().GetLogin().ToUpper();
        }

        /// <summary>
        /// Returns user
        /// </summary>
        /// <returns>User</returns>
        private UserInfo GetUser()
        {
            return this.user;
        }

        /// <summary>
        /// Set user
        /// </summary>
        /// <param name="value">User</param>
        private void SetUser(UserInfo value)
        {
            this.user = value;
        }

        /// <summary>
        /// Mise à jour de la connexion SQL
        /// </summary>
        /// <param name="conn">Connexion SQL</param>
        private void SetConnection(SqlConnection conn)
        {
            this.myConn = conn;
        }

        /// <summary>
        /// Fermeture du DataReader et SQLCommand
        /// et libération des ressources
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <param name="command">SQLCommand</param>
        private void CloseDataReader(SqlDataReader dr, SqlCommand command)
        {
            if (dr != null)
            {
                dr.Dispose();
            }
            DisposeCommand(command);
        }
        /// <summary>
        /// Fermeture du DataTable et SQLCommand
        /// et libération des ressources
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="command">SQLCommand</param>
        private void CloseDataTable(DataTable dt, SqlCommand command)
        {
            if (dt != null)
            {
                dt.Dispose();
            }
            DisposeCommand(command);
        }
        /// <summary>
        /// Libération des ressources SqlCommand
        /// </summary>
        /// <param name="command">SqlCommand</param>
        private void DisposeCommand(SqlCommand command)
        {
            if (command != null) command.Dispose();
        }

        /// <summary>
        /// Libération des ressources SqlDataAdapter
        /// </summary>
        /// <param name="adap">SqlDataAdapter</param>
        private void DisposeSqlDataAdapter(SqlDataAdapter adap)
        {
            if(adap!=null) adap.Dispose();
        }


    }

}