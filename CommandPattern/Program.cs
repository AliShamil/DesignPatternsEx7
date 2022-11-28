using ClosedXML.Excel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Data;

namespace CommandPattern;
#nullable disable

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class ExcelFile<T>
{
    private readonly List<T> _list;
    public string FileName = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/{typeof(T).Name}.xlsx";

    public ExcelFile(List<T> list)
    {
        _list=list;
    }


    public MemoryStream Create()
    {
        var wb = new XLWorkbook();
        var ds = new DataSet();


        ds.Tables.Add(GetTable());

        wb.Worksheets.Add(ds);

        var excelMemory = new MemoryStream();
        wb.SaveAs(excelMemory);

        return excelMemory;

    }



    private DataTable GetTable()
    {
        var table = new DataTable();
        var type = typeof(T);

        type.GetProperties().ToList()
            .ForEach(x => table.Columns.Add(x.Name, x.PropertyType));

        _list.ForEach(x =>
        {
            var values = type.GetProperties()
            .Select(properyInfo => properyInfo.GetValue(x, null))
            .ToArray();

            table.Rows.Add(values);
        });

        return table;
    }
}

public class PdfFile<T>
{
    private readonly List<T> _list;
    public string FileName => $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/{typeof(T).Name}.pdf";


    public PdfFile(List<T> list)
    {
        _list = list;
    }

    public MemoryStream Create()
    {
        var dataTable = GetTable();
        MemoryStream pdfMemory = new MemoryStream();
        //var doc = new Document();
        //doc.SetPageSize(PageSize.A4);

        //PdfWriter pdfWriter = PdfWriter.GetInstance(doc, pdfMemory);
        //doc.Open();

        //var pdfTable = new PdfPTable(dataTable.Columns.Count);

        //for (int i = 0; i < dataTable.Columns.Count; i++)
        //{
        //    var cell = new PdfPCell();
        //    cell.AddElement(new Chunk(dataTable.Columns[i].ColumnName.ToUpper()));
        //    pdfTable.AddCell(cell);
        //}

        //for (int i = 0; i < dataTable.Rows.Count; i++)
        //{
        //    for (int j = 0; j < dataTable.Columns.Count; j++)
        //        pdfTable.AddCell(dataTable.Rows[i][j].ToString());
        //}

        //doc.Add(pdfTable);
        //doc.Close();

        //pdfWriter.Close();
        Document document = new Document();
        PdfWriter writer = PdfWriter.GetInstance(document, pdfMemory);
        document.Open();
        Font font5 = FontFactory.GetFont(FontFactory.HELVETICA, 5);

        PdfPTable table = new PdfPTable(dataTable.Columns.Count);

        float[] widths = new float[dataTable.Columns.Count];
        for (int i = 0; i < dataTable.Columns.Count; i++)
            widths[i] = 4f;

        table.SetWidths(widths);

        table.WidthPercentage = 100;

        PdfPCell cell = new PdfPCell(new Phrase("Products"));

        cell.Colspan = dataTable.Columns.Count;

        foreach (DataColumn c in dataTable.Columns)
        {
            table.AddCell(new Phrase(c.ColumnName, font5));
        }

        foreach (DataRow r in dataTable.Rows)
        {
            if (dataTable.Rows.Count > 0)
            {
                for (int h = 0; h < dataTable.Columns.Count; h++)
                {
                    table.AddCell(new Phrase(r[h].ToString(), font5));
                }
            }
        }
        document.Add(table);
        document.Close();
        return pdfMemory;



    }
    private DataTable GetTable()
    {
        var table = new DataTable();

        var type = typeof(T); // Product

        type.GetProperties()
            .ToList()
            .ForEach(x => table.Columns.Add(x.Name, x.PropertyType));


        _list.ForEach(x =>
        {
            var values = type.GetProperties()
                                .Select(properyInfo => properyInfo
                                .GetValue(x, null))
                                .ToArray();

            table.Rows.Add(values);
        });

        return table;
    }






}



public interface ITableActionCommand
{
    void Execute();
}



public class CreateExcelTableActionCommand<T> : ITableActionCommand
{
    private readonly ExcelFile<T> _excelFile;

    public CreateExcelTableActionCommand(ExcelFile<T> excelFile) => _excelFile = excelFile;

    public void Execute()
    {
        MemoryStream excelMemoryStream = _excelFile.Create();
        File.WriteAllBytes(_excelFile.FileName, excelMemoryStream.ToArray());
    }
}

public class CreatePdfTableActionCommand<T> : ITableActionCommand
{
    private readonly PdfFile<T> _pdfFile;

    public CreatePdfTableActionCommand(PdfFile<T> pdfFile)
      => _pdfFile = pdfFile;


    public void Execute()
    {
        MemoryStream pdfMemoryStream = _pdfFile.Create();
        File.WriteAllBytes(_pdfFile.FileName, pdfMemoryStream.ToArray());
    }
}



// Invoker
class FileCreateInvoker
{
    private ITableActionCommand _tableActionCommand;
    private List<ITableActionCommand> _tableActionCommands = new();

    public void SetCommand(ITableActionCommand tableActionCommand)
    {
        _tableActionCommand = tableActionCommand;
    }


    public void AddCommand(ITableActionCommand tableActionCommand)
    {
        _tableActionCommands.Add(tableActionCommand);
    }

    public void CreateFile()
    {
        _tableActionCommand.Execute();
    }

    public void CreateFiles()
    {
        _tableActionCommands.ForEach(cmd => cmd.Execute());
    }

}

class Program
{
    static void Main()
    {

        List<Product> products = Enumerable.Range(1, 30).Select(index =>
            new Product
            {
                Id = index,
                Name = $"Product {index}",
                Price = index + 100,
                Stock = index
            }
        ).ToList();

        ExcelFile<Product> receiverExcel = new(products);
        PdfFile<Product> receiverPdf = new(products);


        FileCreateInvoker invoker = new();
        invoker.AddCommand(new CreateExcelTableActionCommand<Product>(receiverExcel));
        invoker.AddCommand(new CreatePdfTableActionCommand<Product>(receiverPdf));
        invoker.CreateFiles();
    }
}