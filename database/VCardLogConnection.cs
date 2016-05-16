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
using SafeNetWS.ENettService;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.creditcard.virtualcard.enett;
using SafeNetWS.creditcard.creditcardvalidator;

namespace SafeNetWS.database
{
    /// <summary>
    /// This class log request and amend for Vcards
    ///</summary>

     
    public class VCardLogConnection
    {

        // SQL connection
        private SqlConnection myConn;
        // User (application)
        private UserInfo user;

        /// <summary>
        /// Define a new connection
        /// </summary>
        /// <param name="useri">User information</param>
        public VCardLogConnection(UserInfo useri)
        {
            // Set user
            SetUser(useri);

            // Define a new connection
            // get connection string and set connection
            SetConnection(new SqlConnection(GetConnString()));
        }

        /// <summary>
        /// Returns SQL connection string
        /// </summary>
        /// <returns>Connection string</returns>
        private string GetConnString()
        {
            // We need to return here the connection string
            // First let's check in the cache
            // that will avoid us to rebuild it
            string connString = Global.GetConnStringVCardLog();
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
            connString = Util.BuildSQLConnectionString(EncDec.DecryptPassword(ConfigurationManager.ConnectionStrings["VCardLogConnectionString"].ConnectionString),
                ConfigurationManager.AppSettings["VCardLogConnectionMaxPoolSize"],
                ConfigurationManager.AppSettings["VCardLogConnectionMinPoolSize"]);

            // We have the connection string
            // let's save it in cache..no next time
            Global.SetConnStringVCardLog(connString);

            // simply return the value
            return connString;
        }

        /// <summary>
        /// Open connection string to VCard log table
        /// the VcardLogConnection must be first defined
        /// 
        /// VcardLogConnection conn = new VcardLogConnection(user);
        /// and then ..
        /// conn.Open();
        /// 
        /// </summary>
        public void Open()
        {
            try
            {
                // Open connection
                this.myConn.Open();
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("VCardLogConnection.ErrorConnecting", GetConnection().Database, e.Message, true));
            }
        }


        /// <summary>
        /// Close VCard log connection
        /// </summary>
        public void Close()
        {
            if (GetConnection() != null)
            {
                // We have a connection to close
                try
                {
                    // and free memory
                    this.myConn.Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(GetMessages().GetString("VCardLogConnection.ErrorClosingConnection", e.Message, true));
                }
            }

        }


        /// <summary>
        /// Check if a record with paymenntID exists
        /// in the VCard log table
        /// </summary>
        /// <param name="reader">ENettRequestVAN detail</param>
        public bool IsVPaymentIDExist(string paymentID)
        {
            string request = "SELECT Id FROM VCardBooking WHERE PaymentID= @paymentID";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;
            try
            {
                // add parameters
                command.Parameters.Add("@paymentID", SqlDbType.VarChar, 16);
                command.Parameters["@paymentID"].Value = paymentID;

                // execute
                dr = command.ExecuteReader();

                // we need ro read the result..
                if (dr.Read())
                {
                    // The payment id already exists in the table
                    return true;
                }
            }
            catch (Exception e)
            {
                // Something went wrong
                throw new Exception(GetMessages().GetString("VCardLogConnection.Check.Error", paymentID, e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return false;
        }

        /// <summary>
        /// Log VCard request in the table
        /// </summary>
        /// <param name="reader">ENettRequestVAN detail</param>
        /// <param name="response">CompleteIssueVNettVANResponse</param>
        public void InsertNewRequest(ENettRequestVAN reader, ENettRequestVAN.Traveller traveller, CompleteIssueVNettVANResponse response)
        {
            InsertNewRequest(reader,traveller.TravellerOrder, traveller.Percode, traveller.Name, traveller.CC1, traveller.CC2, 
            traveller.CC3, reader.Channel, response);
        }


        /// <summary>
        /// Log VCard request in the table
        /// </summary>
        /// <param name="reader">XML request</param>
        /// <param name="travelerOrder">Traveler order</param>
        /// <param name="percode">Traveler code</param>
        /// <param name="travelerName">Traveler Name</param>
        /// <param name="cc1">Cost center 1</param>
        /// <param name="cc2">Cost center 2</param>
        /// <param name="cc3">Cost center 3</param>
        /// <param name="channel">channel</param>
        /// <param name="response">CompleteIssueVNettVANResponse</param>
        public void InsertNewRequest(ENettRequestVAN reader,int travelerOrder, string percode, string travelerName
            , string cc1, string cc2, string cc3, string channel, CompleteIssueVNettVANResponse response)
        {
            string request = String.Format("INSERT INTO VCardBooking ("
               + "PaymentId ,TravelerOrder, PosCode, Comcode, BookerPercode, TravelerPercode, TravelerName, AdultCount, AgentLogin, "    
               + "BookingDate, FundedAmount, Currency, CardActivationDate, CardExpirationDate, IsMultiUseCard, "
               + "TruncatedVAN, Product, Channel, ECN, MerchantCategory, MdCode, RefDossierType, RefDossierCode, OriginDate, OriginAddress, "
               + "OriginLocationCode, OriginCountryCode, EndDate, EndAddress, EndLocationCode, EndCountryCode, SupplierName, "
               + "CC1, CC2, CC3, RequestDate, TripName, FreeText1, FreeText2, FreeText3, HotelId, "
               //>>EGE-60949 
               //+ "ErrorCode, ErrorDesc, CreationDate, CreatedBy, ModificationDate, ModifiedBy, SupportLogId, VNettTransactionID) "
               + "ErrorCode, ErrorDesc, CreationDate, CreatedBy, ModificationDate, ModifiedBy, SupportLogId, VNettTransactionID, PosCurrency, VanCountryCode) "
               //<< EGE-60949
               + "VALUES ("
               + "@PaymentId ,@TravelerOrder, @PosCode, @Comcode, @BookerPercode, @TravelerPercode, @TravelerName, @AdultCount, @AgentLogin, "    
               + "@BookingDate, @FundedAmount, @Currency, @CardActivationDate, @CardExpirationDate, @IsMultiUseCard, "
               + "@TruncatedVAN, @Product, @Channel, @ECN, @MerchantCategory, @MdCode, @RefDossierType, @RefDossierCode, @OriginDate, @OriginAddress, "
               + "@OriginLocationCode, @OriginCountryCode, @EndDate, @EndAddress, @EndLocationCode, @EndCountryCode, @SupplierName, "
               + "@CC1, @CC2, @CC3, @RequestDate, @TripName, @FreeText1, @FreeText2, @FreeText3, @HotelId, "
               //>>EGE-60949 
               //+ "@ErrorCode, @ErrorDesc, @CreationDate, @CreatedBy, @ModificationDate, @ModifiedBy, @SupportLogId, @VNettTransactionID)");
               + "@ErrorCode, @ErrorDesc, @CreationDate, @CreatedBy, @ModificationDate, @ModifiedBy, @SupportLogId, @VNettTransactionID, @PosCurrency, @VanCountryCode)");
               //<< EGE-60949
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Send parameters
                command.Parameters.Add("@PaymentId", SqlDbType.VarChar, 16);
                command.Parameters["@PaymentId"].Value = Util.Nvl(response.GetReferenceId(), DBNull.Value);
                
                command.Parameters.Add("@SupportLogId", SqlDbType.VarChar, 256);
                command.Parameters["@SupportLogId"].Value = Util.Nvl(response.GetIssuedVNettResponse().SupportLogId, DBNull.Value);
                
                command.Parameters.Add("@VNettTransactionID", SqlDbType.VarChar, 100);
                command.Parameters["@VNettTransactionID"].Value = Util.Nvl(response.GetIssuedVNettResponse().VNettTransactionID, DBNull.Value);


                //<< EGE-60949
                command.Parameters.Add("@PosCode", SqlDbType.VarChar, 2);
                command.Parameters["@PosCode"].Value = Util.Nvl(reader.PosCountryCode, DBNull.Value);
                command.Parameters.Add("@PosCurrency", SqlDbType.VarChar, 3);
                command.Parameters["@PosCurrency"].Value = Util.Nvl(reader. PosCurrency, DBNull.Value);
                command.Parameters.Add("@VanCountryCode ", SqlDbType.VarChar, 2);
                command.Parameters["@VanCountryCode "].Value = Util.Nvl(reader.Market, DBNull.Value);
                //<< EGE-60949


                command.Parameters.Add("@TravelerOrder", SqlDbType.Int);
                command.Parameters["@TravelerOrder"].Value = travelerOrder;

                //<< EGE-60949
                //command.Parameters.Add("@PosCode", SqlDbType.VarChar, 2);
                //command.Parameters["@PosCode"].Value = Util.Nvl(reader.Market, DBNull.Value);
                //<< EGE-60949

                command.Parameters.Add("@Comcode", SqlDbType.Int);
                command.Parameters["@Comcode"].Value = Util.TryConvertStringToInt(reader.ComCode, DBNull.Value);
                
                command.Parameters.Add("@BookerPercode", SqlDbType.BigInt);
                command.Parameters["@BookerPercode"].Value = Util.TryConvertStringToLong(reader.BookerPerCode, DBNull.Value);
                
                command.Parameters.Add("@TravelerPercode", SqlDbType.BigInt);
                command.Parameters["@TravelerPercode"].Value = Util.TryConvertStringToLong(percode, DBNull.Value);
                
                command.Parameters.Add("@TravelerName", SqlDbType.VarChar, 512);
                command.Parameters["@TravelerName"].Value = Util.Nvl(travelerName, DBNull.Value);
                
                command.Parameters.Add("@AdultCount", SqlDbType.Int);
                command.Parameters["@AdultCount"].Value = Util.TryConvertStringToInt(reader.AdultsCount, DBNull.Value);
                
                command.Parameters.Add("@AgentLogin", SqlDbType.VarChar, 100);
                command.Parameters["@AgentLogin"].Value = Util.Nvl(reader.UserName, DBNull.Value);
                
                command.Parameters.Add("@BookingDate", SqlDbType.DateTime);
                command.Parameters["@BookingDate"].Value = Util.ConvertStringToDate(reader.BookingDate, Const.ExpirationDateFormat);
                
                command.Parameters.Add("@FundedAmount", SqlDbType.Money);
                command.Parameters["@FundedAmount"].Value = response.GetIssuedVNettResponse().IsSuccessful ?response.GetIssuedVNettResponse().FundedAmount:0;
                
                command.Parameters.Add("@Currency", SqlDbType.VarChar, 3);
                command.Parameters["@Currency"].Value = Util.Nvl(reader.Currency, DBNull.Value);
                
                command.Parameters.Add("@CardActivationDate", SqlDbType.DateTime);
                command.Parameters["@CardActivationDate"].Value = Util.ConvertStringToDate(reader.ActivationDate, Const.ExpirationDateFormat);
                
                command.Parameters.Add("@CardExpirationDate", SqlDbType.DateTime);
                command.Parameters["@CardExpirationDate"].Value = Util.ConvertStringToDate(reader.ExpiryDate, Const.ExpirationDateFormat);
                
                command.Parameters.Add("@IsMultiUseCard", SqlDbType.Bit);
                command.Parameters["@IsMultiUseCard"].Value = Util.ConvertStringToBool(reader.IsMultiUse) ? 1 : 0;

                command.Parameters.Add("@TruncatedVAN", SqlDbType.VarChar, 16);
                command.Parameters["@TruncatedVAN"].Value = response.GetIssuedVNettResponse().IsSuccessful ? CreditCardVerifier.TruncatePan(response.GetIssuedVNettResponse().VirtualAccountNumber) : string.Empty;

                command.Parameters.Add("@Product", SqlDbType.VarChar, 30);
                command.Parameters["@Product"].Value = Util.Nvl(reader.Product, DBNull.Value);

                command.Parameters.Add("@Channel", SqlDbType.VarChar, 10);
                command.Parameters["@Channel"].Value = Util.Nvl(channel, DBNull.Value);

                command.Parameters.Add("@ECN", SqlDbType.Int);
                command.Parameters["@ECN"].Value = Util.TryConvertStringToInt(reader.ECN, DBNull.Value);

                command.Parameters.Add("@MerchantCategory", SqlDbType.VarChar, 30);
                command.Parameters["@MerchantCategory"].Value = Util.Nvl(reader.MerchantCategory, DBNull.Value);

                command.Parameters.Add("@MdCode", SqlDbType.Int);
                command.Parameters["@MdCode"].Value = Util.TryConvertStringToInt(reader.MdCode, DBNull.Value);

                command.Parameters.Add("@RefDossierType", SqlDbType.VarChar, 10);
                command.Parameters["@RefDossierType"].Value = Util.Nvl(reader.RefDossierType, DBNull.Value);

                command.Parameters.Add("@RefDossierCode", SqlDbType.Int);
                command.Parameters["@RefDossierCode"].Value = Util.TryConvertStringToInt(reader.RefDossierCode, DBNull.Value);

                // Origin date is not available in verion 1 of the XML
                command.Parameters.Add("@OriginDate", SqlDbType.DateTime);
                if (String.IsNullOrEmpty(reader.OriginDate))
                    command.Parameters["@OriginDate"].Value = DBNull.Value;
                else
                    command.Parameters["@OriginDate"].Value = Util.ConvertStringToDate(reader.OriginDate, Const.ExpirationDateFormat);

                command.Parameters.Add("@OriginAddress", SqlDbType.VarChar, 2000);
                command.Parameters["@OriginAddress"].Value = Util.Nvl(reader.OriginLocationName, DBNull.Value);

                command.Parameters.Add("@OriginLocationCode", SqlDbType.VarChar, 30);
                command.Parameters["@OriginLocationCode"].Value = Util.Nvl(reader.OriginLocationCode, DBNull.Value);

                command.Parameters.Add("@OriginCountryCode", SqlDbType.VarChar, 3);
                command.Parameters["@OriginCountryCode"].Value = Util.Nvl(reader.OriginCountryCode, DBNull.Value);

                // Origin date is not available in verion 1 of the XML
                command.Parameters.Add("@EndDate", SqlDbType.DateTime);
                if (String.IsNullOrEmpty(reader.EndDate))
                    command.Parameters["@EndDate"].Value = DBNull.Value;
                else
                    command.Parameters["@EndDate"].Value = Util.ConvertStringToDate(reader.EndDate, Const.ExpirationDateFormat);

                command.Parameters.Add("@EndAddress", SqlDbType.VarChar, 2000);
                command.Parameters["@EndAddress"].Value = Util.Nvl(reader.EndLocationName, DBNull.Value);

                command.Parameters.Add("@EndLocationCode", SqlDbType.VarChar, 30);
                command.Parameters["@EndLocationCode"].Value = Util.Nvl(reader.EndLocationCode, DBNull.Value);

                command.Parameters.Add("@EndCountryCode", SqlDbType.VarChar, 3);
                command.Parameters["@EndCountryCode"].Value = Util.Nvl(reader.EndCountryCode, DBNull.Value);

                command.Parameters.Add("@SupplierName", SqlDbType.VarChar, 512);
                command.Parameters["@SupplierName"].Value = Util.Nvl(reader.SupplierName, DBNull.Value);

                command.Parameters.Add("@CC1", SqlDbType.VarChar, 100);
                command.Parameters["@CC1"].Value = Util.Nvl(cc1, DBNull.Value);

                command.Parameters.Add("@CC2", SqlDbType.VarChar, 100);
                command.Parameters["@CC2"].Value = Util.Nvl(cc2, DBNull.Value);

                command.Parameters.Add("@CC3", SqlDbType.VarChar, 100);
                command.Parameters["@CC3"].Value = Util.Nvl(cc3, DBNull.Value);

                command.Parameters.Add("@RequestDate", SqlDbType.DateTime);
                command.Parameters["@RequestDate"].Value = DateTime.Now;

                command.Parameters.Add("@TripName", SqlDbType.VarChar, 256);
                command.Parameters["@TripName"].Value = Util.Nvl(reader.TripName, DBNull.Value);

                command.Parameters.Add("@FreeText1", SqlDbType.VarChar, 1000);
                command.Parameters["@FreeText1"].Value = Util.Nvl(reader.FreeText1, DBNull.Value);
                command.Parameters.Add("@FreeText2", SqlDbType.VarChar, 1000);
                command.Parameters["@FreeText2"].Value = Util.Nvl(reader.FreeText2, DBNull.Value);
                command.Parameters.Add("@FreeText3", SqlDbType.VarChar, 1000);
                command.Parameters["@FreeText3"].Value = Util.Nvl(reader.FreeText3, DBNull.Value);

                command.Parameters.Add("@HotelId", SqlDbType.BigInt);
                command.Parameters["@HotelId"].Value = Util.TryConvertStringToLong(reader.HotelID, 0L);
        

                command.Parameters.Add("@ErrorCode", SqlDbType.VarChar, 50);
                command.Parameters["@ErrorCode"].Value = response.GetIssuedVNettResponse().IsSuccessful?0:response.GetIssuedVNettResponse().ErrorCode;
                command.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 256);
                command.Parameters["@ErrorDesc"].Value = response.GetIssuedVNettResponse().IsSuccessful ? string.Empty : response.GetIssuedVNettResponse().ErrorDescription;
                // Technical infos
                command.Parameters.Add("@CreationDate", SqlDbType.DateTime);
                command.Parameters["@CreationDate"].Value = DateTime.Now;
                command.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 256);
                command.Parameters["@CreatedBy"].Value = Util.Nvl(reader.AgentLogIn, DBNull.Value); 
                command.Parameters.Add("@ModificationDate", SqlDbType.DateTime);
                command.Parameters["@ModificationDate"].Value = DateTime.Now;
                command.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 256);
                command.Parameters["@ModifiedBy"].Value = Util.Nvl(reader.AgentLogIn, DBNull.Value);


                // Execute ...
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                // Something wen wrong
                throw new Exception(GetMessages().GetString("VCardLogConnection.Save.Error", response.GetReferenceId(), e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }


        /// <summary>
        /// Log VCard amend in the table
        /// </summary>
        /// <param name="reader">ENettAmendVAN detail</param>
        public void UpdateForAmend(ENettAmendVAN reader, AmendVNettVANResponse result)
        {
            string request = String.Format("UPDATE VCardBooking SET PNR=@pnr, AmendDate =@AmendDate, "
                + "RailTicketNumber=@RailTicketNumber, GrossAmount=@GrossAmount, VATAmount=@VATAmount,IsBookingConfirmed=@IsBookingConfirmed, "
                + "TripName=@TripName, FreeText1=@FreeText1, FreeText2=@FreeText2, FreeText3=@FreeText3, HotelId=@HotelId, "
                + "ExpectedTransactionAmount=@ExpectedTransactionAmount, OriginDate=@OriginDate, OriginAddress=@OriginAddress, "
                + "OriginLocationCode=@OriginLocationCode, OriginCountryCode=@OriginCountryCode, EndDate=@EndDate, EndAddress=@EndAddress, "
                // --> EGE-85532 update expiration date for cancel van as per date.
                + "CardExpirationDate = @CardExpirationDate, "
                //<-- EGE-85532
                //>>EGE-60949 
                //+ "EndLocationCode=@EndLocationCode, EndCountryCode=@EndCountryCode, AdultCount=@AdultCount, FundedAmount=@MaxAuthAmount "
                + "EndLocationCode=@EndLocationCode, EndCountryCode=@EndCountryCode, AdultCount=@AdultCount, FundedAmount=@MaxAuthAmount, TransAmtVanCurr=@TransAmtVanCurr "
                //>>EGE-60949 
                + "WHERE PaymentId=@PaymentId");

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Send parameters
                command.Parameters.Add("@pnr", SqlDbType.VarChar, 256);
                command.Parameters["@pnr"].Value = Util.Nvl(reader.PNR, DBNull.Value);

                command.Parameters.Add("@AmendDate", SqlDbType.DateTime);
                command.Parameters["@AmendDate"].Value = DateTime.Now;

                command.Parameters.Add("@RailTicketNumber", SqlDbType.VarChar, 200);
                command.Parameters["@RailTicketNumber"].Value = Util.Nvl(reader.RailTicketNumbers, DBNull.Value);

                command.Parameters.Add("@GrossAmount", SqlDbType.Money);
                command.Parameters["@GrossAmount"].Value = Util.TryConvertStringToDouble(reader.GrossAmount, 0.00);

                command.Parameters.Add("@VATAmount", SqlDbType.Money);
                command.Parameters["@VATAmount"].Value = Util.TryConvertStringToDouble(reader.VATAmount, 0.00);

                command.Parameters.Add("@ExpectedTransactionAmount", SqlDbType.Money);
                //--> EGE-61686 : CEE- VNETT - Follow transaction amount in XML
                string amount=reader.TransactionAmount;
                if (String.IsNullOrEmpty(amount) || amount.Equals("0")) amount = reader.TransAmtPosCurr;
                command.Parameters["@ExpectedTransactionAmount"].Value = Util.TryConvertStringToDouble(amount, DBNull.Value);
                //command.Parameters["@ExpectedTransactionAmount"].Value = Util.TryConvertStringToDouble(reader.TransactionAmount, DBNull.Value);
                //<-- EGE-61686 : CEE- VNETT - Follow transaction amount in XML

                command.Parameters.Add("@IsBookingConfirmed", SqlDbType.Bit);
                command.Parameters["@IsBookingConfirmed"].Value = Util.ConvertStringToBool(reader.IsBookingConfirmed)?1:0;
                //Begin EGE-85532
                command.Parameters.Add("@CardExpirationDate", SqlDbType.DateTime);
                command.Parameters["@CardExpirationDate"].Value = Util.ConvertStringToDate(reader.ExpiryDate, Const.ExpirationDateFormat);
                // END EGE-85532
                command.Parameters.Add("@TripName", SqlDbType.VarChar, 256);
                command.Parameters["@TripName"].Value = Util.Nvl(reader.TripName, DBNull.Value);

                command.Parameters.Add("@FreeText1", SqlDbType.VarChar, 1000);
                command.Parameters["@FreeText1"].Value = Util.Nvl(reader.FreeText1, DBNull.Value);

                command.Parameters.Add("@FreeText2", SqlDbType.VarChar, 1000);
                command.Parameters["@FreeText2"].Value = Util.Nvl(reader.FreeText2, DBNull.Value);

                command.Parameters.Add("@FreeText3", SqlDbType.VarChar, 1000);
                command.Parameters["@FreeText3"].Value = Util.Nvl(reader.FreeText3, DBNull.Value);

                command.Parameters.Add("@HotelId", SqlDbType.BigInt);
                command.Parameters["@HotelId"].Value = Util.TryConvertStringToLong(reader.HotelID, 0L);

                command.Parameters.Add("@OriginDate", SqlDbType.DateTime);
                // Origin date is not available in verion 1 of the XML
                if (String.IsNullOrEmpty(reader.OriginDate))
                    command.Parameters["@OriginDate"].Value = DBNull.Value;
                else
                    command.Parameters["@OriginDate"].Value = Util.ConvertStringToDate(reader.OriginDate, Const.ExpirationDateFormat);

                command.Parameters.Add("@OriginAddress", SqlDbType.VarChar, 2000);
                command.Parameters["@OriginAddress"].Value = Util.Nvl(reader.OriginLocationName, DBNull.Value);

                command.Parameters.Add("@OriginLocationCode", SqlDbType.VarChar, 30);
                command.Parameters["@OriginLocationCode"].Value = Util.Nvl(reader.OriginLocationCode, DBNull.Value);

                command.Parameters.Add("@OriginCountryCode", SqlDbType.VarChar, 3);
                command.Parameters["@OriginCountryCode"].Value = Util.Nvl(reader.OriginCountryCode, DBNull.Value);

                command.Parameters.Add("@EndDate", SqlDbType.DateTime);
                // Origin date is not available in verion 1 of the XML
                if (String.IsNullOrEmpty(reader.EndDate))
                    command.Parameters["@EndDate"].Value = DBNull.Value;
                else
                    command.Parameters["@EndDate"].Value = Util.ConvertStringToDate(reader.EndDate, Const.ExpirationDateFormat);

                command.Parameters.Add("@EndAddress", SqlDbType.VarChar, 2000);
                command.Parameters["@EndAddress"].Value = Util.Nvl(reader.EndLocationName, DBNull.Value);

                command.Parameters.Add("@EndLocationCode", SqlDbType.VarChar, 30);
                command.Parameters["@EndLocationCode"].Value = Util.Nvl(reader.EndLocationCode, DBNull.Value);

                command.Parameters.Add("@EndCountryCode", SqlDbType.VarChar, 3);
                command.Parameters["@EndCountryCode"].Value = Util.Nvl(reader.EndCountryCode, DBNull.Value);

                command.Parameters.Add("@AdultCount", SqlDbType.Int);
                command.Parameters["@AdultCount"].Value = Util.TryConvertStringToInt(reader.AdultsCount, DBNull.Value);

                command.Parameters.Add("@MaxAuthAmount", SqlDbType.Money);
                command.Parameters["@MaxAuthAmount"].Value = Util.TryConvertStringToDouble(reader.MaxAuthAmount, DBNull.Value);     

                command.Parameters.Add("@PaymentId", SqlDbType.VarChar, 16);
                command.Parameters["@PaymentId"].Value = Util.Nvl(reader.PaymentID, DBNull.Value);

                command.Parameters.Add("@ErrorCode", SqlDbType.VarChar, 50);
                command.Parameters["@ErrorCode"].Value = result.IsSuccessful ? 0 : result.ErrorCode;

                command.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 256);
                command.Parameters["@ErrorDesc"].Value = Util.Nvl(result.ErrorDescription, DBNull.Value);
                // Technical infos
                command.Parameters.Add("@ModificationDate", SqlDbType.DateTime);
                command.Parameters["@ModificationDate"].Value = DateTime.Now;

                command.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 128);
                command.Parameters["@ModifiedBy"].Value = Util.Nvl(reader.AgentLogIn, DBNull.Value);

                //>> EGE-60949
                command.Parameters.Add("@TransAmtVanCurr", SqlDbType.Money); 
                command.Parameters["@TransAmtVanCurr"].Value = Util.TryConvertStringToDouble(reader.TransAmtVanCurr, DBNull.Value);
                //<< EGE-60949


                // Execute ...
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                // Something wen wrong
                throw new Exception(GetMessages().GetString("VCardLogConnection.Save.Error", reader.PaymentID, e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Test function for VCard log Connection
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
                throw new Exception(GetMessages().GetString("VCardLogConnection.Test.Error", e.Message, true));
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