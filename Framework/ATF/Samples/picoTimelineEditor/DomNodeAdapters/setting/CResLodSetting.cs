//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a control point</summary>
    public class CResLodSetting : TimelineSetting
	{
		#region ITimelineSetting Members

		/// <summary>
		/// Gets setting's ui display name
		/// </summary>
		public override string Label
		{
			get { return "cresLod - " + NodeName; }
		}

		///// <summary>
		///// Clones this control point</summary>
		///// <returns>Cloned control point</returns>
		//public IControlPoint Clone()
		//{
		//	DomNode node = new DomNode(Schema.controlPointType.Type);
		//	// clone local attributes
		//	foreach (AttributeInfo attributeInfo in DomNode.Type.Attributes)
		//	{
		//		object value = DomNode.GetLocalAttribute(attributeInfo);
		//		if (value != null)
		//			node.SetAttribute(attributeInfo, attributeInfo.Type.Clone(value));
		//	}

		//	node.InitializeExtensions();
		//	return node.As<IControlPoint>();
		//}

        #endregion


		/// <summary>
		/// Gets and sets the setting's node name</summary>
		public string NodeName
		{
			get { return (string) DomNode.GetAttribute( Schema.cresLodSettingType.nodeNameAttribute ); }
			set { DomNode.SetAttribute( Schema.cresLodSettingType.nodeNameAttribute, value ); }
		}

		/// <summary>
		/// Gets or sets lod0Distance</summary>
		public float Lod0Distance
		{
			get { return GetAttribute<float>( Schema.cresLodSettingType.lod0DistanceAttribute ); }
			set { SetAttribute( Schema.cresLodSettingType.lod0DistanceAttribute, value ); }
		}
	}
}
