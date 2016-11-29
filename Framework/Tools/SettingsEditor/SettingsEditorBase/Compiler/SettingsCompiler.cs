using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using Sce.Atf;

namespace SettingsEditor
{
    public class SettingsCompiler
    {
        Assembly CompileSettings( string inputFilePath )
        {
            string fileName = Path.GetFileNameWithoutExtension( inputFilePath );

            string code = File.ReadAllText( inputFilePath );
            //code = "using SettingsCompiler;\r\n\r\n" + "namespace " + fileName + "\r\n{\r\n" + code;
            //code += "\r\n}";

            Dictionary<string, string> compilerOpts = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            CSharpCodeProvider compiler = new CSharpCodeProvider( compilerOpts );

            string[] sources = { code };
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.GenerateInMemory = true;
            compilerParams.ReferencedAssemblies.Add( "System.dll" );
            //compilerParams.ReferencedAssemblies.Add( "SettingsCompilerAttributes.dll" );
            compilerParams.ReferencedAssemblies.Add( "SettingsEditorAttributes.dll" );
            CompilerResults results = compiler.CompileAssemblyFromSource( compilerParams, sources );
            if ( results.Errors.HasErrors )
            {
                string errMsg = "Errors were returned from the C# compiler:\r\n\r\n";
                foreach ( CompilerError compilerError in results.Errors )
                {
                    int lineNum = compilerError.Line - 4;
                    errMsg += inputFilePath + "(" + lineNum + "): " + compilerError.ErrorText + "\r\n";
                }
                throw new Exception( errMsg );
            }

            return results.CompiledAssembly;
        }

        void ReflectType( Type settingsType, SettingGroup group )
        {
            object settingsInstance = Activator.CreateInstance( settingsType );

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = settingsType.GetFields( flags );
            foreach ( FieldInfo field in fields )
            {
                foreach ( Setting setting in group.Settings )
                    if ( setting.Name == field.Name )
                        throw new Exception( string.Format( "Duplicate setting \"{0}\" detected", setting.Name ) );

                Type fieldType = field.FieldType;
                object fieldValue = field.GetValue( settingsInstance );
                if ( fieldType == typeof( bool ) )
                    group.Settings.Add( new BoolSetting( (bool) fieldValue, field, group ) );
                else if ( fieldType == typeof( int ) )
                    group.Settings.Add( new IntSetting( (int) fieldValue, field, group ) );
                else if ( fieldType.IsEnum )
                    group.Settings.Add( new EnumSetting( fieldValue, field, fieldType, group ) );
                else if ( fieldType == typeof( float ) )
                    group.Settings.Add( new FloatSetting( (float) fieldValue, field, group ) );
                else if (fieldType == typeof( string ))
                    group.Settings.Add( new StringSetting( (string) fieldValue, field, group ) );
                else if ( fieldType == typeof( Direction ) )
                    group.Settings.Add( new DirectionSetting( (Direction)fieldValue, field, group ) );
                //else if ( fieldType == typeof( Orientation ) )
                //	settings.Add( new OrientationSetting( (Orientation) fieldValue, field, group ) );
                else if (fieldType == typeof( Color ))
                    group.Settings.Add( new ColorSetting( (Color) fieldValue, field, group ) );
                else if (fieldType == typeof( Float4 ))
                    group.Settings.Add( new Float4Setting( (Float4) fieldValue, field, group ) );
                else if ( fieldType == typeof( AnimCurve ) )
                    group.Settings.Add( new AnimCurveSetting( field, group ) );
                else
                    throw new Exception( "Invalid type for setting " + field.Name );
            }
        }

        void ReflectGeneratorConfig( Type settingsType )
        {
            object settingsInstance = Activator.CreateInstance( settingsType );

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = settingsType.GetFields( flags );
            foreach (FieldInfo field in fields)
            {
                Type fieldType = field.FieldType;
                object fieldValue = field.GetValue( settingsInstance );
                if (fieldType == typeof( string ))
                {
                    if (field.Name == "SettingsEditorHeaderInclude")
                    {
                        m_settingsEditorHeaderInclude = (string) fieldValue;
                    }
                    else if (field.Name == "SettingsEditorCppInclude")
                    {
                        m_settingsEditorCppInclude = (string) fieldValue;
                    }
                }
                else
                    throw new Exception( "Invalid type for setting " + field.Name );
            }
        }

        private Type[] GetTypesInNamespace( Assembly assembly, string nameSpace )
        {
            return assembly.GetTypes().Where( t => String.Equals( t.Namespace, nameSpace, StringComparison.Ordinal ) ).ToArray();
        }

        private Type[] GetRootTypesInNamespace( Assembly assembly, string nameSpace )
        {
            return assembly.GetTypes().Where( t => String.Equals( t.Namespace, nameSpace, StringComparison.Ordinal ) && t.DeclaringType == null ).ToArray();
        }

        private void ReflectSettings( Assembly assembly, string inputFilePath, SettingGroup rootStructure )
        {
            string filePath = Path.GetFileNameWithoutExtension( inputFilePath );

            Type[] types = GetRootTypesInNamespace( assembly, filePath );
            if (types == null || types.Length == 0)
                throw new Exception( "Settings file " + inputFilePath + " does not define any classes" );

            foreach (Type type in types)
            {
                if ( type.IsEnum )
                {
                    rootStructure.ReflectedEnums.Add( type );
                    continue;
                }

                if (type.Name == "GeneratorConfig")
                {
                    ReflectGeneratorConfig( type );
                    continue;
                }

                ReflectSettingsRecurse( type, rootStructure );
            }
        }

        private void ReflectSettingsRecurse( Type type, SettingGroup parentStructure )
        {
            SettingGroup structure = new SettingGroup( type.Name, type, parentStructure );
            parentStructure.NestedStructures.Add( structure );

            ReflectType( type, structure );

            Type[] nestedTypes = type.GetNestedTypes( BindingFlags.Public | BindingFlags.NonPublic );
            foreach (Type nestedType in nestedTypes)
            {
                if ( nestedType.IsEnum )
                {
                    structure.ReflectedEnums.Add( nestedType );
                    continue;
                }

                ReflectSettingsRecurse( nestedType, structure );
            }
        }

        public static void WriteEnumTypes( FileWriter fw, List<Type> enumTypes )
        {
            foreach ( Type enumType in enumTypes )
            {
                if ( enumType.GetEnumUnderlyingType() != typeof( int ) )
                    throw new Exception( "Invalid underlying type for enum " + enumType.Name + ", must be int" );
                string[] enumNames = enumType.GetEnumNames();
                int numEnumValues = enumNames.Length;

                Array values = enumType.GetEnumValues();
                int[] enumValues = new int[numEnumValues];
                for (int i = 0; i < numEnumValues; ++i)
                    enumValues[i] = (int) values.GetValue( i );

                fw.AddLine( "enum class " + enumType.Name );
                fw.AddLine( "{" );
                fw.IncIndent();
                for (int i = 0; i < enumNames.Length; ++i)
                {
                    fw.AddLine( enumNames[i] + " = " + enumValues[i] + "," );
                }
                fw.AddLine( "NumValues" );

                fw.DecIndent();
                fw.AddLine( "};" );
                fw.EmptyLine();
            }
        }

        static private string SettingTypeToCppType( SettingType type )
        {
            if ( type == SettingType.Bool )
                return "eParamType_bool";
            else if ( type == SettingType.Int )
                return "eParamType_int";
            else if (type == SettingType.Enum)
                return "eParamType_enum";
            else if (type == SettingType.Float)
                return "eParamType_float";
            else if ( type == SettingType.FloatBool )
                return "eParamType_floatBool";
            else if (type == SettingType.String)
                return "eParamType_string";
            else if ( type == SettingType.Direction )
                return "eParamType_direction";
            else if (type == SettingType.Color)
                return "eParamType_color";
            else if (type == SettingType.Float4)
                return "eParamType_float4";
            else if ( type == SettingType.AnimCurve )
                return "eParamType_animCurve";
            else
                throw new Exception( "Unsupported setting type" );
        }

        private static void WriteFileIfChanged( FileWriter fw, string filepath )
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in fw.Lines)
                sb.AppendLine( line );

            string newText = sb.ToString();
            //System.Diagnostics.Debug.WriteLine( sourceCode );

            if ( File.Exists(filepath) )
            {
                string srcText = File.ReadAllText( filepath );
                if (srcText == newText)
                {
                    return;
                }
            }

            File.WriteAllText( filepath, newText );
        }

        //private FileWriter BeginNewHeaderFile( bool shaderSettings )
        //{
        //    FileWriter fw = new FileWriter();

        //    fw.AddLine( "#pragma once" );
        //    fw.AddLine( "" );
        //    fw.AddLine( m_settingsEditorHeaderInclude );
        //    fw.AddLine( "" );

        //    if (shaderSettings)
        //    {
        //        // Disable warning "structure was padded due to __declspec(align())" (because we want to need to store bools as 4 bytes).
        //        fw.AddLine( "#ifdef _MSC_VER" );
        //        fw.AddLine( "#pragma warning(push)" );
        //        fw.AddLine( "#pragma warning(disable:4324)" );
        //        fw.AddLine( "#endif //" );
        //        fw.EmptyLine();
        //    }

        //    fw.AddLine( "namespace " + m_rootStructure.Name + "Namespace" );
        //    fw.AddLine( "{" );
        //    fw.AddLine( "" );

        //    return fw;
        //}

        private void FinalizeHeaderFile( FileWriter fw, bool shaderSettings )
        {
            fw.EmptyLine();

            fw.AddLine( "} // namespace " + m_rootStructure.Name + "Namespace" );

            if (shaderSettings)
            {
                fw.EmptyLine();
                fw.AddLine( "#ifdef _MSC_VER" );
                fw.AddLine( "#pragma warning(pop)" );
                fw.AddLine( "#endif //" );
                fw.EmptyLine();
            }


        }

        private void WritePresetSupport( FileWriter fw, SettingGroup structure, bool shaderSettings )
        {
            if ( !structure.HasPresets )
                return;

            fw.EmptyLine();
            fw.AddLine( "const " + structure.Name + "* getPreset( const char* presetName ) const" );
            fw.AddLine( "{" );
            fw.IncIndent();
            fw.AddLine( "return reinterpret_cast<const " + structure.Name + "*>( " + "SettingsEditor::_internal::getPreset( presetName, impl_ ) );" );
            fw.DecIndent();
            fw.AddLine( "}" );
            fw.EmptyLine();
        }

        private void WriteStructure( FileWriter fw, SettingGroup structure, bool shaderSettings )
        {
            fw.AddLine( "// " + structure.Name );
            fw.AddLine( "//////////////////////////////////////////////////////////////////////////////////" );
            fw.AddLine( "struct " + structure.Name );
            fw.AddLine( "{" );

            fw.IncIndent();
            fw.AddLine( "SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work" );
            fw.EmptyLine();

            // write enums
            //
            WriteEnumTypes( fw, structure.ReflectedEnums );

            int bytes = 0;

            foreach (Setting setting in structure.Settings)
            {
                if (shaderSettings)
                {
                    if (setting.Type == SettingType.Color)
                    {
                        if (bytes % 16 > 4)
                        {
                            fw.AddLine( "__declspec(align(16)) " );
                            bytes += 16 - bytes % 16;
                        }
                        bytes += 12;
                    }
                    else if (setting.Type == SettingType.Float4)
                    {
                        fw.AddLine( "__declspec(align(16)) " );
                        bytes += 16;
                    }
                    else if (setting.Type == SettingType.Bool)
                    {
                        fw.AddLine( "__declspec(align(4)) " );
                        bytes += 4;
                    }
                    else
                    {
                        bytes += 4;
                    }
                }

                setting.WriteDeclaration( fw );
            }

            // shader settings can have only two hierarchy levels, root and one below
            //
            if ( structure.NestedStructures.Count > 0 && (!shaderSettings || structure.ParentStructure == null) )
            {
                fw.EmptyLine();

                if (structure.IsRootLevel)
                {
                    // nested structures
                    //
                    foreach (SettingGroup nestedStructure in structure.NestedStructures)
                    {
                        //FileWriter fwLevel1 = BeginNewHeaderFile( false );
                        FileWriter fwLevel1 = new FileWriter();

                        fwLevel1.AddLine( "#pragma once" );
                        fwLevel1.AddLine( "" );
                        fwLevel1.AddLine( m_settingsEditorHeaderInclude );
                        fwLevel1.AddLine( "#include \"" + m_rootStructure.Name + ".h\"" );
                        fwLevel1.AddLine( "" );

                        if (shaderSettings)
                        {
                            // Disable warning "structure was padded due to __declspec(align())" (because we want to need to store bools as 4 bytes).
                            fwLevel1.AddLine( "#ifdef _MSC_VER" );
                            fwLevel1.AddLine( "#pragma warning(push)" );
                            fwLevel1.AddLine( "#pragma warning(disable:4324)" );
                            fwLevel1.AddLine( "#endif //" );
                            fwLevel1.EmptyLine();
                        }

                        fwLevel1.AddLine( "namespace " + m_rootStructure.Name + "Namespace" );
                        fwLevel1.AddLine( "{" );
                        fwLevel1.AddLine( "" );

                        WriteStructure( fwLevel1, nestedStructure, shaderSettings );

                        FinalizeHeaderFile( fwLevel1, shaderSettings );
                        string filename = m_outputPathHeaderWithoutExtension + "_" + nestedStructure.Name + ".h";
                        WriteFileIfChanged( fwLevel1, filename );
                    }
                }
                else
                {
                    WritePresetSupport( fw, structure, shaderSettings );

                    // nested structures
                    //
                    foreach (SettingGroup nestedStructure in structure.NestedStructures)
                    {
                        WriteStructure( fw, nestedStructure, shaderSettings );
                    }
                }
            }
            else if ( ! structure.IsRootLevel )
            {
                WritePresetSupport( fw, structure, shaderSettings );
            }

            fw.DecIndent();
            //if (structure.ParentStructure != null)
            //{
            //    if (structure.ParentStructure.ParentStructure == null)
            //    {
            //        fw.AddLine( "};" );
            //        fw.EmptyLine();
            //        //fw.AddLine( "private:" );
            //        //fw.IncIndent();
            //        fw.AddLine( structure.Name + " __instanceOf_" + structure.Name + "; // please don't touch this field" );
            //        //fw.DecIndent();
            //        //fw.AddLine( "public:" );
            //        //fw.IncIndent();
            //        fw.AddLine( "const " + structure.Name + "* m" + structure.Name + " = &__instanceOf_" + structure.Name + ";" );
            //        //fw.DecIndent();
            //    }
            //    else
            //    {
            //        fw.AddLine( "} m" + structure.Name + ";" );
            //    }
            //}
            //else
            //    fw.AddLine( "};" );

            if (structure.IsRootLevel)
            {
                fw.IncIndent();
                // nested structures
                //
                foreach (SettingGroup nestedStructure in structure.NestedStructures)
                {
                    fw.AddLine( "const " + nestedStructure.Name + "* m" + nestedStructure.Name + ";" );
                }

                fw.DecIndent();
                fw.AddLine( "};" );
            }
            else if (structure.IsFirstLevel)
            {
                fw.AddLine( "};" );
            }
            else
            {
                fw.AddLine( "} m" + structure.Name + ";" );
            }

            fw.EmptyLine();
        }

        //void GenerateHeader2Wrap( SettingGroup rootStructure, string outputName, string outputPath, bool shaderSettings )
        //{
        //    FileWriter fw = new FileWriter();

        //    fw.AddLine( "#pragma once" );
        //    fw.AddLine( "" );
        //    fw.AddLine( m_settingsEditorHeaderInclude );
        //    fw.AddLine( "" );

        //    if (shaderSettings)
        //    {
        //        // Disable warning "structure was padded due to __declspec(align())" (because we want to need to store bools as 4 bytes).
        //        fw.AddLine( "#ifdef _MSC_VER" );
        //        fw.AddLine( "#pragma warning(push)" );
        //        fw.AddLine( "#pragma warning(disable:4324)" );
        //        fw.AddLine( "#endif //" );
        //        fw.EmptyLine();
        //    }

        //    fw.AddLine( "namespace " + m_rootStructure.Name + "Namespace" );
        //    fw.AddLine( "{" );
        //    fw.AddLine( "" );

        //    fw.AddLine( "class " + outputName + "Wrap : public " + outputName );
        //    fw.AddLine( "{" );
        //    fw.AddLine( "public:" );
        //    fw.IncIndent();
        //    fw.AddLine( outputName + "Wrap()" );
        //    fw.AddLine( "{" );
        //    fw.IncIndent();
        //    fw.AddLine( "unload();" );
        //    fw.DecIndent();
        //    fw.AddLine( "}" );

        //    fw.AddLine( "" );

        //    fw.AddLine( "void load( const char* filePath );" );
        //    //fw.AddLine( "{" );
        //    //fw.IncIndent();
        //    //fw.AddLine( "if ( settingsFile_ )" );
        //    //fw.IncIndent();
        //    //fw.AddLine( "return;" );
        //    //fw.DecIndent();
        //    //fw.AddLine( "" );

        //    //fw.AddLine( "settingsFile_ = SettingsEditor::createSettingsFile( filePath, GetDesc(), this );" );

        //    //fw.DecIndent();
        //    //fw.AddLine( "}" );

        //    fw.AddLine( "" );

        //    fw.AddLine( "void unload()" );
        //    fw.AddLine( "{" );
        //    fw.AddLine( "\tSettingsEditor::releaseSettingsFile( settingsFile_ );" );
        //    fw.AddLine( "}" );
        //    fw.AddLine( "" );

        //    fw.DecIndent();
        //    fw.AddLine( "private:" );
        //    fw.IncIndent();
        //    fw.AddLine( "\tSettingsEditor::SettingsFile* settingsFile_ = nullptr;" );

        //    fw.DecIndent();
        //    fw.AddLine( "}; // class " + outputName + "Wrap" );
        //    fw.AddLine( "" );

        //    fw.EmptyLine();

        //    fw.AddLine( "} // namespace " + m_rootStructure.Name + "Namespace" );

        //    if (shaderSettings)
        //    {
        //        fw.EmptyLine();
        //        fw.AddLine( "#ifdef _MSC_VER" );
        //        fw.AddLine( "#pragma warning(pop)" );
        //        fw.AddLine( "#endif //" );
        //        fw.EmptyLine();
        //    }

        //    string filename = m_outputPathHeaderWithoutExtension + "Wrap.h";
        //    WriteFileIfChanged( fw, filename );
        //}

        void GenerateHeader2( SettingGroup rootStructure, string outputName, string outputPath, bool shaderSettings )
        {
            FileWriter fw = new FileWriter();

            fw.AddLine( "#pragma once" );
            fw.AddLine( "" );
            fw.AddLine( m_settingsEditorHeaderInclude );
            fw.AddLine( "" );

            if (shaderSettings)
            {
                // Disable warning "structure was padded due to __declspec(align())" (because we want to need to store bools as 4 bytes).
                fw.AddLine( "#ifdef _MSC_VER" );
                fw.AddLine( "#pragma warning(push)" );
                fw.AddLine( "#pragma warning(disable:4324)" );
                fw.AddLine( "#endif //" );
                fw.EmptyLine();
            }

            fw.AddLine( "namespace " + outputName + "Namespace" );
            fw.AddLine( "{" );
            fw.AddLine( "" );

            // nested structures
            //
            foreach (SettingGroup nestedStructure in rootStructure.NestedStructures)
            {
                fw.AddLine( "struct " + nestedStructure.Name + ";" );
            }

            fw.EmptyLine();

            WriteStructure( fw, rootStructure, shaderSettings );

            fw.AddLine( "class " + outputName + "Wrap : public " + outputName );
            fw.AddLine( "{" );
            fw.AddLine( "public:" );
            fw.IncIndent();

            // constructor
            //
            fw.AddLine( outputName + "Wrap( const char* filePath)" );
            fw.AddLine( "{" );
            fw.IncIndent();
            fw.AddLine( "load( filePath );" );
            fw.DecIndent();
            fw.AddLine( "}" );
            fw.EmptyLine();

            // destructor
            //
            fw.AddLine( "~" + outputName + "Wrap()" );
            fw.AddLine( "{" );
            fw.IncIndent();
            fw.AddLine( "unload();" );
            fw.DecIndent();
            fw.AddLine( "}" );

            fw.AddLine( "" );

            fw.DecIndent();
            fw.AddLine( "private:" );
            fw.IncIndent();

            fw.AddLine( "void load( const char* filePath );" );
            //fw.AddLine( "{" );
            //fw.IncIndent();
            //fw.AddLine( "if ( settingsFile_ )" );
            //fw.IncIndent();
            //fw.AddLine( "return;" );
            //fw.DecIndent();
            //fw.AddLine( "" );

            //fw.AddLine( "settingsFile_ = SettingsEditor::createSettingsFile( filePath, GetDesc(), this );" );

            //fw.DecIndent();
            //fw.AddLine( "}" );

            //fw.AddLine( "" );

            fw.AddLine( "void unload();" );
            fw.EmptyLine();

            //fw.AddLine( "{" );
            //fw.AddLine( "\tSettingsEditor::releaseSettingsFile( settingsFile_ );" );
            //fw.AddLine( "}" );
            //fw.AddLine( "" );

            fw.AddLine( "\tSettingsEditor::SettingsFile settingsFile_;" );

            fw.DecIndent();
            fw.AddLine( "}; // class " + outputName + "Wrap" );
            fw.AddLine( "" );

            fw.EmptyLine();

            fw.AddLine( "} // namespace " + outputName + "Namespace" );

            fw.EmptyLine();
            fw.AddLine( "// Declared this variable in global scope to simplify usage and debugging" );
            fw.AddLine( "// Visual Studio's debugger doesn't see global variables declared within namespace :(" );
            fw.AddLine( "// Name clashes shouldn't be a big problem..." );
            fw.AddLine( "//" );
            fw.AddLine( "extern " + outputName + "Namespace::" + outputName + "Wrap* g" + outputName + ";" );

            if (shaderSettings)
            {
                fw.EmptyLine();
                fw.AddLine( "#ifdef _MSC_VER" );
                fw.AddLine( "#pragma warning(pop)" );
                fw.AddLine( "#endif //" );
                fw.EmptyLine();
            }

            WriteFileIfChanged( fw, outputPath );

            //StringBuilder sb = new StringBuilder();
            //foreach (string line in fw.Lines)
            //    sb.AppendLine( line );

            //string sourceCode = sb.ToString();
            //System.Diagnostics.Debug.WriteLine( sourceCode );

            //File.WriteAllText( outputPath, sourceCode );
        }

        private static string GetNamespaceName( SettingGroup structure )
        {
            string namespaceName = "NS";
            SettingGroup tmpS = structure;
            while (tmpS != null)
            {
                namespaceName = tmpS.Name + "_" + namespaceName;
                tmpS = tmpS.ParentStructure;
            }
            return namespaceName;
        }

        void WriteDescriptions( FileWriter fw, SettingGroup structure )
        {
            // depth first
            //
            foreach (SettingGroup nestedStructure in structure.NestedStructures)
            {
                WriteDescriptions( fw, nestedStructure );
            }
            fw.EmptyLine();

            //if (structure.ParentStructure != null)
            //{
                string namespaceName = GetNamespaceName( structure );

                fw.EmptyLine();
                fw.AddLine( "namespace " + namespaceName );
                fw.AddLine( "{" );
                fw.IncIndent();

                string fieldCountName = structure.Name + "_Field_Count";
                string nestedStructCountName = structure.Name + "_NestedStructures_Count";
                fw.AddLine( "const unsigned int " + fieldCountName + " = " + structure.Settings.Count + ";" );
                fw.AddLine( "const unsigned int " + nestedStructCountName + " = " + structure.NestedStructures.Count + ";" );
                if ( structure.Settings.Count > 0 )
                    fw.AddLine( "FieldDescription " + structure.Name + "_Fields[" + fieldCountName + "];" );
                if ( structure.NestedStructures.Count > 0 )
                    fw.AddLine( "const StructDescription* " + structure.Name + "_NestedStructures[" + nestedStructCountName + "];" );

                fw.EmptyLine();

                fw.AddLine( "StructDescription " + structure.Name + "_CreateDescription( const StructDescription* parent )" );
                fw.AddLine( "{" );
                fw.IncIndent();

                fw.AddLine( "StructDescription d;" );
                fw.AddLine( "d.name_ = \"" + structure.Name + "\";" );
                fw.AddLine( "d.parentStructure_ = parent; " );
                if (structure.Settings.Count > 0)
                    fw.AddLine( "d.fields_ = " + structure.Name + "_Fields;" );
                else
                    fw.AddLine( "d.fields_ = nullptr;" );
                if ( structure.NestedStructures.Count > 0 )
                    fw.AddLine( "d.nestedStructures_ = " + structure.Name + "_NestedStructures;" );
                else
                    fw.AddLine( "d.nestedStructures_ = nullptr;" );
                fw.AddLine( "d.nFields_ = " + fieldCountName + ";" );
                fw.AddLine( "d.nNestedStructures_ = " + nestedStructCountName + ";" );
                if (structure.ParentStructure != null)
                {
                    //if (structure.ParentStructure.ParentStructure == null)
                    //{
                    //    fw.AddLine( "d.offset_ = offsetof( " + structure.ParentStructure.CppFullName + ", __instanceOf_" + structure.Name + " );" );
                    //}
                    //else
                    //{
                        fw.AddLine( "d.offset_ = offsetof( " + structure.ParentStructure.CppFullName + ", m" + structure.Name + " );" );
                    //}
                }
                else
                    fw.AddLine( "d.offset_ = 0;" );

                fw.AddLine( "d.sizeInBytes_ = sizeof(" + structure.CppFullName + ");" );

                // initialize
                //
                int fieldIndex = 0;
                foreach (Setting field in structure.Settings)
                {
                    fw.AddLine( structure.Name + "_Fields[" + fieldIndex + "] = FieldDescription( \"" + field.Name + "\", offsetof(" + structure.CppFullName + ", " + field.Name + "), " + SettingTypeToCppType( field.Type ) + " );" );
                    ++ fieldIndex;
                }

                int nestedStructureIndex = 0;
                foreach (SettingGroup nestedStructure in structure.NestedStructures)
                {
                    fw.AddLine( structure.Name + "_NestedStructures[" + nestedStructureIndex + "] = " + nestedStructure.CppFullName + "::GetDesc();" );
                    nestedStructureIndex += 1;
                }

                fw.AddLine( "return d;" );
                fw.DecIndent();
                fw.AddLine( "}" );


                fw.DecIndent();
                fw.AddLine( "} // namespace " + namespaceName );
            //}
        }

        void WriteDefinitions( FileWriter fw, SettingGroup structure )
        {
            string parent = "nullptr";
            if (structure.ParentStructure != null)
            {
                //parent = "&" + structure.ParentStructure.CppFullName + "::__desc";
                parent = structure.ParentStructure.CppFullName + "::GetDesc()";
            }

            fw.AddLine( "StructDescription " + structure.CppFullName + "::__desc = " + GetNamespaceName( structure ) + "::" + structure.Name + "_CreateDescription( " + parent + " );" );

            foreach (SettingGroup nestedStructure in structure.NestedStructures)
            {
                WriteDefinitions( fw, nestedStructure );
            }
        }

        private void AddIncludes( FileWriter fw, SettingGroup rootStructure )
        {
            foreach (SettingGroup nestedStructure in rootStructure.NestedStructures)
            {
                fw.AddLine( "#include \"" + m_rootStructure.Name + "_" + nestedStructure.Name + ".h\"" );
            }
        }

        void GenerateCpp( SettingGroup rootStructure, string outputName, string outputPath )
        {
            FileWriter fw = new FileWriter();

            if ( m_settingsEditorCppInclude != null )
                fw.AddLine( m_settingsEditorCppInclude );
            fw.AddLine( "#include \"" + outputName + ".h\"" );
            fw.AddLine( "" );

            AddIncludes( fw, rootStructure );
            fw.EmptyLine();

            fw.AddLine( "using namespace SettingsEditor;" );
            fw.EmptyLine();
            fw.AddLine( "namespace " + outputName + "Namespace" );
            fw.AddLine( "{" );
            fw.IncIndent();

            // write descriptions
            //
            WriteDescriptions( fw, rootStructure );

            // write definitions
            //
            fw.EmptyLine();
            fw.EmptyLine();
            WriteDefinitions( fw, rootStructure );

            fw.DecIndent();
            fw.AddLine( "" );

            fw.IncIndent();
            //fw.AddLine( outputName + "Wrap* g" + outputName + " = nullptr;" );
            //fw.EmptyLine();

            fw.AddLine( "void " + outputName + "Wrap::load( const char* filePath )" );
            fw.AddLine( "{" );
            fw.IncIndent();
            fw.AddLine( "if ( settingsFile_.isValid() )" );
            fw.IncIndent();
            fw.AddLine( "return;" );
            fw.DecIndent();
            fw.AddLine( "" );

            foreach (SettingGroup nestedStructure in rootStructure.NestedStructures)
            {
                fw.AddLine( "m" + nestedStructure.Name + " = new " + nestedStructure.Name + ";" );
            }
            fw.EmptyLine();

            fw.AddLine( "const void* addresses[" + rootStructure.NestedStructures.Count + "];" );
            int igroup = 0;
            foreach (SettingGroup nestedStructure in rootStructure.NestedStructures)
            {
                fw.AddLine( "addresses[" + igroup + "] = m" + nestedStructure.Name + ";" );
                ++igroup;
            }
            fw.EmptyLine();


            fw.AddLine( "settingsFile_ = SettingsEditor::createSettingsFile( filePath, GetDesc(), addresses );" );

            fw.DecIndent();
            fw.AddLine( "}" );
            fw.EmptyLine();

            fw.AddLine( "void " + outputName + "Wrap::unload()" );
            fw.AddLine( "{" );
            fw.IncIndent();

            fw.AddLine( "SettingsEditor::releaseSettingsFile( settingsFile_ );" );
            fw.EmptyLine();

            foreach (SettingGroup nestedStructure in rootStructure.NestedStructures)
            {
                fw.AddLine( "delete m" + nestedStructure.Name + "; m" + nestedStructure.Name + " = nullptr;" );
            }

            fw.DecIndent();
            fw.AddLine( "}" );
            fw.AddLine( "" );

            fw.EmptyLine();
            fw.DecIndent();

            fw.AddLine( "} // namespace " + outputName + "Namespace" );

            fw.EmptyLine();
            fw.AddLine( outputName + "Namespace::" + outputName + "Wrap* g" + outputName + " = nullptr;" );
            fw.EmptyLine();

            WriteFileIfChanged( fw, outputPath );

            //StringBuilder sb = new StringBuilder();
            //foreach (string line in fw.Lines)
            //    sb.AppendLine( line );

            //string sourceCode = sb.ToString();
            //System.Diagnostics.Debug.WriteLine( sourceCode );

            //File.WriteAllText( outputPath, sourceCode );
        }

        // TODO: Layouts are hardcoded for QuickDebug. Change that when we have some other usecases.
        public static void GenerateShaderHeader( List<SettingGroup> settingGroups, string outputName, string outputPath, List<Type> enumTypes )
        {
            //List<string> lines = new List<string>();
            FileWriter fw = new FileWriter();

            string headerGuard = camelCaseToUnderscore( outputName ) + "_H";
            fw.AddLine( "#ifndef " + headerGuard );
            fw.AddLine( "#define " + headerGuard );
            fw.AddLine( "" );
            fw.AddLine( "#include \"picoShaderDef.h\"" );
            fw.AddLine( "" );
            fw.AddLine( "#ifdef __GLSL__" );
            fw.AddLine( "layout( binding = PICO_OPENGL_QUICKDEBUG_BINDING ) uniform cb_" + outputName );
            fw.AddLine( "#else" );
            fw.AddLine( "cbuffer cb_" + outputName + " : PICO_DX11_QUICKDEBUG_REGISTER" );
            fw.AddLine( "#endif //" );
            fw.AddLine( "{" );

            int bytes = 0;
            foreach (SettingGroup settingGroup in settingGroups)
            {
                foreach (Setting setting in settingGroup.Settings)
                {
                    string typeString = "";
                    switch (setting.Type)
                    {
                        case SettingType.Enum:
                        case SettingType.Int:
                            typeString = "int";
                            bytes += 4;
                            break;
                        case SettingType.Bool:
                            typeString = "bool";
                            bytes += 4;
                            break;
                        case SettingType.Float:
                            typeString = "float";
                            bytes += 4;
                            break;
                        case SettingType.FloatBool:
                            typeString = "float";
                            bytes += 4;
                            break;
                        case SettingType.Color:
                            typeString = "float3";
                            if (bytes % 16 > 4)
                                bytes += 16 - bytes % 16;
                            bytes += 12;
                            break;
                        case SettingType.Float4:
                            typeString = "float4";
                            bytes += 16;
                            break;
                        default:
                            // TODO: String.
                            System.Diagnostics.Debug.Assert( false );
                            break;
                    }
                    fw.AddLine( "\t" + typeString + " " + setting.Name + ";" );
                    if ( setting.Type == SettingType.FloatBool )
                    {
                        fw.AddLine( "\tbool " + setting.Name + "Enabled;" );
                        bytes += 4;
                    }
                }
            }
            //int padding = 4 - (bytes % 16 / 4);
            //if (padding < 4)
            //    lines.Add("\tfloat pad_" + bytes + "[" + padding + "];"); // bytes in name serves as unique name generator.

            fw.AddLine( "}" );
            fw.AddLine( "" );

            foreach (Type enumType in enumTypes)
            {
                string[] enumNames = enumType.GetEnumNames();
                Array enumValues = enumType.GetEnumValues();
                for (int i = 0; i < enumNames.Length; ++i)
                {
                    string line = enumType.Name + "_" + enumNames[i] + " " + (int) enumValues.GetValue( i );
                    fw.AddLine( "#define " + camelCaseToUnderscore( line ) );
                }
                fw.AddLine( "" );
            }

            fw.AddLine( "#endif // " + headerGuard );

            WriteFileIfChanged( fw, outputPath );

            //StringBuilder sb = new StringBuilder();
            //foreach (string line in lines)
            //    sb.AppendLine( line );

            //string sourceCode = sb.ToString();
            //System.Diagnostics.Debug.WriteLine( sourceCode );

            //File.WriteAllText( outputPath, sourceCode );
        }

        public static string camelCaseToUnderscore( string line )
        {
            // Camel case to underscore.
            line = Regex.Replace( line, @"([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])", "$1$3_$2$4" );
            line = Regex.Replace( line, @"^([A-Z])_([A-Z])", "$1$2" );
            return line.ToUpper();
        }

        public void GenerateHeaderIfChanged( string filePath, string shaderOutputPath )
        {
            string fileName = Path.GetFileNameWithoutExtension( filePath );
            string outputDir = Path.GetDirectoryName( filePath );
            m_outputPathHeaderWithoutExtension = Path.Combine( outputDir, fileName );
            string outputPathHeader = m_outputPathHeaderWithoutExtension + ".h";
            string outputPathCpp = Path.Combine( outputDir, fileName ) + ".cpp";

            FileInfo srcFileInfo = new FileInfo( filePath );
            FileInfo dstFileInfoHeader = new FileInfo( outputPathHeader );

            // add additional version of the compiler check
            // rebuild is needed if compiler was updated
            //

            if (srcFileInfo.LastWriteTime >= dstFileInfoHeader.LastWriteTime)
            {
                //ReflectSettings( filePath );
                //GenerateHeader( m_reflectedSettings, fileName, outputPath, m_reflectedEnums );
                if (string.IsNullOrEmpty( shaderOutputPath ))
                {
                    GenerateHeader2( m_rootStructure, fileName, outputPathHeader, false );
                    GenerateCpp( m_rootStructure, fileName, outputPathCpp );
                }
                else
                {
                    GenerateHeader2( m_rootStructure, fileName, outputPathHeader, true );
                    GenerateCpp( m_rootStructure, fileName, outputPathCpp );
                    GenerateShaderHeader( m_rootStructure.NestedStructures, fileName, shaderOutputPath, m_rootStructure.ReflectedEnums );
                }
            }
            else
            {
                Outputs.WriteLine( OutputMessageType.Info, "Settings file '{0} is up-to-date", filePath );
            }
        }

        public void ReflectSettings( string filepath )
        {
            Assembly compiledAssembly = CompileSettings( filepath );

            string rootStructureName = Path.GetFileNameWithoutExtension( filepath );
            m_rootStructure = new SettingGroup( rootStructureName, null, null );
            ReflectSettings( compiledAssembly, filepath, m_rootStructure );
        }

        public SettingGroup RootStructure { get { return m_rootStructure; } }
        private SettingGroup m_rootStructure;

        private string m_outputPathHeaderWithoutExtension;

        private string m_settingsEditorHeaderInclude = "#include <SettingsEditor.h>";
        private string m_settingsEditorCppInclude;
    }
}