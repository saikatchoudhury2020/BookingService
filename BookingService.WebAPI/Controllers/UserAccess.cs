using Digimaker.Data.Directory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace WebAPI.Controllers
{
    public class UserAccess
    {

        //todo: implement this in permission system(with role, user in building, service.)
        //if result contains 0, it can do everything.
        public static List<int> GetCurrentUserServices()
        {
            var list = new List<int>();

            var roles = GetRoleList();
            if ( roles.Contains( 1 ) ) //admin group
            {
                list.Add( 0 );
            }
            else
            {
                list.Add( 1169 ); //todo: connect with permission system.
                list.Add( 1170 );
                list.Add(0); //todo: cx-support this better
            }
            return list;
        }

        /// <summary>
        /// Check if user can only book for himself.
        /// </summary>
        /// <returns></returns>
        public static bool SelfOnly()
        {
            var roles = GetRoleList();
            var result = true;
            if( roles.Contains( 1 ) )
            {
                result = false;
            }
            return result;
        }

        public static List<int> GetRoleList()
        {
            var userID = Digimaker.User.Identity.ID;
            var data = PersonHandler.GetAccessRoles(userID);
            var roles = new List<int>();
            foreach (DataRow row in data.AccessRole.Rows)
            {
                roles.Add(Int32.Parse(row["RoleID"].ToString()));
            }
            return roles;
        }

        public static bool HasAccessToRole( int roleID )
        {
            var result = false;
            var roleList = UserAccess.GetRoleList();
            if (roleList.Contains(1) || roleList.Contains( roleID ))
            {
                result = true;
            }
            return result;
        }
    }
}