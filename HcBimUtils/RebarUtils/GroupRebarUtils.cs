using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Structure;

namespace HcBimUtils.RebarUtils
{
    public static class GroupRebarUtils
    {
        private static string id = "4E5B6F62-B8B3-4A2F-9B06-DDD953D4D4BC";

        private static string field = "GroupNameProperty";
        public static void GroupRebar(this List<Rebar> rebars)
        {
            SetEntityGroupName(Guid.NewGuid().ToString(), rebars);
            var count = 0;
            foreach (var rebar in rebars)
            {
                count += rebar.Quantity;
            }


            foreach (var rebar in rebars)
            {
                rebar.SetParameterValueByName("BS_REBAR_QUANTITY_IN_GROUP", count);
            }

        }

        private static void SetEntityGroupName(string name, List<Rebar> rebars)
        {
            Schema schema = Schema.Lookup(new Guid(
                "4E5B6F62-B8B3-4A2F-9B06-DDD953D4D4BC"));

            // 2. Check if schema exists in the memory or not

            if (schema == null)
            {
                // 3. Create it, if not

                schema = CreateSchema();
            }

            // 4. Create entity of the specific schema

            var entity = new Entity(schema);

            // 5. Set the value for the Field.
            // HERE WE HAVE TO REMEMEBER THE
            // NAME OF THE SCHEMA FIELD
            // It would be better to check if the field with
            // such name exists in the schema

            entity.Set("GroupNameProperty", name);

            // 6. Attach entity to the element


            foreach (var rebar in rebars)
            {
                rebar.SetEntity(entity);
            }

        }

        private static Schema CreateSchema()
        {
            SchemaBuilder schemaBuilder =
                new SchemaBuilder(new Guid(
                    "4E5B6F62-B8B3-4A2F-9B06-DDD953D4D4BC"));
            schemaBuilder.SetSchemaName("RebarGroup");
            // Have to define the field name as string and
            // set the type using typeof method
            schemaBuilder.AddSimpleField("GroupNameProperty",
                typeof(string));
            return schemaBuilder.Finish();
        }

        private static void Update(List<Rebar> rebars, List<string> groupNames)
        {
            var dic = new Dictionary<string, List<Rebar>>();
            foreach (var g in groupNames)
            {
                if (dic.ContainsKey(g) == false)
                {
                    dic.Add(g, new List<Rebar>());
                }
            }
            foreach (var rebar in rebars)
            {
                var g = rebar.GetDataAsString(field, id);
                if (dic.ContainsKey(g))
                {
                    dic[g].Add(rebar);
                }
            }
            foreach (var list in dic.Values)
            {
                var c = 0;
                list.ForEach(x => c += x.Quantity);
                foreach (var rebar in list)
                {
                    rebar.SetParameterValueByName("BS_REBAR_QUANTITY_IN_GROUP", c);
                }
            }
        }
    }
}
