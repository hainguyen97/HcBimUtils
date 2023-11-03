using System.IO;
using System.Xml.Serialization;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HcBimUtils
{
    public static class ElementDataExtension
    {
        public static void SetDataString(this Element e, string name, string id, string field, string o)
        {
            if (e.IsValidElement())
            {
                Schema schema = Schema.Lookup(new Guid(id));
                // 2. Check if schema exists in the memory or not
                if (schema == null)
                {
                    // 3. Create it, if not
                    schema = CreateSchema(id, field, name);
                }
                Entity ent = new Entity(schema);
                ent.Set(field, o);
                e.SetEntity(ent);
            }
        }

        public static T GetData<T>(this Element e, string field, string id)
        {
            var s = GetDataAsString(e, field, id);
            XmlSerializer xml = new XmlSerializer(typeof(T));
            using StringReader r = new StringReader(s);
            return (T)xml.Deserialize(r);
        }

        public static string GetDataAsString(this Element e, string field, string id)
        {
            Schema sch = Schema.Lookup(new Guid(id));
            if (sch != null)
            {
                var entity = e.GetEntity(sch);
                if (entity != null && entity.IsValid() && entity.IsValidObject)
                {
                    string s = entity.Get<string>(field);
                    return s;
                }
            }
            return string.Empty;
        }

        private static Schema CreateSchema(string id, string field, string name)
        {
            SchemaBuilder schemaBuilder =
                new SchemaBuilder(new Guid(id));

            schemaBuilder.SetSchemaName(name);

            // Have to define the field name as string and
            // set the type using typeof method

            schemaBuilder.AddSimpleField(field,
                typeof(string));

            return schemaBuilder.Finish();
        }
    }
}