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
using SafeNetWS.messages;
using SafeNetWS.database.row;
using SafeNetWS.login;
using SafeNetWS.utils.crypting;
using SafeNetWS.database.result;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.creditcard.creditcardvalidator.bibit;
using SafeNetWS.utils.cache;


namespace SafeNetWS.database
{
    /// <summary>
    /// Cette classe définie une connection SQL à la base de données
    /// Date : 22 septembre 2009
    /// Author : Samatar
    ///</summary>


    public class EncryptedDataConnection
    {
        // Gestion de la violation de clé lors
        // de l'insertion avec 2 tokens identiques
        public const int ValueNotYetInserted = 0;
        public const int ValueInserted = 1;
        public const int ValueError = 2;
        // Code erreur violation de clé
        public const int KeyViolationErrorNumber = 2627;

        private SqlConnection myConn;

        private UserInfo user;

        /// <summary>
        /// Définition d'une nouvelle connexion
        /// </summary>
        /// <param name="useri">Comte utilisateur</param>
        public EncryptedDataConnection(UserInfo useri)
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
            string connString = Global.GetConnStrinEncr();
            if (connString != null)
            {
                // La chaine de connexion est en cache
                return connString;
            }

            // On construit la chaine de connexion
            connString = Util.BuildSQLConnectionString(EncDec.DecryptPassword(ConfigurationManager.ConnectionStrings["EncryptedDataConnectionString"].ConnectionString),
                ConfigurationManager.AppSettings["EncryptedDataConnectionMaxPoolSize"],
                ConfigurationManager.AppSettings["EncryptedDataConnectionMinPoolSize"]);

            // On sauvegarde la chaine de connexion
            Global.SetConnStringEncr(connString);

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
                // On met à jour le statut
                throw new Exception(GetMessages().GetString("EncryptedDB.ErrorConnecting", GetConnection().Database, e.Message, true));
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
                    throw new Exception(GetMessages().GetString("EncryptedDB.ErrorClosingConnection", e.Message, true));
                }
            }

        }


        /// <summary>
        /// Récupération du numéro de carte encrypté
        /// à partir du token
        /// </summary>
        /// <param name="token">identifiant l'enregistrement unique dans la table</param>
        /// <returns>numéro de carte encrypté</returns>
        public string GetEncryptedPAN(long token)
        {
            string retval = null;
            string request = "SELECT EncryptedPAN FROM Cards (NOLOCK) WHERE Token= @token";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@token", SqlDbType.BigInt);
                command.Parameters["@token"].Value = token;

                dr = command.ExecuteReader();

                // Normalement une seule ligne sera renvoyée
                if (dr.Read())
                {
                    retval = dr["EncryptedPAN"].ToString();
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetEncryptedPAN.ErrorGettingPAN", token, e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;

        }


        /// <summary>
        /// Récupération du token identifiant l'enregistrement
        /// unique dans la table à partir du numéro de carte encrypté
        /// </summary>
        /// <param name="encryptedPan">numéro de carte encrypté</param>
        /// <returns>token</returns>
        public long GetToken(string encryptedPan)
        {
            long retval = -1;
            string request = "SELECT Token FROM Cards (NOLOCK) WHERE EncryptedPAN= @encryptedPAN";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@encryptedPAN", SqlDbType.VarChar, 48);
                command.Parameters["@encryptedPAN"].Value = encryptedPan;

                dr = command.ExecuteReader();

                // Normalement une seule ligne sera renvoyée
                if (dr.Read())
                {
                    // On a trouvé un token pour le cryptogramme
                    retval = Util.ConvertStringToToken(dr["Token"].ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetToken.ErrorGettingToken", e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;

        }

        /// <summary>
        /// Récupération de tous les tokens depuis la table
        /// dans la base des données encryptées
        /// Il faut récupérer tous les tokens dont le
        /// cryptogramme n'a pas été encore mis à jour
        /// </summary>
        /// <returns>Tableaux de Tokens</returns>
        public EncryptedData GetAllTokens()
        {

            EncryptedData retval = new EncryptedData();
            string request = "SELECT Token, EncryptedPAN FROM Cards (NOLOCK) WHERE InternalUse=0";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            DataTable DT = new DataTable();
            SqlDataAdapter adapter = null;
            try
            {
                command.CommandType = CommandType.Text;

                adapter = new SqlDataAdapter(command);
                adapter.Fill(DT);
                if (DT.Rows.Count > 0)
                {
                    foreach (DataRow ds in DT.Rows)
                    {
                        retval.AddData(Util.ConvertStringToToken(ds["Token"].ToString()), ds["EncryptedData"].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetAllTokens.Error", e.Message, true));
            }
            finally
            {
                DisposeSqlDataAdapter(adapter);
                CloseDataTable(DT, command);
            }
            return retval;

        }

        /// <summary>
        /// Insertion d'une carte encryptée
        /// dans la base des cartes encryptés
        /// </summary>
        /// <param name="encryptedPan">numéro de carte encrypté</param>
        /// <returns>token de la valeur insérée</returns>
        public long InsertEncryptedCard(string encryptedPan)
        {
            long token = -1;
            string request = "INSERT INTO Cards(Token,EncryptedPAN,CreationDate,CreationUser,LastUpdateTime,LastUpdateUser,InternalUse)"
            + "VALUES (@token, @encryptedPAN, @creationDate, @creationUser, @lastUpdateDateTime, @lastUpdateUser, @keyRotationIndicatorFieldName)";


            // Initilisation de l'indicateur d'insertion
            int InsertValueStatus = ValueNotYetInserted;

            while (InsertValueStatus == ValueNotYetInserted)
            {
                // On passe dans cette boucle dés lors que la carte
                // n'a pas été insérée et qu'il n'y a pas d'erreur (autre que la violation de clé)
                // On va donc réessayer à chaque fois avec un nouveau token

                // objet command
                SqlCommand command = new SqlCommand(request, GetConnection());

                // On va générer un token
                token = Util.GenerateBOToken(GetUser());

                try
                {
                    // On prépare l'instruction en passant les varaibles
                    command.Parameters.Add("@token", SqlDbType.BigInt);
                    command.Parameters.Add("@encryptedPAN", SqlDbType.VarChar, 48);
                    command.Parameters.Add("@creationDate", SqlDbType.DateTime);
                    command.Parameters.Add("@creationUser", SqlDbType.VarChar, 100);
                    command.Parameters.Add("@lastUpdateDateTime", SqlDbType.DateTime);
                    command.Parameters.Add("@lastUpdateUser", SqlDbType.VarChar, 100);
                    command.Parameters.Add("@keyRotationIndicatorFieldName", SqlDbType.Int);

                    command.Parameters["@token"].Value = token;
                    command.Parameters["@encryptedPAN"].Value = encryptedPan;
                    command.Parameters["@creationDate"].Value = DateTime.Now;
                    command.Parameters["@creationUser"].Value = GetLogin();
                    command.Parameters["@lastUpdateDateTime"].Value = DateTime.Now;
                    command.Parameters["@lastUpdateUser"].Value = GetLogin();
                    command.Parameters["@keyRotationIndicatorFieldName"].Value = 0;

                    // Exécution de la requête
                    command.ExecuteNonQuery();

                    // Le token avec le cryptogramme a été correctement insérée
                    InsertValueStatus = ValueInserted;
                }
                catch (SqlException s)
                {
                    if (s.Number == KeyViolationErrorNumber)
                    {
                        // Erreur de violation de clé lors de l'insertion
                        // Cela veut dire que le token généré existe déjà dans la base
                        // On doit être dans une configuration avec au moins 2 serveurs
                        // désynchronisés
                        // Dans ce cas, on va réessayer avec un nouveau token jusqu'à insertion
                    }
                    else
                    {
                        // Une exception a été levée lors de l'insertion
                        InsertValueStatus = ValueError;
                        // On retourne l'erreur
                        throw new Exception(GetMessages().GetString("EncryptedDB.InsertEncryptedCard.Error", s.Message, true));
                    }

                }
                catch (Exception e)
                {
                    // Une exception a été levée lors de l'insertion
                    InsertValueStatus = ValueError;
                    // On retourne l'erreur
                    throw new Exception(GetMessages().GetString("EncryptedDB.InsertEncryptedCard.Error", e.Message, true));
                }
                finally
                {
                    // On libère les ressources
                    DisposeCommand(command);
                }
            }
            return token;
        }

        /// <summary>
        /// Mise à jour d'un cryptogramme de cartes
        /// Ce process est utilisé par la rotation de clés
        /// </summary>
        /// <param name="token">Token à mettre à jour</param>
        /// <param name="encryptedPan">Cryptogramme (après rotation)</param>
        public void UpdateEncryptedCard(long token, string encryptedPan)
        {
            string request = "UPDATE Cards SET EncryptedPAN = @encryptedPAN, InternalUse = @rotated, LastUpdateTime = @lastUpdateDateTime, LastUpdateUser = @lastUpdateUser WHERE Token =@token";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Préparation des variables bindées
                command.Parameters.Add("@encryptedPAN", SqlDbType.VarChar, 48);
                command.Parameters.Add("@rotated", SqlDbType.Int);
                command.Parameters.Add("@lastUpdateDateTime", SqlDbType.DateTime);
                command.Parameters.Add("@lastUpdateUser", SqlDbType.VarChar, 100);
                command.Parameters.Add("@token", SqlDbType.BigInt);
                // Affectation des variables bindées
                command.Parameters["@encryptedPAN"].Value = encryptedPan;
                command.Parameters["@rotated"].Value = 1;
                command.Parameters["@lastUpdateDateTime"].Value = DateTime.Now;
                command.Parameters["@lastUpdateUser"].Value = GetLogin();
                command.Parameters["@token"].Value = token;
                // Exécution de la requête
                command.ExecuteNonQuery();
                // Tout s'est bien passé
            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("EncryptedDB.UpdateEncryptedCard.Error", token, e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }

      
        /// <summary>
        /// Mise à jour d'un cryptogramme de cartes
        /// Ce process est utilisé pour la rotation de clés
        /// Si le process de mise à jour des cartes s'est achevé avec succès
        /// On remet l'indicateur pour tous les Tokens à 0
        /// </summary>
        public void SetRotationEnded()
        {
            string request = "UPDATE Cards SET InternalUse = @rotated WHERE InternalUse = @notrotated";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Préparation des variables bindées
                command.Parameters.Add("@rotated", SqlDbType.Int);
                command.Parameters.Add("@notrotated", SqlDbType.Int);
                // Affectation des variables bindées
                command.Parameters["@rotated"].Value = 0;
                command.Parameters["@notrotated"].Value = 1;

                // Exécution de la requête
                command.ExecuteNonQuery();
                // Tout s'est bien passé
            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("EncryptedDB.SetRotationEnded.Error", e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Construire le mapping entre les tokens
        /// BackOffice et FrontOffice
        /// </summary>
        /// <param name="BOToken">Token BackOffice</param>
        /// <param name="FOToken">Token FrontOffice</param>
        /// <param name="ExpirationDate">Token FrontOffice</param>
        public void SetTokensMapping(long BOToken, string FOToken, DateTime ExpirationDate)
        {
            string request = "INSERT INTO TokensMapping (TokenBack,TokenFront, ExpirationDate, CreationDate, CreationUser) "
            + "VALUES (@BOToken, @FOToken, @expirationDate, @creationDate, @creationUser)";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                command.Parameters.Add("@BOToken", SqlDbType.BigInt);
                command.Parameters.Add("@FOToken", SqlDbType.VarChar, 50);
                command.Parameters.Add("@creationDate", SqlDbType.DateTime);
                command.Parameters.Add("@expirationDate", SqlDbType.DateTime);
                command.Parameters.Add("@creationUser", SqlDbType.VarChar, 100);

                command.Parameters["@BOToken"].Value = BOToken;
                command.Parameters["@FOToken"].Value = FOToken;
                command.Parameters["@expirationDate"].Value = ExpirationDate;
                command.Parameters["@creationDate"].Value = DateTime.Now;
                command.Parameters["@creationUser"].Value = GetLogin();

                // Exécution de la requête
                command.ExecuteNonQuery();
            }
            catch (SqlException s)
            {
                if (s.Number == KeyViolationErrorNumber)
                {
                    // key violation error
                    // there is already keys in the mapping
                    // just ignore it
                }
                else
                {
                    // On retourne l'erreur
                    throw new Exception(GetMessages().GetString("EncryptedDB.SetTokensMapping.Error", BOToken, FOToken, s.Message, true));
                }

            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("EncryptedDB.SetTokensMapping.Error", BOToken, FOToken, e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }

        }

        /// <summary>
        /// Récupération du token BackOffice
        /// depuis le token FrontOffice
        /// Le contrôle va se faire dans la table de
        /// Mapping des tokens
        /// </summary>
        /// <param name="token">Token FrontOffice</param>
        /// <returns>TokensMappingResult</returns>
        public TokensMappingResult GetBOTokenFromMapping(string token)
        {
            TokensMappingResult mapping = new TokensMappingResult();

            string request = "SELECT TokenBack AS token, ExpirationDate AS expirationDate FROM TokensMapping (NOLOCK) WHERE TokenFront = @FOToken";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@FOToken", SqlDbType.VarChar, 100);
                command.Parameters["@FOToken"].Value = token;

                dr = command.ExecuteReader();

                // Normalement une seule ligne sera renvoyée
                if (dr.Read())
                {
                    // On a trouvé un token pour le cryptogramme
                    mapping.SetBOToken(Util.ConvertStringToToken(dr["token"].ToString()));
                    mapping.SetExpirationDate(Util.GetSQLDataTime(dr, "expirationDate"));
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetBOTokenFromMapping.Error", token, e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return mapping;

        }

        /// <summary>
        /// Insertion du statut de la réponse de RBS
        /// </summary>
        /// <param name="encryptedPan">Cryptogramme du PAN</param>
        public void InsertBibitResponseStatus(string encryptedPan)
        {
            string request = "INSERT INTO db_bibit.BibitCache (EncryptedPAN, LastAccessTime) VALUES (@encryptedPan, @lastAccessTime)";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // On prépare l'instruction en passant les variables
                command.Parameters.Add("@encryptedPan", SqlDbType.VarChar, 48);
                command.Parameters.Add("@lastAccessTime", SqlDbType.DateTime);

                command.Parameters["@encryptedPan"].Value = encryptedPan;
                command.Parameters["@lastAccessTime"].Value = DateTime.Now;

                // Exécution de la requête
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("EncryptedDB.CacheBibitResponseStatus.Error", e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Mise à jour du statut de la réponse de RBS
        /// </summary>
        /// <param name="encryptedPan">Cryptogramme du PAN</param>
        public void UpdateBibitResponseStatus(string encryptedPan)
        {
            string request = "UPDATE db_bibit.BibitCache SET LastAccessTime = @lastAccessTime WHERE EncryptedPAN= @encryptedPan" ;

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // On prépare l'instruction en passant les variables
                command.Parameters.Add("@lastAccessTime", SqlDbType.DateTime);
                command.Parameters.Add("@encryptedPan", SqlDbType.VarChar, 48);

                command.Parameters["@lastAccessTime"].Value = DateTime.Now;
                command.Parameters["@encryptedPan"].Value = encryptedPan;

                // Exécution de la requête
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("EncryptedDB.UpdateBibitResponseStatus.Error", e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Retire toutes les entrées du cache BIBIT
        /// </summary>
        /// <returns>Nombre d'entrées retirées du cache</returns>
        public int ClearBibitCache()
        {
            int retval = 0;
            string request = "DELETE FROM db_bibit.BibitCache";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Exécution de la requête
                retval=command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("EncryptedDB.ClearBibitCache.Error", e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
            return retval;
        }

        /// <summary>
        /// Récupération du statut de la dernière validation
        /// d'une carte via le service RBS
        /// </summary>
        /// <param name="encryptedPan">Cryptogramme du PAN</param>
        /// <returns>CachedBibitResult (Statut)</returns>
        public CachedValidationResult GetCachedBibitResponseStatus(string encryptedPan)
        {
            CachedValidationResult retval = new CachedValidationResult();

            string request = "SELECT LastAccessTime AS lastAccessTime FROM db_bibit.BibitCache (NOLOCK) WHERE EncryptedPAN= @encryptedPan";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@encryptedPan", SqlDbType.VarChar, 48);
                command.Parameters["@encryptedPan"].Value = encryptedPan;

                dr = command.ExecuteReader();

                // Normalement une seule ligne sera renvoyée
                if (dr.Read())
                {
                    // On a trouvé une entrée
                    DateTime lastCheckTime = Util.GetSQLDataTime(dr, "lastAccessTime");
                    if (Util.DateDiffInDays(lastCheckTime, DateTime.Now) <= 1)
                    {
                        // La carte a été testé (et accepté) il y a moins de 24h
                        retval.SetStatus(CachedValidationResult.CacheStatus.FoundValid);
                        retval.SetLastAccessTime(lastCheckTime);
                    }
                    else
                    {
                        // Ce test a expiré
                        retval.SetStatus(CachedValidationResult.CacheStatus.FoundExpired);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetCachedBibitResponseStatus.Error", e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;
        }



        /// <summary>
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
            if (adap != null) adap.Dispose();
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
            return GetUser().GetLogin();
        }
    }

}