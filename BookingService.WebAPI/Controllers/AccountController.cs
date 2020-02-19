using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Models;
using WebAPI.Filters;
using System.IO;
using System.Text;


using System.Web;
using System.Web.Handlers;
using System.Web.Caching;
using System.Drawing;
using System.Data.SqlClient;
using System.Data;

using Digimaker;
using Digimaker.Directory;
using Digimaker.Schemas.Directory;
using SiteBuilder.WebControls.Page;
using SiteBuilder.WebControls;
using Digimaker.Schemas.Content;
using Digimaker.Content;

using Digimaker.Schemas.Web;
using Digimaker.Data.Content;
using System.Net.Http.Formatting;
using Digimaker.Data.Directory;
using Digimaker.Data.Database;
using System.Configuration;
using System.Threading.Tasks;
using BookingService.WebAPI.Models;
using WebAPI.DTO;

namespace WebAPI.Controllers
{

    public class AccountController : ApiController
    {
        List<UserMaster> userMaster = new List<UserMaster>();
        public Digimaker.Schemas.Web.PersonViewData _persons;

        #region Schemas / DataSets, etc..
        private int _personId = 0;
        private PersonData _exdata;
        private Digimaker.Schemas.Directory.PersonData.PersonRow _exitem;
        private Digimaker.Directory.Person _experson;
        #endregion

        //http://localhost:8082/DigimakerWebApi/api/Account/GetLogin?UserId=123&Password=1&DeviceId=sdksdssl&PlatformType=1
        public IEnumerable<UserRegistration> GetLogin(string UserId, string Password, string DeviceId, string PlatformType)
        {
            // Managing user validation/authentication true / false
            string usr = GetUserInfo(UserId, Password, DeviceId, PlatformType);

            List<UserRegistration> userReg = new List<UserRegistration>();
            userReg.Add(new UserRegistration { User = usr.ToString() });

            return userReg;
        }


        [HttpGet]
        [ActionName("GetPersonListByCustomer")]
        public IEnumerable<UserMaster> GetPersonListByCompany(int customerNo)
        {
            List<UserMaster> um = new List<UserMaster>();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
               
                var result = entity.OrganizationUnit_Person.Join(entity.People, a => a.PersonID, b => b.PersonID, (A, B) => new { a = A, b = B })
                                                          .Where(w => w.a.OrganizationUnitID == customerNo).Select(s => s.b).ToList();
                foreach (var item in result)
                {
                    um.Add(new UserMaster { PersonID = item.PersonID, DisplayName = item.DisplayName });
                }
            }
            return um;
        }

      
        [HttpGet]
        [ActionName("GetPersonList")]
        public IEnumerable<UserMaster> GetPersonList(int OrgUnitId)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
               
            

                    //if( UserAccess.SelfOnly() )
                    //{
                    //    _persons = SiteBuilder.Content.Person.ByPersonId( Digimaker.User.Identity.ID, true );
                    //}
                    //else
                    //{
                    _persons = SiteBuilder.Content.Person.ByOrgUnitId(OrgUnitId, true, true);
          //  }


            for (int i = 0; i < _persons.Person.Count; i++)
            {
                    if(string.IsNullOrEmpty(_persons.Person[i]["Custom2"] == DBNull.Value ? string.Empty :_persons.Person[i]["Custom2"].ToString()))
                    {
                        //string orgUnitName = string.Empty;
                        //string orgUnitId = string.Empty;
                        //bool isPrimary = false;
                        //bool flag = false;
                        //Digimaker.Schemas.Web.PersonViewData.OrganizationUnit_PersonRow[] orgs = _persons.Person[i].GetOrganizationUnit_PersonRows();
                        //if (orgs.Length > 0)
                        //{
                        //    orgUnitName = orgs[0].OrganizationUnitName.ToString();
                        //    orgUnitId = orgs[0].OrganizationUnitId.ToString();
                        //    if (orgUnitId != null)
                        //    {
                        //        int orgUId = Convert.ToInt32(orgUnitId);
                        //        int OrgParentId = Convert.ToInt32(entity.OrganizationUnits.Where(w => w.OrganizationUnitID == orgUId).Select(s => s.ParentID).FirstOrDefault());

                        //        // internal Customers
                        //        string internalCustomersIds = Digimaker.Config.Custom.AppSettings["InternalCustomers"].ToString(); // "2038,4, 2037,2030";
                        //        List<int> internalCustIds = internalCustomersIds.Split(',').Select(int.Parse).ToList();
                        //        if (internalCustIds.Contains(OrgParentId))
                        //        {
                        //            isPrimary = true;
                        //            flag = true;
                        //        }

                        //        // external Costumers
                        //        string externalCostumersIds = Digimaker.Config.Custom.AppSettings["ExternalCostumers"].ToString(); //"3039";
                        //        List<int> externalCustIds = externalCostumersIds.Split(',').Select(int.Parse).ToList();
                        //        if (externalCustIds.Contains(OrgParentId))
                        //        {
                        //            isPrimary = false;
                        //            flag = true;
                        //        }
                        //    }

                        //}
                        //if (flag) { 
                        userMaster.Add(new UserMaster { PersonID = Convert.ToInt32(_persons.Person[i].PersonId), GivenName = _persons.Person[i].GivenName.ToString(), FamilyName = _persons.Person[i].FamilyName.ToString(), DisplayName = _persons.Person[i].DisplayName.ToString() });
                       // }
                    }
                 
              
            }
            }
            return userMaster;
        }

        [HttpPost]
        [ActionName("CreateNewUser")]
        public int CreateNewUser([FromBody]UserViewModel user)
        {
            int rec = 0;
            if (ModelState.IsValid)
            {
                rec = Person_Add(user.CustomerId, null, user.FirstName, user.LastName, user.Mobile, user.Email, null, null, Digimaker.Config.Custom.AppSettings["Role_AccessToBooking"].ToString(), null);
            }
    
            return rec;
        }

        [HttpGet]
        [ActionName("GetCustomerList")]
        public IEnumerable<CompanyMaster> GetCustomerList()
        {
            List<CompanyMaster> customerList = new List<CompanyMaster>();
            CompanyMaster customer;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {

                string internalCustomersIds = Digimaker.Config.Custom.AppSettings["InternalCustomers"].ToString(); // "2038,4, 2037,2030";
                var internalCustIds = internalCustomersIds.Split(',').ToArray();
                string externalCostumersIds = Digimaker.Config.Custom.AppSettings["ExternalCostumers"].ToString(); //"3039";
                var externalCustIds = externalCostumersIds.Split(',').ToArray();
              
                 var internalCustomerList = (from u in entity.OrganizationUnits
                               where internalCustIds.Contains(u.ParentID.ToString())
                               select u).ToList();
                var ExternalCustomerList = (from u in entity.OrganizationUnits
                                            where externalCustIds.Contains(u.ParentID.ToString())
                                            select u).ToList();

                foreach (var item in internalCustomerList)
                {
                    customer = new CompanyMaster();
                    if (!string.IsNullOrEmpty(item.Code))
                    {
                        customer.IsMVA = false;
                    }
                    else
                    {
                        customer.IsMVA = true;
                    }
                    customer.IsPrimary = true;
                    customer.OrganizationUnitId = item.OrganizationUnitID.ToString();
                    customer.OrganizationUnitName = item.OrganizationUnitName;

                    customerList.Add(customer);
                }
                foreach (var item in ExternalCustomerList)
                {
                    customer = new CompanyMaster();
                    customer.IsPrimary = false;
                    if (!string.IsNullOrEmpty(item.Code))
                    {
                        customer.IsMVA = false;
                    }
                    else
                    {
                        customer.IsMVA = true;
                    }
                    customer.OrganizationUnitId = item.OrganizationUnitID.ToString();
                    customer.OrganizationUnitName = item.OrganizationUnitName;
                    customerList.Add(customer);
                }

            }
            return customerList;
        }
        private string GetUserInfo(string UserId, string Password, string DeviceId, string PlatformType)
        {
            int result = 0;
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.sel_UserRegistration", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = UserId.ToString();
                cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 50).Value = Password.ToString();
                cmd.Parameters.Add("@DeviceId", SqlDbType.NVarChar, -1).Value = DeviceId.ToString();
                cmd.Parameters.Add("@PlatformType", SqlDbType.Int).Value = Convert.ToInt32(PlatformType);
                // open connection, execute command, close connection
                conn.Open();
                result = (int)cmd.ExecuteScalar();
                conn.Close();

            }
            if (result == 1)
                return "1";
            else
                return "0";
        }


        //Post from get: http://localhost:8082/digimakerWebApi/api/Account/UpdateUsers?OrganizationNumber=123&Department=xcy&FirstName=Saikat&LastName=Choudhury&Mobile=12345&Email=saikat@abc.com&Status=1&UserID=999&AccessToBooking=true&JPGPicture=sdjsk
        //, string Department , string FirstName , string LastName , string Mobile , string Email , string Status , string UserID , Boolean AccessToBooking , string JPGPicture)
        [HttpPost]
        public string UpdateUsers(FormDataCollection form)
        {
            return this.UpdateUsers(form.Get("UserID"),
                                    form.Get("OrganizationNumber"),
                                    form.Get("Department"),
                                    form.Get("FirstName"),
                                    form.Get("LastName"),
                                    form.Get("Mobile"),
                                    form.Get("Email"),
                                    form.Get("Status"),
                                    form.Get("AccessToBooking"),
                                    form.Get("JPGPicture"));
        }


        /*
         * null value - no such parameter, meaning no change on this field.
         * 
         * userid+status
         * userid+email+user information(optional)
         * userid+AccessToBooking(1/0)
         * userid+Status(1/0): deactivate/activate user.
         * 
         * result code:
            10 - successfully updated
            11 - successfully created
            12 - deactive user(remove role from it)

            01 - invalid organization
            02 - invalid user id(username)

        */
        [HttpGet]
        public string UpdateUsers( string UserID, string OrganizationNumber = null,
                                                  string Department = null,
                                                  string FirstName = null, 
                                                  string LastName = null, 
                                                  string Mobile = null,
                                                  string Email = null,
                                                  string Status = null, 
                                                  string AccessToBooking = null,
                                                  string JPGPicture = null)
        {
            var result = string.Empty;

            string pictureName = null;
            if (JPGPicture != null && JPGPicture != "")
            {
                pictureName = OrganizationNumber + "-" + FirstName + "-" + LastName + ".jpg";
                var fullPath = ConfigurationManager.AppSettings["RestPictureUploadFolder"] + @"\" + pictureName;
                var bytes = Convert.FromBase64String( JPGPicture );
                try
                {
                    File.WriteAllBytes(fullPath, bytes);
                }
                catch( Exception ex )
                {
                    throw ex;
                }
            }
            try
            { 
                if (CheckUserExist(UserID)) //if exists
                {
                    if (AccessToBooking.Equals( "1" ) )
                    {
                        //update role for booking
                        DataSet dsInDatabase = CheckUserPresentInDatabase(UserID);
                        string PersonID = dsInDatabase.Tables[0].Rows[0]["PersonID"].ToString();

                        UpdateUserRoleInDatabase(PersonID, Digimaker.Config.Custom.AppSettings["Role_AccessToBooking"].ToString(), 1);
                        result = "10: access to booking enabled.";
                    }
                    else if( AccessToBooking.Equals( "0" ) )
                    {
                        //update
                        DataSet dsInDatabase = CheckUserPresentInDatabase(UserID);
                        string PersonID = dsInDatabase.Tables[0].Rows[0]["PersonID"].ToString();

                        UpdateUserRoleInDatabase(PersonID, Digimaker.Config.Custom.AppSettings["Role_AccessToBooking"].ToString(), 2);
                        result = "12: access to booking disabled.";
                    }

                    //Update user profile
                    var personData = PersonHandler.GetPersonData( UserID );
                    var row = personData.Tables[0].Rows[0];
                   
                    if( FirstName != null )
                    {
                        row["GivenName"] = FirstName;
                    }

                    if( LastName != null )
                    {
                        row["FamilyName"] = LastName;
                    }

                    if( FirstName != null || LastName != null )
                    {
                        row["DisplayName"] = FirstName + " " + LastName;
                    }

                    if( Department !=null )
                    {
                        //todo: support this
                    }

                    if( Mobile != null )
                    {
                        row["MobilePhone"] = Mobile;
                    }

                    if( Email != null )
                    {
                        row["Email"] = Email;
                    }
                    if(Status=="Active" || Status == "active")
                    {
                        row["Status"] = "0";
                    }
                    else { 
                        //row["Status"] = "1";
                        row["Custom2"] = "hide";
                    }

                    if ( pictureName != null )
                    {                        
                        //upload
                        var pictureID = row["PictureID"].ToString();
                        var pictureObject = new Digimaker.Content.Picture();
                        var sizeArray = GetPictureSizes();
                        var directory = ConfigurationManager.AppSettings["RestPictureUploadFolder"];
                        if (pictureID != "")
                        {
                            var pictureProperty = (Digimaker.Schemas.Content.PictureData.PicturePropertyRow)PictureHandler.GetData(int.Parse(pictureID), new int[3] { 1, 2, 4 }).PictureProperty.Rows[0];
                            Picture.DeleteMain( pictureProperty.PictureMainID );
                            result += "Picture removed.";
                        }
                        var resultID = pictureObject.SavePicture(sizeArray, directory, pictureName, pictureName, "", "", "",
                                        int.Parse( ConfigurationManager.AppSettings["RestPictureCategory"] ) );
                        if (resultID != -1)
                        {
                            foreach(Digimaker.Schemas.Content.PictureData.PicturePropertyRow propertyRow in PictureHandler.GetDataOnPictureMainid( resultID, new int[3] { 1, 2, 4 } ).PictureProperty.Rows )
                            {
                                if( propertyRow.IsDefault )
                                {
                                    row["PictureID"] = propertyRow.PicturePropertyID.ToString();
                                    break;
                                }
                            }
                            result += "Picture added.";
                        }
                        else
                        {
                            result += "Picture not added.";
                        }                                                                                                                                            
                    }

                    PersonHandler.Update( personData, Int32.Parse( row["PersonID"].ToString() ) );

                    //Update User Organization In Database
                    DataSet dsInDatabase1 = CheckUserPresentInDatabase(UserID);
                    string PersonID1 = dsInDatabase1.Tables[0].Rows[0]["PersonID"].ToString();
                    UpdateUserOrganizationInDatabase(PersonID1, OrganizationNumber, 1);

                    result = "13: user profile is updated. "+result;

                }
                else if( Email != null ) //If not exists, create user(need email).
                {
                    var organizationUnitID = CheckOrganizationNumber(OrganizationNumber);
                    if ( organizationUnitID != 0 )
                    {
                        //Create user
                        //AddActor(OrganizationNumber, Department, FirstName, LastName, Mobile, Email, Status, UserID);
                        Person_Add( organizationUnitID.ToString(), Department, FirstName, LastName, Mobile, Email, Status, UserID, Digimaker.Config.Custom.AppSettings["Role_AccessToBooking"].ToString(), pictureName );
                        result = "11: user created.";
                    }
                    else
                    {
                        result = "01: organization was not found.";
                    }
                }
                else
                {
                    result = "02";
                }          
            }
            catch( Exception ex )
            {
                result = ex.ToString(); //todo: log it
            }

            return result;
        }
        public int CheckOrganizationNumber(string OrganizationNumber )
        {
            var organizationUnitID = 0;

            var conn = DatabaseHandler.NewConnection();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;


            cmd.CommandText = "SELECT OrganizationUnitID FROM OrganizationUnit WHERE OrgNumber = '"+OrganizationNumber+"'";

            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;

            conn.Open();

            reader = cmd.ExecuteReader();
            if( reader.HasRows )
            {
                reader.Read();
                organizationUnitID = reader.GetInt32(0);
               
            }
            conn.Close();
            return organizationUnitID;
        }

        private bool CheckUserExist(string userName)
        {
            bool UserFound = false;
            Digimaker.Schemas.Directory.PersonData pData = Digimaker.Data.Directory.PersonHandler.GetPersonData(userName);
            if (pData.Person.Count > 0)
            {
                UserFound = true;
            }
            return UserFound;
        }
        private DataSet CheckUserPresentInDatabase(string userName)
        {
            return Digimaker.Data.Directory.PersonHandler.GetPersonData(userName);
        }
        private void UpdateUserRoleInDatabase(string PersonID, string RoleID, int Type)
        {
            // set up connection and command
            int result = 0;
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.Upd_UserRole_BasedONUser", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                cmd.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                cmd.Parameters.Add("@RoleID", SqlDbType.Int).Value = RoleID;
                cmd.Parameters.Add("@Type", SqlDbType.Int).Value = Type;
                // open connection, execute command, close connection
                conn.Open();
                //result = (int)cmd.ExecuteScalar();
                cmd.ExecuteScalar();
                conn.Close();

            }
        }

        private void UpdateUserOrganizationInDatabase(string PersonID, string OrganizationUnitID, int Type)
        {
            // set up connection and command

            string OrgUnitId= OrganizationUnitID.Replace(" ", String.Empty);
            int result = 0;
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.Upd_UserOrganization_BasedONUser", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                cmd.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                cmd.Parameters.Add("@OrganizationUnitID", SqlDbType.NVarChar, 50).Value = OrgUnitId;
                cmd.Parameters.Add("@Type", SqlDbType.Int).Value = Type;
                // open connection, execute command, close connection
                conn.Open();
                //result = (int)cmd.ExecuteScalar();
                cmd.ExecuteScalar();
                conn.Close();

            }
        }

        private string[] GetPictureSizes()
        {
            string[] sizeArray = Digimaker.Config.Picture.AutoGeneratePictures.Split(',');
            var thumbnailSize = Digimaker.Config.Picture.ThumbnailSize; 
            var fullSizeArray = new string[sizeArray.Length + 2];
            fullSizeArray[0] = "1";
            fullSizeArray[1] = thumbnailSize;
            for (var i = 0; i < sizeArray.Length; i++)
            {
                fullSizeArray[i + 2] = sizeArray[i];
            }
            return fullSizeArray;
        }      


        private int Person_Add(string organizationUnitID, string Department, string FirstName, string LastName, string Mobile, string Email, string Status, string UserID, string RoleID, string pictureName = null)
        {
            int rec = 0;
            string pictureID = null;
            if ( pictureName !=null )
            {
                var pictureObject = new Digimaker.Content.Picture();
                var sizeArray = GetPictureSizes();
                var directory = ConfigurationManager.AppSettings["RestPictureUploadFolder"];
                var resultID = pictureObject.SavePicture( sizeArray, directory, pictureName, pictureName, "", "", "",
                    int.Parse(ConfigurationManager.AppSettings["RestPictureCategory"]) );
                if(resultID != -1)
                {
                    foreach (Digimaker.Schemas.Content.PictureData.PicturePropertyRow propertyRow in PictureHandler.GetDataOnPictureMainid(resultID, new int[3] { 0, 1, 4 }).PictureProperty.Rows)
                    {
                        if (propertyRow.Comment == "Thumbnail")
                        {
                            pictureID = propertyRow.PicturePropertyID.ToString();
                            break;
                        }
                    }
                }
            }
            String strConn = Digimaker.Config.ConnectionString;

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {

                    //var list = lstEmp.Where(item => lstEmp. == item.ItemCode);
                    //INSERT INTO dbo.Person (GivenName, FamilyName, DisplayName, LoginName, PasswordHash, PasswordSalt, CanReceiveHTMLMail, Email,Custom1) 
                    conn.Open();

                        SqlCommand cmd = new SqlCommand("[InsUpd_SericeToDigimakerEmployees]", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@GivenName", FirstName);
                            cmd.Parameters.AddWithValue("@FamilyName", LastName);

                        cmd.Parameters.AddWithValue("@DisplayName", FirstName+" "+ LastName);

                        cmd.Parameters.AddWithValue("@LoginName", UserID);
                        cmd.Parameters.AddWithValue("@PasswordHash", DateTime.Now.ToString()+@"$%$%^^" );
                        cmd.Parameters.AddWithValue("@Email", Email);
                        cmd.Parameters.AddWithValue("@EmployeeNo", UserID);
                        cmd.Parameters.AddWithValue("@MobilePhone", Mobile);
                        if(pictureID != null)
                        {
                            cmd.Parameters.AddWithValue("@PictureID", Convert.ToInt32(pictureID) );
                        }
                        cmd.Parameters.AddWithValue("@RoleID", Convert.ToInt32(RoleID));

                        cmd.Parameters.AddWithValue("@OrganizationUnitID", Convert.ToInt32(organizationUnitID));
                    cmd.Parameters.Add("@PersonNumber", SqlDbType.Int).Direction = ParameterDirection.Output;
                   // cmd.Parameters.Add("@NewId", SqlDbType.Int).Direction = ParameterDirection.Output;

                    rec = cmd.ExecuteNonQuery();
                    rec = Convert.ToInt32( cmd.Parameters["@PersonNumber"].Value);

                }
                finally
                {
                    
                    if (conn != null && conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }
            return rec;
        }

        public async Task<HttpResponseMessage> PostFile()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                StringBuilder sb = new StringBuilder(); // Holds the response body

                // Read the form data and return an async task.
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the form data.
                foreach (var key in provider.FormData.AllKeys)
                {
                    foreach (var val in provider.FormData.GetValues(key))
                    {
                        sb.Append(string.Format("{0}: {1}\n", key, val));
                    }
                }

                // This illustrates how to get the file names for uploaded files.
                foreach (var file in provider.FileData)
                {
                    FileInfo fileInfo = new FileInfo(file.LocalFileName);
                    sb.Append(string.Format("Uploaded file: {0} ({1} bytes)\n", fileInfo.Name, fileInfo.Length));
                }
                return new HttpResponseMessage()
                {
                    Content = new StringContent(sb.ToString())
                };
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
        #region Enum: ObjectClass
        /// <summary>
        /// The ObjectClass enumeration specified the different object classes used in ADSI.
        /// </summary>
        public enum ObjectClass
        {
            user = 0,
            organizationalUnit = 1,
            group = 2
        }

        #endregion
        private void UpdateAddress(string objectClass, int objectID, string adrStreet, string adrPOBox, string adrPostalCode, string adrPostOffice, string adrRegion, string adrCountry)
        {
            AddressData aData;
            bool addressChanged = false;
            bool updatePrimaryAddress = Digimaker.Config.UsePrimaryAddress;
            bool addressFound = false;

            //get address data based on the objectClass, clears any existing rows, and updates the dataset
            if (objectClass.ToLower() == ObjectClass.user.ToString())
            {
                aData = Digimaker.Data.Directory.PersonHandler.GetAddress(objectID);
                if (aData.Address.Rows.Count > 0)
                {
                    for (int i = 0; i < aData.Address.Rows.Count; i++)
                    {
                        if (updatePrimaryAddress)	/// look for Primary address
                        {
                            if (aData.Address[i].PrimaryAddress)
                            {
                                if (adrStreet != string.Empty)
                                {
                                    aData.Address[i].StreetAddress = adrStreet;
                                    aData.Address[i].PostalCode = adrPostalCode;
                                    aData.Address[i].PostOfficeAddress = adrPOBox; ;
                                    aData.Address[i].Region = adrRegion;
                                    aData.Address[i].Country = adrCountry;
                                    aData.Address[i].Locality = adrPostOffice;
                                    aData.Address[i].ExtendedAddress = string.Empty;
                                }
                                addressFound = true;
                                break;
                            }
                        }
                        else		/// than look for ADAddress in addresses..
                        {
                            if (aData.Address[i].ADAddress)
                            {
                                if (adrStreet != string.Empty)
                                {
                                    aData.Address[i].StreetAddress = adrStreet;
                                    aData.Address[i].PostalCode = adrPostalCode;
                                    aData.Address[i].PostOfficeAddress = adrPOBox; ;
                                    aData.Address[i].Region = adrRegion;
                                    aData.Address[i].Country = adrCountry;
                                    aData.Address[i].Locality = adrPostOffice;
                                    aData.Address[i].ExtendedAddress = string.Empty;
                                }
                                addressFound = true;
                                break;
                            }
                        }
                    }
                    if (addressFound)	/// If found than only update address else will need to create new...
                    {
                        Digimaker.Data.Directory.PersonHandler.UpdateAddress(aData, objectID);
                        addressChanged = true;
                    }
                }
            }
            else
            {
                aData = Digimaker.Data.Directory.OrgUnitHandler.GetAddress(objectID);
                if (aData.Address.Rows.Count > 0)
                {
                    for (int i = 0; i < aData.Address.Rows.Count; i++)
                    {
                        if (updatePrimaryAddress)	/// look for Primary address
                        {
                            if (aData.Address[i].PrimaryAddress)
                            {
                                if (adrStreet != string.Empty)
                                {
                                    aData.Address[i].StreetAddress = adrStreet;
                                    aData.Address[i].PostalCode = adrPostalCode;
                                    aData.Address[i].Region = adrRegion;
                                    aData.Address[i].Country = adrCountry;
                                    aData.Address[i].Locality = adrPostOffice;
                                    aData.Address[i].ExtendedAddress = string.Empty;
                                }
                                addressFound = true;
                                break;
                            }
                        }
                        else	/// Look for ADAddress from addresses
                        {
                            if (aData.Address[i].ADAddress)
                            {
                                if (adrStreet != string.Empty)
                                {
                                    aData.Address[i].StreetAddress = adrStreet;
                                    aData.Address[i].PostalCode = adrPostalCode;
                                    aData.Address[i].Region = adrRegion;
                                    aData.Address[i].Country = adrCountry;
                                    aData.Address[i].Locality = adrPostOffice;
                                    aData.Address[i].ExtendedAddress = string.Empty;
                                }
                                addressFound = true;
                                break;
                            }
                        }
                    }

                    if (addressFound)	/// If found than only update address else will need to create new...
                    {
                        Digimaker.Data.Directory.OrgUnitHandler.UpdateAddress(aData, objectID);
                        addressChanged = true;
                    }

                }/// Else ended for RowCount > 0
            }/// Else ended for orgunit

            if (!addressChanged)
            {
                //update street address
                if (adrStreet != string.Empty)
                {
                    AddressData.AddressRow aRow = aData.Address.NewAddressRow();
                    aRow.StreetAddress = adrStreet;
                    aRow.PostalCode = adrPostalCode;
                    if (objectClass.ToLower() == ObjectClass.user.ToString())
                    {
                        aRow.PostOfficeAddress = adrPOBox;
                    }
                    aRow.Region = adrRegion;
                    aRow.Country = adrCountry;
                    aRow.Locality = adrPostOffice;
                    aRow.ExtendedAddress = string.Empty;

                    if (aData.Address.Rows.Count == 0)
                        aRow.PrimaryAddress = true;

                    if (updatePrimaryAddress)
                    {
                        aRow.AddressDescription = "Street address";
                        aRow.ADAddress = false;
                    }
                    else
                    {
                        aRow.AddressDescription = Digimaker.Config.DirectoryAddressDefault;
                        aRow.ADAddress = true;
                    }
                    aData.Address.AddAddressRow(aRow);
                }

                //Update dataset
                if (aData.HasChanges())
                {
                    if (objectClass.ToLower() == ObjectClass.user.ToString())
                        Digimaker.Data.Directory.PersonHandler.UpdateAddress(aData, objectID);
                    else
                        Digimaker.Data.Directory.OrgUnitHandler.UpdateAddress(aData, objectID);
                }
            }
        }
        [HttpPost]
        [ActionName("AddingNewCustomer")]
        public int AddingNewCustomer([FromBody]CustomerViewModel Customer)
        {
            int externalCostumersIds = Convert.ToInt32( Digimaker.Config.Custom.AppSettings["ExternalCostumers"].ToString());
            int organizationUnitID = AddingNewOrganizationUnit(Customer, externalCostumersIds);
          
            return organizationUnitID;
        }
            private int AddingNewOrganizationUnit(CustomerViewModel customer, int Parentid)
        {

            Digimaker.Schemas.Directory.OrgUnitData _data = new Digimaker.Schemas.Directory.OrgUnitData();
            Digimaker.Schemas.Directory.OrgUnitData.OrganizationUnitRow _item = _data.OrganizationUnit.NewOrganizationUnitRow();

            // Setup the default values
            _item.OrganizationUnitName = customer.Name;
            _item.Type = 0;
            _item.OrgNumber = customer.Id;
           // _item.Modified = DateTime.Now;
            _item.ParentID = Parentid;
            _item.Status = 0;
            _item.Type = 0;
            _item.Priority = 100;
            
            // Add the new item to our datasource
            _data.OrganizationUnit.AddOrganizationUnitRow(_item);
            Digimaker.Data.Directory.OrgUnitHandler.Update(_data,1,1, new int[] { 1 },"");

            int organizationUnitID = CheckOrganizationNumber(customer.Id);
            UpdateAddress("1", organizationUnitID,customer.Address1, "",customer.Pincode,customer.Country,"","");
            return organizationUnitID;
        }
    }

}