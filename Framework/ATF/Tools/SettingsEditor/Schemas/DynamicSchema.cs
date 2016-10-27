using System;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using misz.Gui;
using Sce.Atf;
using pico.Controls.PropertyEditing;

namespace SettingsEditor
{
    public class DynamicSchema
    {
        public DynamicSchema( SettingsCompiler compiler )
        {
            m_compiler = compiler;
        }


        private void SyncProperties( Group group, SettingGroup settingGroup )
        {
            List<DynamicProperty> propertiesCopy = new List<DynamicProperty>();
            foreach ( DynamicProperty dp in group.Properties )
                propertiesCopy.Add( dp );

            group.Properties.Clear();

            // add properties in correct order, creating missing ones along the way
            //
            foreach ( Setting s in settingGroup.Settings )
            {
                DynamicProperty dpOld = propertiesCopy.Find( p => p.Name == s.Name );
                DynamicProperty dp = DynamicProperty.CreateFromSetting( group, s, dpOld );
                group.Properties.Add( dp );
            }
        }

        public void CreateNodesRecurse( DomNode parent, SettingGroup parentStructure )
        {
            // clear children list
            //
            IList<DomNode> childList = null;
            if ( parent.Type == Schema.settingsFileType.Type )
                childList = parent.GetChildList( Schema.settingsFileType.groupChild );
            else
                childList = parent.GetChildList( Schema.groupType.groupChild );

            List<DomNode> childrenCopy = new List<DomNode>();
            foreach ( DomNode dn in childList )
                childrenCopy.Add( dn );

            childList.Clear();

            // add groups in correct order, creating missing groups along the way
            //
            foreach ( SettingGroup structure in parentStructure.NestedStructures)
            {
                DomNode group = childrenCopy.Find( n => n.As<Group>().Name == structure.Name );

                Group g = null;

                if ( group == null )
                {
                    group = new DomNode( Schema.groupType.Type );

                    g = group.As<Group>();
                    g.Name = structure.Name;
                }
                else
                {
                    g = group.As<Group>();
                }

                childList.Add( group );

                g.SettingGroup = structure;

                SyncProperties( g, structure );

                if ( structure.HasPresets )
                {
                    foreach ( Preset p in g.Presets )
                    {
                        Group pGroup = p.As<Group>();
                        SyncProperties( pGroup, structure );
                    }
                }
                else
                {
                    g.Presets.Clear();
                }

                CreateNodesRecurse( group, structure );
            }

        }

        public void CreateNodes( DomNode rootNode )
        {
            CreateNodesRecurse( rootNode, m_compiler.RootStructure );
        }

        private int[] GetEnumIntValues( Type type )
        {
            System.Array valuesArray = Enum.GetValues( type );
            int[] intArray = new int[valuesArray.Length];
            for (int i = 0; i < valuesArray.Length; ++i)
            {
                intArray[i] = (int) valuesArray.GetValue( i );
            }
            return intArray;
        }

        SettingsCompiler m_compiler;
    }

    //public class DependsOnNodes : ICustomEnableAttributePropertyDescriptorCallback
    //{
    //    public DependsOnNodes()
    //    {
    //        m_dependsOnList = new List<Tuple<DomNodeType, AttributeInfo, bool>>();
    //    }

    //    public DependsOnNodes( List<Tuple<DomNodeType, AttributeInfo, bool>> dependsOn )
    //    {
    //        m_dependsOnList = dependsOn;
    //    }

    //    public void Add( DomNodeType dnt, AttributeInfo attrInfo, bool condition )
    //    {
    //        m_dependsOnList.Add( new Tuple<DomNodeType, AttributeInfo, bool>( dnt, attrInfo, condition ) );
    //    }

    //    public bool IsReadOnly( DomNode domNode, AttributePropertyDescriptor descriptor )
    //    {
    //        DomNode root = domNode.GetRoot();

    //        foreach( Tuple<DomNodeType, AttributeInfo, bool> p in m_dependsOnList )
    //        {
    //            foreach( DomNode dn in root.Subtree )
    //            {
    //                if ( dn.Type == p.Item1 )
    //                {
    //                    bool bval = (bool)dn.GetAttribute( p.Item2 );
    //                    if ( bval != p.Item3 )
    //                        return true;
    //                }
    //            }
    //        }

    //        return false;
    //    }

    //    List<Tuple<DomNodeType, AttributeInfo, bool>> m_dependsOnList;
    //}

}
