using System;
using System.IO;
using System.Threading;
using System.Web;
using Kudu.Core.Infrastructure;

namespace NpmLoop
{
    /// <summary>
    /// Summary description for npm
    /// </summary>
    public class npm : IHttpHandler
    {
        internal static string ResolveNpmPath()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            return Path.Combine(programFiles, "nodejs", "npm.cmd");
        }

        public void ProcessRequest(HttpContext context)
        {
            string loopValue = context.Request["l"];
            int loops;
            if (!Int32.TryParse(loopValue, out loops))
            {
                loops = 1;
            }

            string wwwroot = Path.Combine(HttpRuntime.AppDomainAppPath, "wwwroot");
            string nodeDir = Path.Combine(wwwroot, "node_modules");

            var npmPath = ResolveNpmPath();
            var exe = new Executable(npmPath, wwwroot);

            for (int i = 0; i < loops; i++)
            {
                try
                {
                    var result = exe.Execute("install");
                    context.Response.Write(@"<div style=""font-weight:bold;color:green"">Run " + (i + 1) + " success! </div>");
                    context.Response.Flush();
                    FileSystemHelpers.DeleteDirectorySafe(nodeDir);
                }
                catch (Exception ex)
                {
                    context.Response.Write(@"<div style=""font-weight:bold;color:red"">Run " + (i + 1) + " failed!</div>");
                    context.Response.Write(@"<pre>" + ex.Message + "</pre>");
                    context.Response.Flush();
                    break;
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}