using NUnit.Framework;
using PlumbingProps.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitBusinessMain
{

    public class SourceExcel
    {
        public class INTEGRANTES_TITULOS
        {
            public string Nombre { get; set; }
            public decimal Edad { get; set; }
            public DateTime FechaNacimiento { get; set; }
            public int Cantidad { get; set; }
            public bool Habil { get; set; }
        }

        public class INTEGRANTES
        {
            public string Nombre { get; set; }
            public decimal Edad { get; set; }
            public DateTime FechaNacimiento { get; set; }
            public int Cantidad { get; set; }
            public bool Habil { get; set; }
        }
        public string NombreProyecto { get; set; }
        public string NombreDesarrollador { get; set; }
        public List<INTEGRANTES> TBLINTEGRANTES { get; set; }
    }


    public class TestExcel
    {
        [Test]
        public void GenerarExcel()
        {
            string PathFilePlantilla = @"d:\tmp\PLANTILLA10.xml";
            ExcelHelper target = new ExcelHelper(PathFilePlantilla, 2);
            SourceExcel Source = new SourceExcel() { NombreProyecto = "Proyecto rapidito", NombreDesarrollador = "Ruben Chalco" };
            Source.TBLINTEGRANTES = new List<SourceExcel.INTEGRANTES>();            
            Source.TBLINTEGRANTES.Add(new SourceExcel.INTEGRANTES() { Edad = 90, FechaNacimiento = DateTime.Now, Nombre = "HHHHH1", Cantidad = 12, Habil = false });
            Source.TBLINTEGRANTES.Add(new SourceExcel.INTEGRANTES() { Edad = 90, FechaNacimiento = DateTime.Now, Nombre = "HHHHH1", Cantidad = 12, Habil = false });
            Source.TBLINTEGRANTES.Add(new SourceExcel.INTEGRANTES() { Edad = 90, FechaNacimiento = DateTime.Now, Nombre = "HHHHH1", Cantidad = 12, Habil = false });
            Source.TBLINTEGRANTES.Add(new SourceExcel.INTEGRANTES() { Edad = 90, FechaNacimiento = DateTime.Now, Nombre = "HHHHH1", Cantidad = 12, Habil = false });
            Source.TBLINTEGRANTES.Add(new SourceExcel.INTEGRANTES() { Edad = 90, FechaNacimiento = DateTime.Now, Nombre = "HHHHH1", Cantidad = 12, Habil = false });
            Source.TBLINTEGRANTES.Add(new SourceExcel.INTEGRANTES() { Edad = 90, FechaNacimiento = DateTime.Now, Nombre = "HHHHH1", Cantidad = 12, Habil = false });

            target.DataExcel[0].IndexRow = 6;
            target.DataExcel[0].IndexColumn = 1;
            target.DataExcel[0].NameTable = "TBLINTEGRANTES";

            target.DataExcel[1].IndexRow = 6;
            target.DataExcel[1].IndexColumn = 12;
            target.DataExcel[1].NameTable = "TBLNUMEROS";
            

            foreach (var item in Source.TBLINTEGRANTES)
            {
                target.DataExcel[0].AddRow(item);
            }
            
            for (int i = 0; i < 100; i++)
            {
                target.DataExcel[1].AddRow(new { val1 = i, val2 = i, val3 = i, val4 = i, val5 = i, formula = new ExcelHelper.Formula() { FormulaValue = "=+RC[-3]+RC[-2]+RC[-1]" } }, isTitle: i == 0 ? true : false);
            }

            target.DataExcel[1].AddRow(new
            {
                formula0 = new ExcelHelper.Formula() { FormulaValue = "=SUM(R[-30]C:R[-1]C)" },
                formula1 = new ExcelHelper.Formula() { FormulaValue = "=SUM(R[-30]C:R[-1]C)" },
                formula2 = new ExcelHelper.Formula() { FormulaValue = "=SUM(R[-20]C:R[-1]C)" }
            }, indexcolumn: 12);

            string PathTemp = @"d:\tmp\";
            string actual;
            actual = target.GenerarDocumento(Source, PathTemp);
        }
    }
}
