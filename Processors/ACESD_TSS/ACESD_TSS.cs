﻿using PluginBase;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Xml.Linq;

sing System;
using System.Data;
using System.IO;
using PluginBase;
using OfficeOpenXml;

namespace ACESD_TSS
{
    public class ACESD_TSS : DataProcessor
    {
        public override string id { get => "acesd_tss.0"; }
        public override string name { get => "ACESD_TSS"; }
        public override string description { get => "Processor used for ACESD_TSS translation to universal template"; }
        public override string file_type { get => ".xlsx"; }
        public override string version { get => "1.0"; }
        public override string input_file { get; set; }
        public override string path { get; set; }

        public override DataTableResponseMessage Execute()
        {
            DataTableResponseMessage rm = null;
            DataTable dt = null;
            try
            {
                rm = VerifyInputFile();
                if (rm.IsValid == false)
                    return rm;

                dt = GetDataTable();
                FileInfo fi = new FileInfo(input_file);
                dt.TableName = System.IO.Path.GetFileNameWithoutExtension(fi.FullName);

                //New in version 5 - must deal with License
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                //This is a new way of using the 'using' keyword with braces
                using var package = new ExcelPackage(fi);

                var worksheet = package.Workbook.Worksheets[0];  //Worksheets are zero-based index                
                string name = worksheet.Name;

                //File validation
                if (worksheet.Dimension == null)
                {
                    string msg = string.Format("No data in Sheet 1 in InputFile:  {0}", input_file);
                    rm.LogMessage = msg;
                    rm.ErrorMessage = msg;
                    return rm;
                }

                int startRow = worksheet.Dimension.Start.Row;
                int startCol = worksheet.Dimension.Start.Column;
                int numRows = worksheet.Dimension.End.Row;
                int numCols = worksheet.Dimension.End.Column;

                string D3_analyteID = "Initial Weight";
                string E3_analyteID = "Final Weight";
                string F3_analyteID = "Ashed Weight";

                for (int rowIdx=4;rowIdx<numRows;rowIdx++)
                {
                    aliquot = GetXLStringValue(worksheet.Cells[rowIdx, ColumnIndex1.A]);

                    measuredVal = GetXLDoubleValue(worksheet.Cells[rowIdx, ColumnIndex1.D]);
                    DataRow dr = dt.NewRow();
                    dr["Aliquot"] = aliquot;
                    dr["Analyte Identifier"] = D3_analyteID;
                    dr["Measured Value"] = measuredVal;
                    dt.Rows.Add(dr);

                    measuredVal = GetXLDoubleValue(worksheet.Cells[rowIdx, ColumnIndex1.E]);
                    dr = dt.NewRow();
                    dr["Aliquot"] = aliquot;
                    dr["Analyte Identifier"] = E3_analyteID;
                    dr["Measured Value"] = measuredVal;
                    dt.Rows.Add(dr);

                    measuredVal = GetXLDoubleValue(worksheet.Cells[rowIdx, ColumnIndex1.F]);
                    DataRow dr = dt.NewRow();
                    dr["Aliquot"] = aliquot;
                    dr["Analyte Identifier"] = F3_analyteID;
                    dr["Measured Value"] = measuredVal;
                    dt.Rows.Add(dr);

                }

            }

            catch (Exception ex)
            {
                rm.LogMessage = string.Format("Processor: {0},  InputFile: {1}, Exception: {2}", name, input_file, ex.Message);
                rm.ErrorMessage = string.Format("Problem executing processor {0} on input file {1}.", name, input_file);
            }

            rm.TemplateData = dt;

            return rm;
        }
    }
}