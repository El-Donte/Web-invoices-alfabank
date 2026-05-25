namespace InvoicesWebService.Services.Authorization;

public static class Permissions
{
    public const string InvoiceRead =  "invoices:read";
    public const string InvoiceCreate = "invoices:create";
    public const string InvoiceUpdate=  "invoices:update";
    public const string InvoiceDelete =  "invoices:delete";
    public const string InvoiceApprove = "invoices:approve";
}

public static class CustomClaim
{
    public const string Permission = "permission";
}