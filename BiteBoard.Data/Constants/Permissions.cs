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
        else if (module == "Batches")
            modulePermissions.Add($"Permissions.{module}.BatchRecall");
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

    public static class Companies
    {
        public const string View = "Permissions.Companies.View";
        public const string Create = "Permissions.Companies.Create";
        public const string Edit = "Permissions.Companies.Edit";
        public const string Delete = "Permissions.Companies.Delete";
    }

    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
    }

    public static class Batches
    {
        public const string View = "Permissions.Batches.View";
        public const string Create = "Permissions.Batches.Create";
        public const string Edit = "Permissions.Batches.Edit";
        public const string Delete = "Permissions.Batches.Delete";
        public const string BatchRecall = "Permissions.Batches.BatchRecall";
    }

    public static class Suppliers
    {
        public const string View = "Permissions.Suppliers.View";
        public const string Create = "Permissions.Suppliers.Create";
        public const string Edit = "Permissions.Suppliers.Edit";
        public const string Delete = "Permissions.Suppliers.Delete";
    }

    public static class MaterialCompositions
    {
        public const string View = "Permissions.MaterialCompositions.View";
        public const string Create = "Permissions.MaterialCompositions.Create";
        public const string Edit = "Permissions.MaterialCompositions.Edit";
        public const string Delete = "Permissions.MaterialCompositions.Delete";
    }

    public static class DigitalProductPassports
    {
        public const string View = "Permissions.DigitalProductPassports.View";
        public const string Create = "Permissions.DigitalProductPassports.Create";
        public const string Edit = "Permissions.DigitalProductPassports.Edit";
        public const string Delete = "Permissions.DigitalProductPassports.Delete";
    }

    public static class Widgets
    {
        public const string View = "Permissions.Widgets.View";
        public const string Create = "Permissions.Widgets.Create";
        public const string Edit = "Permissions.Widgets.Edit";
        public const string Delete = "Permissions.Widgets.Delete";
    }

    public static class Pages
    {
        public const string View = "Permissions.Pages.View";
        public const string Create = "Permissions.Pages.Create";
        public const string Edit = "Permissions.Pages.Edit";
        public const string Delete = "Permissions.Pages.Delete";
    }

    public static class Templates
    {
        public const string View = "Permissions.Templates.View";
        public const string Create = "Permissions.Templates.Create";
        public const string Edit = "Permissions.Templates.Edit";
        public const string Delete = "Permissions.Templates.Delete";
    }

    public static class Tenants
    {
        public const string View = "Permissions.Tenants.View";
        public const string Create = "Permissions.Tenants.Create";
        public const string Edit = "Permissions.Tenants.Edit";
        public const string Delete = "Permissions.Tenants.Delete";
        public const string Enable = "Permissions.Tenants.Enable";
    }
}