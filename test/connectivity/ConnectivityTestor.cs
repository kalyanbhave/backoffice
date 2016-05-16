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
using SafeNetWS.ENettService;
using SafeNetWS.database;
using SafeNetWS.business.response.writer;
using System.Configuration;
using SafeNetWS.business;
using SafeNetWS.creditcard.creditcardvalidator.bibit;
using SafeNetWS.login;
using SafeNetWS.login.ldap;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.creditcard.virtualcard.enett;

namespace SafeNetWS.test.connectivity
{
    public class ConnectivityTestor
    {


        /// <summary>
        /// This function test all components
        /// and return a status for each one
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Status for all components</returns>
        public static TestAllComponentsResponse TestAllComponents(TestAllComponentsResponse retval)
        {
            UserInfo user = retval.GetUser();

            // Let's start with AD
            try
            {
                // Connect to AD
                TestAD(user);
                // AD is available
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetActiveDirectoryFailed(e);
            }


            // Let's start with Navision database
            try
            {
                // Connect to database and perform test function
                TestNavisionDb(user);
                // Navision database is available
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetNavisionDababaseFailed(e);
            }


            // Let's test now encrypted BO database
            try
            {
                // Connect to database and perform test function
                TestEncryptedBODb(user);
                // Encrypted BO is available
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetEncryptedBODatabaseFailed(e);
            }


            // Let's test now encrypted FO database
            try
            {
                // Connect to database and perform test function
                TestEncryptedFODb(user);
                // Encrypted FO is available
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetEncryptedFODatabaseFailed(e);
            }

            // Let's test now Credit card log database
            try
            {
                // Connect to database and perform test function
                TestCreditCardLogDb(user);
                // Encrypted FO is available
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetCredicardLogDatabaseFailed(e);
            }


            // Let's test now VCard log database
            try
            {
                // Connect to database and perform test function
                TestVCardLogDb(user);
                // Encrypted FO is available
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetVCardLogDatabaseFailed(e);
            }

            // Let's test SafeNet
            try
            {
                // Let's encrypt fake value
                string valueToEncrypt = "encryptMePlease";
                // Send value for encryption
                string cryptedValue = Services.EncryptBOCard(user, valueToEncrypt);
                // SafeNet is available
                cryptedValue = Services.EncryptFOCard(user, valueToEncrypt);
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetSafeNetFailed(e);
            }


            // Now let's test RBS Worldplay
            try
            {
                TestRBSService(user);
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetRBSWssFailed(e);
            }

            // Now let's test ENett API
            try
            {
                TestENettService(user);
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetENettWssFailed(e);
            }

            // Now let's test Navision Ws
            try
            {
                TestNavisionService(user);
            }
            catch (Exception e)
            {
                // Something is wrong
                retval.SetNavisionWssFailed(e);
            }

            return retval;
        }

        /// <summary>
        /// Test AD connectivity
        /// </summary>
        /// <param name="user">User</param>
        private static void TestAD(UserInfo user)
        {
            LdapAuthentication adAuth = null;
            try
            {
                // Connection au LDAP pour vérifier le compte user
                adAuth = new LdapAuthentication(user.GetMessages());

                // Check
                adAuth.Test();

                // We are good here
                // Let's disconnect
            }
            finally
            {
                // On va fermer proprement la connexion
                // au serveur LDAP
                if (adAuth != null)
                {
                    try
                    {
                        adAuth.Disconnect();
                    }
                    catch (Exception) { } // On ignore cette erreur  
                }
            }

        }


        /// <summary>
        /// This function test Navision database
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="Exception"/>
        private static void TestNavisionDb(UserInfo user)
        {
            // Let's connect to Navision database
            NavisionDbConnection conn = null;
            try
            {
                // Define a new connection
                conn = new NavisionDbConnection(user, utils.Const.PosFrance);
                // Let's open the connection
                conn.Open();
                // the connection is opened..so for so good
                // it's time to run the test function
                conn.Test();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }


        /// <summary>
        /// This function test encrypted BO database
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="Exception"/>
        private static void TestEncryptedBODb(UserInfo user)
        {
            // Let's connect to Navision database
            EncryptedDataConnection conn = null;
            try
            {
                // Define a new connection
                conn = new EncryptedDataConnection(user);
                // Let's open the connection
                conn.Open();
                // the connection is opened..so for so good
                // it's time to run the test function
                conn.Test();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        /// <summary>
        /// This function test encrypted FO database
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="Exception"/>
        private static void TestEncryptedFODb(UserInfo user)
        {
            // Let's connect to Navision database
            EncryptedFODataConnection conn = null;
            try
            {
                // Define a new connection
                conn = new EncryptedFODataConnection(user);
                // Let's open the connection
                conn.Open();
                // the connection is opened..so for so good
                // it's time to run the test function
                conn.Test();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        /// <summary>
        /// This function test credit catf log database
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="Exception"/>
        private static void TestCreditCardLogDb(UserInfo user)
        {
            // Let's connect to Navision database
            CreditCardLogConnection conn = null;
            try
            {
                // Define a new connection
                conn = new CreditCardLogConnection(user);
                // Let's open the connection
                conn.Open();
                // the connection is opened..so for so good
                // it's time to run the test function
                conn.Test();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }


        /// <summary>
        /// This function test VCard log database
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="Exception"/>
        private static void TestVCardLogDb(UserInfo user)
        {
            // Let's connect to Navision database
            VCardLogConnection conn = null;
            try
            {
                // Define a new connection
                conn = new VCardLogConnection(user);
                // Let's open the connection
                conn.Open();
                // the connection is opened..so for so good
                // it's time to run the test function
                conn.Test();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }



        /// <summary>
        /// This function test RBS API
        /// </summary>
        /// <param name="user">User</param>
        private static void TestRBSService(UserInfo user)
        {
            // Test RBS worldplay
            BibitVerifier.CheckBibitVerifier(user);
        }

        /// <summary>
        /// Test connectivity for ENett API
        /// </summary>
        /// <param name="user">User</param>
        public static void TestENettService(UserInfo user)
        {
            vNettService client = null;
    
            try
            {
                // Define a new vNett service connection
                client = new vNettService();
           
                // Let's test Enett API
                // for a fictive Payment ID
                ENettGetVANDetails reader = EnettUtils.ReadGetVANDetails("<GetVANDetails><PaymentID>XXXXX</PaymentID></GetVANDetails>");

                // We will submit a fictive payment id and requets detail for it
                // Of course we won't receive sucessful response but we expect that the call will be done
                // without any connection issue
                GetVNettVANResponse result =Services.GetENettVANDetails(user, reader);
                
                // We have called successfully ENett API
            }
            finally
            {
                // dispose
                if (client != null) client.Dispose();
            }
        }

        public static void TestNavisionService(UserInfo user)
        {
            // Test Navision Wss
            NavServiceUtils.Test();
        }

    }
}
