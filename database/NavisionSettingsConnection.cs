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

namespace SafeNetWS.database
{
    /// <summary>
    /// This class allows to connect to Navision settings database
    /// Navision settings database contains all multi POS tables
    /// and define Navision connection for each POS 
    /// Date : 20 juillet 2013
    /// Author : Samatar
    ///</summary>

     
    public class NavisionSettingsConnection
    {

        private SqlConnection myConn;

        private UserInfo user;

        /// <summary>
        /// Define a new connection for Navision Settings
        /// </summary>
        /// <param name="useri">user</param>
        public NavisionSettingsConnection(UserInfo useri)
        {
            SetUser(useri);

            // Let's define a new connection
            SetConnection(new SqlConnection(GetConnString()));
        }

        /// <summary>
        /// Returns connection string
        /// /// </summary>
        /// <returns>Connection string</returns>
        private string GetConnString()
        {
            // We need to return here the connection string
            // First let's check in the cache
            // that will avoid us to rebuild it
           string connString = Global.GetConnStringNavSettings();

           if (connString != null)
           {
               // We have something in the cache
               // great, no need to continue
               // we will thie string and that's it.
               return connString;
           }

           // We don't have the connection string in the cache
           // we need to build it
           // There are two parts that need to be added
           // Database information (from Web.config file) 
           // and pooling information (from Web.config file) 
           connString = Util.BuildSQLConnectionString(EncDec.DecryptPassword(ConfigurationManager.ConnectionStrings["NavisionSettingsConnectionString"].ConnectionString),
                          ConfigurationManager.AppSettings["NavisionSettingsConnectionMaxPoolSize"],
                          ConfigurationManager.AppSettings["NavisionSettingsConnectionMinPoolSize"]);

           // Save connection string in the cache...for next time
           Global.SetConnStringNavSettings(connString);

           return connString;
        }

        /// <summary>
        /// Open the connection
        /// </summary>
        public void Open()
        {
            try
            {
                this.myConn.Open();
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("NavisionSettingsConnection.ErrorConnecting", GetConnection().Database, e.Message, true));
            }
        }

        /// <summary>
        /// Returns Navision connection string for the POS
        /// </summary>
        /// <param name="POS">Point Of Sale</param>
        /// <returns>Navision connection string</returns>
        public string GetNavisionConnectionString(string POS)
        {

            // objet command
            SqlCommand command = new SqlCommand("GetNavConnString", GetConnection());
            command.CommandType = CommandType.StoredProcedure;
            SqlDataReader dr = null;
            try
            {
                // Send parameters (bind variable)
                command.Parameters.Add("@pos", SqlDbType.VarChar, 15);
                command.Parameters["@pos"].Value = POS;

                dr = command.ExecuteReader();

                // Return connectiobn string
                if (dr.Read())
                {
                   // We have found something
                   // the connection string is correctly formated
                   return dr[0].ToString();
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("NavisionSettingsConnection.ErrorGettingConnString", POS, e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return null;
        }



        /// <summary>
        /// Close connection string
        /// </summary>
        public void Close()
        {
            if (GetConnection() != null)
            {
                // We have a connection to close
                try
                {
                    this.myConn.Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(GetMessages().GetString("NavisionSettingsConnection.ErrorClosingConnection", e.Message, true));
                }
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
                throw new Exception(GetMessages().GetString("NavisionSettingsConnection.Test.Error", GetConnection().Database,e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }



        /// <summary>
        /// Close du DataReader and SQLCommand
        /// and free memory
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
        /// Close DataTable and SQLCommand
        /// and free memory
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
        /// Dispose SqlCommand
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
            return GetUser().GetLogin().ToUpper();
        }

        /// <summary>
        /// Retourne le nom de l'application
        /// que le client a sollicité
        /// </summary>
        /// <returns>Nom application</returns>
        private string GetApplicationName()
        {
            return UserInfo.GetApplicationName(GetUser().GetApplication());
        }
    }

}