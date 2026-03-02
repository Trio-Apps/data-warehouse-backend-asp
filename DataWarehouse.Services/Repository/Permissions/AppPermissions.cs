using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Permissions
{
    public static class AppPermissions
    {
        #region Auth
        // user
        #region users
        public const string Users_Create = "Users.Create";
        public const string Users_Edit = "Users.Edit";
        public const string Users_Delete = "Users.Delete";
        public const string Users_Get = "Users.Get";
        #endregion

        // role
        #region roles
        public const string Roles_Create = "Roles.Create";
        public const string Roles_Edit = "Roles.Edit";
        public const string Roles_Delete = "Roles.Delete";
        public const string Roles_Get = "Roles.Get";
        #endregion

        #region Passwords
        public const string Passwords_Change = "Passwords.Change";
        public const string Passwords_Validate = "Passwords.Validate";
        public const string Passwords_Has_Password = "Passwords.Has.Password";

        #endregion


        //#region Permissions
        //public const string Permissions_Create = "Permissions.Create";
        //public const string Permissions_Edit = "Permissions.Edit";
        //public const string Permissions_Delete = "Permissions.Delete";
        //public const string Permissions_Get = "Permissions.Get";
        //#endregion

        #endregion

        #region Actors
        // Items
        #region Items
     //   public const string Items_Create = "Items.Create";
      //  public const string Items_Edit = "Items.Edit";
      //  public const string Items_Delete = "Items.Delete";
        public const string Items_Get = "Items.Get";
        #endregion

        // Warehouses
        #region Warehouses
       // public const string Warehouses_Create = "Warehouses.Create";
      //  public const string Warehouses_Edit = "Warehouses.Edit";
       // public const string Warehouses_Delete = "Warehouses.Delete";
        public const string Warehouses_Get = "Warehouses.Get";
        #endregion


        #endregion

        #region AllinAll
        // Items
        #region Saps
        public const string Saps_Create = "Saps.Create";
        public const string Saps_Edit = "Saps.Edit";
        public const string Saps_Delete = "Saps.Delete";
        public const string Saps_Get = "Saps.Get";
        #endregion

        // Warehouses
        #region Companys
        public const string Companys_Create = "Companys.Create";
        public const string Companys_Edit = "Companys.Edit";
        public const string Companys_Delete = "Companys.Delete";
        public const string Companys_Get = "Companys.Get";
        #endregion


        #endregion

        // Processes
        #region Processes

        #region Purchases
        public const string Purchases_Create = "Purchases.Create";
        public const string Purchases_Edit = "Purchases.Edit";
        public const string Purchases_Delete = "Purchases.Delete";
        public const string Purchases_Get = "Purchases.Get";
        #endregion

        #region Receipt
        public const string Receipt_Create = "Receipt.Create";
        public const string Receipt_Edit = "Receipt.Edit";
        public const string Receipt_Delete = "Receipt.Delete";
        public const string Receipt_Get = "Receipt.Get";
        #endregion

        #region GoodsReturn
        public const string GoodsReturn_Create = "GoodsReturn.Create";
        public const string GoodsReturn_Edit = "GoodsReturn.Edit";
        public const string GoodsReturn_Delete = "GoodsReturn.Delete";
        public const string GoodsReturn_Get = "GoodsReturn.Get";
        #endregion

        #region Sales
        public const string Sales_Create = "Sales.Create";
        public const string Sales_Edit = "Sales.Edit";
        public const string Sales_Delete = "Sales.Delete";
        public const string Sales_Get = "Sales.Get";
        #endregion

        #region DeliveryNote
        public const string DeliveryNote_Create = "DeliveryNote.Create";
        public const string DeliveryNote_Edit = "DeliveryNote.Edit";
        public const string DeliveryNote_Delete = "DeliveryNote.Delete";
        public const string DeliveryNote_Get = "DeliveryNote.Get";
        #endregion
        #region SalesReturn
        public const string SalesReturn_Create = "SalesReturn.Create";
        public const string SalesReturn_Edit = "SalesReturn.Edit";
        public const string SalesReturn_Delete = "SalesReturn.Delete";
        public const string SalesReturn_Get = "SalesReturn.Get";
        #endregion

        #region Productions
        public const string Productions_Create = "Productions.Create";
        public const string Productions_Edit = "Productions.Edit";
        public const string Productions_Delete = "Productions.Delete";
        public const string Productions_Get = "Productions.Get";
        #endregion

        #region Counting
        public const string Counting_Create = "Counting.Create";
        public const string Counting_Edit = "Counting.Edit";
        public const string Counting_Delete = "Counting.Delete";
        public const string Counting_Get = "Counting.Get";
        #endregion

        #region Transferred
        public const string Transferred_Create = "Transferred.Create";
        public const string Transferred_Edit = "Transferred.Edit";
        public const string Transferred_Delete = "Transferred.Delete";
        public const string Transferred_Get = "Transferred.Get";
        #endregion

        #region Received
        public const string Received_Create = "Received.Create";
        public const string Received_Edit = "Received.Edit";
        public const string Received_Delete = "Received.Delete";
        public const string Received_Get = "Received.Get";
        #endregion

        #endregion

        #region Approval
        // ApprovalSteps
        #region ApprovalSteps
        public const string ApprovalSteps_Create = "ApprovalSteps.Create";
        public const string ApprovalSteps_Edit = "ApprovalSteps.Edit";
        public const string ApprovalSteps_Delete = "ApprovalSteps.Delete";
        public const string ApprovalSteps_Get = "ApprovalSteps.Get";
        #endregion

        // ProcessApprovals
        #region ProcessApprovals
        public const string ProcessApprovals_Create = "ProcessApprovals.Create";
        public const string ProcessApprovals_Edit = "ProcessApprovals.Edit";
        public const string ProcessApprovals_Delete = "ProcessApprovals.Delete";
        public const string ProcessApprovals_Get = "ProcessApprovals.Get";
        #endregion

        // ProcessProgresses (ProcessItemIsProgress)
        #region ProcessProgresses
        public const string ProcessProgresses_Create = "ProcessProgresses.Create";
        public const string ProcessProgresses_Edit = "ProcessProgresses.Edit";
        public const string ProcessProgresses_Delete = "ProcessProgresses.Delete";
        public const string ProcessProgresses_Get = "ProcessProgresses.Get";
        #endregion
        // Approvals (My inbox + actions)

        #region Approvals
        public const string Approvals_GetMy = "Approvals.GetMy";
        public const string Approvals_Action = "Approvals.Action";
        #endregion
        #endregion

        public static readonly PermissionSeed[] All =
        {
        #region Auth
            // user
             #region users
              new(AppPermissions.Users_Create, "Create User", "Users"),
              new(AppPermissions.Users_Edit,   "Edit User",   "Users"),
              new(AppPermissions.Users_Delete, "Delete User", "Users"),
              new(AppPermissions.Users_Get, "Get User", "Users"),
            #endregion

             #region roles
              new(AppPermissions.Roles_Create, "Create Role", "Roles"),
              new(AppPermissions.Roles_Edit,   "Edit Role",   "Roles"),
              new(AppPermissions.Roles_Delete, "Delete Role", "Roles"),
              new(AppPermissions.Roles_Get, "Get Role", "Roles"),
            #endregion

            #region Passwords
              new(AppPermissions.Passwords_Change, "Change Password", "Passwords"),
              new(AppPermissions.Passwords_Validate,   "Validate Password",   "Passwords"),
              new(AppPermissions.Passwords_Has_Password, "Has Password Password", "Passwords"),
            #endregion

            //   #region Permissions
            //  new(AppPermissions.Permissions_Create, "Create Permission", "Permissions"),
            //  new(AppPermissions.Permissions_Edit,   "Edit Permission",   "Permissions"),
            //  new(AppPermissions.Permissions_Delete, "Delete Permission", "Permissions"),
            //  new(AppPermissions.Permissions_Get, "Get Permission", "Permissions"),
            //#endregion

            #endregion

        #region Actors
            // Items
             #region Items
              //new(AppPermissions.Items_Create, "Create Item", "Items"),
              //new(AppPermissions.Items_Edit,   "Edit Item",   "Items"),
              //new(AppPermissions.Items_Delete, "Delete Item", "Items"),
              new(AppPermissions.Items_Get, "Get Item", "Items"),
            #endregion

             #region Warehouses
              //new(AppPermissions.Warehouses_Create, "Create Warehouse", "Warehouses"),
              //new(AppPermissions.Warehouses_Edit,   "Edit Warehouse",   "Warehouses"),
              //new(AppPermissions.Warehouses_Delete, "Delete Warehouse", "Warehouses"),
              new(AppPermissions.Warehouses_Get, "Get Warehouse", "Warehouses"),
            #endregion

          

            #endregion

        #region AllinAll
            // Saps
             #region Saps
              new(AppPermissions.Saps_Create, "Create Sap", "Saps"),
              new(AppPermissions.Saps_Edit,   "Edit Sap",   "Saps"),
              new(AppPermissions.Saps_Delete, "Delete Sap", "Saps"),
              new(AppPermissions.Saps_Get, "Get Sap", "Saps"),
            #endregion

             #region Companys
              new(AppPermissions.Companys_Create, "Create Company", "Companys"),
              new(AppPermissions.Companys_Edit,   "Edit Company",   "Companys"),
              new(AppPermissions.Companys_Delete, "Delete Company", "Companys"),
              new(AppPermissions.Companys_Get, "Get Company", "Companys"),
            #endregion

            #endregion

        #region Processes
                #region Purchases
    new(AppPermissions.Purchases_Create, "Create Purchase", "Purchases"),
    new(AppPermissions.Purchases_Edit,   "Edit Purchase",   "Purchases"),
    new(AppPermissions.Purchases_Delete, "Delete Purchase", "Purchases"),
    new(AppPermissions.Purchases_Get,    "Get Purchases",   "Purchases"),
    #endregion

    #region Receipt
    new(AppPermissions.Receipt_Create, "Create Receipt", "Receipt"),
    new(AppPermissions.Receipt_Edit,   "Edit Receipt",   "Receipt"),
    new(AppPermissions.Receipt_Delete, "Delete Receipt", "Receipt"),
    new(AppPermissions.Receipt_Get,    "Get Receipts",   "Receipt"),
    #endregion

    #region GoodsReturn
    new(AppPermissions.GoodsReturn_Create, "Create Goods Return", "GoodsReturn"),
    new(AppPermissions.GoodsReturn_Edit,   "Edit Goods Return",   "GoodsReturn"),
    new(AppPermissions.GoodsReturn_Delete, "Delete Goods Return", "GoodsReturn"),
    new(AppPermissions.GoodsReturn_Get,    "Get Goods Returns",   "GoodsReturn"),
    #endregion

    #region Sales
    new(AppPermissions.Sales_Create, "Create Sale", "Sales"),
    new(AppPermissions.Sales_Edit,   "Edit Sale",   "Sales"),
    new(AppPermissions.Sales_Delete, "Delete Sale", "Sales"),
    new(AppPermissions.Sales_Get,    "Get Sales",   "Sales"),
    #endregion

      #region DeliveryNote
    new(AppPermissions.DeliveryNote_Create, "Create DeliveryNote", "DeliveryNote"),
    new(AppPermissions.DeliveryNote_Edit,   "Edit DeliveryNote",   "DeliveryNote"),
    new(AppPermissions.DeliveryNote_Delete, "Delete DeliveryNote", "DeliveryNote"),
    new(AppPermissions.DeliveryNote_Get,    "Get DeliveryNote",   "DeliveryNote"),
    #endregion

    #region SalesReturn
    new(AppPermissions.SalesReturn_Create, "Create Sales Return", "SalesReturn"),
    new(AppPermissions.SalesReturn_Edit,   "Edit Sales Return",   "SalesReturn"),
    new(AppPermissions.SalesReturn_Delete, "Delete Sales Return", "SalesReturn"),
    new(AppPermissions.SalesReturn_Get,    "Get Sales Returns",   "SalesReturn"),
    #endregion

    #region Productions
    new(AppPermissions.Productions_Create, "Create Production", "Productions"),
    new(AppPermissions.Productions_Edit,   "Edit Production",   "Productions"),
    new(AppPermissions.Productions_Delete, "Delete Production", "Productions"),
    new(AppPermissions.Productions_Get,    "Get Productions",   "Productions"),
    #endregion

    #region Counting
    new(AppPermissions.Counting_Create, "Create Counting", "Counting"),
    new(AppPermissions.Counting_Edit,   "Edit Counting",   "Counting"),
    new(AppPermissions.Counting_Delete, "Delete Counting", "Counting"),
    new(AppPermissions.Counting_Get,    "Get Countings",   "Counting"),
    #endregion

    #region Transferred
    new(AppPermissions.Transferred_Create, "Create Transfer", "Transferred"),
    new(AppPermissions.Transferred_Edit,   "Edit Transfer",   "Transferred"),
    new(AppPermissions.Transferred_Delete, "Delete Transfer", "Transferred"),
    new(AppPermissions.Transferred_Get,    "Get Transfers",   "Transferred"),
    #endregion

    #region Received
    new(AppPermissions.Received_Create, "Create Receiving", "Received"),
    new(AppPermissions.Received_Edit,   "Edit Receiving",   "Received"),
    new(AppPermissions.Received_Delete, "Delete Receiving", "Received"),
    new(AppPermissions.Received_Get,    "Get Receivings",   "Received"),
            #endregion
            #endregion

        #region Approval

        #region ApprovalSteps
new(AppPermissions.ApprovalSteps_Create, "Create Approval Step", "ApprovalSteps"),
new(AppPermissions.ApprovalSteps_Edit,   "Edit Approval Step",   "ApprovalSteps"),
new(AppPermissions.ApprovalSteps_Delete, "Delete Approval Step", "ApprovalSteps"),
new(AppPermissions.ApprovalSteps_Get,    "Get Approval Steps",   "ApprovalSteps"),
#endregion

        #region ProcessApprovals
        new (AppPermissions.ProcessApprovals_Create, "Create Process Approval", "ProcessApprovals"),
new (AppPermissions.ProcessApprovals_Edit,   "Edit Process Approval",   "ProcessApprovals"),
new (AppPermissions.ProcessApprovals_Delete, "Delete Process Approval", "ProcessApprovals"),
new (AppPermissions.ProcessApprovals_Get,    "Get Process Approvals",   "ProcessApprovals"),
#endregion
        
        #region ProcessProgresses
new(AppPermissions.ProcessProgresses_Create, "Create Process Progress", "ProcessProgresses"),
new(AppPermissions.ProcessProgresses_Edit,   "Edit Process Progress",   "ProcessProgresses"),
new(AppPermissions.ProcessProgresses_Delete, "Delete Process Progress", "ProcessProgresses"),
new(AppPermissions.ProcessProgresses_Get,    "Get Process Progresses",  "ProcessProgresses"),
#endregion

        #region Approvals
        new (AppPermissions.Approvals_GetMy,  "Get My Approvals",      "Approvals"),
new (AppPermissions.Approvals_Action, "Approve / Reject Step", "Approvals"),
#endregion

        #endregion

    };

        public readonly record struct PermissionSeed(string Key, string Name, string Group);
    }

}
