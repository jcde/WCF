using System;
using System.Runtime.Serialization;
using WcfDomain.Contracts;

namespace WcfDomain
{
    [DataContract(Namespace = Namespaces.Entity)]
    [KnownType("GetKnownTypes")]
    [Serializable]
    public abstract class KnownEntity
    {
        /*
        public override ISlot GetSlot()
        {
            if (DataAccess == DAType.Web)
                return (ISlot) Activator.CreateInstance(typeof (WebSlot), new[] {GetType()});
            return base.GetSlot();
        }

        public static List<KnownEntity> List(IList l)
        {
            var nl = new List<KnownEntity>();
            foreach (KnownEntity o in l)
            {
                nl.Add(o);
            }
            return nl;
        }

        public static Dictionary<Type, List<KnownEntity>> Dictionary(List<KnownEntity> l)
        {
            var d= new Dictionary<Type, List<KnownEntity>>();
            foreach (var o in l)
            {
                List<KnownEntity> n;
                if (!d.ContainsKey(o.GetType()))
                {
                    n = new List<KnownEntity>();
                    d.Add(o.GetType(), n);
                }
                else
                {
                    n = d[o.GetType()];
                }
                n.Add(o);
            }
            return d;
        }

        public static IEnumerable<Type> GetKnownTypes()
        {
            var l = new List<Type> ();
            foreach (TypeDescriptor td in DescriptorCache.Instance)
                if (td.RealType.IsSubclassOf(typeof (Entity)))
                {
                    // there are too many Entity's children
                    if (td.RealType.IsSubclassOf(typeof(KnownEntity)))
                        l.Add(td.RealType);
                    foreach (ScalarFieldDescriptor sfd in td.Fields)
                    {
                            var t = sfd.OriginalPropertyType;
                            if (t != null)
                            {
                                if ((t.IsEnum || t.IsGenericType && t.GetGenericArguments()[0].IsEnum)
                                    && !l.Contains(sfd.OriginalPropertyType))
                                    l.Add(sfd.OriginalPropertyType);
                            }
                    }
                }
            return l;
        }

        public override void Delete(bool withCheck)
        {
            if (DataAccess == DAType.Web)
            {
                if (!new WebSlot(GetType()).Delete(this, withCheck))
                    throw new CascadeDeleteException("");
            }
            else
            {
                base.Delete(withCheck);
            }
        }*/
    }
}