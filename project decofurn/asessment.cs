using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// Define your model classes based on the tables.sql script
public class InvoiceHeader
{
    public int InvoiceId { get; set; }
    public int InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string Address { get; set; }
    public float InvoiceTotal { get; set; }
    public List<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}

public class InvoiceLine
{
    public int LineId { get; set; }
    public int InvoiceNumber { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitSellingPriceExVAT { get; set; }
}

public static class InvoiceImporter

{
   
    private static readonly string connectionString = "Server=localhost;Database=bbq;Trusted_Connection=True;"; //  cooooonection string

    public static void ImportInvoices(string filePath)
    {
        var invoices = ReadInvoicesFromCsv("data.csv");

        using (var context = new InvoiceContext())
        {
            // No duplicates
            foreach (var invoice in invoices)
            {
                context.InvoiceHeaders.UpdateRange(invoice.InvoiceHeaders);
            }

            context.SaveChanges();

            ValidateTotalQuantity(invoices);
        }
    }

    private static List<Invoice> ReadInvoicesFromCsv(string filePath)
    {
        var invoices = new List<Invoice>();
        var currentInvoice = new Invoice();

        try
        {
            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var fields = line.Split(',');

                    if (fields.Length != 5)
                    {
                        Console.WriteLine(< span class="math-inline">"Error\: Invalid line format in CSV file\: \{line\}"\);
continue;
\}
var invoiceNumber \= int\.Parse\(fields\[0\]\);
if \(invoiceNumber \!\= currentInvoice\.InvoiceNumber\)
\{
invoices\.Add\(currentInvoice\);
currentInvoice \= new Invoice\(\);
\}
currentInvoice\.InvoiceNumber \= invoiceNumber;
currentInvoice\.InvoiceDate \= DateTime\.Parse\(fields\[1\]\);
currentInvoice\.InvoiceLines\.Add\(new InvoiceLine
\{
    ItemCode \= int\.Parse\(fields\[2\]\),
Description \= fields\[3\],
Quantity \= int\.Parse\(fields\[4\]\),
UnitSellingPriceExVAT \= decimal\.Parse\(fields\[4\], CultureInfo\.InvariantCulture\)
\}\);
\}
invoices\.Add\(currentInvoice\); // Add the last invoice
\}
\}
catch \(Exception ex\)
\{
Console\.WriteLine\(</span>"Error reading CSV file: {ex.Message}");
        }

        return invoices;
    }

// validating line balances

    private static void ValidateTotalQuantity(List<Invoice> invoices)
{
    var totalQuantityFromCsv = invoices.Sum(i => i.InvoiceLines.Sum(l => l.Quantity));
    var totalQuantityFromDb = 0;

    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        var command = new SqlCommand("SELECT SUM(Quantity) FROM InvoiceLines", connection);
        totalQuantityFromDb = (int)command.ExecuteScalar();
    }

    if (totalQuantityFromCsv != totalQuantityFromDb)
    {
        Console.WriteLine($"Error: Validation failed. Total quantity from CSV ({totalQuantityFromCsv}) does not match total quantity from database ({totalQuantityFromDb}).");
    }
    else
    {
        Console.WriteLine("Validation successful. Total quantity matches.");
    }
}
}

public class InvoiceContext : DbContext
{
    public InvoiceContext(DbContextOptions<InvoiceContext> options) : base(options) { }

    public DbSet<InvoiceHeader> InvoiceHeaders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //
    }
}

public class Invoice
{
    public int InvoiceId { get; set; }
    public int InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string Address { get; set; }
    public float InvoiceTotal { get; set; }
    public List<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
