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
using System.Data.SqlClient;
using System.Configuration;
using SafeNetWS.utils;
using SafeNetWS.utils.crypting;
using SafeNetWS.messages;
using SafeNetWS.login;
using SafeNetWS.creditcard;

namespace SafeNetWS.database
{
    /// <summary>
    /// Cette classe définie une connection SQL à la base de données
    /// qui contient les cartes rejetées lors de la validation RBS
    /// Cette 
    /// Date : 29 juillet 2010
    /// Author : Samatar
    ///</summary>

     
    public class CreditCardLogConnection
    {

        private SqlConnection myConn;
        private UserInfo user;

        /// <summary>
        /// Définition d'une nouvelle connexion
        /// </summary>
        /// <param name="useri">Comte utilisateur</param>
        public CreditCardLogConnection(UserInfo useri)
        {
            SetUser(useri);

            // On définit une nouvelle connexion
            SetConnection(new SqlConnection(GetConnString()));
        }

        /// <summary>
        /// Retourne la chaine de connexion
        /// à la base de données
        /// </summary>
        /// <returns>Chaine de connexion</returns>
        private string GetConnString()
        {
            // On recherche la chaine de connexion
            // dans le cache
            string connString = Global.GetConnStringRejectedCCLog();
            if (connString != null)
            {
                // La chaine de connexion est en cache
                return connString;
            }

            // On construit la chaine de connexion
            connString = Util.BuildSQLConnectionString(EncDec.DecryptPassword(ConfigurationManager.ConnectionStrings["CreditCardLogConnectionString"].ConnectionString),
                ConfigurationManager.AppSettings["CreditCardLogConnectionMaxPoolSize"],
                ConfigurationManager.AppSettings["CreditCardLogConnectionMinPoolSize"]);

            // On sauvegarde la chaine de connexion
            // en cache pour réutilisation ultérieure
            Global.SetConnStringRejectedCCLog(connString);

            return connString;
        }

        /// <summary>
        /// Ouverture d'une connection vers la
        /// base de données contenant les données encryptées
        /// </summary>
        public void Open()
        {
            try
            {
                this.myConn.Open();
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("CreditCardLogConnection.ErrorConnecting", GetConnection().Database, e.Message, true));
            }
        }


        /// <summary>
        /// Fermeture de la connection vers la
        /// base de données contenant les données encryptées
        /// Si un pool de connexions est utilisé, la connexion
        /// sera rendue dans le pool
        /// </summary>
        public void Close()
        {
            if (GetConnection() != null)
            {
                // Vérification du statut de le connection
                // fermeture et libération des ressources
                try
                {
                    this.myConn.Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(GetMessages().GetString("CreditCardLogConnection.ErrorClosingConnection", e.Message, true));
                }
            }

        }


        /// <summary>
        /// Insertion de la carte rejetée
        /// dans la table des traces
        /// </summary>
        /// <param name="card">Informations carte</param>
        /// <param name="orderCode">Numéro de la transaction BIBIT</param>
        /// <param name="status">Statut</param>
        /// <param name="fullError">Error complète</param>
        /// <param name="completeResponse">Response complète</param>
        public void LogCard(CardInfos card, string orderCode, string status, string fullError, string completeResponse)
        {
            string request = "INSERT INTO CreditCardLog (ComCode,PerCode,Service,Token,TruncatedPAN"
                + ",Source,OrderCode,Status,Error,CompleteResponse"
                + ",CreationDate,CreationUser,CardType) VALUES (@comcode, @percode, @service, @token, @truncatedPAN, "
                + "@source, @orderCode, @status, @fullError, @completeResponse, @creationDate, @creationUser,@cardType)";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                command.Parameters.Add("@comcode", SqlDbType.VarChar, 20);
                command.Parameters.Add("@percode", SqlDbType.VarChar, 20);
                command.Parameters.Add("@service", SqlDbType.VarChar, 50);
                command.Parameters.Add("@token", SqlDbType.VarChar, 50);
                command.Parameters.Add("@truncatedPAN", SqlDbType.VarChar, 20);
                command.Parameters.Add("@source", SqlDbType.VarChar, 50);
                command.Parameters.Add("@orderCode", SqlDbType.VarChar, 50);
                command.Parameters.Add("@status", SqlDbType.VarChar, 50);
                command.Parameters.Add("@fullError", SqlDbType.VarChar, 200);
                command.Parameters.Add("@completeResponse", SqlDbType.VarChar, 4000);
                command.Parameters.Add("@creationDate", SqlDbType.DateTime);
                command.Parameters.Add("@creationUser", SqlDbType.VarChar, 100);
                command.Parameters.Add("@cardType", SqlDbType.VarChar, 50);

                command.Parameters["@comcode"].Value = Util.Nvl(card.GetCustomerCode(), string.Empty);
                command.Parameters["@percode"].Value = Util.Nvl(card.GetTravellerCode(), string.Empty);
                command.Parameters["@service"].Value = Util.Nvl(card.GetService(), string.Empty);
                command.Parameters["@token"].Value = Util.Nvl(card.GetToken(), string.Empty);
                command.Parameters["@truncatedPAN"].Value = Util.Nvl(card.GetTruncatedPAN(), string.Empty);
                command.Parameters["@source"].Value = GetApplicationName();
                command.Parameters["@orderCode"].Value = Util.Nvl(orderCode, string.Empty);
                command.Parameters["@status"].Value = status;
                command.Parameters["@fullError"].Value = Util.Nvl(fullError, string.Empty);
                command.Parameters["@completeResponse"].Value = Util.Nvl(completeResponse, string.Empty);
                command.Parameters["@creationDate"].Value = DateTime.Now;
                command.Parameters["@creationUser"].Value = GetLogin();
                command.Parameters["@cardType"].Value = card.GetCardType();

                // Exécution de la requête
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("CreditCardLogConnection.LogCard.Error", e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }


        /// <summary>
        /// Test function for CreditCard log Connection
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
                throw new Exception(GetMessages().GetString("CreditCardLogConnection.Test.Error", e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
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
                // Dispose() internally calls Close()
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
        /// Retourne la connexion SQL
        /// courante
        /// </summary>
        /// <returns>Connexion SQL</returns>
        private SqlConnection GetConnection()
        {
            return this.myConn;
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
        /// Retourne le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
        private UserInfo GetUser()
        {
            return this.user;
        }

        /// <summary>
        /// Mise à jour du compte utilisateur
        /// </summary>
        /// <param name="useri">Compte utilisateur</param>
        private void SetUser(UserInfo useri)
        {
            this.user = useri;
        }

        /// <summary>
        /// Retourne le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
        private string GetLogin()
        {
            return this.user.GetLogin().ToUpper();
        }

        /// <summary>
        /// Retourne le nom de l'application
        /// que le client a sollicité
        /// </summary>
        /// <returns>Nom application</returns>
        private string GetApplicationName()
        {
            return UserInfo.GetApplicationName(this.user.GetApplication());
        }
    }

}