﻿using System;
using System.Linq;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Logic.Utils
{
    public sealed class SessionFactory
    {
        private readonly ISessionFactory _factory;

        public SessionFactory(string connectionString)
        {
            _factory = BuildSessionFactory(connectionString);
        }

        internal ISession OpenSession()
        {
            return _factory.OpenSession();
        }

        private static ISessionFactory BuildSessionFactory(string connectionString)
        {
            try
            {
                FluentConfiguration configuration = Fluently.Configure()
                    .Database(MsSqlConfiguration.MsSql7.ConnectionString(connectionString))
                    .Mappings(m => m.FluentMappings
                        .AddFromAssembly(Assembly.GetExecutingAssembly())
                        .Conventions.Add(
                            ForeignKey.EndsWith("ID"),
                            ConventionBuilder.Property.When(criteria => criteria.Expect(x => x.Nullable, Is.Not.Set), x => x.Not.Nullable()))
                        .Conventions.Add<OtherConversions>()
                        .Conventions.Add<TableNameConvention>()
                        .Conventions.Add<HiLoConvention>()
                    );
                return configuration.BuildSessionFactory();
            }
            catch (FluentConfigurationException ex)
            {
                Console.WriteLine("Fluent NHibernate configuration failed: " + ex.Message);
        
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                // Log potential reasons for better debugging
                if (ex.PotentialReasons.Any())
                {
                    Console.WriteLine("Potential Reasons:");
                    foreach (var reason in ex.PotentialReasons)
                    {
                        Console.WriteLine($"- {reason}");
                    }
                }
                throw;
            }
        }

        private class OtherConversions : IHasManyConvention, IReferenceConvention
        {
            public void Apply(IOneToManyCollectionInstance instance)
            {
                instance.LazyLoad();
                instance.AsBag();
                instance.Cascade.SaveUpdate();
                instance.Inverse();
            }

            public void Apply(IManyToOneInstance instance)
            {
                instance.LazyLoad(Laziness.Proxy);
                instance.Cascade.None();
                instance.Not.Nullable();
            }
        }

        public class TableNameConvention : IClassConvention
        {
            public void Apply(IClassInstance instance)
            {
                instance.Table(instance.EntityType.Name);
            }
        }

        public class HiLoConvention : IIdConvention
        {
            public void Apply(IIdentityInstance instance)
            {
                instance.Column(instance.EntityType.Name + "ID");
                instance.GeneratedBy.Native();
            }
        }
    }
}
