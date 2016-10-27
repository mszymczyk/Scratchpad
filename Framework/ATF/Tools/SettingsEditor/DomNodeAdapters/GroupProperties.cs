using System;
using System.Collections.Generic;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;
using pico.Controls.PropertyEditing;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using misz.Gui;

namespace SettingsEditor
{
    /// <summary>
    /// Provides property descriptors on DynamicProperties. Based on MaterialEditor's/CircuitEditor's ModuleProperties class.</summary>
    public class GroupProperties : CustomTypeDescriptorNodeAdapter, IDynamicTypeDescriptor
    {
        /// <summary>
        /// Creates an array of property descriptors that are associated with the adapted DomNode's
        /// DomNodeType. No duplicates are in the array (based on the property descriptor's Name
        /// property).</summary>
        /// <returns>Array of property descriptors</returns>
        protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
        {
            System.ComponentModel.PropertyDescriptor[] baseDescriptors = base.GetPropertyDescriptors();

            Group group = DomNode.Cast<Group>();
            IList<DynamicProperty> miParameters = group.Properties;
            if (miParameters.Count == 0)
                return baseDescriptors;

            var result = new List<System.ComponentModel.PropertyDescriptor>(baseDescriptors);
            int childIndex = 0;
            foreach (var child in miParameters)
            {
                Setting sett = child.Setting;

                PropertyDescriptor newDescriptor = null;
                if ( child.Setting is BoolSetting )
                {
                    BoolSetting bsett = child.Setting as BoolSetting;

                    newDescriptor = CreateAttributePropertyDescriptor(
                        child
                        , group
                        , Schema.dynamicPropertyType.bvalAttribute
                        , Schema.groupType.propChild
                        , childIndex
                        , new BoolEditor()
                        , null
                        );
                }
                else if ( child.Setting is IntSetting )
                {
                    IntSetting isett = child.Setting as IntSetting;

                    newDescriptor = CreateAttributePropertyDescriptor(
                        child
                        , group
                        , Schema.dynamicPropertyType.ivalAttribute
                        , Schema.groupType.propChild
                        , childIndex
                        , new BoundedIntEditor( isett.MinValue, isett.MaxValue )
                        , null
                        );
                }
                else if ( child.Setting is EnumSetting )
                {
                    EnumSetting esett = child.Setting as EnumSetting;

                    Type enumType = esett.EnumType;
                    string[] enumNames = enumType.GetEnumNames();
                    for ( int i = 0; i < enumNames.Length; ++i )
                    {
                        FieldInfo enumField = enumType.GetField( enumNames[i] );
                        EnumLabelAttribute attr = enumField.GetCustomAttribute<EnumLabelAttribute>();
                        enumNames[i] = attr != null ? attr.Label : enumNames[i];
                    }

                    Array values = enumType.GetEnumValues();
                    int[] enumValues = new int[enumNames.Length];
                    for ( int i = 0; i < enumNames.Length; ++i )
                        enumValues[i] = (int)values.GetValue( i );

                    var formatEditor = new LongEnumEditor();
                    formatEditor.DefineEnum( enumNames );
                    formatEditor.MaxDropDownItems = 10;

                    newDescriptor = CreateAttributePropertyDescriptor(
                        child
                        , group
                        , Schema.dynamicPropertyType.evalAttribute
                        , Schema.groupType.propChild
                        , childIndex
                        , formatEditor
                        , new IntEnumTypeConverter( enumNames, enumValues )
                        );
                }
                else if ( child.Setting is FloatSetting )
                {
                    FloatSetting fsett = child.Setting as FloatSetting;

                    object editor = new FlexibleFloatEditor(
                        fsett.MinValue,
                        fsett.MaxValue,
                        fsett.HasCheckBox
                        //Schema.dynamicPropertyType.minAttribute,
                        //Schema.dynamicPropertyType.maxAttribute,
                        //Schema.dynamicPropertyType.stepAttribute,
                        //Schema.dynamicPropertyType.extraNameAttribute,
                        //Schema.dynamicPropertyType.checkedAttribute
                        );

                    //newDescriptor = CreateAttributePropertyDescriptor(
                    newDescriptor = CreateFlexibleFloatAttributePropertyDescriptor(
                        child
                        , group
                        , Schema.dynamicPropertyType.fvalAttribute
                        , Schema.dynamicPropertyType.minAttribute
                        , Schema.dynamicPropertyType.maxAttribute
                        , Schema.dynamicPropertyType.stepAttribute
                        , Schema.dynamicPropertyType.extraNameAttribute
                        , Schema.dynamicPropertyType.checkedAttribute
                        , Schema.groupType.propChild
                        , childIndex
                        , editor
                        , null
                        );
                }
                else if ( child.Setting is Float4Setting )
                {
                    Float4Setting fsett = child.Setting as Float4Setting;

                    newDescriptor = CreateAttributePropertyDescriptor(
                        child
                        , group
                        , Schema.dynamicPropertyType.f4valAttribute
                        , Schema.groupType.propChild
                        , childIndex
                        , new NumericTupleEditor( typeof(float), new string[] { "X", "Y", "Z", "W" } )
                        , null
                        );
                }
                else if ( child.Setting is ColorSetting )
                {
                    ColorSetting isett = child.Setting as ColorSetting;

                    newDescriptor = CreateAttributePropertyDescriptor(
                        child
                        , group
                        , Schema.dynamicPropertyType.colvalAttribute
                        , Schema.groupType.propChild
                        , childIndex
                        , new ColorPickerEditor()
                        , new Float3ColorConverter() // what about gamma?
                        );
                }
                else if ( child.Setting is StringSetting )
                {
                    StringSetting ssett = child.Setting as StringSetting;

                    newDescriptor = CreateAttributePropertyDescriptor(
                        child
                        , group
                        , Schema.dynamicPropertyType.svalAttribute
                        , Schema.groupType.propChild
                        , childIndex
                        , null
                        , null
                        );
                }
                else if ( child.Setting is DirectionSetting )
                {
                    DirectionSetting ssett = child.Setting as DirectionSetting;

                    newDescriptor = CreateAttributePropertyDescriptor(
                        child
                        , group
                        , Schema.dynamicPropertyType.dirvalAttribute
                        , Schema.groupType.propChild
                        , childIndex
                        , new DirectionPropertyEditor( D3DManipulator.Instance )
                        , null
                        );
                }
                //else
                //    throw new InvalidOperationException( "Unknown valueType attribute '" + valueType +
                //        "' for dynamic property: " + displayName );

                if ( newDescriptor != null )
                    result.Add(newDescriptor);
                childIndex++;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns true iff this custom type descriptor can provide a PropertyDescriptorCollection
        /// (via GetProperties) that is the same for all instances of this type of object
        /// and that can be permanently cached</summary>
        /// <remarks>Returning 'true' greatly improves performance.</remarks>
        public bool CacheableProperties { get { return false; } }
        //public bool CacheableProperties { get { return true; } }

        static DynamicProperty FindDynamicProperty( Group group, string name )
        {
            foreach ( DynamicProperty dp in group.Properties )
            {
                if ( dp.Name == name )
                {
                    if ( dp.Setting is BoolSetting )
                    {
                        return dp;
                    }
                }
            }

            return null;
        }

        static List<Tuple<DomNode, bool>> GetDependsOnListValidated( List<Tuple<string, bool>> dependsOn, Group group )
        {
            List<Tuple<DomNode, bool>> validatedList = new List<Tuple<DomNode, bool>>();

            foreach ( var tup in dependsOn )
            {
                bool err = true;
                string s = tup.Item1;

                if ( s.Contains( "." ) )
                {
                    string[] tokens = s.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
                    {
                        DomNode root = group.DomNode.GetRoot();
                        DomNode foundNode = null;
                        int itoken = 0;
                        while ( foundNode == null )
                        {
                            foreach ( DomNode dn in root.Children )
                            {
                                Group og = dn.As<Group>();
                                if ( og != null )
                                {
                                    if ( og.Name == tokens[itoken] )
                                    {
                                        ++itoken;
                                        root = dn;
                                        if ( itoken == tokens.Length - 1 )
                                        {
                                            foundNode = dn;
                                        }
                                        break;
                                    }
                                }
                            }

                            break;
                        }

                        if ( foundNode != null )
                        {
                            DynamicProperty dp = FindDynamicProperty( foundNode.Cast<Group>(), tokens[tokens.Length - 1] );
                            if ( dp != null )
                            {
                                err = false;
                                validatedList.Add( new Tuple<DomNode, bool>( dp.DomNode, tup.Item2 ) );
                            }
                        }
                    }
                }
                else
                {
                    DynamicProperty dp = FindDynamicProperty( group, s );
                    if ( dp != null )
                    {
                        err = false;
                        validatedList.Add( new Tuple<DomNode, bool>( dp.DomNode, tup.Item2 ) );
                    }
                }

                if ( err )
                {
                    Outputs.WriteLine( OutputMessageType.Error, s + " is not boolean attribute. DependsOn must refer to boolean attribute" );
                }
            }

            return validatedList;
        }

        static private AttributePropertyDescriptor CreateAttributePropertyDescriptor(
            DynamicProperty dp, Group group, AttributeInfo ai, ChildInfo ci, int childIndex, object editor, TypeConverter typeConverter
            )
        {
            Setting sett = dp.Setting;

            List<Tuple<DomNode, bool>> dependsOn = GetDependsOnListValidated( sett.DependsOn, group );
            Group groupParent = group.DomNode.Parent.As<Group>();
            if ( groupParent != null && sett.Group.ParentStructure != null && sett.Group.DependsOn != null )
            {
                List<Tuple<DomNode, bool>> structureDependsOn = GetDependsOnListValidated( sett.Group.DependsOn, groupParent );
                dependsOn.AddRange( structureDependsOn );
            }

            dependsOn = dependsOn.Distinct().ToList();

            string displayName = string.IsNullOrEmpty(dp.ExtraName) ? sett.DisplayName : string.Format( "{0}({1})", sett.DisplayName, dp.ExtraName );

            AttributePropertyDescriptor apd;
            if ( dependsOn.Count > 0 )
            {
                DependsOnNodes don = new DependsOnNodes( dependsOn );

                apd = new CustomEnableChildAttributePropertyDescriptor(
                        displayName,
                        ai,
                        ci,
                        childIndex,
                        sett.Category,
                        sett.HelpText,
                        false,
                        editor,
                        typeConverter,
                        don
                    );
            }
            else
            {
                apd = new ChildAttributePropertyDescriptor(
                    displayName,
                    ai,
                    ci,
                    childIndex,
                    sett.Category,
                    sett.HelpText,
                    false,
                    editor,
                    typeConverter
                );
            }

            return apd;
        }

        static private AttributePropertyDescriptor CreateFlexibleFloatAttributePropertyDescriptor(
                        DynamicProperty dp, Group group,
                        AttributeInfo attributeInfo,
                        AttributeInfo softMinAttribute,
                        AttributeInfo softMaxAttribute,
                        AttributeInfo stepAttribute,
                        AttributeInfo extraNameAttribute,
                        AttributeInfo checkedAttribute,
                        ChildInfo ci, int childIndex, object editor, TypeConverter typeConverter
            )
        {
            Setting sett = dp.Setting;

            List<Tuple<DomNode, bool>> dependsOn = GetDependsOnListValidated( sett.DependsOn, group );
            Group groupParent = group.DomNode.Parent.As<Group>();
            if ( groupParent != null && sett.Group.ParentStructure != null && sett.Group.DependsOn != null )
            {
                List<Tuple<DomNode, bool>> structureDependsOn = GetDependsOnListValidated( sett.Group.DependsOn, groupParent );
                dependsOn.AddRange( structureDependsOn );
            }

            dependsOn = dependsOn.Distinct().ToList();

            string displayName = string.IsNullOrEmpty( dp.ExtraName ) ? sett.DisplayName : string.Format( "{0}({1})", sett.DisplayName, dp.ExtraName );

            DependsOnNodes don = null;
            if ( dependsOn.Count > 0 )
                don = new DependsOnNodes( dependsOn );

            AttributePropertyDescriptor apd = new FlexibleFloatChildAttributePropertyDescriptor(
                    displayName,
                    attributeInfo,
                    softMinAttribute,
                    softMaxAttribute,
                    stepAttribute,
                    extraNameAttribute,
                    checkedAttribute,
                    ci,
                    childIndex,
                    sett.Category,
                    sett.HelpText,
                    false,
                    editor,
                    typeConverter,
                    don
                );
            return apd;
        }

        public class DependsOnNodes : ICustomEnableAttributePropertyDescriptorCallback
        {
            public DependsOnNodes()
            {
                m_dependsOnList = new List<Tuple<DomNode, bool>>();
            }

            public DependsOnNodes( List<Tuple<DomNode, bool>> dependsOn )
            {
                m_dependsOnList = dependsOn;
            }

            public void Add( DomNode dn, bool condition )
            {
                m_dependsOnList.Add( new Tuple<DomNode, bool>( dn, condition ) );
            }

            public bool IsReadOnly( DomNode domNode, AttributePropertyDescriptor descriptor )
            {
                DomNode root = domNode.GetRoot();

                foreach ( var t in m_dependsOnList )
                {
                    DynamicProperty dp = t.Item1.Cast<DynamicProperty>();
                    System.Diagnostics.Debug.Assert( dp.Setting is BoolSetting );
                    bool bval = dp.BoolValue;
                    if ( bval != t.Item2 )
                        return true;
                }

                return false;
            }

            List<Tuple<DomNode, bool>> m_dependsOnList;
        }
    }
}
