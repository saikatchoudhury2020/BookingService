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
using BookingService.WebAPI.Models;
using WebAPI.DTO;
using System.Data;
using BookingService.WebAPI.DTO;
using System.Transactions;

namespace BookingService.WebAPI.Controllers
{
    public class CustomerCompanyController : ApiController
    {
        [HttpGet]
        [ActionName("GetCustomerCompanyDetail")]
        public OrgUnit GetCustomerCompanyDetail(string orgId)
        {
            int id = 0;
            if (orgId != null)
            {
                id = Convert.ToInt32(orgId);
            }
            OrgUnit orgUnit;
            List<OrganizationPerson> personlist;
            OrganizationPerson person;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var orgResult = entity.OrganizationUnits.Where(w => w.OrganizationUnitID == id).Select(s => s).FirstOrDefault();
                    personlist = new List<OrganizationPerson>();
                    orgUnit = new OrgUnit();
                    orgUnit.id = orgResult.OrganizationUnitID;
                    orgUnit.name = orgResult.OrganizationUnitName;
                orgUnit.Code = string.IsNullOrEmpty(orgResult.Code)?true :false ;
                    orgUnit.OrganizationNumber = orgResult.OrgNumber;
                if (orgResult.PictureID != null)
                {
                    orgUnit.PicturePath = entity.PictureProperties.Where(w => w.PicturePropertyID == orgResult.PictureID).SingleOrDefault().Filepath.ToString();
                }
                var persons = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                     .Where(w => w.o.OrganizationUnitID == orgResult.OrganizationUnitID).Select(s => s.p).OrderBy(o=> o.DisplayName).ToList();
                    if (persons != null)
                    {
                        foreach (var p in persons.Where(p => p.Custom2 == null))
                        {
                            person = new OrganizationPerson();
                            person.id = p.PersonID;
                            person.name = p.DisplayName;
                            person.telephone = p.MobilePhone;
                            person.UserEmail = p.Email;
                            person.Department = p.Custom1;
                            person.UserGroup = string.Join(",", entity.UserGroups.Join(entity.UserGroup_User, gp => gp.UserGroupId, go => go.UserGroupId, (GP, GO) => new { gp = GP, go = GO })
                     .Where(w => w.go.UserId == p.PersonID).Select(gs => gs.gp.UserGroupName));
                        personlist.Add(person);
                        }
                    }
                    orgUnit.personList = personlist;
            }
            return orgUnit;
        }
        [HttpGet]
        [ActionName("GetUserGroupList")]
        public UserGroupList GetUserGroupList(string userGroupId)
        {
            UserGroupList userGroup;
            List<UserGroupListUser> personlist;
            UserGroupListUser person;

            int id = 0;
            if (userGroupId != null)
            {
                id = Convert.ToInt32(userGroupId);
            }
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                userGroup = new UserGroupList();
                var usrResult = entity.UserGroups.Where(u=> u.UserGroupId == id).Select(s => s).FirstOrDefault();
                if (usrResult != null)
                {
                    personlist = new List<UserGroupListUser>();
                  
                    userGroup.userGroupId = usrResult.UserGroupId;
                    userGroup.name = usrResult.UserGroupName;
                    var persons = entity.People.Join(entity.UserGroup_User, p => p.PersonID, o => o.UserId, (P, O) => new { p = P, o = O })
                         .Where(w => w.o.UserGroupId == usrResult.UserGroupId).Select(s => s.p).OrderBy(o => o.DisplayName).ToList();
                    if (persons != null)
                    {
                        foreach (var p in persons.Where(p => p.Custom2 == null))
                        {
                            person = new UserGroupListUser();
                            person.id = p.PersonID;
                            person.name = p.DisplayName;
                            person.telephone = p.MobilePhone;
                            person.UserEmail = p.Email;
                            person.PositionTitle = p.Title;

                            var comp = entity.OrganizationUnits.Join(entity.OrganizationUnit_Person, ou => ou.OrganizationUnitID, oup => oup.OrganizationUnitID, (OU, OUP) => new { ou = OU, oup = OUP })
                                     .Where(w => w.oup.PersonID == p.PersonID).Select(s => s.ou).FirstOrDefault();
                            if (comp != null)
                            {
                                person.companyId = comp.OrganizationUnitID;
                                person.companyName = comp.OrganizationUnitName;
                            }


                            personlist.Add(person);
                        }
                    }
                    userGroup.personList = personlist;
                }
                   
             
            }
            return userGroup;
        }

        [HttpGet]
        [ActionName("GetUserGroup")]
        public IEnumerable<UserGroupList> GetUserGroup()
        {
            List<UserGroupList> userGroupList;
            UserGroupList userGroup;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var usrResult = entity.UserGroups.Select(s => s).ToList();
                userGroupList = new List<UserGroupList>();
                foreach (var usr in usrResult)
                {
                    userGroup = new UserGroupList();
                    userGroup.userGroupId = usr.UserGroupId;
                    userGroup.name = usr.UserGroupName;
                    userGroupList.Add(userGroup);
                }
            }
            return userGroupList;
        }

        [HttpPost]
        [ActionName("AddToUserGroupList")]
        public string AddToUserGroupList(UserAssignedGroupModel userGroupModel)
        {
            UserGroup_User _userGroupUsr;
            string message = "";
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
               
                    using (var scope = entity.Database.BeginTransaction())
                    {

                    try
                    {
                        var ExistingGrp = entity.UserGroup_User.Where(w => w.UserId == userGroupModel.userId).ToList();
                        if (ExistingGrp != null)
                        {
                            foreach (var item in ExistingGrp)
                            {
                                entity.UserGroup_User.Remove(item);
                                entity.SaveChanges();
                            }
                            
                        }
                        if (userGroupModel.userGroup != null)
                        {
                            foreach (var userGroup in userGroupModel.userGroup)
                            {
                                _userGroupUsr = new UserGroup_User();
                                _userGroupUsr.UserId = userGroupModel.userId;
                                _userGroupUsr.UserGroupId = userGroup;
                                entity.UserGroup_User.Add(_userGroupUsr);
                            }
                            entity.SaveChanges();
                            scope.Commit();
                            message = "successfully assigned UserGroup";
                        }
                        else
                        {
                            message = "UserGroup not assigned";
                        }
                       
                    }
                    catch (Exception)
                    {

                        scope.Rollback();
                        message = "";
                    }
                    finally{
                        scope.Dispose();
                    }

                }
                
               
            }
            return message;
        }

        [HttpGet]
        [ActionName("CreateNewUserGroup")]
        public string CreateNewUserGroup(string userGroupName)
        {
            UserGroup _userGroup = new UserGroup();
            string message = "";
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {

                var Isalready = entity.UserGroups.Where(w => w.UserGroupName.Trim().ToUpper() == userGroupName.Trim().ToUpper()).Count();
                if (Isalready > 0)
                {
                    message = "Already exist";
                }
                else
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        _userGroup.UserGroupName = userGroupName;


                        entity.UserGroups.Add(_userGroup);
                        entity.SaveChanges();
                        scope.Complete();
                        message = "Succesfully Added";
                    }
                }
            }
            return message;
        }

        [HttpGet]
        [ActionName("DeleteUserGroup")]
        public string DeleteUserGroup(int userGroupId)
        {
            string message = "";
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                //For multiple records
                var allRec = entity.UserGroup_User.Where(w => w.UserGroupId == userGroupId);
                entity.UserGroup_User.RemoveRange(allRec);
                //For Single record
                var singleRec = entity.UserGroups.FirstOrDefault(x => x.UserGroupId == userGroupId);// object your want to delete
                entity.UserGroups.Remove(singleRec);

                entity.SaveChanges();
                message = "Delete Successfully";
            }

            return message;
        }

        [HttpGet]
        [ActionName("DeleteUserGroupList")]
        public string DeleteUserGroupList(int userGroupId, int userId)
        {
            string message = "";
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                //For Single record
                var singleRec = entity.UserGroup_User.FirstOrDefault(x => x.UserGroupId == userGroupId && x.UserId == userId); // object your want to delete

                if (singleRec != null)
                {
                    entity.UserGroup_User.Remove(singleRec);

                    entity.SaveChanges();
                    message = "Delete Successfully";
                }
                else
                {
                    message = "Delete Unsuccessfully";
                }
                
            }

            return message;
        }


        [HttpGet]
        [ActionName("UserWiseGroupList")]
        public IEnumerable<UserGroupList> UserWiseGroupList(int userId)
        {
            List<UserGroupList> userGroupList;
            UserGroupList userGroup;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var usrResult = entity.UserGroups.Join(entity.UserGroup_User, p => p.UserGroupId, o => o.UserGroupId, (P, O) => new { p = P, o = O })
                     .Where(w => w.o.UserId == userId).Select(s => s.p).OrderBy(o => o.UserGroupName).ToList();
                userGroupList = new List<UserGroupList>();
                foreach (var usr in usrResult)
                {
                    userGroup = new UserGroupList();
                    userGroup.userGroupId = usr.UserGroupId;
                    userGroup.name = usr.UserGroupName;
                    userGroupList.Add(userGroup);
                }
            }
            return userGroupList;
        }

        [HttpGet]
        [ActionName("UserListbyBuilding")]
        public IEnumerable<OrganizationPerson> UserListbyBuilding(int buildingId, int orgId)
        {

            List<OrganizationPerson> userList = new List<OrganizationPerson>();
            OrganizationPerson user;
          
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                if (buildingId != 0)
                {
                    var orgList = entity.OrganizationUnits.Where(w => w.ParentID == buildingId).Select(s => new { s.OrganizationUnitID, s.OrganizationUnitName } ).ToList();
                    if (orgList != null)
                    {
                        foreach (var item in orgList)
                        {
                            var persons = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                       .Where(w => w.o.OrganizationUnitID == item.OrganizationUnitID).Select(s => s.p).OrderBy(o => o.DisplayName).ToList();
                            if (persons != null)
                            {
                                foreach (var p in persons.Where(p => p.Custom2 == null || p.Custom2 == ""))
                                {
                                    user = new OrganizationPerson();
                                    user.id = p.PersonID;
                                    user.name = p.DisplayName;
                                    user.telephone = p.MobilePhone;
                                    user.UserEmail = p.Email;
                                    user.Department = p.Custom1;
                                    user.UserGroup = string.Join(",", entity.UserGroups.Join(entity.UserGroup_User, gp => gp.UserGroupId, go => go.UserGroupId, (GP, GO) => new { gp = GP, go = GO })
                             .Where(w => w.go.UserId == p.PersonID).Select(gs => gs.gp.UserGroupName));
                                    user.OrganizationName = item.OrganizationUnitName;
                                    userList.Add(user);
                                }
                            }
                        }

                    }
                   
                }
                else
                {
                    if (orgId != 0)
                    {

                        var persons = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                  .Where(w => w.o.OrganizationUnitID == orgId).Select(s => s.p).OrderBy(o => o.DisplayName).ToList();
                        if (persons != null)
                        {
                            foreach (var p in persons.Where(p => p.Custom2 == null || p.Custom2 == "" ))
                            {
                                user = new OrganizationPerson();
                                user.id = p.PersonID;
                                user.name = p.DisplayName;
                                user.telephone = p.MobilePhone;
                                user.UserEmail = p.Email;
                                user.Department = p.Custom1;
                                user.UserGroup = string.Join(",", entity.UserGroups.Join(entity.UserGroup_User, gp => gp.UserGroupId, go => go.UserGroupId, (GP, GO) => new { gp = GP, go = GO })
                         .Where(w => w.go.UserId == p.PersonID).Select(gs => gs.gp.UserGroupName));
                                userList.Add(user);
                            }
                        }
                    }
                }


            }
            userList = userList.OrderBy( o => o.name ).ToList();
                return userList;
        }
    }
}