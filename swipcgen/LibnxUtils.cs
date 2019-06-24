using System;
using System.Collections.Generic;
using System.Text;

namespace swipcgen
{
    public static class LibnxUtils
    {
        public static string MakeVersionGuard(VersionDecorator Version)
        {
            switch(Version.Mode)
            {
                case VersionMode.Major:
                    var ver1 = new Version(Version.Version);
                    string guard1 = "if(hosversionBefore(" + ver1.Major + "," + ver1.Minor + "," + ver1.Micro + ")) { return MAKERESULT(Module_Libnx, LibnxError_IncompatSysVer); }";
                    return guard1;
                case VersionMode.Single:
                    var ver2 = new Version(Version.Version);
                    string guard2 = "if(hosversionGet() != MAKEHOSVERSION(" + ver2.Major + "," + ver2.Minor + "," + ver2.Micro + ")) { return MAKERESULT(Module_Libnx, LibnxError_IncompatSysVer); }";
                    return guard2;
                case VersionMode.Interval:
                    var ver31 = new Version(Version.MinVersion);
                    var ver32 = new Version(Version.MaxVersion);
                    string guard3 = "if(hosversionGet() < (MAKEHOSVERSION(" + ver31.Major + "," + ver31.Minor + "," + ver31.Micro + ")) || (hosversionGet() > MAKEHOSVERSION(" + ver32.Major + "," + ver32.Minor + "," + ver32.Micro + "))) { return MAKERESULT(Module_Libnx, LibnxError_IncompatSysVer); }";
                    return guard3;
            }
            return null;
        }

        public static string MakeInterfaceServiceBaseHeader(Interface Iface)
        {
            string code = "// " + Iface.NnName + "\n";
            if (Iface.IsService)
            {
                string normalizedsrv = Iface.FormattedName;

                code += "Result " + normalizedsrv + "_Initialize();\n";
                code += "void " + normalizedsrv + "_Exit();";
            }
            else
            {
                code += "typedef struct {\n    Service s;\n} " + Iface.FormattedNnName + ";";
            }
            return code;
        }

        public static string MakeInterfaceServiceBaseSource(Interface Iface)
        {
            if (Iface.IsService)
            {
                string normalizedsrv = Iface.FormattedName;

                string code = "static Service g_" + normalizedsrv + "_ISrv;\nstatic u64 g_" + normalizedsrv + "_RefCnt;\n\n";

                code += "Result " + normalizedsrv + "_Initialize()"; code += "\n";
                code += "{"; code += "\n";
                code += "    atomicIncrement64(&g_" + normalizedsrv + "_RefCnt);"; code += "\n";
                code += "    if(serviceIsActive(&g_" + normalizedsrv + "_ISrv)) { return 0; }"; code += "\n";
                if(Iface.IsManagedPort)
                {
                    code += "    // Workaround for managed ports"; code += "\n";
                    code += "    Result rc = svcConnectToNamedPort(&g_" + normalizedsrv + "_ISrv.handle, \"" + Iface.ServiceName + "\");"; code += "\n";
                    code += "    if(R_SUCCEEDED(rc)) { &g_" + normalizedsrv + "_ISrv.object_id = IPC_INVALID_OBJECT_ID; &g_" + normalizedsrv + "_ISrv.type = ServiceType_Normal; }";
                    code += "    return rc;";
                }
                else code += "    return smGetService(&g_" + normalizedsrv + "_ISrv, \"" + Iface.ServiceName + "\");"; code += "\n";
                code += "}"; code += "\n\n";

                code += "void " + normalizedsrv + "_Exit()"; code += "\n";
                code += "{"; code += "\n";
                code += "    if(atomicDecrement64(&g_" + normalizedsrv + "_RefCnt) == 0) { serviceClose(&g_" + normalizedsrv + "_ISrv); }"; code += "\n";
                code += "}";

                return code;
            }
            else return "";
        }

        public static string MakeCustomTypedefs()
        {
            string code = "";
            foreach(var t in Static.CustomTypes)
            {
                code += "// " + t.Name + "\n";
                if((t.EqType != null) && !string.IsNullOrEmpty(t.EqType.CName))
                {
                    code += "typedef " + t.EqType.CName + " " + t.CName + ";";
                }
                else if((t.EqType == null) && (t.Bytes == 0))
                {
                    code += "typedef void *" + t.CName + ";";
                }
                else code += "typedef struct {\n    u8 data[" + t.Bytes + "];\n} PACKED " + t.CName + ";";
            }
            return code;
        }
    }
}
