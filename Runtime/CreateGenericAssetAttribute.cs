﻿namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using UnityEditor;

    public class CreateCustomAssetMenuAttribute : Attribute, IMetadataAttribute
    {
        private readonly string _menuName;

        public CreateCustomAssetMenuAttribute(string menuName)
        {
            _menuName = menuName;
        }

        public Attribute Process()
        {
            return new MenuItem($"Assets/Create/{_menuName}");
        }
    }

    public interface IMetadataAttribute
    {
        Attribute Process();
    }

    public class GenericSOPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _original;

        public GenericSOPropertyDescriptor(PropertyDescriptor originalProperty)
            : base(originalProperty) { _original = originalProperty; }

        public override AttributeCollection Attributes
        {
            get
            {
                var attributes = base.Attributes.Cast<Attribute>();
                var result = new List<Attribute>();

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is IMetadataAttribute metadataAttribute)
                    {
                        Attribute realAttribute = metadataAttribute.Process();
                        if (realAttribute != null)
                            result.Add(realAttribute);
                    }
                    else
                    {
                        result.Add(attribute);
                    }
                }

                return new AttributeCollection(result.ToArray());
            }
        }

        public override bool CanResetValue(object component) => _original.CanResetValue(component);

        public override object GetValue(object component) => _original.GetValue(component);

        public override void ResetValue(object component) => _original.ResetValue(component);

        public override void SetValue(object component, object value) => _original.SetValue(component, value);

        public override bool ShouldSerializeValue(object component) => _original.ShouldSerializeValue(component);

        public override Type ComponentType => _original.ComponentType;
        public override bool IsReadOnly => _original.IsReadOnly;
        public override Type PropertyType => _original.PropertyType;
    }

    public class GenericSOTypeDescriptor : CustomTypeDescriptor
    {
        public GenericSOTypeDescriptor(ICustomTypeDescriptor originalDescriptor)
            : base(originalDescriptor) { }

        public override PropertyDescriptorCollection GetProperties() => GetProperties(new Attribute[] { });

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var properties = base.GetProperties(attributes)
                .Cast<PropertyDescriptor>()
                .Select(p => new GenericSOPropertyDescriptor(p))
                .ToArray();

            return new PropertyDescriptorCollection(properties);
        }
    }

    public class GenericSODescriptionProvider : TypeDescriptionProvider
    {
        public GenericSODescriptionProvider()
            : base(TypeDescriptor.GetProvider(typeof(object))) { }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            ICustomTypeDescriptor baseDescriptor = base.GetTypeDescriptor(objectType, instance);
            return new GenericSOTypeDescriptor(baseDescriptor);
        }
    }
}