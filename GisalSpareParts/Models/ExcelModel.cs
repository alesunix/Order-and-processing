using ClosedXML.Excel;
using Microsoft.JSInterop;
namespace GisalSpareParts.Models
{
    public class ExcelModel
    {
        private async Task ExportExcel(IJSRuntime js, string fileName, byte[] data)
        {
            await js.InvokeAsync<object>("SaveXls", fileName, Convert.ToBase64String(data));
        }
        public async void GenerateExcel(IJSRuntime js, List<OrderModel> Table, string fileName)
        {
            XLWorkbook wb = new();
            wb.Properties.Author = "Gisal";
            wb.Properties.Title = "Выгрузка";
            wb.Properties.Subject = fileName;

            var ws = wb.Worksheets.Add("Заказы");
            ws.Cell(1, 1).Value = "Наименование";
            ws.Cell(1, 2).Value = "Дата заказа";
            ws.Cell(1, 3).Value = "Дата поставки";
            ws.Cell(1, 4).Value = "Код поставщика Gisal";
            ws.Cell(1, 5).Value = "Код машины";
            ws.Cell(1, 6).Value = "Код линии";
            ws.Cell(1, 7).Value = "№ Модуля";
            ws.Cell(1, 8).Value = "Срочность";
            ws.Cell(1, 9).Value = "Количество";
            ws.Cell(1, 10).Value = "Статус";
            ws.Cell(1, 11).Value = "Примечание";
            ws.Cell(1, 12).Value = "Удален";
            for (int i = 0; i < 12; i++)
            {
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Font.FontSize = 12;
            }
            ws.SheetView.FreezeRows(1);/// Закрепить строку
            for (int i = 0; i < Table.Count; i++)
            {
                int row = i + 2;
                ws.Cell(row, 1).Value = Table[i].Name;
                ws.Cell(row, 2).Value = Table[i].Dateorder.ToShortDateString();
                ws.Cell(row, 3).Value = Table[i].Datedelivery.ToShortDateString();
                ws.Cell(row, 4).Value = Table[i].Codegisal;
                ws.Cell(row, 5).Value = Table[i].Codemachine;
                ws.Cell(row, 6).Value = Table[i].Codeline;
                ws.Cell(row, 7).Value = Table[i].Module;
                ws.Cell(row, 8).Value = Table[i].UrgencyName;
                ws.Cell(row, 9).Value = Table[i].Amount;
                ws.Cell(row, 10).Value = Table[i].Statusname;
                ws.Cell(row, 11).Value = Table[i].Rem;
                ws.Cell(row, 12).Value = Table[i].Deleted == "F" ? "Нет" : "Да";
            }
            //ws.Rows().AdjustToContents();/// Высота строк по содержимому
            ws.Columns().AdjustToContents();/// Ширина столбца по содержимому
            var rowHeader = ws.Row(1);
            //rowHeader.Style.Fill.BackgroundColor = XLColor.Orange;
            rowHeader.Height = 20;
            byte[] bytes = Array.Empty<byte>();
            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                bytes = ms.ToArray();
            }
            await ExportExcel(js, $"{fileName}.xlsx", bytes);
        }
    }
}
