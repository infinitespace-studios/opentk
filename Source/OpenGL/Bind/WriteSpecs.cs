#region License
//Copyright (c) 2006 Stephen Apostolopoulos
//See license.txt for license info
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

namespace OpenTK.OpenGL.Bind
{
    static partial class SpecWriter
    {
        #region Write specs

        public static void WriteSpecs(string output_path, string class_name, List<Function> functions, List<Function> wrappers, Hashtable enums)
        {
            WriteEnumSpecs(output_path, class_name, enums);
            WriteCoreFunctionSpecs(output_path, class_name, functions, wrappers);
            WriteExtensionFunctionSpecs(output_path, class_name, functions, wrappers);
        }

        #endregion

        #region Write extension function specs

        private static void WriteExtensionFunctionSpecs(string output_path, string class_name, List<Function> functions, List<Function> wrappers)
        {
            string filename = Path.Combine(output_path, class_name + "Extensions.cs");

            if (!Directory.Exists(output_path))
                Directory.CreateDirectory(output_path);

            StreamWriter sw = new StreamWriter(filename, false);

            Console.WriteLine("Writing {0} class to {1}", class_name, filename);

            WriteLicense(sw);
            WriteUsingDirectives(sw);

            sw.WriteLine("namespace {0}", Settings.OutputNamespace);
            sw.WriteLine("{");

            WriteTypes(sw);

            sw.WriteLine("    static public partial class {0}", class_name);
            sw.WriteLine("    {");
            sw.WriteLine("        static public class Extensions");
            sw.WriteLine("        {");

            WriteExtensionFunctionSignatures(sw, functions);
            WriteExtensionFunctions(sw, functions);
            WriteExtensionWrappers(sw, wrappers);

            sw.WriteLine("        }");
            sw.WriteLine("    }");
            sw.WriteLine("}");
            sw.WriteLine();

            sw.Flush();
            sw.Close();
        }

        #endregion

        #region Write core function specs

        private static void WriteCoreFunctionSpecs(string output_path, string class_name, List<Function> functions, List<Function> wrappers)
        {
            string filename = Path.Combine(output_path, class_name + ".cs");

            if (!Directory.Exists(output_path))
                Directory.CreateDirectory(output_path);

            StreamWriter sw = new StreamWriter(filename, false);

            Console.WriteLine("Writing {0} class to {1}", class_name, filename);

            WriteLicense(sw);
            WriteUsingDirectives(sw);

            WriteTypes(sw);

            sw.WriteLine("namespace {0}", Settings.OutputNamespace);
            sw.WriteLine("{");
            sw.WriteLine("    static public partial class {0}", class_name);
            sw.WriteLine("    {");

            WriteCoreFunctionSignatures(sw, functions);
            WriteDllImports(sw, functions);
            WriteCoreFunctions(sw, functions);
            WriteCoreWrappers(sw, wrappers);
            WriteCoreConstructor(sw, class_name, functions);

            sw.WriteLine("    }");

            sw.WriteLine("}");
            sw.WriteLine();

            sw.Flush();
            sw.Close();
        }

        #endregion

        #region Write enum specs

        private static void WriteEnumSpecs(string output_path, string class_name, Hashtable enums)
        {
            string filename = Path.Combine(output_path, class_name + "Enums.cs");

            if (!Directory.Exists(output_path))
                Directory.CreateDirectory(output_path);

            StreamWriter sw = new StreamWriter(filename, false);

            Console.WriteLine("Writing {0} class to {1}", class_name, filename);

            WriteLicense(sw);
            WriteUsingDirectives(sw);

            sw.WriteLine("namespace {0}", Settings.OutputNamespace);
            sw.WriteLine("{");

            WriteTypes(sw);
            WriteEnums(sw, enums);

            sw.WriteLine("}");
            sw.WriteLine();

            sw.Flush();
            sw.Close();
        }

        #endregion

        #region Write license

        public static void WriteLicense(StreamWriter sw)
        {
            sw.WriteLine("#region License");
            sw.WriteLine("//THIS FILE IS AUTOMATICALLY GENERATED");
            sw.WriteLine("//DO NOT EDIT BY HAND!!");
            sw.WriteLine("//See license.txt for license info");
            sw.WriteLine("#endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write using directivers

        private static void WriteUsingDirectives(StreamWriter sw)
        {
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Runtime.InteropServices;");
            sw.WriteLine("using System.Text;");
            sw.WriteLine();
        }

        #endregion

        #region Write types

        private static void WriteTypes(StreamWriter sw)
        {
            sw.WriteLine("    #region Types");
            //foreach ( c in constants)
            foreach (string key in Translation.CSTypes.Keys)
            {
                sw.WriteLine("    using {0} = System.{1};", key, Translation.CSTypes[key]);
                //sw.WriteLine("        public const {0};", c.ToString());
            }
            sw.WriteLine("    #endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write enums

        private static void WriteEnums(StreamWriter sw, Hashtable enums)
        {
            sw.WriteLine("    #region Enums");
            sw.WriteLine("    public struct Enums");
            sw.WriteLine("    {");

            foreach (Enum e in enums.Values)
            {
                sw.WriteLine(e.ToString());
            }

            sw.WriteLine("    }");
            sw.WriteLine("    #endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write core function signatures

        private static void WriteCoreFunctionSignatures(StreamWriter sw, List<Function> functions)
        {
            sw.WriteLine("        #region Function signatures");
            sw.WriteLine();
            sw.WriteLine("        public static class Delegates");
            sw.WriteLine("        {");

            foreach (Function f in functions)
            {
                if (f.Extension)
                    continue;
                sw.WriteLine("            public delegate {0};", f.ToString());
            }

            sw.WriteLine("        }");
            sw.WriteLine("        #endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write core dll imports

        private static void WriteDllImports(StreamWriter sw, List<Function> functions)
        {
            sw.WriteLine("        #region Imports");
            sw.WriteLine();
            sw.WriteLine("        internal class Imports");
            sw.WriteLine("        {");

            foreach (Function f in functions)
            {
                if (!f.Extension)
                {
                    sw.WriteLine("            [DllImport(\"opengl32.dll\", EntryPoint = \"gl{0}\")]", f.Name.TrimEnd('_'));
                    sw.WriteLine("            public static extern {0};", f.ToString());
                }
            }

            sw.WriteLine("        }");
            sw.WriteLine("        #endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write core functions

        private static void WriteCoreFunctions(StreamWriter sw, List<Function> functions)
        {
            sw.WriteLine("        #region Static Functions (and static initialisation)");
            sw.WriteLine();

            foreach (Function f in functions)
            {
                if (f.Extension)
                    continue;
                sw.WriteLine("        public static Delegates.{0} {0} = new Delegates.{0}(Imports.{0});", f.Name);
            }

            sw.WriteLine();
            sw.WriteLine("        #endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write core wrappers

        public static void WriteCoreWrappers(StreamWriter sw, List<Function> wrappers)
        {
            sw.WriteLine("        #region Wrappers");
            sw.WriteLine();

            if (wrappers != null)
            {
                foreach (Function f in wrappers)
                {
                    if (f.Extension)
                        continue;

                    sw.WriteLine("        #region {0}{1}", f.Name, f.Parameters.ToString());
                    sw.WriteLine();

                    sw.WriteLine("        public static");
                    sw.WriteLine(f.ToString("        "));

                    sw.WriteLine("        #endregion");
                    sw.WriteLine();
                }
            }

            sw.WriteLine("        #endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write core constructor

        private static void WriteCoreConstructor(StreamWriter sw, string class_name, List<Function> functions)
        {
            sw.WriteLine("        #region static Constructor");
            sw.WriteLine();
            sw.WriteLine("        static {0}()", class_name);
            sw.WriteLine("        {");

            List<String> import_list = new List<string>();

            #region Older Windows Core

            // Load core for older windows versions.
            sw.WriteLine("            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major < 6 || Environment.OSVersion.Platform == PlatformID.Win32Windows)");
            sw.WriteLine("            {");
            sw.WriteLine("                #region Older Windows Core");
            import_list.Add("1.2");
            import_list.Add("1.3");
            import_list.Add("1.4");
            import_list.Add("1.5");
            import_list.Add("2.0");
            import_list.Add("2.1");
            foreach (Function f in functions)
            {
                if (!f.Extension)
                    if (import_list.Contains(f.Version))
                        sw.WriteLine("                {0} = (Delegates.{0})WindowsGetAddress(\"{1}\", typeof(Delegates.{0}));", f.Name, "gl"+ f.Name.TrimEnd('_'));
            }
            sw.WriteLine("                #endregion Older Windows Core");
            sw.WriteLine("            }");
        
            #endregion
            
            #region Windows Vista Core

            // Load core for windows vista.
            sw.WriteLine("            else if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)");
            sw.WriteLine("            {");
            sw.WriteLine("                #region Windows Vista Core");
            import_list.Remove("1.2");
            import_list.Remove("1.3");
            import_list.Remove("1.4");
            foreach (Function f in functions)
            {
                if (!f.Extension)
                    if (import_list.Contains(f.Version))
                        sw.WriteLine("                {0} = (Delegates.{0})WindowsGetAddress(\"{1}\", typeof(Delegates.{0}));", f.Name, "gl" + f.Name.TrimEnd('_'));
            }
            sw.WriteLine("                #endregion Windows Vista Core");
            sw.WriteLine("            }");

            #endregion

            #region X11 Core

            // Load core for windows X11.
            sw.WriteLine("            else if (Environment.OSVersion.Platform == PlatformID.Unix)");
            sw.WriteLine("            {");
            sw.WriteLine("                #region X11 Core");
            import_list.Remove("1.5");
            import_list.Remove("1.6");
            import_list.Remove("2.0");
            import_list.Remove("2.1");
            foreach (Function f in functions)
            {
                if (!f.Extension)
                    if (import_list.Contains(f.Version))
                        sw.WriteLine("                {0} = (Delegates.{0})WindowsGetAddress(\"{1}\", typeof(Delegates.{0}));", f.Name, "gl" + f.Name.TrimEnd('_'));
            }
            sw.WriteLine("                #endregion X11 Core");
            sw.WriteLine("            }");

            #endregion
            
            sw.WriteLine("        }");
            sw.WriteLine("        #endregion static Constructor");
        }

        #endregion

        #region Write extension function signatures

        private static void WriteExtensionFunctionSignatures(StreamWriter sw, List<Function> functions)
        {
            sw.WriteLine("        #region Function signatures");
            sw.WriteLine();
            sw.WriteLine("        public static class Delegates");
            sw.WriteLine("        {");

            foreach (Function f in functions)
            {
                if (f.Extension)
                    sw.WriteLine("            public delegate {0};", f.ToString());
            }

            sw.WriteLine("        }");
            sw.WriteLine("        #endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write extension functions

        private static void WriteExtensionFunctions(StreamWriter sw, List<Function> functions)
        {
            sw.WriteLine("        #region Static Functions (and static initialisation)");
            sw.WriteLine();

            foreach (Function f in functions)
            {
                if (f.Extension)
                    sw.WriteLine("        public static Delegates.{0} {0} = new Delegates.{0}(Imports.{0});", f.Name);
            }

            sw.WriteLine();
            sw.WriteLine("        #endregion");
            sw.WriteLine();
        }

        #endregion

        #region Write extension wrappers

        public static void WriteExtensionWrappers(StreamWriter sw, List<Function> wrappers)
        {
            sw.WriteLine("        #region Wrappers");
            sw.WriteLine();

            if (wrappers != null)
            {
                foreach (Function w in wrappers)
                {
                    if (!w.Extension)
                        continue;

                    sw.WriteLine("        #region {0}{1}", w.Name, w.Parameters.ToString());
                    sw.WriteLine();

                    sw.WriteLine("        public static");
                    sw.WriteLine(w.ToString("        "));

                    sw.WriteLine("        #endregion");
                    sw.WriteLine();
                }
            }

            sw.WriteLine("        #endregion");
        }

        #endregion

    }
}