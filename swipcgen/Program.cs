using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace swipcgen
{
    static class Program
    {
        public static List<Interface> Interfaces = new List<Interface>();

        public static CCode Parse(string IDText)
        {
            VersionDecorator cur_v = null;
            for (int i = 0; i < IDText.Length; i++)
            {
                char cur = IDText[i];
                if (cur == '@')
                {
                    string tmpv = IDText.Substring(i + 1, 7);
                    if (tmpv == "version")
                    {
                        tmpv = IDText.Substring(i + 9).Split(')')[0];
                        cur_v = new VersionDecorator(tmpv);
                    }
                }
                else if(cur == '#')
                {
                    do
                    {
                        i++;
                    } while (IDText[i] == '\n');
                }
                else if (cur == 't')
                {
                    string tmpt = IDText.Substring(i, 4);
                    if (tmpt == "type")
                    {
                        int tmpid = i + 3;
                        int tmpspaces = 0;
                        do
                        {
                            tmpid++;
                            tmpspaces++;
                        } while (IDText[tmpid] == ' ');
                        string typename = "";
                        while ((IDText[tmpid] != ' ') && (IDText[tmpid] != '='))
                        {
                            typename += IDText[tmpid];
                            tmpid++;
                        }
                        if (IDText[tmpid] == ' ') tmpid++;
                        do
                        {
                            tmpid++;
                            tmpspaces++;
                        } while (IDText[tmpid] == ' ');
                        string eqname = "";
                        while (IDText[tmpid] != ';')
                        {
                            eqname += IDText[tmpid];
                            tmpid++;
                        }
                        var t = Static.GetCustomType(eqname);
                        if (t == null) t = Static.GetBuiltInType(eqname);
                        if (t != null)
                        {
                            var at = new Type(typename, t);
                            if (cur_v != null)
                            {
                                at.Version = cur_v;
                                cur_v = null;
                            }
                            Static.CustomTypes.Add(at);
                        }
                        else
                        {
                            if (eqname.Substring(0, 5) == "bytes")
                            {
                                long bsz;
                                string sz = eqname.Substring(eqname.IndexOf('<') + 1).Split(',')[0].Replace(">", "");
                                if (sz.StartsWith("0x")) bsz = long.Parse(sz.Substring(2), System.Globalization.NumberStyles.HexNumber);
                                else bsz = long.Parse(sz);
                                var at = new Type(typename, bsz);
                                if (cur_v != null)
                                {
                                    at.Version = cur_v;
                                    cur_v = null;
                                }
                                Static.CustomTypes.Add(at);
                            }
                        }
                    }
                }
                else if (cur == 'i')
                {
                    if ((IDText.Length > (i + 9)) && (IDText.Substring(i, 9) == "interface"))
                    {
                        var intf = new Interface();
                        int tmpid = i + 9;
                        int tmpspaces = 0;
                        do
                        {
                            tmpid++;
                            tmpspaces++;
                        } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r')));
                        string intfname = "";
                        while ((IDText.Length > tmpid) && (IDText[tmpid] != ' '))
                        {
                            intfname += IDText[tmpid];
                            tmpid++;
                        }
                        do
                        {
                            tmpid++;
                            tmpspaces++;
                        } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r'))); ;
                        intf.NnName = intfname;
                        string srvname = null;
                        if ((IDText.Length > tmpid) && (IDText.Substring(tmpid, 2) == "is"))
                        {
                            tmpid++;
                            tmpspaces = 0;
                            do
                            {
                                tmpid++;
                                tmpspaces++;
                            } while (IDText[tmpid] == ' ');
                            srvname = "";
                            while (IDText[tmpid] != ' ')
                            {
                                srvname += IDText[tmpid];
                                tmpid++;
                            }
                            do
                            {
                                tmpid++;
                                tmpspaces++;
                            } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r')));
                        }
                        if(srvname == "@managedport")
                        {
                            intf.IsManagedPort = true;
                            tmpid--;
                            do
                            {
                                tmpid++;
                                tmpspaces++;
                            } while (IDText[tmpid] == ' ');
                            srvname = "";
                            while (IDText[tmpid] != ' ')
                            {
                                srvname += IDText[tmpid];
                                tmpid++;
                            }
                            do
                            {
                                tmpid++;
                                tmpspaces++;
                            } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r')));

                        }
                        intf.ServiceName = srvname;
                        if (IDText[tmpid] == '{')
                        {
                            while ((tmpid + 1) < IDText.Length)
                            {
                                tmpid++;
                                if (IDText[tmpid] == '}') break;
                                if (IDText[tmpid] == '[')
                                {
                                    var c = new Command();
                                    tmpid++;
                                    string tmpno = IDText.Substring(tmpid).Split(']')[0];
                                    c.Id = uint.Parse(tmpno);
                                    tmpid += tmpno.Length + 1;
                                    do
                                    {
                                        tmpid++;
                                        tmpspaces++;
                                    } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r')));
                                    string cmdname = IDText.Substring(tmpid).Split('(')[0];
                                    tmpid += cmdname.Length + 1;
                                    c.Name = cmdname;
                                    string indata = IDText.Substring(tmpid).Split(')')[0];
                                    var inargs = ParseNames(indata);

                                    foreach (var pair in inargs)
                                    {
                                        var t2 = Static.GetCustomType(pair.Key);
                                        if (t2 == null) t2 = Static.GetBuiltInType(pair.Key);
                                        if (t2 != null)
                                        {
                                            var at = new Argument<Type>(t2);
                                            if (!string.IsNullOrEmpty(pair.Value)) at.Name = pair.Value;
                                            c.InRawTypes.Add(at);
                                        }
                                        else
                                        {
                                            if (pair.Key.StartsWith("object<"))
                                            {
                                                string objname = pair.Key.Substring(pair.Key.IndexOf('<') + 1).Split('>')[0];
                                                if (Interfaces.Count > 0)
                                                {
                                                    foreach (var intf1 in Interfaces)
                                                    {
                                                        if (intf1.NnName == objname)
                                                        {
                                                            var it = new Argument<Interface>(intf1);
                                                            if (!string.IsNullOrEmpty(pair.Value)) it.Name = pair.Value;
                                                            c.InInterfaces.Add(it);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    tmpid += indata.Length;

                                    do
                                    {
                                        tmpid++;
                                        tmpspaces++;
                                    } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r')));
                                    if ((IDText.Length > tmpid) && (IDText[tmpid] == '-'))
                                    {
                                        tmpid++;
                                        if ((IDText.Length > tmpid) && (IDText[tmpid] == '>'))
                                        {
                                            do
                                            {
                                                tmpid++;
                                                tmpspaces++;
                                            } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '(') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r')));
                                            string outdata = IDText.Substring(tmpid).Split(')')[0].Split(';')[0];
                                            var args = ParseNames(outdata);
                                            foreach (var pair in args)
                                            {
                                                var t2 = Static.GetCustomType(pair.Key);
                                                if (t2 == null) t2 = Static.GetBuiltInType(pair.Key);
                                                if (t2 != null)
                                                {
                                                    var at = new Argument<Type>(t2);
                                                    if (!string.IsNullOrEmpty(pair.Value)) at.Name = pair.Value;
                                                    c.OutRawTypes.Add(at);
                                                }
                                                else
                                                {
                                                    if (pair.Key.StartsWith("object<"))
                                                    {
                                                        string objname = pair.Key.Substring(pair.Key.IndexOf('<') + 1).Split('>')[0];
                                                        if (Interfaces.Count > 0)
                                                        {
                                                            foreach (var intf1 in Interfaces)
                                                            {
                                                                if (intf1.NnName == objname)
                                                                {
                                                                    var it = new Argument<Interface>(intf1);
                                                                    if (!string.IsNullOrEmpty(pair.Value)) it.Name = pair.Value;
                                                                    c.OutInterfaces.Add(it);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            tmpid += outdata.Length;
                                            
                                        }
                                            
                                    }
                                    intf.Commands.Add(c);
                                    if (IDText[tmpid] == ';')
                                    {
                                        do
                                        {
                                            tmpid++;
                                            tmpspaces++;
                                        } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r')));
                                        tmpid--;
                                        continue;
                                    }
                                    else
                                    {
                                        tmpid++;
                                        if (IDText[tmpid] == ';')
                                        {
                                            do
                                            {
                                                tmpid++;
                                                tmpspaces++;
                                            } while ((IDText.Length > tmpid) && ((IDText[tmpid] == ' ') || (IDText[tmpid] == '\n') || (IDText[tmpid] == '\t') || (IDText[tmpid] == '\r')));
                                            tmpid--;
                                            continue;
                                        }
                                    }
                                    
                                }
                            }
                            Interfaces.Add(intf);
                        }
                    }
                }
            }
            CCode code = new CCode();
            code.Header = "#pragma once\n#include <switch.h>\n\n// Bindings generated by swipcgen: https://github.com/XorTroll/swipcgen" + LibnxUtils.MakeCustomTypedefs();
            foreach(var intf in Interfaces)
            {
                code.Header += "\n\n" + intf.GenerateHeader();
                code.Source += intf.GenerateSource() + "\n\n";
            }
            code.Source = code.Source.Substring(0, code.Source.Length - 2);
            return code;
        }

        public static Dictionary<string, string> ParseNames(string Base)
        {
            var names = new Dictionary<string, string>();
            string tmpa = "";
            string tmpb = "";
            bool afinish = false;
            for(int i = 0; i < Base.Length; i++)
            {
                char ch = Base[i];
                if(ch == ' ')
                {
                    if(!afinish)
                    {
                        if(!string.IsNullOrEmpty(tmpa)) afinish = true;
                    }
                }
                else if(ch == ',')
                {
                    if(!string.IsNullOrEmpty(tmpa))
                    {
                        names.Add(tmpa, tmpb);
                        tmpa = "";
                        tmpb = "";
                        afinish = false;
                    }
                }
                else
                {
                    if (afinish) tmpb += ch;
                    else tmpa += ch;
                }
            }
            if (!string.IsNullOrEmpty(tmpa))
            {
                names.Add(tmpa, tmpb);
            }
            return names;
        }

        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
        }

        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: dotnet swipcgen <swipc ID file> [<output header> <output source>]");
            }
                 
            string id = File.ReadAllText(args[0]);
            var code = Parse(id);
            string header = Path.GetDirectoryName(args[0]) + "\\" + Path.GetFileNameWithoutExtension(args[0]) + ".h";
            string source = Path.GetDirectoryName(args[0]) + "\\" + Path.GetFileNameWithoutExtension(args[0]) + ".c";
            if (args.Length > 1)
            {
                header = args[1];
                if(args.Length > 2)
                {
                    source = args[2];
                }
                else source = Path.GetDirectoryName(header) + "\\" + Path.GetFileNameWithoutExtension(header) + ".c";
            }
            code.Save(header, source);
        }
    }
}
