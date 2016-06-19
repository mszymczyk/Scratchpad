using System;
using System.Collections.Generic;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;
using pico.Controls.PropertyEditing;

namespace CircuitEditorSample
{
    /// <summary>
    /// Provides property descriptors on MaterialInstances. Based on ModuleProperties class. Work in progress...</summary>
    public class MaterialInstanceProperties : CustomTypeDescriptorNodeAdapter, IDynamicTypeDescriptor
    {
        /// <summary>
        /// Creates an array of property descriptors that are associated with the adapted DomNode's
        /// DomNodeType. No duplicates are in the array (based on the property descriptor's Name
        /// property).</summary>
        /// <returns>Array of property descriptors</returns>
        protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
        {
            System.ComponentModel.PropertyDescriptor[] baseDescriptors = base.GetPropertyDescriptors();

            MaterialInstance materialInstance = DomNode.Cast<MaterialInstance>();
            IList<MaterialInstanceParameter> miParameters = materialInstance.Parameters;
            if (miParameters.Count == 0)
                return baseDescriptors;

            var result = new List<System.ComponentModel.PropertyDescriptor>(baseDescriptors);
            int childIndex = 0;
            foreach (var child in miParameters)
            {
                //var displayName = (string)child.GetAttribute(Schema.dynamicPropertyType.nameAttribute);
                string displayName = child.DisplayName;
                //var category = (string)child.GetAttribute(Schema.dynamicPropertyType.categoryAttribute);
                //string category = child.Category;
                string category = child.Module.Id;
                //var description = (string)child.GetAttribute(Schema.dynamicPropertyType.descriptionAttribute);
                string description = child.Description;
                bool readOnly = false;
                //var editorTypeAndParameters = (string)child.GetAttribute(Schema.dynamicPropertyType.editorAttribute);
                //object editor = CreateObject(editorTypeAndParameters);
                //var typeConverterAndParameters = (string)child.GetAttribute(Schema.dynamicPropertyType.converterAttribute);
                //var typeConverter = (TypeConverter)CreateObject(typeConverterAndParameters);
                //string valueType = (string)child.GetAttribute(Schema.dynamicPropertyType.valueTypeAttribute);
                string valueType = child.ValueType;

                ChildAttributePropertyDescriptor overrideDescriptor = new ChildAttributePropertyDescriptor(
                    "Override",
                    Schema.materialInstanceParameterType.overrideAttribute,
                    Schema.materialInstanceType.parameterChild,
                    childIndex,
                    category, "Override material setting", false,
                    new BoolEditor(), null );
                result.Add( overrideDescriptor );

                PropertyDescriptor newDescriptor = null;
                if (valueType == "stringValue")
                {
                    //newDescriptor = new ChildAttributePropertyDescriptor(
                    //    displayName,
                    //    Schema.dynamicPropertyType.stringValueAttribute,
                    //    Schema.moduleType.dynamicPropertyChild,
                    //    childIndex,
                    //    category, description, readOnly, editor, typeConverter);
                }
                else if (valueType == "floatValue")
                {
                    newDescriptor = new CustomEnableChildAttributePropertyDescriptor(
                        displayName,
                        Schema.materialInstanceParameterType.floatValueAttribute,
                        Schema.materialInstanceType.parameterChild,
                        childIndex,
                        category, description, readOnly,
                        new NumericEditor()
                        , null
                        , new CustomEnableAttributePropertyDescriptorCallback( Schema.materialInstanceParameterType.overrideAttribute, CustomEnableAttributePropertyDescriptorCallback.Condition.ReadOnlyIfSetToFalse )
                        );
                }
                //else if (valueType == "vector3Value")
                //{
                //    newDescriptor = new ChildAttributePropertyDescriptor(
                //        displayName,
                //        Schema.dynamicPropertyType.vector3ValueAttribute,
                //        Schema.moduleType.dynamicPropertyChild,
                //        childIndex,
                //        category, description, readOnly, editor, typeConverter);
                //}
                //else if (valueType == "boolValue")
                //{
                //    newDescriptor = new ChildAttributePropertyDescriptor(
                //        displayName,
                //        Schema.dynamicPropertyType.boolValueAttribute,
                //        Schema.moduleType.dynamicPropertyChild,
                //        childIndex,
                //        category, description, readOnly, editor, typeConverter);
                //}
                else if (valueType == "colorValue")
                {
                    newDescriptor = new CustomEnableChildAttributePropertyDescriptor(
                        displayName,
                        Schema.materialInstanceParameterType.colorValueAttribute,
                        Schema.materialInstanceType.parameterChild,
                        childIndex,
                        category, description, readOnly,
                        new Sce.Atf.Controls.PropertyEditing.ColorPickerEditor(),
                        new Sce.Atf.Controls.PropertyEditing.IntColorConverter()
                        , new CustomEnableAttributePropertyDescriptorCallback( Schema.materialInstanceParameterType.overrideAttribute, CustomEnableAttributePropertyDescriptorCallback.Condition.ReadOnlyIfSetToFalse )
                    );
                }
                else if ( valueType == "uriValue" )
                {
                    newDescriptor = new CustomEnableChildAttributePropertyDescriptor(
                        displayName,
                        Schema.materialInstanceParameterType.uriValueAttribute,
                        Schema.materialInstanceType.parameterChild,
                        childIndex,
                        category, description, readOnly,
                        new FileUriEditor( "TextureType file (*.dds)|*.dds" ),
                        null
                        , new CustomEnableAttributePropertyDescriptorCallback( Schema.materialInstanceParameterType.overrideAttribute, CustomEnableAttributePropertyDescriptorCallback.Condition.ReadOnlyIfSetToFalse )
                    );
                }
                else
                    throw new InvalidOperationException("Unknown valueType attribute '" + valueType +
                        "' for dynamic property: " + displayName);

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

        //// This code came from Sce.Atf.Dom.PropertyDescriptor.
        //private object CreateObject(string typeName)
        //{
        //    string paramString = string.Empty;
        //    if (!string.IsNullOrEmpty(typeName))
        //    {
        //        // check for params
        //        int colonIndex = typeName.IndexOf(':');
        //        if (colonIndex >= 0)
        //        {
        //            int paramsIndex = colonIndex + 1;
        //            paramString = typeName.Substring(paramsIndex, typeName.Length - paramsIndex);
        //            typeName = typeName.Substring(0, colonIndex);
        //        }

        //        // create object from type name
        //        Type objectType = Type.GetType(typeName);
        //        if (objectType == null)
        //            throw new InvalidOperationException("Couldn't find type " + typeName);

        //        // initialize with params
        //        object obj = Activator.CreateInstance(objectType);
        //        IAnnotatedParams annotatedObj = obj as IAnnotatedParams;
        //        if (annotatedObj != null)
        //        {
        //            string[] parameters = null;

        //            if (!string.IsNullOrEmpty(paramString))
        //                parameters = paramString.Split(',');
        //            //else
        //            //    parameters = TryGetEnumeration(domNodeType, annotation);
        //            if (parameters != null)
        //                annotatedObj.Initialize(parameters);
        //        }

        //        return obj;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
    }
}
