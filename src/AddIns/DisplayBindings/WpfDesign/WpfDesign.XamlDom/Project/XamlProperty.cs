﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace ICSharpCode.WpfDesign.XamlDom
{
	/// <summary>
	/// Describes a property on a <see cref="XamlObject"/>.
	/// </summary>
	public sealed class XamlProperty
	{
		XamlObject parentObject;
		XamlPropertyInfo propertyInfo;
		XamlPropertyValue propertyValue;
		List<XamlPropertyValue> collectionElements;
		bool _isCollection;
		
		static readonly IList<XamlPropertyValue> emptyCollectionElementsArray = new XamlPropertyValue[0];
		
		// for use by parser only
		internal XamlProperty(XamlObject parentObject, XamlPropertyInfo propertyInfo, XamlPropertyValue propertyValue)
		{
			this.parentObject = parentObject;
			this.propertyInfo = propertyInfo;
			
			this.propertyValue = propertyValue;
			if (propertyValue != null) {
				propertyValue.ParentProperty = this;
			} else {
				if (propertyInfo.IsCollection) {
					_isCollection = true;
					collectionElements = new List<XamlPropertyValue>();
				}
			}
		}
		
		internal XamlProperty(XamlObject parentObject, XamlPropertyInfo propertyInfo)
		{
			this.parentObject = parentObject;
			this.propertyInfo = propertyInfo;
			_isCollection = propertyInfo.IsCollection;
		}
		
		/// <summary>
		/// Gets the parent object for which this property was declared.
		/// </summary>
		public XamlObject ParentObject {
			get { return parentObject; }
		}
		
		/// <summary>
		/// Gets the property name.
		/// </summary>
		public string PropertyName {
			get { return propertyInfo.Name; }
		}
		
		/// <summary>
		/// Gets the value of the property. Can be null if the property is a collection property.
		/// </summary>
		public XamlPropertyValue PropertyValue {
			get { return propertyValue; }
		}
		
		/// <summary>
		/// Gets if the property is a collection property.
		/// </summary>
		public bool IsCollection {
			get { return _isCollection; }
		}
		
		/// <summary>
		/// Gets the collection elements of the property. Is empty if the property is not a collection.
		/// </summary>
		public IList<XamlPropertyValue> CollectionElements {
			get { return collectionElements != null ? collectionElements.AsReadOnly() : emptyCollectionElementsArray; }
		}
		
		/// <summary>
		/// Gets if the property is set.
		/// </summary>
		public bool IsSet {
			get { return propertyValue != null || collectionElements != null; }
		}
		
		/// <summary>
		/// Resets the properties value.
		/// </summary>
		public void Reset()
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// used internally by the XamlParser.
		/// Add a collection element that already is part of the XML DOM.
		/// </summary>
		internal void ParserAddCollectionElement(XamlPropertyValue val)
		{
			collectionElements.Add(val);
			val.ParentProperty = this;
		}
		
		/// <summary>
		/// Gets/Sets the value of the property on the instance without updating the XAML document.
		/// </summary>
		public object ValueOnInstance {
			get{
				return propertyInfo.GetValue(parentObject.Instance);
			}
			set {
				propertyInfo.SetValue(parentObject.Instance, value);
			}
		}
		
		/*public bool IsAttributeSyntax {
			get {
				return attribute != null;
			}
		}
		
		public bool IsElementSyntax {
			get {
				return element != null;
			}
		}
		
		public bool IsImplicitDefaultProperty {
			get {
				return attribute == null && element == null;
			}
		}*/
	}
	
	/// <summary>
	/// Used for the value of a <see cref="XamlProperty"/>.
	/// Can be a <see cref="XamlTextValue"/> or a <see cref="XamlObject"/>.
	/// </summary>
	public abstract class XamlPropertyValue
	{
		/// <summary>
		/// used internally by the XamlParser.
		/// </summary>
		internal abstract object GetValueFor(XamlPropertyInfo targetProperty);
		
		XamlProperty _parentProperty;
		
		/// <summary>
		/// Gets the parent property that this value is assigned to.
		/// </summary>
		public XamlProperty ParentProperty {
			get { return _parentProperty; }
			internal set { _parentProperty = value; }
		}
	}
	
	/// <summary>
	/// A textual value in a .xaml file.
	/// </summary>
	public sealed class XamlTextValue : XamlPropertyValue
	{
		XmlAttribute attribute;
		XmlText textNode;
		XmlSpace xmlSpace;
		
		internal XamlTextValue(XmlAttribute attribute)
		{
			this.attribute = attribute;
		}
		
		internal XamlTextValue(XmlText textNode, XmlSpace xmlSpace)
		{
			this.xmlSpace = xmlSpace;
			this.textNode = textNode;
		}
		
		/// <summary>
		/// The text represented by the value.
		/// </summary>
		public string Text {
			get {
				if (attribute != null)
					return attribute.Value;
				else
					return NormalizeWhitespace(textNode.Value);
			}
			set {
				if (attribute != null)
					attribute.Value = value;
				else
					textNode.Value = value;
			}
		}
		
		string NormalizeWhitespace(string text)
		{
			if (xmlSpace == XmlSpace.Preserve) {
				return text.Replace("\r", "");
			}
			StringBuilder b = new StringBuilder();
			bool wasWhitespace = true;
			foreach (char c in text) {
				if (char.IsWhiteSpace(c)) {
					if (!wasWhitespace) {
						b.Append(' ');
					}
					wasWhitespace = true;
				} else {
					wasWhitespace = false;
					b.Append(c);
				}
			}
			if (b.Length > 0 && wasWhitespace)
				b.Length -= 1;
			return b.ToString();
		}
		
		internal override object GetValueFor(XamlPropertyInfo targetProperty)
		{
			if (targetProperty == null)
				return this.Text;
			TypeConverter converter = targetProperty.TypeConverter;
			if (converter != null) {
				return converter.ConvertFromInvariantString(this.Text);
			} else {
				return this.Text;
			}
		}
	}
}
