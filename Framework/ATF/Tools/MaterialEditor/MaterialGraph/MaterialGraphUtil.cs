using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Adaptation;

namespace CircuitEditorSample
{
    /// <summary>
    /// Utilities for material graph manipulation and queries
    /// </summary>
    public class MaterialGraphUtil
    {
        public static MaterialModule GetMaterialModule( Circuit circuit )
        {
            foreach ( Module m in circuit.Elements )
                if ( m.DomNode.Type.Equals( MaterialModule.materialType ) )
                {
                    return m.Cast<MaterialModule>();
                }

            return null;
        }

        public static bool IsConnectedToMaterial( IMaterialGraphModule materialModule )
        {
            Module module = materialModule.As<Module>();
            Circuit circuit = module.DomNode.GetRoot().As<Circuit>();
            if ( circuit != null )
                return IsConnectedToMaterialRecurse( module, circuit );

            Group group = module.DomNode.GetRoot().As<Group>();
            if ( group != null )
                return IsConnectedToMaterialRecurse( module, group );

            return false;
        }

        private static bool IsConnectedToMaterialRecurse( Module module, Circuit circuit )
        {
            foreach ( ICircuitPin pin in module.AllOutputPins )
            {
                MaterialGraphPin mgpin = pin as MaterialGraphPin;
                if ( mgpin != null )
                {
                    Module dstModule;
                    MaterialGraphPin dstPin;
                    if ( GetDstModule( circuit, module, mgpin, out dstModule, out dstPin ) )
                    {
                        IMaterialGraphModule mm = dstModule.As<IMaterialGraphModule>();
                        if ( mm != null )
                        {
                            if ( mm.Is<MaterialModule>() )
                                return true;

                            bool connectedToMaterial = IsConnectedToMaterialRecurse( dstModule, circuit );
                            if ( connectedToMaterial )
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsConnectedToMaterialRecurse( Module module, Group group )
        {
            foreach ( ICircuitPin pin in module.AllOutputPins )
            {
                MaterialGraphPin mgpin = pin as MaterialGraphPin;
                if ( mgpin != null )
                {
                    Module dstModule;
                    MaterialGraphPin dstPin;
                    if ( GetDstModule( group, module, mgpin, out dstModule, out dstPin ) )
                    {
                        IMaterialGraphModule mm = dstModule.As<IMaterialGraphModule>();
                        if ( mm != null )
                        {
                            if ( mm.Is<MaterialModule>() )
                                return true;

                            bool connectedToMaterial = IsConnectedToMaterialRecurse( dstModule, group );
                            if ( connectedToMaterial )
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public static IList<IMaterialParameterModule> GetMaterialParameterModules( MaterialModule material )
        {
            List<IMaterialParameterModule> materialParameterModules = new List<IMaterialParameterModule>();

            Module materialModule = material.As<Module>();
            GetMaterialParameterModulesRecurse( materialModule, materialParameterModules );

            return materialParameterModules;
        }

        private static void GetMaterialParameterModulesRecurse( Module module, IList<IMaterialParameterModule> materialParameterModules )
        {
            Circuit circuit = module.DomNode.GetRoot().As<Circuit>();

            foreach ( ICircuitPin pin in module.AllInputPins )
            {
                MaterialGraphPin mgpin = pin as MaterialGraphPin;
                if ( mgpin != null )
                {
                    Module srcModule;
                    MaterialGraphPin srcPin;
                    if ( GetSrcModule( circuit, module, mgpin, out srcModule, out srcPin ) )
                    {
                        IMaterialParameterModule mpm = srcModule.As<IMaterialParameterModule>();
                        if ( mpm != null )
                            materialParameterModules.Add( mpm );

                        GetMaterialParameterModulesRecurse( srcModule, materialParameterModules );
                    }
                }
            }
        }

        public static bool GetSrcModule( Circuit circuit, Module module, MaterialGraphPin pin, out Module srcModule, out MaterialGraphPin srcPin )
        {
            foreach ( Connection conn in circuit.Wires )
            {
                if ( conn.InputPin == pin && conn.InputPinTarget.LeafDomNode == module.DomNode )
                {
                    // can't just use conn.OutputElement and conn.OutputPin
                    // to handle grouping we need to use conn.OutputPinTarget
                    srcModule = conn.OutputPinTarget.LeafDomNode.As<Module>();
                    srcPin = srcModule.OutputPin( conn.OutputPinTarget.LeafPinIndex ).As<MaterialGraphPin>();
                    return true;
                }
            }

            srcModule = null;
            srcPin = null;
            return false;
        }

        public static bool GetDstModule( Circuit circuit, Module module, MaterialGraphPin pin, out Module dstModule, out MaterialGraphPin dstPin )
        {
            return GetDstModule( circuit.Wires, module, pin, out dstModule, out dstPin );
        }

        public static bool GetDstModule( Group group, Module module, MaterialGraphPin pin, out Module dstModule, out MaterialGraphPin dstPin )
        {
            return GetDstModule( group.Wires, module, pin, out dstModule, out dstPin );
        }

        public static bool GetDstModule( IList<Wire> wires, Module module, MaterialGraphPin pin, out Module dstModule, out MaterialGraphPin dstPin )
        {
            foreach ( Connection conn in wires )
            {
                if ( conn.OutputPin == pin && conn.OutputPinTarget.LeafDomNode == module.DomNode )
                {
                    // can't just use conn.InputElement and conn.InputPin
                    // to handle grouping we need to use conn.InputPinTarget
                    dstModule = conn.InputPinTarget.LeafDomNode.As<Module>();
                    dstPin = dstModule.InputPin( conn.InputPinTarget.LeafPinIndex ).As<MaterialGraphPin>();
                    return true;
                }
            }

            dstModule = null;
            dstPin = null;
            return false;
        }
    }
}
