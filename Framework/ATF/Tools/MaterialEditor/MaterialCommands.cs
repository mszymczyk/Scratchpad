using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using ScrubberManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dScrubberManipulator;

namespace CircuitEditorSample
{
    /// <summary>
    /// Command component that defines Material-specific commands. All of these are
    /// popup commands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof( MaterialCommands ) )]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MaterialCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public MaterialCommands( ICommandService commandService, IContextRegistry contextRegistry, IDocumentRegistry documentRegistry )
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering material commands</summary>
        void IInitializable.Initialize()
        {
            m_commandService.RegisterMenu( MaterialMenu );

            m_commandService.RegisterCommand(                
                Command.GenerateShader,
                "Material",
                null,
                "Generate Shader",
                "Generates Shader Code",
                Keys.None,
                Resources.ButtonImage,
                CommandVisibility.Default,
                this);

            //m_commandService.RegisterCommand(
            //    Command.UpdateMaterialInstances,
            //    "Material",
            //    null,
            //    "Update Material Instances",
            //    "Updates material instances to reflect current shader graph",
            //    Keys.None,
            //    Resources.AndImage,
            //    CommandVisibility.Default,
            //    this );
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            return DoCommand(commandTag, false);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            DoCommand(commandTag, true);
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, CommandState commandState)
        {
            if (commandTag is Command)
            {
                //switch ((Command)commandTag)
                //{
                //    case Command.ToggleSplitMode:
                //        commandState.Check = document.SplitManipulator != null ? document.SplitManipulator.Active : false;
                //        break;
                //}
            }
        }

        #endregion

        private bool DoCommand(object commandTag, bool doing)
        {
            //IDocument document = m_documentRegistry.ActiveDocument;
            CircuitEditingContext context = m_contextRegistry.ActiveContext.As<CircuitEditingContext>();
            if ( context == null )
                return false;

            Circuit circuit = context.CircuitContainer.As<Circuit>();
            if ( circuit == null )
                return false;

            if (commandTag is Command)
            {
				Command command = (Command)commandTag;

                if ( command == Command.GenerateShader )
                {
                    if ( doing )
                    {
                        // generate shader here
                        //
                        MaterialGenerator matGen = new MaterialGenerator( circuit );
                        matGen.Generate();
                    }

                    return true;
                }
                //else if ( command == Command.UpdateMaterialInstances )
                //{
                //    if ( doing )
                //    {
                //        CircuitDocument circuitDocument = circuit.Cast<CircuitDocument>();
                //        if ( circuitDocument.MaterialInstances.Count > 0 )
                //        {
                //            ITransactionContext transactionContext = circuitDocument.Cast<ITransactionContext>();

                //            transactionContext.DoTransaction( delegate
                //            {
                //                MaterialModule materialModule = MaterialGraphUtil.GetMaterialModule( circuit.Cast<Circuit>() );
                //                IList<IMaterialParameterModule> materialParameterModules = MaterialGraphUtil.GetMaterialParameterModules( materialModule );

                //                foreach ( IMaterialInstance materialInstance in circuitDocument.MaterialInstances )
                //                {
                //                    UpdateMaterialInstance( materialInstance.Cast<MaterialInstance>(), materialModule, materialParameterModules );
                //                }
                //            },
                //            "Update material instances" );
                //        }
                //    }

                //    return true;
                //}
            }

            return false;
        }

        //private void UpdateMaterialInstance( MaterialInstance materialInstance, MaterialModule materialModule, IList<IMaterialParameterModule> materialParameterModules )
        //{
        //    List<MaterialInstanceParameter> mipsToDelete = new List<MaterialInstanceParameter>();

        //    // delete parameters that are referring to non existing parameter modules
        //    foreach ( MaterialInstanceParameter mip in materialInstance.Parameters )
        //    {
        //        //bool found = false;
        //        //foreach ( IMaterialParameterModule parameterModule in materialParameterModules )
        //        //{
        //        //    Module pm = parameterModule.As<Module>();
        //        //    if ( mip.Module.Equals( pm ) )
        //        //    {
        //        //        found = true;
        //        //        break;
        //        //    }
        //        //}

        //        //if ( !found )
        //        if ( !materialParameterModules.Any<IMaterialParameterModule>( mpm => mip.Module.Equals( mpm.Cast<Module>() ) ) )
        //            mipsToDelete.Add( mip );
        //    }

        //    mipsToDelete.ForEach( mip => materialInstance.Parameters.Remove( mip ) );
        //    //foreach ( MaterialInstanceParameter mip in mipsToDelete )
        //    //    materialInstance.Parameters.Remove( mip );

        //    // add new or modified parameters to material instance
        //    foreach( IMaterialParameterModule parameterModule in materialParameterModules )
        //    {
        //        if ( ! materialInstance.Parameters.Any<MaterialInstanceParameter>( mip => mip.Module.Equals( parameterModule.Cast<Module>() ) ) )
        //        {
        //            MaterialInstanceParameter mip = MaterialInstanceParameter.CreateFromParameterModule( parameterModule );
        //            materialInstance.Parameters.Add( mip );
        //        }
        //    }
        //}

        private enum Command
        {
            GenerateShader,
            //UpdateMaterialInstances, // ReferenceValidator makes sure all instance parameters pointing to 'parameter modules' are deleted
        }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;

        public static MenuInfo MaterialMenu =
            new MenuInfo( "Material", "Material".Localize( "this is the name of a menu" ), "Material Commands".Localize() );
	}
}
