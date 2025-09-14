using System.Collections.Generic;

namespace BiteBoard.Data.Constants;

public static class Permissions
{
    public static List<string> GeneratePermissionsForModule(string module)
    {
        List<string> modulePermissions = new();
        if (module == "Dashboard")
            modulePermissions.Add($"Permissions.{module}.View");
        else
        {
            modulePermissions.Add($"Permissions.{module}.View");
            modulePermissions.Add($"Permissions.{module}.Create");
            modulePermissions.Add($"Permissions.{module}.Edit");
            modulePermissions.Add($"Permissions.{module}.Delete");
        }
        // Additional permissions than CRUD
        if (module == "Users")
            modulePermissions.Add($"Permissions.{module}.Roles");
        else if (module == "Roles")
            modulePermissions.Add($"Permissions.{module}.Permissions");
        else if (module == "Tenants")
            modulePermissions.Add($"Permissions.{module}.Enable");
        else if (module == "ModifierGroups" || module == "MenuItems" || module == "Deals" || module == "Tables" || module == "Orders")
            modulePermissions.Add($"Permissions.{module}.Manage");
        return modulePermissions;
    }

    public static class Dashboard
    {
        public const string View = "Permissions.Dashboard.View";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
        public const string Roles = "Permissions.Users.Roles";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
        public const string Permissions = "Permissions.Roles.Permissions";
    }

    public static class Tenants
    {
        public const string View = "Permissions.Tenants.View";
        public const string Create = "Permissions.Tenants.Create";
        public const string Edit = "Permissions.Tenants.Edit";
        public const string Delete = "Permissions.Tenants.Delete";
        public const string Enable = "Permissions.Tenants.Enable";
    }

    public static class Categories
    {
        public const string View = "Permissions.Categories.View";
        public const string Create = "Permissions.Categories.Create";
        public const string Edit = "Permissions.Categories.Edit";
        public const string Delete = "Permissions.Categories.Delete";
    }

    public static class ModifierGroups
    {
        public const string View = "Permissions.ModifierGroups.View";
        public const string Create = "Permissions.ModifierGroups.Create";
        public const string Edit = "Permissions.ModifierGroups.Edit";
        public const string Delete = "Permissions.ModifierGroups.Delete";
        public const string Manage = "Permissions.ModifierGroups.Manage";
    }

    public static class MenuItems
    {
        public const string View = "Permissions.MenuItems.View";
        public const string Create = "Permissions.MenuItems.Create";
        public const string Edit = "Permissions.MenuItems.Edit";
        public const string Delete = "Permissions.MenuItems.Delete";
        public const string Manage = "Permissions.MenuItems.Manage";
    }

    public static class Deals
    {
        public const string View = "Permissions.Deals.View";
        public const string Create = "Permissions.Deals.Create";
        public const string Edit = "Permissions.Deals.Edit";
        public const string Delete = "Permissions.Deals.Delete";
        public const string Manage = "Permissions.Deals.Manage";
    }

    public static class Tables
    {
        public const string View = "Permissions.Tables.View";
        public const string Create = "Permissions.Tables.Create";
        public const string Edit = "Permissions.Tables.Edit";
        public const string Delete = "Permissions.Tables.Delete";
        public const string Manage = "Permissions.Tables.Manage";
    }

    public static class Orders
    {
        public const string View = "Permissions.Orders.View";
        public const string Create = "Permissions.Orders.Create";
        public const string Edit = "Permissions.Orders.Edit";
        public const string Delete = "Permissions.Orders.Delete";
        public const string Manage = "Permissions.Orders.Manage";
    }
}