using Sce.Atf.Dom;
using System.ComponentModel;

namespace SettingsEditor
{
	public interface IDynamicSchema
	{
		void AddDynamicAttributes( Schema schema );
		void InitGui( Schema schema );
        void CreateNodes( DomNode rootNode );

        SchemaLoader Loader { set; get; }
        Schema Schema { set; get; }
	}
}
