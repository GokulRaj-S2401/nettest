using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZOI.BAL.Utilities
{
    public class Constants
    {
        public static class APIResponseStatus
        {
            public static string Success = "S";
            public static string Failed = "F";
        }

        public static class APIResponseMessage
        {
            public static string SuccessMessage = "Success";
            public static string UserAuthentication = "UserAuthentication ";
            public static string Success = "Successfully";
            public static string Inserted = " Added ";
            public static string Updated = " Updated ";
            public static string Deleted = " Deleted ";
            public static string Retrieved = " Retrieved ";
            public static string Data = " Data ";
            public static string Failed = "Unable to process your request.";
            public static string InvalidEmailOrPassword = "Invalid username or password";
            public static string MobileAlreadyExists = "Mobile number already exists";
            public static string nameAlreadyExists = "Name already exists";
            public static string EmailAlreadyExists = "Email already exists";
            public static string MobileAndEmailAlreadyExists = "Mobile number and Email already exists";
            public static string TokenEmptyMessage = "User's token is required. ";
            public static string PasswordAlreadySet = "Password has already set.";
            public static string TokenHasExpired = "Token has expired. Please contact your Administrator";
        }

        public static class Tables
        {
            public const string UserInvite = "UserInvite";
            public const string Template = "Templates";
            public const string Roles = "Roles";
            public const string DocumentRoles = "DocumentRoles";
            public const string Documents = "Documents";
            public const string DocumentStatus = "DocumentStatus";
            public const string History = "History";
            public const string Users = "Users";
            public const string ContentLevelPermission = "ContentLevelPermission";
            public const string ErrorLog = "ErrorLog";
            public const string Notes = "Notes";
            public const string Gallery = "Gallery";
            public const string TimeSpentOnDocumentUsers = "TimeSpentOnDocumentUsers";
            public const string TimeSpentOnDocuments = "TimeSpentOnDocuments";
            public const string Variables = "Variables";
            public const string DocumentGroup = "DocumentGroup";
            public const string Videos = "Videos";
        }

        public static class DateTimeFormat
        {
            public static string DateTimeWith24HrsFormat = "yyyy-MM-dd HH:mm:ss";
        }

        public static class AES
        {
            public static string AES256EncryptString = "WHH4nv43Let4huxP6vBqoabnE7JkpibkMf6wCGRPJBc=";
            public static string AES256IVStringAccID = "bfcvJCbmwS0qaQRmamEyJg==";

        }

    }
}
