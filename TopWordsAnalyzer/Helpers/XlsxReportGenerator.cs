using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using TopWordsAnalyzer.Interfaces;
using TopWordsAnalyzer.Model;

namespace TopWordsAnalyzer.Helpers;

public class XlsxReportGenerator : IXlsxReportGenerator
{
    public Stream Generate(Report data)
    {
        MemoryStream stream = new MemoryStream();

        using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            WorksheetPart tresholdsWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            tresholdsWorksheetPart.Worksheet = new Worksheet(new SheetData());

            WorksheetPart wordsWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            wordsWorksheetPart.Worksheet = new Worksheet(new SheetData());

            Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
            Sheet tresholdsSheet = new Sheet() { Id = workbookPart.GetIdOfPart(tresholdsWorksheetPart), SheetId = 1, Name = "Tresholds" };
            Sheet wordsSheet = new Sheet() { Id = workbookPart.GetIdOfPart(wordsWorksheetPart), SheetId = 2, Name = "Words" };

            sheets.Append(tresholdsSheet);
            sheets.Append(wordsSheet);

            AddTresholdsSheet(tresholdsWorksheetPart, data.Tresholds);
            AddWordsSheet(wordsWorksheetPart, data.WordsOccurriances);

            workbookPart.Workbook.Save();
        }

        stream.Position = 0;
        return stream;
    }

    private void AddTresholdsSheet(WorksheetPart worksheetPart, List<Treshold> tresholds)
    {
        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

        int startingRow = 1;

        foreach (var treshold in tresholds)
        {
            Row row1 = new Row();
            row1.Append(new Cell(new InlineString(new Text($"{treshold.PercentOfAllWords}%"))));
            row1.Append(new Cell(new InlineString(new Text("Number of words"))));
            row1.Append(new Cell(new InlineString(new Text($"{treshold.WordsCount}"))));
            sheetData.AppendChild(row1);

            Row row2 = new Row();
            row2.Append(new Cell());
            row2.Append(new Cell(new InlineString(new Text("Occuriences"))));
            row2.Append(new Cell(new InlineString(new Text($"{treshold.OccurrencesCount}"))));
            sheetData.AppendChild(row2);

            Row row3 = new Row();
            row3.Append(new Cell());
            row3.Append(new Cell(new InlineString(new Text("Percent of unique words"))));
            row3.Append(new Cell(new InlineString(new Text($"{treshold.PercentOfUniqueWords}"))));
            sheetData.AppendChild(row3);

            Row row4 = new Row();
            row4.Append(new Cell());
            row4.Append(new Cell(new InlineString(new Text(string.Join(", ", treshold.Words)))));
            row4.Append(new Cell());
            sheetData.AppendChild(row4);

            MergeTresholdsSheetCells(worksheetPart, startingRow);
            startingRow += 4;
        }
    }

    private void MergeTresholdsSheetCells(WorksheetPart worksheetPart, int startingRow)
    {
        MergeCells mergeCells;
        if (worksheetPart.Worksheet.Elements<MergeCells>().Count() > 0)
        {
            mergeCells = worksheetPart.Worksheet.Elements<MergeCells>().First();
        }
        else
        {
            mergeCells = new MergeCells();
            if (worksheetPart.Worksheet.Elements<CustomSheetView>().Count() > 0)
            {
                worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<CustomSheetView>().First());
            }
            else
            {
                worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
            }
        }

        string mergeRange = $"A{startingRow}:A{startingRow + 3}";
        MergeCell mergeCell = new MergeCell() { Reference = new StringValue(mergeRange) };
        mergeCells.Append(mergeCell);

        string additionalMergeRange = $"B{startingRow + 3}:C{startingRow + 3}";
        mergeCell = new MergeCell() { Reference = new StringValue(additionalMergeRange) };
        mergeCells.Append(mergeCell);
    }

    private void AddWordsSheet(WorksheetPart worksheetPart, Dictionary<string, int> wordsOccuriences)
    {
        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

        Row header = new Row();
        header.Append(new Cell(new InlineString(new Text("Word"))));
        header.Append(new Cell(new InlineString(new Text($"Occuriences"))));
        sheetData.AppendChild(header);

        foreach (var occurience in wordsOccuriences)
        {
            Row row = new Row();
            row.Append(new Cell(new InlineString(new Text($"{occurience.Key}"))));
            row.Append(new Cell(new InlineString(new Text($"{occurience.Value}"))));
            sheetData.AppendChild(row);
        }
    }
}
