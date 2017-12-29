using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ConversorGts7Geomat
{
    enum Gts7Type { JOB, INST, UNITS, STN, XYZ, BS, SD, SS };

    class Gts7Line
    {
        public Gts7Type type { get; }
        public string name;
        public string unit;
        public string ssCommand;

        public decimal value1;
        public decimal value2;
        public decimal value3;

        public Gts7Line(Gts7Type t)
        {
            type = t;
        }
    }

    class Gts7Converter
    {
        static string[] typeNames = { "JOB", "INST", "UNITS", "STN", "XYZ", "BS", "SD", "SS" };

        /*
//JOB     PHTS420,
//INST    station
//UNITS   M,D
//STN     E10,1.423,
//XYZ     618523.123,7815623.235,850.145
//BS      E9,1.500,
//SD      0.0000,72.1136,5.671
//BS      E9,1.500,
//XYZ     618523.123,7815628.634,851.802
//SS      1,1.500,CERCA
*/
        static public Gts7Line processLine(string line)
        {
            string[] lineSplited = Regex.Split(line, " +");
            if (lineSplited.Length > 0)
            {
                string[] vv = lineSplited[1].Split(',');
                switch (lineSplited[0])
                {
                    case "JOB":
                        Gts7Line job = new Gts7Line(Gts7Type.JOB);
                        job.name = vv[0];
                        return job;
                    case "INST":
                        Gts7Line inst = new Gts7Line(Gts7Type.INST);
                        inst.name = vv[0];
                        return inst;
                    case "UNITS":
                        Gts7Line units = new Gts7Line(Gts7Type.UNITS);
                        units.name = vv[0];
                        units.unit = vv[1];
                        return units;
                    case "STN":
                        Gts7Line stn = new Gts7Line(Gts7Type.STN);
                        stn.name = vv[0];
                        stn.value1 = decimal.Parse(vv[1]);
                        return stn;
                    case "XYZ":
                        Gts7Line xyz = new Gts7Line(Gts7Type.XYZ);
                        xyz.value1 = decimal.Parse(vv[0]);
                        xyz.value2 = decimal.Parse(vv[1]);
                        xyz.value3 = decimal.Parse(vv[2]);
                        return xyz;
                    case "BS":
                        Gts7Line bs = new Gts7Line(Gts7Type.BS);
                        bs.name = vv[0];
                        bs.value1 = decimal.Parse(vv[1]);
                        return bs;
                    case "SD":
                        Gts7Line sd = new Gts7Line(Gts7Type.SD);
                        sd.value1 = decimal.Parse(vv[0]);
                        sd.value2 = decimal.Parse(vv[1]);
                        sd.value3 = decimal.Parse(vv[2]);
                        return sd;
                    case "SS":
                        Gts7Line ss = new Gts7Line(Gts7Type.SS);
                        ss.name = vv[0];
                        ss.value1 = decimal.Parse(vv[1]);
                        ss.ssCommand = vv[2];
                        return ss;
                }
            }
            return null;
        }

        private static string formatValue8(Decimal d)
        {
            return String.Format("{0:+00000000;-00000000}", d * 1000).Substring(0, 9);
        }
        private static string formatValue9(Decimal d)
        {
            return String.Format("{0:+000000000;-000000000}", d * 1000).Substring(0, 10);
        }
        private static string formatValue10(Decimal d)
        {
            return String.Format("{0:+0000000000;-0000000000}", d * 1000).Substring(0, 11);
        }
        private static string formatName(string s)
        {
            return s.PadLeft(8, '0').Substring(0, 8);
        }

        private static string formatCoordLine(int stnCount, string name, string command, Gts7Line xyzLine)
        {
            return String.Format("11{0:000}+{1} 81..10{2} 82..10{3} 83..10{4} 41..00+{5}"
                , stnCount, name, formatValue9(xyzLine.value1), formatValue10(xyzLine.value2), formatValue8(xyzLine.value3), formatName(command));

        }

        public static string cadernetaGenerator(Queue<Gts7Line> queue)
        {
            StringBuilder buffer = new StringBuilder();
            string lastBsName = "";
            string lastBsValue = "";
            string cadLine = "";
            while (queue.Count > 0)
            {
                Gts7Line line = queue.Dequeue();
                switch (line.type)
                {
                    case Gts7Type.JOB:
                        string jname = formatName(line.name);
                        line = queue.Peek();
                        if (line.type != Gts7Type.INST)
                        {
                            Console.WriteLine("Found line:" + typeNames[(int)line.type] + " after JOB");
                            cadLine = null;
                            break;
                        }
                        line = queue.ElementAt(1);
                        if (line.type != Gts7Type.UNITS)
                        {
                            Console.WriteLine("Found line:" + typeNames[(int)line.type] + " after INST");
                            cadLine = null;
                            break;
                        }
                        // cadLine = String.Format("41....+00INICIO 42....+{0} 43....+00000000 44....+00000000", jname);
                        cadLine = null;
                        break;

                    case Gts7Type.INST:
                    case Gts7Type.UNITS:
                    case Gts7Type.XYZ:
                        cadLine = null;
                        break;

                    case Gts7Type.STN:
                        string sname = formatName(line.name);
                        string svalue = formatValue8(line.value1);
                        if (sname.CompareTo(lastBsName) == 0 && svalue.CompareTo(lastBsValue) == 0)
                        {
                            cadLine = null;
                            break;
                        }
                        lastBsName = sname;
                        lastBsValue = svalue;
                        cadLine = String.Format("41....+00OCUPAR 42....+{0} 43....+00000000 44....{1}", sname, svalue);
                        break;

                    case Gts7Type.BS:
                        string bsName = formatName(line.name);
                        string bsValue = formatValue8(line.value1);
                        if (bsName.CompareTo(lastBsName) == 0)
                        {
                            cadLine = null;
                            break;
                        }
                        lastBsName = bsName;
                        lastBsValue = bsValue;
                        cadLine = String.Format("41....+000000RE 42....+{0} 43....+00000000 44....+00000000", lastBsName);
                        break;

                    case Gts7Type.SD:
                        string dvalue1 = formatValue8(line.value1 * 100);
                        string dvalue2 = formatValue8(line.value2 * 100);
                        string dvalue3 = formatValue8(line.value3);
                        cadLine = String.Format("110001+{0} 21.104{1} 22.104{2} 31..00{3} 51....+0041+000 87..10{4} 88..10+00000000", lastBsName, dvalue1, dvalue2, dvalue3, lastBsValue);
                        break;

                    case Gts7Type.SS:
                        string ssName = formatName(line.name);
                        string ssValue = formatValue8(line.value1);
                        string ssCommand = formatName(line.ssCommand);
                        if (ssName.CompareTo(lastBsName) == 0 && ssValue.CompareTo(lastBsValue) == 0)
                        {
                            cadLine = null;
                            break;
                        }
                        lastBsName = ssName;
                        lastBsValue = ssValue;
                        if (ssCommand.CompareTo("VANTE") != 0)
                        {
                            ssValue = formatValue8(0);
                        }
                        cadLine = String.Format("41....+{0} 42....{1} 43....+00000000 44....+00000000", ssCommand, ssValue);
                        break;
                }
                if (cadLine != null)
                {
                    Console.WriteLine(cadLine);
                    buffer.Append(cadLine + System.Environment.NewLine);
                }
            }

            return buffer.ToString();
        }

        public static string coordenadaGenerator(Queue<Gts7Line> queue)
        {
            StringBuilder buffer = new StringBuilder();
            string cooLine = "";
            int stnCount = 0;
            int idx = 0;
            while (queue.Count > idx)
            {
                int xyzIdx = 0;
                Gts7Line xyzLine = null;
                string xValue = null;
                string yValue = null;
                string zValue = null;

                Gts7Line line = queue.ElementAt(idx);
                switch (line.type)
                {
                    case Gts7Type.JOB:
                    case Gts7Type.INST:
                    case Gts7Type.UNITS:
                    case Gts7Type.XYZ:
                    case Gts7Type.SD:
                        cooLine = null;
                        break;

                    case Gts7Type.STN:
                        string sname = formatName(line.name);
                        if (stnCount > 0)
                        {
                            cooLine = null;
                            stnCount = 0;
                            break;
                        }
                        xyzIdx = idx + 1;
                        while (xyzIdx < queue.Count && queue.ElementAt(xyzIdx).type != Gts7Type.XYZ)
                        {
                            xyzIdx++;
                        }
                        if (xyzIdx == queue.Count)
                        {
                            Console.WriteLine("Not found XYZ line after STN");
                            cooLine = null;
                            break;
                        }
                        stnCount++;
                        idx = xyzIdx;

                        cooLine = formatCoordLine(stnCount, sname, "ESTACAO", queue.ElementAt(xyzIdx));
                        break;

                    case Gts7Type.BS:
                        string bsName = formatName(line.name);

                        xyzIdx = idx + 1;
                        while (xyzIdx < queue.Count && queue.ElementAt(xyzIdx).type != Gts7Type.XYZ)
                        {
                            xyzIdx++;
                        }
                        if (xyzIdx == queue.Count)
                        {
                            Console.WriteLine("Not found XYZ line after BS");
                            cooLine = null;
                            break;
                        }
                        stnCount++;
                        idx = xyzIdx;

                        cooLine = formatCoordLine(stnCount, bsName, "RE", queue.ElementAt(xyzIdx));
                        break;

                    case Gts7Type.SS:
                        string ssName = formatName(line.name);
                        string ssCommand = formatName(line.ssCommand);
                        xyzIdx = idx + 1;
                        while (xyzIdx < queue.Count && queue.ElementAt(xyzIdx).type != Gts7Type.XYZ)
                        {
                            xyzIdx++;
                        }
                        if (xyzIdx == queue.Count)
                        {
                            Console.WriteLine("Not found XYZ line after SS");
                            cooLine = null;
                            break;
                        }
                        stnCount++;
                        idx = xyzIdx;

                        cooLine = formatCoordLine(stnCount, ssName, ssCommand, queue.ElementAt(xyzIdx));
                        break;
                }
                if (cooLine != null)
                {
                    Console.WriteLine(cooLine);
                    buffer.Append(cooLine + System.Environment.NewLine);
                }
                idx++;
            }

            return buffer.ToString();
        }


    }
}
