namespace AspNetCoreHistory.Utilities;

public static class PatchUtilities
{
    public static T Apply<T, TOther>(T obj, TOther changes) where T: class
    {
        var type = typeof(T);
        var properties = type.GetProperties();
        var changesProperties = typeof(TOther).GetProperties().ToDictionary(x => x.Name);

        foreach (var prop in properties)
        {
            var newValue = prop.GetValue(changes);

            if (newValue != null)
            {
                prop.SetValue(obj, newValue);
            }
        }

        return obj;
    }
}
