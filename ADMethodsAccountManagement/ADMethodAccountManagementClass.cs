using System;
using System.Collections;
using System.Text;
using System.DirectoryServices;
using System.Data;
using System.Configuration;
using System.DirectoryServices.AccountManagement;

public class ADMethodsAccountManagement
{
    
        #region Variables

        private string sDomain = "";
        private string sDefaultOU;
        private string sDefaultRootOU = "";
        private string sServiceUser = "";
        private string sServicePassword = "";

    //All us to set and retrieve the private strings
    public string Domain
    {
        get
        {
            return sDomain;
        }
        set
        {
            sDomain = value;
        }
    }

    public string DefaultOU
    {
        get
        {
            return sDefaultOU;
        }
        set
        {
            sDefaultOU = value;
        }
    }

    public string RootOU
    {
        get
        {
            return sDefaultRootOU;
        }
        set
        {
            sDefaultRootOU = value;
        }
    }

    public string DomainUser
    {
        get
        {
            return sServiceUser;
        }
        set
        {
            sServiceUser = value;
        }
    }

    public string DomainUserPassword
    {
        get
        {
            return sServicePassword;
        }
        set
        {
            sServicePassword = value;
        }
    }

    #endregion
    #region Validate Methods

    /// &lt;summary>
    /// Validates the username and password of a given user
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to validate&lt;/param>
    /// &lt;param name="sPassword">The password of the username to validate&lt;/param>
    /// &lt;returns>Returns True of user is valid&lt;/returns>
    public bool ValidateCredentials(string sUserName, string sPassword)
    {
        PrincipalContext oPrincipalContext = GetPrincipalContext();
        return oPrincipalContext.ValidateCredentials(sUserName, sPassword);
    }

    /// &lt;summary>
    /// Checks if the User Account is Expired
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to check&lt;/param>
    /// &lt;returns>Returns true if Expired&lt;/returns>
    public bool IsUserExpired(string sUserName)
    {
        UserPrincipal oUserPrincipal = GetUser(sUserName);
        if (oUserPrincipal.AccountExpirationDate != null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// &lt;summary>
    /// Checks if user exists on AD
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to check&lt;/param>
    /// &lt;returns>Returns true if username Exists&lt;/returns>
    public bool IsUserExisiting(string sUserName)
    {
        if (GetUser(sUserName) == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// &lt;summary>
    /// Checks if user account is locked
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to check&lt;/param>
    /// &lt;returns>Returns true of Account is locked&lt;/returns>
    public bool IsAccountLocked(string sUserName)
    {
        UserPrincipal oUserPrincipal = GetUser(sUserName);
        return oUserPrincipal.IsAccountLockedOut();
    }
    #endregion

    #region Search Methods

    /// &lt;summary>
    /// Gets a certain user on Active Directory
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to get&lt;/param>
    /// &lt;returns>Returns the UserPrincipal Object&lt;/returns>
    public UserPrincipal GetUser(string sUserName)
    {
        PrincipalContext oPrincipalContext = GetPrincipalContext();

        UserPrincipal oUserPrincipal =
           UserPrincipal.FindByIdentity(oPrincipalContext, sUserName);
        return oUserPrincipal;
    }

    /// &lt;summary>
    /// Gets a certain group on Active Directory
    /// &lt;/summary>
    /// &lt;param name="sGroupName">The group to get&lt;/param>
    /// &lt;returns>Returns the GroupPrincipal Object&lt;/returns>
    public GroupPrincipal GetGroup(string sGroupName)
    {
        PrincipalContext oPrincipalContext = GetPrincipalContext();

        GroupPrincipal oGroupPrincipal =
           GroupPrincipal.FindByIdentity(oPrincipalContext, sGroupName);
        return oGroupPrincipal;
    }

    #endregion

    #region User Account Methods

    /// &lt;summary>
    /// Sets the user password
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to set&lt;/param>
    /// &lt;param name="sNewPassword">The new password to use&lt;/param>
    /// &lt;param name="sMessage">Any output messages&lt;/param>
    public void SetUserPassword(string sUserName, string sNewPassword, out string sMessage)
    {
        try
        {
            UserPrincipal oUserPrincipal = GetUser(sUserName);
            oUserPrincipal.SetPassword(sNewPassword);
            sMessage = "";
        }
        catch (Exception ex)
        {
            sMessage = ex.Message;
        }
    }

    /// &lt;summary>
    /// Enables a disabled user account
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to enable&lt;/param>
    public void EnableUserAccount(string sUserName)
    {
        UserPrincipal oUserPrincipal = GetUser(sUserName);
        oUserPrincipal.Enabled = true;
        oUserPrincipal.Save();
    }

    /// &lt;summary>
    /// Force disabling of a user account
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to disable&lt;/param>
    public void DisableUserAccount(string sUserName)
    {
        UserPrincipal oUserPrincipal = GetUser(sUserName);
        oUserPrincipal.Enabled = false;
        oUserPrincipal.Save();
    }

    /// &lt;summary>
    /// Force expire password of a user
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to expire the password&lt;/param>
    public void ExpireUserPassword(string sUserName)
    {
        UserPrincipal oUserPrincipal = GetUser(sUserName);
        oUserPrincipal.ExpirePasswordNow();
        oUserPrincipal.Save();
    }

    /// &lt;summary>
    /// Unlocks a locked user account
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username to unlock&lt;/param>
    public void UnlockUserAccount(string sUserName)
    {
        UserPrincipal oUserPrincipal = GetUser(sUserName);
        oUserPrincipal.UnlockAccount();
        oUserPrincipal.Save();
    }

    /// &lt;summary>
    /// Creates a new user on Active Directory
    /// &lt;/summary>
    /// &lt;param name="sOU">The OU location you want to save your user&lt;/param>
    /// &lt;param name="sUserName">The username of the new user&lt;/param>
    /// &lt;param name="sPassword">The password of the new user&lt;/param>
    /// &lt;param name="sGivenName">The given name of the new user&lt;/param>
    /// &lt;param name="sSurname">The surname of the new user&lt;/param>
    /// &lt;returns>returns the UserPrincipal object&lt;/returns>
    public UserPrincipal CreateNewUser(string sOU,
       string sUserName, string sPassword, string sGivenName, string sSurname)
    {
        if (!IsUserExisiting(sUserName))
        {
            PrincipalContext oPrincipalContext = GetPrincipalContext(sOU);

            UserPrincipal oUserPrincipal = new UserPrincipal
               (oPrincipalContext, sUserName, sPassword, true /*Enabled or not*/);

            //User Log on Name
            oUserPrincipal.UserPrincipalName = sUserName;
            oUserPrincipal.GivenName = sGivenName;
            oUserPrincipal.Surname = sSurname;
            oUserPrincipal.Save();

            return oUserPrincipal;
        }
        else
        {
            return GetUser(sUserName);
        }
    }

    /// &lt;summary>
    /// Deletes a user in Active Directory
    /// &lt;/summary>
    /// &lt;param name="sUserName">The username you want to delete&lt;/param>
    /// &lt;returns>Returns true if successfully deleted&lt;/returns>
    public bool DeleteUser(string sUserName)
    {
        try
        {
            UserPrincipal oUserPrincipal = GetUser(sUserName);

            oUserPrincipal.Delete();
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Group Methods

    /// &lt;summary>
    /// Creates a new group in Active Directory
    /// &lt;/summary>
    /// &lt;param name="sOU">The OU location you want to save your new Group&lt;/param>
    /// &lt;param name="sGroupName">The name of the new group&lt;/param>
    /// &lt;param name="sDescription">The description of the new group&lt;/param>
    /// &lt;param name="oGroupScope">The scope of the new group&lt;/param>
    /// &lt;param name="bSecurityGroup">True is you want this group 
    /// to be a security group, false if you want this as a distribution group&lt;/param>
    /// &lt;returns>Returns the GroupPrincipal object&lt;/returns>
    public GroupPrincipal CreateNewGroup(string sOU, string sGroupName,
       string sDescription, GroupScope oGroupScope, bool bSecurityGroup)
    {
        PrincipalContext oPrincipalContext = GetPrincipalContext(sOU);

        GroupPrincipal oGroupPrincipal = new GroupPrincipal(oPrincipalContext, sGroupName);
        oGroupPrincipal.Description = sDescription;
        oGroupPrincipal.GroupScope = oGroupScope;
        oGroupPrincipal.IsSecurityGroup = bSecurityGroup;
        oGroupPrincipal.Save();

        return oGroupPrincipal;
    }

    /// &lt;summary>
    /// Adds the user for a given group
    /// &lt;/summary>
    /// &lt;param name="sUserName">The user you want to add to a group&lt;/param>
    /// &lt;param name="sGroupName">The group you want the user to be added in&lt;/param>
    /// &lt;returns>Returns true if successful&lt;/returns>
    public bool AddUserToGroup(string sUserName, string sGroupName)
    {
        try
        {
            UserPrincipal oUserPrincipal = GetUser(sUserName);
            GroupPrincipal oGroupPrincipal = GetGroup(sGroupName);
            if (oUserPrincipal != null || oGroupPrincipal != null)
            {
                if (!IsUserGroupMember(sUserName, sGroupName))
                {
                    oGroupPrincipal.Members.Add(oUserPrincipal);
                    oGroupPrincipal.Save();
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// &lt;summary>
    /// Removes user from a given group
    /// &lt;/summary>
    /// &lt;param name="sUserName">The user you want to remove from a group&lt;/param>
    /// &lt;param name="sGroupName">The group you want the user to be removed from&lt;/param>
    /// &lt;returns>Returns true if successful&lt;/returns>
    public bool RemoveUserFromGroup(string sUserName, string sGroupName)
    {
        try
        {
            UserPrincipal oUserPrincipal = GetUser(sUserName);
            GroupPrincipal oGroupPrincipal = GetGroup(sGroupName);
            if (oUserPrincipal != null || oGroupPrincipal != null)
            {
                if (IsUserGroupMember(sUserName, sGroupName))
                {
                    oGroupPrincipal.Members.Remove(oUserPrincipal);
                    oGroupPrincipal.Save();
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// &lt;summary>
    /// Checks if user is a member of a given group
    /// &lt;/summary>
    /// &lt;param name="sUserName">The user you want to validate&lt;/param>
    /// &lt;param name="sGroupName">The group you want to check the 
    /// membership of the user&lt;/param>
    /// &lt;returns>Returns true if user is a group member&lt;/returns>
    public bool IsUserGroupMember(string sUserName, string sGroupName)
    {
        UserPrincipal oUserPrincipal = GetUser(sUserName);
        GroupPrincipal oGroupPrincipal = GetGroup(sGroupName);

        if (oUserPrincipal != null || oGroupPrincipal != null)
        {
            return oGroupPrincipal.Members.Contains(oUserPrincipal);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a list of the users group memberships
    /// </summary>
    /// <param name="sUserName">The user you want to get the group memberships</param>
    /// <returns>Returns an arraylist of group memberships</returns>
    public ArrayList GetUserGroups(string sUserName)
    {
        ArrayList myItems = new ArrayList();
        UserPrincipal oUserPrincipal = GetUser(sUserName);

        PrincipalSearchResult<Principal> oPrincipalSearchResult = oUserPrincipal.GetGroups();

        foreach (Principal oResult in oPrincipalSearchResult)
        {
            myItems.Add(oResult.Name);
        }
        return myItems;
    }

    /// <summary>
    /// Gets a list of the users authorization groups
    /// </summary>
    /// <param name="sUserName">The user you want to get authorization groups</param>
    /// <returns>Returns an arraylist of group authorization memberships</returns>
    public ArrayList GetUserAuthorizationGroups(string sUserName)
    {
        ArrayList myItems = new ArrayList();
        UserPrincipal oUserPrincipal = GetUser(sUserName);

        PrincipalSearchResult<Principal> oPrincipalSearchResult = oUserPrincipal.GetAuthorizationGroups();

        foreach (Principal oResult in oPrincipalSearchResult)
        {
            myItems.Add(oResult.Name);
        }
        return myItems;
    }

    #endregion

    #region Helper Methods

    /// &lt;summary>
    /// Gets the base principal context
    /// &lt;/summary>
    /// &lt;returns>Returns the PrincipalContext object&lt;/returns>
    public PrincipalContext GetPrincipalContext()
    {
        PrincipalContext oPrincipalContext = new PrincipalContext
           (ContextType.Domain, sDomain, sDefaultOU, ContextOptions.SimpleBind,
           sServiceUser, sServicePassword);
        return oPrincipalContext;
    }

    /// &lt;summary>
    /// Gets the principal context on specified OU
    /// &lt;/summary>
    /// &lt;param name="sOU">The OU you want your Principal Context to run on&lt;/param>
    /// &lt;returns>Returns the PrincipalContext object&lt;/returns>
    public PrincipalContext GetPrincipalContext(string sOU)
    {
        PrincipalContext oPrincipalContext =
           new PrincipalContext(ContextType.Domain, sDomain, sOU,
           ContextOptions.SimpleBind, sServiceUser, sServicePassword);
        return oPrincipalContext;
    }

    #endregion
}
