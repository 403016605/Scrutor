﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor
{
    internal class AttributeSelector : ISelector
    {
        public AttributeSelector(IEnumerable<Type> types)
        {
            Types = types;
        }

        private IEnumerable<Type> Types { get; }

        void ISelector.Populate(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            foreach (var type in Types)
            {
                var typeInfo = type.GetTypeInfo();

                var attributes = typeInfo.GetCustomAttributes<ServiceDescriptorAttribute>().ToArray();

                // Check if the type has multiple attributes with same ServiceType.
                var duplicates = GetDuplicates(attributes);

                if (duplicates.Any())
                {
                    throw new InvalidOperationException($@"Type ""{type.FullName}"" has multiple ServiceDescriptor attributes with the same service type.");
                }

                foreach (var attribute in attributes)
                {
                    var serviceTypes = attribute.GetServiceTypes(type);

                    foreach (var serviceType in serviceTypes)
                    {
                        var descriptor = new ServiceDescriptor(serviceType, type, attribute.Lifetime);

                        services.Add(descriptor);
                    }
                }
            }
        }

        private static IEnumerable<ServiceDescriptorAttribute> GetDuplicates(IEnumerable<ServiceDescriptorAttribute> attributes)
        {
            return attributes.GroupBy(s => s.ServiceType).SelectMany(grp => grp.Skip(1));
        }
    }
}
