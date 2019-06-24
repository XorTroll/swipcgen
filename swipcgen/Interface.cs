using System;
using System.Collections.Generic;
using System.Linq;

namespace swipcgen
{
    public class Interface
    {
        public string NnName { get; set; }

        public string ServiceName { get; set; }

        public VersionDecorator Version { get; set; }

        public List<Command> Commands { get; set; }

        public bool IsManagedPort { get; set; }

        public bool IsService
        {
            get
            {
                return !string.IsNullOrEmpty(ServiceName);
            }
        }

        public string FormattedName
        {
            get
            {
                return ServiceName.Replace("-", "").Replace(":", "");
            }
        }

        public string FormattedNnName
        {
            get
            {
                return NnName.Replace("::", "_");
            }
        }

        public Interface()
        {
            Commands = new List<Command>();
        }
        public string GenerateHeader()
        {
            string code = LibnxUtils.MakeInterfaceServiceBaseHeader(this) + "\n\n";
            foreach (var c in Commands)
            {
                string cmd = "Result ";
                if(IsService) cmd += FormattedName + "_";
                else cmd += FormattedNnName + "_";
                cmd += c.Name + "(";
                if (!IsService) cmd += FormattedNnName + " *session";
                uint incounter = 0;
                if (c.InRawTypes.Count > 0)
                {
                    if (!IsService) cmd += ", ";
                    foreach (var irt in c.InRawTypes)
                    {
                        if (irt.ArgType.IsValid) cmd += irt.ArgType.CName + " ";
                        else cmd += "u8 *";
                        cmd += "in_" + incounter;
                        cmd += ", ";
                        incounter++;
                    }
                    cmd = cmd.Substring(0, cmd.Length - 2);
                }
                cmd += ");"; cmd += "\n";
                code += cmd + "\n";
            }
            return code;
        }


        public string GenerateSource()
        {
            string code = LibnxUtils.MakeInterfaceServiceBaseSource(this) + "\n\n";
            foreach(var c in Commands)
            {
                string cmd = "Result ";
                if(IsService) cmd += FormattedName + "_";
                else cmd += FormattedNnName + "_";
                cmd += c.Name + "(";
                if(!IsService) cmd += FormattedNnName + "* session";
                uint incounter = 0;
                uint outcounter = 0;
                uint ioutcounter = 0;
                if(c.InRawTypes.Count > 0)
                {
                    if (!IsService) cmd += ", ";
                    foreach(var irt in c.InRawTypes)
                    {
                        if (irt.ArgType.IsValid) cmd += irt.ArgType.CName + " ";
                        else cmd += "u8 *";
                        if(string.IsNullOrEmpty(irt.Name))
                        {
                            cmd += "in_" + incounter;
                            incounter++;
                        }
                        else cmd += irt.Name;
                        cmd += ", ";
                    }
                    cmd = cmd.Substring(0, cmd.Length - 2);
                }
                if(c.OutRawTypes.Count > 0)
                {
                    foreach(var rawt in c.OutRawTypes)
                    {
                        cmd += ", ";
                        if (rawt.ArgType.IsValid) cmd += rawt.ArgType.CName + " *";
                        else cmd += "u8 *";
                        if (string.IsNullOrEmpty(rawt.Name))
                        {
                            cmd += "out_" + outcounter;
                            outcounter++;
                        }
                        else cmd += rawt.Name;
                    }
                }
                if (c.OutInterfaces.Count > 0)
                {
                    foreach(var intf in c.OutInterfaces)
                    {
                        cmd += ", ";
                        cmd += intf.ArgType.FormattedNnName + " *";
                        if(string.IsNullOrEmpty(intf.Name))
                        {
                            cmd += "iout_" + ioutcounter;
                            ioutcounter++;
                        }
                        else cmd += intf.Name;
                    }
                }
                cmd += ")"; cmd += "\n";
                cmd += "{"; cmd += "\n";
                cmd += "    IpcCommand c;"; cmd += "\n";
                cmd += "    ipcInitialize(&c);"; cmd += "\n";
                cmd += "    struct {"; cmd += "\n";
                cmd += "        u64 magic;"; cmd += "\n";
                cmd += "        u64 cmd_id;"; cmd += "\n";
                incounter = 0;
                if(c.InRawTypes.Count > 0)
                {
                    foreach(var irt in c.InRawTypes)
                    {
                        cmd += "        ";
                        if (irt.ArgType.IsValid) cmd += irt.ArgType.CName + " in_" + incounter + ";";
                        else cmd += "u8 in_" + incounter + "[" + irt.ArgType.Bytes + "];";
                        cmd += "\n";
                        incounter++;
                    }
                }
                string srvref = IsService ? "&g_" + FormattedName + "_ISrv" : "&session->s";
                cmd += "    } *raw = serviceIpcPrepareHeader(" + srvref + ", &c, sizeof(*raw));"; cmd += "\n";
                cmd += "    raw->magic = SFCI_MAGIC;"; cmd += "\n";
                cmd += "    raw->cmd_id = " + c.Id + ";"; cmd += "\n";
                incounter = 0;
                if(c.InRawTypes.Count > 0)
                {
                    foreach (var irt in c.InRawTypes)
                    {
                        if (irt.ArgType.IsValid)
                        {
                            cmd += "    raw->in_" + incounter + " = ";
                            if (string.IsNullOrEmpty(irt.Name)) cmd += "in_" + incounter;
                            else cmd += irt.Name;
                            cmd += ";"; cmd += "\n";
                        }
                        else
                        {
                            cmd += "    memcpy(raw->in_" + incounter + ", ";
                            if (string.IsNullOrEmpty(irt.Name)) cmd += "in_" + incounter;
                            else cmd += irt.Name;

                            cmd += ", " + irt.ArgType.Bytes + ");"; cmd += "\n";
                        }
                        if (string.IsNullOrEmpty(irt.Name)) incounter++;
                        incounter++;
                    }
                }
                cmd += "    Result rc = serviceIpcDispatch(" + srvref + ");"; cmd += "\n";
                cmd += "    if(R_SUCCEEDED(rc))"; cmd += "\n";
                cmd += "    {"; cmd += "\n";
                cmd += "        IpcParsedCommand r;"; cmd += "\n";
                cmd += "        struct {"; cmd += "\n";
                cmd += "            u64 magic;"; cmd += "\n";
                cmd += "            u64 result;"; cmd += "\n";
                outcounter = 0;
                if (c.OutRawTypes.Count > 0)
                {
                    foreach (var rawt in c.OutRawTypes)
                    {
                        cmd += "            ";
                        if (rawt.ArgType.IsValid) cmd += rawt.ArgType.CName + " out_" + outcounter + ";";
                        else cmd += "u8 out_" + outcounter + "[" + rawt.ArgType.Bytes + "];";
                        cmd += "\n";
                        outcounter++;
                    }
                }
                cmd += "        } *resp;"; cmd += "\n";
                cmd += "        serviceIpcParse(" + srvref + ", &r, sizeof(*resp));"; cmd += "\n";
                cmd += "        resp = r.Raw;"; cmd += "\n";
                cmd += "        rc = resp->result;"; cmd += "\n";
                if((c.OutInterfaces.Count > 0) || (c.OutRawTypes.Count > 0))
                {
                    cmd += "        if(R_SUCCEEDED(rc))"; cmd += "\n";
                    cmd += "        {"; cmd += "\n";
                    if (c.OutRawTypes.Count > 0)
                    {
                        outcounter = 0;
                        foreach (var rawt in c.OutRawTypes)
                        {
                            cmd += "            if(";
                            if (string.IsNullOrEmpty(rawt.Name))
                            {
                                cmd += "iout_" + outcounter;
                            }
                            else cmd += rawt.Name;
                            cmd += ")";
                            if (rawt.ArgType.IsValid)
                            {
                                cmd += " { *";
                                if (string.IsNullOrEmpty(rawt.Name)) cmd += "out_" + outcounter;
                                else cmd += rawt.Name;
                                cmd += " = resp->out_" + outcounter + "; }";
                                cmd += "\n";

                            }
                            else
                            {
                                cmd += "\n";
                                cmd += "        {"; cmd += "\n";
                                cmd += "            memcpy(";
                                if (string.IsNullOrEmpty(rawt.Name))
                                {
                                    cmd += "out_" + outcounter;
                                }
                                else cmd += rawt.Name;
                                cmd += ", resp->out_" + outcounter + ", " + rawt.ArgType.Bytes + ");"; cmd += "\n";
                            }
                            if (string.IsNullOrEmpty(rawt.Name)) outcounter++;
                        }
                    }
                    if (c.OutInterfaces.Count > 0)
                    {
                        ioutcounter = 0;
                        foreach(var intf in c.OutInterfaces)
                        {
                            cmd += "            serviceCreateSubservice(";
                            if (string.IsNullOrEmpty(intf.Name))
                            {
                                cmd += "iout_" + ioutcounter;
                                ioutcounter++;
                            }
                            else cmd += intf.Name;
                            cmd += ", " + srvref + ", &r, 0);";
                            cmd += "\n";
                            
                        }
                    }
                    cmd += "        }"; cmd += '\n';
                }
                cmd += "    }"; cmd += "\n";
                cmd += "    return rc;"; cmd += "\n";
                cmd += "}"; cmd += "\n";
                code += cmd + "\n";
            }
            return code;
        }
    }
}
