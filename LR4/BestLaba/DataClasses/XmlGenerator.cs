using System.Data;
using System.IO;

namespace ServiceLib
{
    public class XmlGenerator
    {
        readonly string outputFolder;

        public XmlGenerator(string outputFolder)
        {
            this.outputFolder = outputFolder;
        }

        public void WriteToXml(DataSet dataSet, string fileName)
        {
            dataSet.WriteXml(Path.Combine(outputFolder, $"{fileName}.xml"));

            dataSet.WriteXmlSchema(Path.Combine(outputFolder, $"{fileName}.xsd"));
        }
    }
}
